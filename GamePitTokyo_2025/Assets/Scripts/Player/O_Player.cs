using UnityEngine;
using UnityEngine.InputSystem;

public class O_Player : MonoBehaviour
{
    [SerializeField] GameObject Player;
    private GameInput gameinput;
    private GameInput.PlayerActions m_player;
    Animator animator;
    float groundCheckRadius = 0.4f;
    float groundCheckOffsetY = 0.2f;
    [SerializeField]float speed = 5.0f;
    [SerializeField] float JumpPower = 3f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    bool isRun = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gameinput = new GameInput();
        animator = GetComponent<Animator>();

        gameinput.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
            isRun = true;

        };
        gameinput.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
            isRun = false;
        };
        gameinput.Player.Jump.performed += ctx =>
        {
            if (isGrounded())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpPower);
            }
        };
    }
    private void OnEnable()
    {
        gameinput.Enable();
    }
    private void OnDisable()
    {
        gameinput.Disable();
    }
    void FixedUpdate()
    {
        Vector2 lScale = Player.transform.localScale;
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
        if (moveInput.x > 0 && isGrounded()) {
            lScale = new Vector2(-1, 1);
        }
        if (moveInput.x < 0 && isGrounded()) 
        {
            lScale = new Vector2(1,1);
        }
        transform.localScale = lScale;
        UpdateAnimator();
    }
    bool isGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
            );
    }
    void UpdateAnimator()
    {
        animator.SetBool("Grounded", isGrounded());
        animator.SetBool("Run",isGrounded()&& isRun);
    }
}
