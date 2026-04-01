using System;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public Gun gun;
    public Transform gunPivot;
    public Transform leftHandMount;
    public Transform rightHandMount;

    private PlayerInput playerInput;
    private Animator playerAnimator;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
    }


    void Start()
    {
        
    }

    void Update()
    {
        if (playerInput.fire)
        {
            gun.Fire();
        }

        if (playerInput.reload)
        {
            if (gun.Reload())
            {
                playerAnimator.SetTrigger("Reload");    

            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        gunPivot.position = playerAnimator.GetIKHintPosition(AvatarIKHint.RightElbow); // 총의 위치를 오른쪽 팔꿈치 위치로 설정하여 총이 팔과 자연스럽게 연결되도록 함
        
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f); // 왼손의 IK 위치 가중치를 1로 설정하여 왼손이 총을 잡도록 함
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f); // 왼손의 IK 회전 가중치를 1로 설정하여 왼손이 총을 잡도록 함

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandMount.rotation);

        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f); // 왼손의 IK 위치 가중치를 1로 설정하여 왼손이 총을 잡도록 함
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f); // 왼손의 IK 회전 가중치를 1로 설정하여 왼손이 총을 잡도록 함

        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHandMount.rotation);
    }

}
