using UnityEngine;
using UnityEngine.UI;

// --- Player의 HP 관리 ---

public class PlayerHealth : LivingEntity // LivingEntity를 상속받았기 때문에, MonoBehaviour & IDamageable을 상속 받은 것과 같음
{
    public Image uiHealth;

    public AudioClip deathClip;
    public AudioClip hitClip;

    private AudioSource playerAudioSource;
    private Animator playerAnimator;

    // --- player가 죽으면 disable, 다시 살아나면 enable ---
    private PlayerMovement playerMovement;
    private PlayerShooter playerShooter;

    private void Awake()
    {
        playerAudioSource = GetComponent<AudioSource>();
        playerAnimator = GetComponent<Animator>();

        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>(); 
    }

    // --- 처음 게임을 실행할 때 and 플레이어가 죽었다가 다시 살아났을 때 초기화 ---
    protected override void OnEnable()
    {
        base.OnEnable();

        uiHealth.gameObject.SetActive(true);
        

        uiHealth.fillAmount = 1f; // 체력 UI의 Foreground fill image 꽉 참

        playerMovement.enabled = true;  
        playerShooter.enabled = true;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!IsDead)
        {
            playerAudioSource.PlayOneShot(hitClip);
        }

        base.OnDamage(damage, hitPoint, hitNormal); // 실제로 hp 깎임

        uiHealth.fillAmount = Health / startingHealth; // 깎인 hp로 ui 갱신

    }

    public override void OnHeal(float health)
    {
        base.OnHeal(health);

        uiHealth.fillAmount = Health / startingHealth;
    }

    public override void Die()
    {
        if (IsDead) return;
        base.Die();

        uiHealth.gameObject.SetActive(false);

        playerAudioSource.PlayOneShot(deathClip);
        playerAnimator.SetTrigger("Die");

        playerMovement.enabled=false;
        playerShooter.enabled=false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var item = other.GetComponent<Item>();
        if (item != null)
        {
            item.Use(gameObject);
        }
    }
}
