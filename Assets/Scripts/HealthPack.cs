using UnityEngine;

public class HealthPack : MonoBehaviour, Item
{
    public float amount = 50f;

    public void Use(GameObject target)
    {
        var livingEntity = target.GetComponent<LivingEntity>();
       if (livingEntity != null)
        {
            livingEntity.OnHeal(amount);
        }

        Destroy(gameObject);

    }

}
