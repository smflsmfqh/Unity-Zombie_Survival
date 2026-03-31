using UnityEngine;

public interface IDamageable
{
    // 데미지 크기, 맞은 지점, 맞은 표면의 방향을 매개변수로 받음 -> 피 뿌리기 위해서
    void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal);
}
