using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class O_Player : MonoBehaviour
{
	[SerializeField] GameObject Player;
	private GameInput gameinput;
	private GameInput.PlayerActions m_player;
	Animator animator;
	float groundCheckRadius = 0.4f;
	float groundCheckOffsetY = 0.2f;
	[SerializeField] float speed = 5.0f;
	[SerializeField] float JumpPower = 3f;
	[SerializeField] LayerMask groundLayer;
	[SerializeField] Transform groundCheck;
	[SerializeField] Vector3 startPos;
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

	// 攻撃判定コライダー
	private Collider2D attackCollider;
    private SpriteRenderer attackspriteRenderer;
    private bool hasHitThisAttack = false;
	[SerializeField]
	private float baseAttackDamage = 10f;
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
		gameinput.Player.Return.performed += ctx =>
		{
			ReturnPlayer();
		};
		gameinput.Player.CutOff.performed += ctx =>
		{
			float now = Time.time;//現在時間取得.

			//時間切れならリセット.
			if (now - CutOffStartTime > conboLimit)
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

        startPos = transform.position;
        //HP初期化.
        currentHP = maxHP;

		// 攻撃判定コライダーを取得（子オブジェクト「AttackCollider」から）
		Transform attackObj = transform.Find("AttackCollider");
		if (attackObj != null)
		{
			attackCollider = attackObj.GetComponent<Collider2D>();
            attackspriteRenderer = attackObj.GetComponent<SpriteRenderer>();

            if (attackCollider != null)
			{
                attackspriteRenderer.enabled = false;
                attackCollider.enabled = false; // 初期は無効
				Debug.Log("✅ AttackColliderを取得しました");
			}
			else
			{
				Debug.LogWarning("⚠️ AttackColliderにCollider2Dコンポーネントがありません");
			}
		}
		else
		{
			Debug.LogWarning("⚠️ AttackColliderという子オブジェクトが見つかりません");
		}
	}
	private void OnEnable()
	{
		gameinput.Enable();
	}
	private void OnDisable()
	{
		gameinput.Disable();
	}

	private void Update()
	{
		// 攻撃キーが押されている間、攻撃判定を有効化
		if (gameinput.Player.CutOff.IsPressed())
		{
			EnableAttackCollider();
		}
		else
		{
			DisableAttackCollider();
		}
	}

	void FixedUpdate()
	{
		Vector2 lScale = Player.transform.localScale;
		if (!isCutOff)
		{
			rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
		}

		if (moveInput.x > 0 && isGrounded())
		{
			lScale = new Vector2(-1, 1);
		}
		if (moveInput.x < 0 && isGrounded())
		{
			lScale = new Vector2(1, 1);
		}
		transform.localScale = lScale;
		UpdateAnimator();
	}
	void ReturnPlayer()
	{
		transform.position = startPos;
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
		animator.SetBool("Run", isGrounded() && isRun);
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

	#region 攻撃判定制御
	/// <summary>
	/// 攻撃キー押下中に攻撃判定を有効にします
	/// </summary>
	public void EnableAttackCollider()
	{
		if (attackCollider != null && !attackCollider.enabled)
		{
			attackspriteRenderer.enabled = true;
			attackCollider.enabled = true;
			hasHitThisAttack = false; // 新しい攻撃なのでリセット
			Debug.Log($"⚔️ 攻撃判定が有効になりました");
		}
	}

	/// <summary>
	/// 攻撃キーが離されたら攻撃判定を無効にします
	/// </summary>
	public void DisableAttackCollider()
	{
		if (attackCollider != null && attackCollider.enabled)
		{
			attackspriteRenderer.enabled = false;
            attackCollider.enabled = false;
			Debug.Log($"🔒 攻撃判定が無効になりました");
		}
	}

	/// <summary>
	/// 攻撃力を取得します
	/// </summary>
	public float GetAttackPower()
	{
		return baseAttackDamage + p_attackPower;
	}
	#endregion

	#region ダメージ処理
	/// <summary>
	/// プレイヤーがダメージを受けます.
	/// </summary>
	/// <param name="damage">受けるダメージ量.</param>
	public void TakeDamage(float damage)
	{
		Debug.Log($"📍 TakeDamage呼び出し: 受け取ったダメージ = {damage}");

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
		Debug.Log($"💀 プレイヤーが死亡しました");
		Destroy(gameObject);
		// シーン遷移
		SceneManager.LoadScene("GameOver");
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
		Debug.Log($"📈 攻撃力アップ！ 現在の攻撃力: {p_attackPower}");
	}

	/// <summary>
	/// 防御力をアップさせます.
	/// </summary>
	public void IncreaseDefense(float amount)
	{
		p_attackPower += amount;
		Debug.Log($"🛡️ 防御力アップ！ 現在の防御力: {p_attackPower}");
	}

	/// <summary>
	/// ジャンプ力をアップさせます.
	/// </summary>
	public void IncreaseJumpPower(float amount)
	{
		JumpPower += amount;
		Debug.Log($"⬆️ ジャンプ力アップ！ 現在のジャンプ力ボーナス: {p_attackPower}");
	}

	#region トリガー処理
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision == null)
		{
			Debug.LogWarning("⚠️ collision が null です");
			return;
		}

		Debug.Log($"🔔 プレイヤーがトリガー接触: {collision.gameObject.name}");

		// プレイヤーの攻撃判定は敵へのダメージで、敵からのダメージではない
		if (attackCollider != null && collision == attackCollider)
		{
			return;
		}

		// MeleeEnemy（近接敵）からのダメージ
		if (collision.transform.parent != null)
		{
			MeleeEnemy enemy = collision.transform.parent.GetComponent<MeleeEnemy>();
			if (enemy != null)
			{
				float damage = enemy.GetAttackDamage();
				TakeDamage(damage);
				return;
			}
		}

		// BossEnemy（ボス）からのダメージ
		BossEnemy boss = collision.GetComponentInParent<BossEnemy>();
		if (boss != null)
		{
			Debug.Log($"🎯 BossEnemyの攻撃に接触！");
			System.Reflection.FieldInfo field = boss.GetType().GetField("currentAttackDamage",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			if (field != null)
			{
				float damage = (float)field.GetValue(boss);
				Debug.Log($"📊 ボスのダメージ: {damage}");
				TakeDamage(damage);
				Debug.Log($"💢 プレイヤーがボスのダメージを受けた！ダメージ: {damage}");
			}
			return;
		}

		Debug.Log($"📍 弾または他のオブジェクトです: {collision.gameObject.name}");
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		// プレイヤーの攻撃判定が有効な場合のみ、敵にダメージを与える
		if (attackCollider == null || !attackCollider.enabled)
			return;

		if (hasHitThisAttack)
			return;

		if (collision == null)
			return;

		// BaseEnemy の派生クラスにダメージを与える
		BaseEnemy enemy = collision.GetComponentInParent<BaseEnemy>();
		if (enemy != null)
		{
			float damage = GetAttackPower();
			enemy.TakeDamage(damage);
			hasHitThisAttack = true;
			Debug.Log($"💥 敵にダメージ！ダメージ量: {damage}, 敵HP: {enemy.GetCurrentHP()}");
			DisableAttackCollider();
			return;
		}
	}
	#endregion
}