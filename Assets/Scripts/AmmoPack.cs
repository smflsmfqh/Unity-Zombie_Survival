using UnityEngine;

public class AmmoPack : MonoBehaviour, Item
{
    private int amount = 100;
    public void Use(GameObject target)
    {
        if (target == GameObject.FindWithTag("Player"))
        {
            var shooter = target.GetComponent<PlayerShooter>();
            shooter?.gun?.AddAmmo(amount);
        }

        Destroy(gameObject);

    }
}
