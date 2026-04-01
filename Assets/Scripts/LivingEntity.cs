using UnityEngine;
using UnityEngine.Events;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public float startingHealth = 100f;
    public float Health { get; private set; }
    public bool IsDead { get; private set; }

    public UnityEvent onDead; // 이벤트 선언, 오브젝트마다 죽을 때 수행해야하는 동작이 다름

    // --- 초기화 ---
    protected virtual void OnEnable()
    {
        IsDead = false;
        Health = startingHealth;
    }

    public virtual void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        Health -= damage;   

        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    public virtual void OnHeal(float health)
    {
        Health += health;   
        if (Health > startingHealth)
        {
            Health = startingHealth;    
        }
    }

    public virtual void Die()
    {
        IsDead = true;
        onDead?.Invoke();
    }
}
