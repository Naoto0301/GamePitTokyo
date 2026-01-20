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

    #region  プレイヤーステータス関連
    [SerializeField]
    [Tooltip("プレイヤーの最大HP")]

    private float maxHP = 100f;
    private float currentHP;
    private float defencePower = 5f;
    private float p_attackPower;
    #endregion

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
    private void Start()
    {
        //HP初期化.
        currentHP = maxHP;
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
    #region ダメージ処理
    // <summary>
    /// プレイヤーがダメージを受けます.
    /// </summary>
    /// <param name="damage">受けるダメージ量.</param>
    public void TakeDamage(float damage)
    {
        damage = damage - defencePower;
        currentHP -= damage;
        Debug.Log($"プレイヤーがダメージを受けた！ 受けたダメージ: {damage}, 現在HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($" プレイヤーが死亡しました");
        Destroy(gameObject);
    }

    public float GetCurrentHP()
    {
        return currentHP;
    }

    public float GetMaxHP()
    {
        return maxHP;
    }


    #endregion
    // ========== 宝箱システム用メソッド ==========

    /// <summary>
    /// 攻撃力をアップさせます.
    /// </summary>
    public void IncreaseAttack(float amount)
    {
        p_attackPower += amount;
        Debug.Log($" 攻撃力アップ！ 現在の攻撃力: {p_attackPower}");
    }

    /// <summary>
    /// 防御力をアップさせます.
    /// </summary>
    public void IncreaseDefense(float amount)
    {
        p_attackPower += amount;
        Debug.Log($"防御力アップ！ 現在の防御力: {p_attackPower}");
    }

    /// <summary>
    /// ジャンプ力をアップさせます.
    /// </summary>
    public void IncreaseJumpPower(float amount)
    {
        JumpPower += amount;
        Debug.Log($"ジャンプ力アップ！ 現在のジャンプ力ボーナス: {p_attackPower}");
    }

}
