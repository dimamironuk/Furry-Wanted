using UnityEngine;
using Zenject;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float runSpeed = 6f;

    [Inject] private AnimationPlayer animationPlayer;
    [Inject] private IDialogService dialogService;

    private Vector2 movement;
    private float currentSpeed;

    private void Awake()
    {
        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        if (dialogService != null && dialogService.IsDialogueActive)
        {
            movement = Vector2.zero;
            animationPlayer.SetMovement(Vector2.zero, false);
            return;
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isRunning ? runSpeed : moveSpeed;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        animationPlayer.SetMovement(movement, isRunning);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animationPlayer.PerformJump();
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }
}