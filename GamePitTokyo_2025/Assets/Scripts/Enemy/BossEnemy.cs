using UnityEngine;

/// <summary>
/// ボスエネミークラス.
/// BaseEnemyを継承し、3つの異なる行動パターンをランダムに実行します.
/// </summary>
public class BossEnemy : BaseEnemy
{
	#region インスペクター設定.

	[Header("ボス設定")]
	[SerializeField]
	[Tooltip("ボスの最大HP.")]
	private float bossMaxHP = 100f;

	[SerializeField]
	[Tooltip("行動パターン変更までの時間（秒）.")]
	private float actionChangeInterval = 4f;

	[Header("ヒップドロップ設定")]
	[SerializeField]
	[Tooltip("ヒップドロップのジャンプ高さ.")]
	private float hipDropJumpHeight = 5f;

	[SerializeField]
	[Tooltip("ヒップドロップの攻撃範囲.")]
	private float hipDropRadius = 3f;

	[SerializeField]
	[Tooltip("ヒップドロップの準備時間.")]
	private float hipDropChargeTime = 1f;

	[SerializeField]
	[Tooltip("ヒップドロップ攻撃コライダーのオフセット（X）.")]
	private float hipDropColliderOffsetX = 0f;

	[SerializeField]
	[Tooltip("ヒップドロップ攻撃コライダーのオフセット（Y）.")]
	private float hipDropColliderOffsetY = -1f;

	[SerializeField]
	[Tooltip("ヒップドロップ攻撃コライダーのサイズ（幅）.")]
	private float hipDropColliderSizeX = 4f;

	[SerializeField]
	[Tooltip("ヒップドロップ攻撃コライダーのサイズ（高さ）.")]
	private float hipDropColliderSizeY = 2f;

	[Header("ため殴り設定")]
	[SerializeField]
	[Tooltip("ため殴りのため時間.")]
	private float chargeAttackChargeTime = 1.5f;

	[SerializeField]
	[Tooltip("ため殴りの攻撃力倍率.")]
	private float chargeAttackPowerMultiplier = 2f;

	[SerializeField]
	[Tooltip("ため殴りの移動速度.")]
	private float chargeAttackSpeed = 12f;

	[SerializeField]
	[Tooltip("ため殴りの継続時間.")]
	private float chargeAttackDuration = 0.8f;

	[Header("ため蹴り設定")]
	[SerializeField]
	[Tooltip("ため蹴りのため時間.")]
	private float chargeKickChargeTime = 1.5f;

	[SerializeField]
	[Tooltip("ため蹴りの攻撃力倍率.")]
	private float chargeKickPowerMultiplier = 2.5f;

	[SerializeField]
	[Tooltip("ため蹴りの移動速度.")]
	private float chargeKickSpeed = 15f;

	[SerializeField]
	[Tooltip("ため蹴りの継続時間.")]
	private float chargeKickDuration = 0.8f;

	[Header("攻撃設定")]
	[SerializeField]
	[Tooltip("攻撃範囲.")]
	private float attackRange = 2f;

	[SerializeField]
	[Tooltip("攻撃コリジョンの名前.")]
	private string attackColliderName = "AttackCollider";

	[SerializeField]
	[Tooltip("攻撃アニメーション再生用のAnimator.")]
	private Animator animator;

	[SerializeField]
	[Tooltip("移動フラグパラメータ名.")]
	private string moveParameter = "Move";

	[SerializeField]
	[Tooltip("攻撃フラグパラメータ名.")]
	private string attackParameter = "Attack";

	#endregion

	#region 列挙型.

	/// <summary>
	/// ボスの行動パターン.
	/// </summary>
	private enum BossActionPattern
	{
		HipDrop,
		ChargeAttack,
		ChargeKick
	}

	#endregion

	#region 保護された変数.

