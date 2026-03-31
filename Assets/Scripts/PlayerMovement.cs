using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{

    public static readonly int HashMove = Animator.StringToHash("Move");  // 문자열 리터럴을 넘겨주는 대신, 해시값을 넘겨주는 방식으로 최적화

    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;

    private Animator playerAnimator;
    private PlayerInput playerInput;
    private Rigidbody playerRb;
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerRb = GetComponent<Rigidbody>();   
        playerAnimator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Rotate();
        Move();

        playerAnimator.SetFloat(HashMove, playerInput.move);
    }

    private void Move()
    {
        Vector3 moveDistance = playerInput.move * transform.forward * moveSpeed * Time.deltaTime;
        playerRb.MovePosition(playerRb.position + moveDistance);
    }

    private void Rotate()
    {
        float turn = playerInput.rotate * rotateSpeed * Time.deltaTime;
        playerRb.rotation = playerRb.rotation * Quaternion.Euler(0f, turn, 0f);
    }
}
