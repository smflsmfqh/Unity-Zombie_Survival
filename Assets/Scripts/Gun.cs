using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    // --- 총의 상태를 나타내는 열거형---
    public enum Stats
    {
        Ready, // 총이 발사될 수 있는 상태인 Ready
        Empty, // 탄창이 비어있는 상태인 Empty
        Reloading // 재장전 중인 상태인 Reloading
    }

    public Stats State { get; private set; }
    public UIManager uiManager;

    public Transform fireTransform; // 총알이 발사되는 위치 -> 범위로 정의해야 방향같은 걸 정의할 수 있음, 빈 게임 오브젝트활용

    // --- public으로 선언한 이유? - 인스펙터 창에서 할당하기 위해서, 총구 화염 효과와 탄피 배출 효과는 총마다 다를 수 있기 때문에, 각 총마다 다른 효과를 할당할 수 있도록 public으로 선언
    public ParticleSystem muzzleFlashEffect;  // 총구 화염 효과
    public ParticleSystem shellEjectEffect;  // 탄피 배출 효과  
    public LayerMask targetLayer;
    private LineRenderer bulletLineRenderer;  // 총알 궤적을 그리기 위한 LineRenderer
                                              // 충돌한 지점을 끝점으로 설정하여 총알 궤적을 시각적으로 표현

    private AudioSource gunAudioPlayer;  // 총 소리를 재생하기 위한 AudioSource 

    public GunData gunData;

    private float fireDistance = 50f; // 사정 거리, 예제에서는 Gun에 넣었지만, GunData에 넣는 것이 더 적절함
    
    public int ammoRemain = 100; // 남은 전체 탄약 수
    public int magAmmo; // 현재 탄창에 남아있는 총알 수

    private float lastFireTime; // 총을 마지막으로 발사한 지점, 시간 간격을 줘서 Update마다 일정하게 동작하도록 함

    private Coroutine coShot;

    // --- 처음 한 번만 실행되는 초기화 작업은 Awake() 메서드에서 수행, 총이 처음 생성될 때 필요한 컴포넌트들을 가져오고, 총알 궤적을 그리기 위한 LineRenderer를 초기화 ---
    private void Awake()
    {
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        bulletLineRenderer.positionCount = 2; // 사용할 점을 두 개로 변경 (직선 렌더링이기 때문에)
        bulletLineRenderer.enabled = false; // 라인 렌더러를 비활성화, 총 쏠 때만 활성화하도록 하기 위해서
    }

    // --- 반복적으로 총을 사용할 때마다 초기화하는 부분이 필요하기 때문에 OnEnable() 메서드에서 초기화 작업을 수행, 총이 활성화될 때마다 총알 수와 상태를 초기화 --- 
    private void OnEnable()
    {
        ammoRemain = gunData.startAmmoRemain;
        magAmmo = gunData.magCapacity;

        uiManager.SetAmmoText(magAmmo, ammoRemain);

        State = Stats.Ready;
        lastFireTime = 0f;
    }

    // --- 총을 발사하는 로직을 처리하는 Fire() 메서드 ---
    // 총알이 발사될 수 있는 상태인지 확인,
    // 총알이 발사될 수 있는 상태는 Ready 상태이고,
    // 마지막으로 총을 발사한 시간에서 일정 시간이 지났을 때 Shot() 메서드를 호출하여 실제로 총을 발사 
    public void Fire()
    {
        if (State == Stats.Ready && Time.time >= lastFireTime + gunData.timeBetFire)
        {
            lastFireTime = Time.time; // 총을 발사한 시간을 업데이트하여 다음 발사까지의 간격을 유지
            Shot();
        }
    }

    // --- 총을 발사하는 실제 로직을 처리하는 Shot() 메서드 ---
    // Raycast를 사용하여 총알이 충돌하는 지점을 계산하고,
    // 충돌한 대상이 IDamageable 인터페이스를 구현하고 있다면 데미지를 적용,
    // 총알 궤적 효과와 사운드 효과도 함께 처리
    private void Shot()
    {
        // Raycast: 반직선, 시작점과 방향을 지정하여 충돌하는 지점을 계산하는 방법
        Ray ray = new Ray(fireTransform.position, fireTransform.forward); // 총구의 시작점, 총구의 앞 방향

        Vector3 hitPosition = Vector3.zero; // Raycast의 결과로 충돌한 지점을 저장할 변수, 총알이 충돌한 지점이 없을 경우에는 총구에서 사정 거리만큼 떨어진 지점을 저장

        // 총구 정보, 충돌 정보를 저장할 변수, 총알의 사정 거리, Raycast()의 반환형: bool
        if (Physics.Raycast(ray, out RaycastHit hit, fireDistance, targetLayer)) // targetLayer 설정 -> Inspector에서 Everything 선택 -> NoHit(HitBox의 레이어)만 체크 제외 => HItBox가 아닌 좀비 몸의 Capsule Collider를 맞추기 위함
        {
            var target = hit.collider.GetComponent<LivingEntity>();
            if (target != null) // 충돌한 대상이 IDamageable 인터페이스를 구현하고 있다면 데미지를 적용
            {
                if (!target.IsDead)
                {
                    target.OnDamage(gunData.damage, hit.point, hit.normal);
                }

            }

            hitPosition = hit.point; // Raycast가 충돌한 지점으로 설정 (world 좌표)
               
        }
        else
        {
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
        }

        // 총알 궤적 효과를 처리하는 Coroutine을 시작하기 전에, 이전에 실행 중인 Coroutine이 있다면 중지하여 총알 궤적 효과가 겹치지 않도록 함
        // 패턴화해서 사용하도록 함
        if (coShot != null)
        {
            StopCoroutine(coShot); // 이전에 실행 중인 총알 궤적 효과가 있다면 중지
            coShot = null;
        }

        coShot = StartCoroutine(CoShotEffect(hitPosition)); // 호출될 때마다 Coroutine 클래스가 매번 생성됨

        magAmmo--;
        uiManager.SetAmmoText(magAmmo, ammoRemain);

        if (magAmmo <= 0)
        {
            State = Stats.Empty;
        }

    }

    // --- 총알 궤적 효과와 사운드 효과를 처리하는 Coroutine인 CoShotEffect() 메서드 ---
    // 총알이 발사될 때마다 총구 화염 효과와 탄피 배출 효과를 재생하고,
    // 총 소리를 재생, LineRenderer를 사용하여 총알 궤적을 시각적으로 표현,
    // 일정 시간 동안 총알 궤적이 보이도록 대기한 후 LineRenderer를 비활성화 
    private IEnumerator CoShotEffect(Vector3 hitPosition)
    {
        muzzleFlashEffect.Play();
        shellEjectEffect.Play();

        gunAudioPlayer.PlayOneShot(gunData.shotClip);

        bulletLineRenderer.SetPosition(0, fireTransform.position); // 시작점은 총구 위치
        bulletLineRenderer.SetPosition(1, hitPosition); // 끝점은 충돌한 지점으로 설정하여 총알 궤적을 시각적으로 표현
        bulletLineRenderer.enabled = true;

        yield return new WaitForSeconds(0.03f); // 총알 궤적이 잠깐 보이도록 0.03초 동안 대기

        bulletLineRenderer.enabled = false;

        coShot = null; // Coroutine이 끝났음을 표시하여 다음 총알 발사 시 새로운 Coroutine이 시작될 수 있도록 함
    }

    public bool Reload()
    {
        // 이미 재장전 중이거나, 남은 총알이 없거나, 탄창에 총알이 이미 가득한 경우 재장전 불가
        if (State == Stats.Reloading || ammoRemain <= 0 || magAmmo >= gunData.magCapacity)
        {
            return false;
        }

        StartCoroutine(CoReloadRoutine());
        return true;
    }


    // --- 총을 재장전하는 로직을 처리하는 Coroutine인 CoReloadRoutine() 메서드 ---
    // 총이 재장전 중인 상태로 변경되고, 재장전 사운드를 재생
    // 일정 시간 동안 대기한 후 탄창에 총알을 채우고 남은 총알 수를 업데이트
    // 총이 다시 발사될 수 있는 상태로 변경
    private IEnumerator CoReloadRoutine()
    {
        State = Stats.Reloading;
        gunAudioPlayer.PlayOneShot(gunData.reloadClip);

        yield return new WaitForSeconds(gunData.reloadTime);

        int ammoToFill = gunData.magCapacity - magAmmo; // 탄창에 채워야 하는 총알 수, 총알이 가득 찰 때까지 필요한 총알 수

        if (ammoRemain < ammoToFill) // 남은 총알 수가 채워야 하는 총알 수보다 적은 경우, 남은 총알 수만큼만 채움
        {
            ammoToFill = ammoRemain;
        }

        magAmmo += ammoToFill; // 탄창에 총알을 채움
        ammoRemain -= ammoToFill; // 남은 총알 수에서 채운 총알 수만큼 감소

        uiManager.SetAmmoText(magAmmo, ammoRemain);

        State = Stats.Ready;
    }

    public void AddAmmo(int ammo)
    {
        ammoRemain += ammo;
        uiManager.SetAmmoText(magAmmo, ammoRemain);
    }
}