	private float actionTimer = 0f;
	private float chargeTimer = 0f;
	private float attackExecutionTimer = 0f;
	private BossActionPattern currentPattern = BossActionPattern.ChargeAttack;
	private BossActionPattern previousPattern = BossActionPattern.ChargeKick;
	private Collider2D attackCollider;
	private BoxCollider2D boxCollider;
	private CircleCollider2D circleCollider;
	private Vector3 originalColliderOffset = Vector3.zero;
	private Vector2 originalColliderSize = Vector2.zero;
	private bool isCharging = false;
	private bool isExecutingAttack = false;
	private Vector2 attackDirection = Vector2.right;
	private bool isBossJumping = false;
	private float bossJumpVelocity = 0f;
	private float currentAttackDamage = 0f;

	#endregion

	#region Unityライフサイクル.

	protected override void Start()
	{
		maxHP = bossMaxHP;
		base.Start();

		if (rb != null)
		{
			rb.gravityScale = 1f;
			rb.constraints = RigidbodyConstraints2D.FreezeRotation;
			rb.linearVelocity = Vector2.zero;
		}

		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}

		FindAttackCollider();
		SelectRandomPattern();

		Debug.Log($"✅ BossEnemy初期化完了 - HP: {maxHP}");
	}

	protected override void Update()
	{
		DetectPlayer();
		UpdateTimers();

		if (isPlayerDetected)
		{
			OnPlayerDetected();
		}
		else
		{
			Wander();
		}

		// ジャンプ中の重力処理（HipDropパターンのみ、かつジャンプ中の時のみ）
		if (isBossJumping && rb != null)
		{
			bossJumpVelocity -= 9.8f * Time.deltaTime;
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, bossJumpVelocity);

			// 着地判定
			if (rb.linearVelocity.y <= 0 && transform.position.y <= 0f)
			{
				isBossJumping = false;
				rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
				transform.position = new Vector3(transform.position.x, 0, transform.position.z);
			}
		}

		if (Time.frameCount % 60 == 0)
		{
			Debug.Log($"📊 ボス状態 - プレイヤー検出: {isPlayerDetected}, パターン: {currentPattern}, 充電中: {isCharging}, 攻撃中: {isExecutingAttack}, 距離: {(playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : 999f):F2}");
		}
	}

	#endregion

	#region プレイヤー検出.

	protected override void DetectPlayer()
	{
		if (playerTransform == null)
		{
			isPlayerDetected = false;
			return;
		}

		float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

		if (distanceToPlayer <= detectionRange)
		{
			isPlayerDetected = true;
		}
		else
		{
			isPlayerDetected = false;
		}
	}

	#endregion

	#region タイマー更新.

	private void UpdateTimers()
	{
		actionTimer += Time.deltaTime;

		if (actionTimer >= actionChangeInterval)
		{
			actionTimer = 0f;
			chargeTimer = 0f;
			attackExecutionTimer = 0f;
			isCharging = false;
			isExecutingAttack = false;
			isBossJumping = false;
			bossJumpVelocity = 0f;

			if (rb != null)
			{
				rb.linearVelocity = Vector2.zero;
			}

			// アニメーションフラグをリセット
			if (animator != null)
			{
				animator.SetBool(moveParameter, false);
				animator.SetBool(attackParameter, false);
			}

			ResetColliderToOriginal();
			SelectRandomPattern();
			Debug.Log($"🎯 ボスの行動パターンを変更: {currentPattern}");
		}
	}

	#endregion

	#region 行動パターン選択.

	private void SelectRandomPattern()
	{
		int patternIndex = Random.Range(0, 3);
		currentPattern = (BossActionPattern)patternIndex;
	}

	#endregion

	#region プレイヤー検出時の動作.

	protected override void OnPlayerDetected()
	{
		// パターンが変わったかチェック
		if (previousPattern != currentPattern)
		{
			chargeTimer = 0f;
			attackExecutionTimer = 0f;
			isCharging = false;
			isExecutingAttack = false;
			isBossJumping = false;
			bossJumpVelocity = 0f;
			previousPattern = currentPattern;

			if (rb != null)
			{
				rb.linearVelocity = Vector2.zero;
			}

			// アニメーションフラグをリセット
			if (animator != null)
			{
				animator.SetBool(moveParameter, false);
				animator.SetBool(attackParameter, false);
			}

			ResetColliderToOriginal();
		}

		switch (currentPattern)
		{
			case BossActionPattern.HipDrop:
				ExecuteHipDrop();
				break;
			case BossActionPattern.ChargeAttack:
				ExecuteChargeAttack();
				break;
			case BossActionPattern.ChargeKick:
				ExecuteChargeKick();
				break;
		}
	}

	#endregion

	#region 行動パターン実装.

	private void ExecuteHipDrop()
	{
		if (playerTransform == null)
		{
			return;
		}

		Vector2 directionToPlayer = GetDirectionToPlayer();
		UpdateFacingDirection(directionToPlayer);

		// ため中
		if (!isCharging && chargeTimer < hipDropChargeTime)
		{
			chargeTimer += Time.deltaTime;
			Debug.Log($"⏳ ボスがヒップドロップをためています... {chargeTimer:F1}秒");

			// 移動フラグをON
			if (animator != null)
			{
				animator.SetBool(moveParameter, true);
			}

			if (rb != null)
			{
				rb.linearVelocity = new Vector2(directionToPlayer.x * 3f, 0);
			}
		}
		// ためが完了したらジャンプ
		else if (!isCharging && chargeTimer >= hipDropChargeTime)
		{
			isCharging = true;
			isBossJumping = true;
			Debug.Log($"📍 ボスがヒップドロップで跳び上がりました！");

			// 移動フラグをOFF
			if (animator != null)
			{
				animator.SetBool(moveParameter, false);
			}

			// ジャンプ初速度を計算
			bossJumpVelocity = Mathf.Sqrt(2f * 9.8f * hipDropJumpHeight);

			if (rb != null)
			{
				rb.linearVelocity = new Vector2(rb.linearVelocity.x, bossJumpVelocity);
			}
		}

		// 着地したら攻撃実行
		if (isCharging && !isBossJumping && !isExecutingAttack)
		{
			Debug.Log($"💥 ヒップドロップで着地！攻撃実行");
			PerformHipDrop();
			isExecutingAttack = true;
		}
	}

	private void ExecuteChargeAttack()
	{
		if (playerTransform == null)
		{
			return;
		}

		Vector2 directionToPlayer = GetDirectionToPlayer();
		directionToPlayer.y = 0f;
		directionToPlayer = directionToPlayer.normalized;
		attackDirection = directionToPlayer;

		UpdateFacingDirection(directionToPlayer);

		// 充電フェーズ
		if (!isCharging && chargeTimer < chargeAttackChargeTime)
		{
			chargeTimer += Time.deltaTime;
			Debug.Log($"⏳ ボスがため殴りをためています... {chargeTimer:F1}秒");

			if (rb != null)
			{
				rb.linearVelocity = Vector2.zero;
			}
		}
		// 充電完了、攻撃開始
		else if (!isCharging && chargeTimer >= chargeAttackChargeTime)
		{
			isCharging = true;
			isExecutingAttack = true;
			attackExecutionTimer = 0f;
			Debug.Log($"💥 ボスがため殴りを実行！");
			PerformChargeAttack();
		}

		// 攻撃実行中の移動
		if (isCharging && isExecutingAttack)
		{
			attackExecutionTimer += Time.deltaTime;

			// 移動フラグをON
			if (animator != null)
			{
				animator.SetBool(moveParameter, true);
			}

			if (rb != null)
			{
				rb.linearVelocity = new Vector2(attackDirection.x * chargeAttackSpeed, 0);
			}

			// 攻撃時間終了
			if (attackExecutionTimer >= chargeAttackDuration)
			{
				isExecutingAttack = false;
				if (animator != null)
				{
					animator.SetBool(moveParameter, false);
				}
				if (rb != null)
				{
					rb.linearVelocity = Vector2.zero;
				}
			}
		}
	}

	private void ExecuteChargeKick()
	{
		if (playerTransform == null)
		{
			return;
		}

		Vector2 directionToPlayer = GetDirectionToPlayer();
		directionToPlayer.y = 0f;
		directionToPlayer = directionToPlayer.normalized;
		attackDirection = directionToPlayer;

		UpdateFacingDirection(directionToPlayer);

		// 充電フェーズ
		if (!isCharging && chargeTimer < chargeKickChargeTime)
		{
			chargeTimer += Time.deltaTime;
			Debug.Log($"⏳ ボスがため蹴りをためています... {chargeTimer:F1}秒");

			if (rb != null)
			{
				rb.linearVelocity = Vector2.zero;
			}
		}
		// 充電完了、攻撃開始
		else if (!isCharging && chargeTimer >= chargeKickChargeTime)
		{
			isCharging = true;
			isExecutingAttack = true;
			attackExecutionTimer = 0f;
			Debug.Log($"🦵 ボスがため蹴りを実行！");
			PerformChargeKick();
		}

		// 攻撃実行中の移動
		if (isCharging && isExecutingAttack)
		{
			attackExecutionTimer += Time.deltaTime;

			// 移動フラグをON
			if (animator != null)
			{
				animator.SetBool(moveParameter, true);
			}

			if (rb != null)
			{
				rb.linearVelocity = new Vector2(attackDirection.x * chargeKickSpeed, 0);
			}

			// 攻撃時間終了
			if (attackExecutionTimer >= chargeKickDuration)
			{
				isExecutingAttack = false;
				if (animator != null)
				{
					animator.SetBool(moveParameter, false);
				}
				if (rb != null)
				{
					rb.linearVelocity = Vector2.zero;
				}
			}
		}
	}

	#endregion

	#region 攻撃実装.

	private void PerformHipDrop()
	{
		currentAttackDamage = GetAttackPower();
		Debug.Log($"💥 ボスがヒップドロップ攻撃を実行！ 範囲: {hipDropRadius}");
		Debug.Log($"📊 ヒップドロップダメージ: {currentAttackDamage}");

		// 攻撃フラグをON
		if (animator != null)
		{
			animator.SetBool(attackParameter, true);
		}

		// ヒップドロップ用にコライダーを調整
		AdjustColliderForHipDrop();

		Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, hipDropRadius);

		foreach (Collider2D collider in hitColliders)
		{
			if (collider.CompareTag("Player"))
			{
				DamagePlayer(collider, currentAttackDamage);
			}
		}
	}

	private void PerformChargeAttack()
	{
		currentAttackDamage = GetAttackPower() * chargeAttackPowerMultiplier;
		Debug.Log($"💥 ボスがため殴り攻撃を実行！ ダメージ: {currentAttackDamage}");

		// 攻撃フラグをON
		if (animator != null)
		{
			animator.SetBool(attackParameter, true);
		}

		ResetColliderToOriginal();
		EnableAttackCollider(currentAttackDamage);
	}

	private void PerformChargeKick()
	{
		currentAttackDamage = GetAttackPower() * chargeKickPowerMultiplier;
		Debug.Log($"🦵 ボスがため蹴り攻撃を実行！ ダメージ: {currentAttackDamage}");

		// 攻撃フラグをON
		if (animator != null)
		{
			animator.SetBool(attackParameter, true);
		}

		ResetColliderToOriginal();
		EnableAttackCollider(currentAttackDamage);
	}

	private void DamagePlayer(Collider2D collider, float damage)
	{
		Debug.Log($"💢 DamagePlayer呼び出し: ダメージ = {damage}");
		Debug.Log($"📍 対象: {collider.gameObject.name}");

		var takeDamageMethod = collider.GetComponent<MonoBehaviour>();
		if (takeDamageMethod != null)
		{
			Debug.Log($"📍 SendMessage実行");
			takeDamageMethod.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
			Debug.Log($"💢 プレイヤーに{damage}ダメージを与えました！");
		}
		else
		{
			Debug.LogWarning($"⚠️ MonoBehaviourが見つかりません");
		}
	}

	#endregion

	#region 攻撃コリジョン管理.

	private void FindAttackCollider()
	{
		foreach (Transform child in transform)
		{
			if (child.name == attackColliderName)
			{
				attackCollider = child.GetComponent<Collider2D>();

				// BoxCollider2Dをチェック
				boxCollider = child.GetComponent<BoxCollider2D>();
				if (boxCollider != null)
				{
					originalColliderOffset = boxCollider.offset;
					originalColliderSize = boxCollider.size;
					attackCollider.enabled = false;
					Debug.Log($"✅ ボスの攻撃コリジョン取得（BoxCollider2D）: {attackColliderName}");
					return;
				}

				// CircleCollider2Dをチェック
				circleCollider = child.GetComponent<CircleCollider2D>();
				if (circleCollider != null)
				{
					originalColliderOffset = circleCollider.offset;
					attackCollider.enabled = false;
					Debug.Log($"✅ ボスの攻撃コリジョン取得（CircleCollider2D）: {attackColliderName}");
					return;
				}

				if (attackCollider != null)
				{
					attackCollider.enabled = false;
					Debug.Log($"✅ ボスの攻撃コリジョン取得: {attackColliderName}");
					return;
				}
			}
		}

		Debug.LogWarning($"⚠️ ボスの攻撃コリジョンが見つかりません: {attackColliderName}");
	}

	private void AdjustColliderForHipDrop()
	{
		if (boxCollider == null)
		{
			Debug.LogWarning("⚠️ BoxCollider2Dが見つかりません。ヒップドロップ用の調整ができません。");
			return;
		}

		// コライダーの位置とサイズを調整
		boxCollider.offset = new Vector2(hipDropColliderOffsetX, hipDropColliderOffsetY);
		boxCollider.size = new Vector2(hipDropColliderSizeX, hipDropColliderSizeY);

		Debug.Log($"📍 ヒップドロップ用にコライダー調整 - オフセット: ({hipDropColliderOffsetX}, {hipDropColliderOffsetY}), サイズ: ({hipDropColliderSizeX}, {hipDropColliderSizeY})");

		EnableAttackCollider(currentAttackDamage);
	}

	private void ResetColliderToOriginal()
	{
		if (boxCollider != null)
		{
			boxCollider.offset = originalColliderOffset;
			boxCollider.size = originalColliderSize;
			Debug.Log($"🔄 コライダーをリセット");
		}
	}

	private void EnableAttackCollider(float damage = -1f)
	{
		if (attackCollider != null)
		{
			if (damage > 0)
			{
				currentAttackDamage = damage;
			}
			attackCollider.enabled = true;
			Debug.Log($"🔓 ボスの攻撃コリジョン有効化 - ダメージ: {currentAttackDamage}");

			Invoke(nameof(DisableAttackCollider), 0.5f);
		}
	}

	private void DisableAttackCollider()
	{
		if (attackCollider != null)
		{
			attackCollider.enabled = false;
			// 攻撃フラグをOFF
			if (animator != null)
			{
				animator.SetBool(attackParameter, false);
			}
			Debug.Log($"🔒 ボスの攻撃コリジョン無効化");
		}
	}

	#endregion

	#region ダメージと健康.

	public override void TakeDamage(float damage)
	{
		currentHP -= damage;
		Debug.Log($"💢 ボスがダメージを受けました！ 現在HP: {currentHP}/{maxHP}");

		if (currentHP <= 0)
		{
			Die();
		}
	}

	#endregion
}