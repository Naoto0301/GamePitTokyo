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
    //CutOff系.
    
    bool isCutOff;//一回目ふってるかどうか.
    bool isCutOff2;
    bool isCutOff3;
    int cutOffStep = 0;
    float CutOffStartTime = 0f;
    float conboLimit = 0.5f;

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
        gameinput.Player.CutOff.performed += ctx =>
        {
            float now = Time.time;//現在時間取得.

            //時間切れならリセット.
            if(now - CutOffStartTime > conboLimit)
            {
                cutOffStep = 0;
            }

            cutOffStep++;
            CutOffStartTime = now;

            ResetCutOffTriggers();
            switch (cutOffStep)
            {
                case 1:
                    animator.SetTrigger("CutOff1");
                    break;
                case 2:
                    animator.SetTrigger("CutOff2");
                    break;
                case 3:
                    animator.SetTrigger("CutOff3");
                    break;
                default:
                    cutOffStep = 0;
                    break;
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
        if (!isCutOff)
        {
            rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
        }
        
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
    public void CutOffMove()
    {
        Debug.Log("cutoffmove");
        isCutOff = true;
        
        float dir = transform.localScale.x > 0 ? -1f : 1f;
        rb.AddForce(new Vector2(dir * 3f, 0), ForceMode2D.Impulse);
    }
    public void ToCutOffFalse()
    {
        isCutOff = false;

        // 3段目まで行ったら完全リセット
        if (cutOffStep >= 3)
        {
            cutOffStep = 0;
        }
    }
    void ResetCutOffTriggers()
    {
        animator.ResetTrigger("CutOff1");
        animator.ResetTrigger("CutOff2");
        animator.ResetTrigger("CutOff3");
    }
}
