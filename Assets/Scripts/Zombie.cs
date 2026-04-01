using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : LivingEntity
{
    // Status Enum -> Status Property & switch문 패턴 -> Update 내에서 switch문 패턴 => 상태에 따른 행동 패턴을 쉽게 관리할 수 있도록 하는 구조
    public enum Status
    {
        Idle,
        Trace,
        Attack,
        Die,
    }

    public Transform target;
    public HitBox hitBox;
    public LayerMask targetLayer;
    public ParticleSystem bloodEffect;
    public Collider zombieCollider;
    public Renderer zombieRenderer;

    private NavMeshAgent agent;
    private Animator zombieAnimator;

    public float traceDistance = 10f;
    public float attackDistance = 1f;
    private float attackInterval = 0.5f;
    private float lastAttackTime;
    private float damage;

    public AudioClip deathClip;
    public AudioClip hitClip;

    private AudioSource zombieAudioSource;

    private Status currentStatus;

    public Status CurrentStatus 
    { 
        get { return currentStatus; }
        set
        {
            var prevStatus = currentStatus;
            currentStatus = value;

            Debug.Log($"{currentStatus}, {prevStatus}");    

            // 상태가 변경된 경우에 애니메이션을 업데이트
            switch (currentStatus)
            {
                case Status.Idle:
                    zombieAnimator.SetBool("HasTarget", false);
                    agent.isStopped = true; 
                    break;
                case Status.Trace:
                    zombieAnimator.SetBool("HasTarget", true);
                    agent.isStopped = false;
                    break;
                case Status.Attack:
                    zombieAnimator.SetBool("HasTarget", false);
                    agent.isStopped = true;
                    break;
                case Status.Die:
                    zombieAudioSource.PlayOneShot(deathClip);
                    zombieAnimator.SetTrigger("Die");
                    agent.isStopped = true;
                    zombieCollider.enabled = false; 
                    hitBox.Colliders.Clear();   
                    hitBox.gameObject.SetActive(false);
                    break;
            }
        }
    }
    public void Setup(ZombieData data)
    {
        gameObject.SetActive(false);

        startingHealth = data.maxHP;
        damage = data.damage;
        agent.speed = data.speed;
        zombieRenderer.material.color = data.skinColor;

        gameObject.SetActive(true);

    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        zombieAudioSource = GetComponent<AudioSource>();
        zombieAnimator = GetComponent<Animator>();
        zombieCollider = GetComponent<Collider>(); 
        

        zombieAnimator.SetBool("HasTarget", true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        agent.enabled = true;
        agent.isStopped = false;
        agent.ResetPath(); // NavMesh 에이전트가 이전 경로(및 목적지, 속도)에 대한 정보를 담고 있을 수 있음 -> 초기화해줌

        // 1. NavMeshAgent는 NavMesh 위에 올라가 있어야만 정상 동작함
        // 2. zombie가 활성화 될 때 위치가 만약 NavMesh에서 벗어난다면, SetDestination()을 호출할 때 경로를 못 찾거나 이상하게 움직임
        // 3. 따라서 NavMeshAgent의 위치를 변경하려면, NavMesh에서 제공하는 메서드 사용해야함
        // 4. hit -> transform.position을 넘겨서 가장 가까운 NavMesh 위의 한 점을 담아둠 => zombie가 해당 위치로 활성화되도록 함
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas)) // NavMesh.SamplePosition() 반환형: bool
        {
            agent.Warp(hit.position); // hit.position = transform.position과 가장 가까운 Nav Mesh의 한 점의 위치
        }
        zombieCollider.enabled = true;
        hitBox.gameObject.SetActive(true); 
        

        CurrentStatus = Status.Idle;
    }

    private void Update()
    {
        switch (currentStatus)
        {
            case Status.Idle:
                UpdateIdle();
                break;
            case Status.Trace:
                UpdateTrace();
                break;
            case Status.Attack:
                UpdateAttck();
                break;
            case Status.Die:
                UpdateDie();
                break;
        }
    }

    

    private void UpdateDie()
    {

    }

    private void UpdateAttck()
    {
        if (target == null)
        {
            CurrentStatus = Status.Trace;
            return;
        }

        var find = hitBox.Colliders.Find(x => x.transform == target);

        if (find == null)
        {
            CurrentStatus = Status.Trace;
            return;
        }

        // 타겟을 계속 바라볼 수 있게 방향 조정
        var lookAt = target.position;
        lookAt.y = transform.position.y;
        transform.LookAt(lookAt);
        
        if (Time.time > lastAttackTime + attackInterval)
        {
            Debug.Log("Zombie Attacked the Player");
            lastAttackTime = Time.time;

            var livingEntity = target.GetComponent<LivingEntity>();    
            if (livingEntity != null )
            {
                if (!livingEntity.IsDead)
                {
                    livingEntity.OnDamage(damage, transform.position, -transform.forward);
                }
            }
        }
    }

    // --- UpdateTrace & UpdateIdle: 매 프레임마다 할 필요없이 타이머 줘서 간격 조절해야함 ---

    private void UpdateTrace()
    {
        if (target != null)
        {
            var find = hitBox.Colliders.Find(x => x.transform == target);

            if (find != null)
            {
                CurrentStatus = Status.Attack;
                return;
            }
        }

        if (target == null || Vector3.Distance(target.position, transform.position) > traceDistance)
        {
           
            target = null;
            CurrentStatus = Status.Idle;
            return;
        }

        agent.SetDestination(target.position);

    }

    private void UpdateIdle()
    {
        if (target != null && Vector3.Distance(target.position, transform.position) < traceDistance)
        {
            CurrentStatus = Status.Trace;
            return;
        }

        target = FindTarget(traceDistance);
    }

    private Transform FindTarget(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, targetLayer); // 구의 중점, 구의 반경, 타겟 레이어 (복수 개 및 전체 레이어로 설정 가능)
                                                                                               // 특정한 레이어들을 넘겨 받음
        if (colliders.Length == 0) return null;

        var target = colliders.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).First(); // 제일 앞에 있는 콜라이더 반환

        return target.transform;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        zombieAudioSource.PlayOneShot(hitClip);

        base.OnDamage(damage, hitPoint, hitNormal); // 실제로 hp 깎임

        // 피 뿌리는 효과
        bloodEffect.transform.position = hitPoint;
        bloodEffect.transform.forward = hitNormal;
        bloodEffect.Play(); 

    }
    public override void Die()
    {
        if (IsDead) return;
        base.Die();

        CurrentStatus = Status.Die;
        //enabled = false;
    }
}
