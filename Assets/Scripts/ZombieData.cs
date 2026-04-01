using UnityEngine;

[CreateAssetMenu(fileName = "ZombieData", menuName = "Scriptable Objects/ZobieData")]
public class ZombieData : ScriptableObject
{
    // 기본 좀비 세팅값
    // scriptable object 생성으로 세 개의 좀비 data 만듦
    public float maxHP = 100f;
    public float damage = 20f;
    public float speed = 2f;

    public Color skinColor = Color.white;
}
