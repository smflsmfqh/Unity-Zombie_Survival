using UnityEngine;

public class Coin : MonoBehaviour, Item
{
    public int amount = 500;
    public void Use(GameObject target)
    {
        var findGo = GameObject.FindWithTag("GameController");
        var gm = findGo.GetComponent<GameManager>();    
        gm?.AddScore(amount);

        Destroy(gameObject);
    }

}
