using UnityEngine;

/// <summary>
/// プレイヤーを追尾して近接攻撃する敵クラス（ジャンプ対応）.
/// 攻撃はコリジョンで判定します.
/// </summary>
public class MeleeEnemy : BaseEnemy
{
	#region インスペクター設定.

	[Header("近接敵設定")]
	[SerializeField]
	[Tooltip("攻撃範囲.")]
	private float attackRange = 1.5f;

	[SerializeField]
	[Tooltip("攻撃間隔（秒）.")]
	private float attackCooldown = 1.5f;

	[SerializeField]
	[Tooltip("攻撃コリジョンの名前.")]
	private string attackColliderName = "AttackCollider";

	[SerializeField]
	[Tooltip("攻撃時のアニメーションを再生するAnimator.")]
	private Animator animator;

	[SerializeField]
	[Tooltip("攻撃アニメーションパラメータ名.")]
	private string attackAnimationTrigger = "Attack";

	[SerializeField]
	[Tooltip("移動アニメーションパラメータ名.")]
	private string moveAnimationParameter = "IsMoving";

	[SerializeField]
	[Tooltip("攻撃力.")]
	private float attackDamage = 10f;

	[Header("ジャンプ設定")]
	[SerializeField]
	[Tooltip("ジャンプ力.")]
	private float jumpForce = 5f;

	[SerializeField]
	[Tooltip("地面判定用のレイキャスト距離.")]
	private float groundCheckDistance = 0.2f;

	[SerializeField]
	[Tooltip("地面判定用レイヤーマスク.")]
	private LayerMask groundLayer;

	[SerializeField]
	[Tooltip("障害物検出範囲.")]
	private float obstacleDetectionRange = 2f;

	[SerializeField]
	[Tooltip("ジャンプ判定の高さの差.")]
	private float jumpHeightThreshold = 1.5f;

	#endregion


	#region プライベート変数.

	private float attackTimer = 0f;
	private Collider2D attackCollider;
	private bool isMoving = false;
	private SpriteRenderer spriteRenderer;
	private bool isGrounded = false;
	private float jumpCooldown = 0f;
	private Collider2D enemyCollider;
	private bool hasHitPlayerThisAttack = false;

	#endregion

	#region Unityライフサイクル.

	/// <summary>
	/// 初期化処理.
	/// </summary>
	protected override void Start()
	{
		base.Start();
		attackTimer = attackCooldown;

		// Animatorが指定されていない場合は自動取得.
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}

		// SpriteRendererを取得.
		spriteRenderer = GetComponent<SpriteRenderer>();

		// Collider2Dをキャッシュ（必ず取得する）.
		if (enemyCollider == null)
		{
			enemyCollider = GetComponent<Collider2D>();
		}

		if (enemyCollider == null)
		{
			// 子オブジェクトから探す
			enemyCollider = GetComponentInChildren<Collider2D>();
		}

		Debug.Log($"✅ Collider2D取得: {enemyCollider?.name ?? "null"}");

		// **Y軸のロックを解除（ジャンプ対応）**
		if (rb != null)
		{
			rb.constraints = RigidbodyConstraints2D.FreezeRotation;
			Debug.Log($"✅ Y軸ロック解除: ジャンプ可能に設定");
		}

		// 攻撃コリジョンを取得.
		FindAttackCollider();

		Debug.Log($"✅ MeleeEnemy初期化完了, PlayerTransform: {playerTransform}");
	}

	/// <summary>
	/// 毎フレーム更新.
	/// </summary>
	protected override void Update()
	{
		base.Update();

		// 地面判定を更新.
		CheckGrounded();

		// ジャンプクールダウンを減らす.
		if (jumpCooldown > 0)
		{
			jumpCooldown -= Time.deltaTime;
		}

		// プレイヤーを追尾（BaseEnemy の OnPlayerDetected が呼ばれない場合の対策）.
		if (playerTransform != null)
		{
			OnPlayerDetected();
		}
		else
		{
			Debug.LogWarning("⚠️ playerTransform が null のままです");
		}
	}

	/// <summary>
	/// 前方の障害物をチェックしてジャンプ.
	/// </summary>
	private void CheckObstacleAndJump()
	{
		if (playerTransform == null)
		{
			return;
		}

		Vector2 directionToPlayer = GetDirectionToPlayer();

		if (directionToPlayer.x == 0)
		{
			Debug.Log($"⚠️ 水平方向の移動がありません");
			return;
		}

		Vector2 rayDirection = new Vector2(directionToPlayer.x > 0 ? 1 : -1, 0);

		Debug.Log($"🔍 障害物チェック開始: 敵位置={transform.position}, rayDirection={rayDirection}");
		Debug.Log($"敵のコライダー: {enemyCollider?.name ?? "null"}");

		// 敵の前方と周囲からレイキャスト（複数箇所）.
		Vector2[] rayOrigins = new Vector2[]
		{
			transform.position,
			transform.position + Vector3.up * 0.3f,
			transform.position + Vector3.up * 0.6f,
		};

		bool obstacleFound = false;

		for (int i = 0; i < rayOrigins.Length; i++)
		{
			Vector2 origin = rayOrigins[i];
			Debug.Log($"🔍 Ray {i} 実行: origin={origin}");

			// 全てのコライダーを取得
			RaycastHit2D[] hits = Physics2D.RaycastAll(
				origin,
				rayDirection,
				2f
			);

			Debug.DrawRay(origin, rayDirection * 2f, Color.yellow);

			Debug.Log($"🔫 Raycast from {origin}: Hits={hits.Length}");

			if (hits.Length > 0)
			{
				// 敵自身以外の最初のコライダーを探す
				foreach (RaycastHit2D hit in hits)
				{
					bool isSelf = hit.collider == enemyCollider || hit.collider.gameObject == gameObject;
					Debug.Log($"  → Hit: {hit.collider?.name}, Distance={hit.distance}, 敵と同じ？{isSelf}");

					if (hit.collider != null && !isSelf)
					{
						Debug.Log($"🚧 障害物検出: {hit.collider.name}");
						obstacleFound = true;
						break;
					}
				}
			}

			if (obstacleFound)
			{
				break;
			}
		}

		Debug.Log($"📊 ジャンプ判定: obstacleFound={obstacleFound}, isGrounded={isGrounded}, jumpCooldown={jumpCooldown}");

		if (obstacleFound)
		{
			if (!isGrounded)
			{
				Debug.Log($"❌ 地面に触れていません");
			}
			if (jumpCooldown > 0)
			{
				Debug.Log($"❌ ジャンプクールダウン中: {jumpCooldown}");
			}

			if (isGrounded && jumpCooldown <= 0)
			{
				TryJump();
				Debug.Log($"🆙 ジャンプ実行！");
			}
		}
	}

	#endregion

	#region 追尾と攻撃.

	private Vector2 lastMoveDirection = Vector2.zero;

	/// <summary>
	/// プレイヤーが検出された時、追尾して攻撃します.
	/// </summary>
	protected override void OnPlayerDetected()
	{
		if (playerTransform == null)
		{
			Debug.LogWarning("⚠️ playerTransformがnullです");
			return;
		}

		float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
		Vector2 directionToPlayer = GetDirectionToPlayer();

		Debug.Log($"📍 プレイヤー距離: {distanceToPlayer}, 方向: {directionToPlayer}");
		Debug.Log($"🟢 isGrounded: {isGrounded}, jumpCooldown: {jumpCooldown}");

		// 敵の向きをプレイヤー方向に更新.
		UpdateFacingDirection(directionToPlayer);

		// 攻撃範囲外なら追尾.
		if (distanceToPlayer > attackRange)
		{
			Debug.Log($"🚶 プレイヤーに追尾中...");

			var rb = GetComponent<Rigidbody2D>();

			// 地面にいる場合のみ水平移動を更新（空中では現在の水平速度を保持）
			if (isGrounded)
			{
				lastMoveDirection = new Vector2(directionToPlayer.x * chaseSpeed, 0);
				Move(lastMoveDirection);
				SetMovingAnimation(true);

				// 前方の障害物をチェック.
				CheckObstacleAndJump();
			}
			else
			{
				// 空中では垂直速度を保持して、水平速度のみ減速
				if (rb != null)
				{
					rb.linearVelocity = new Vector2(lastMoveDirection.x * 0.5f, rb.linearVelocity.y);
				}
				SetMovingAnimation(false);
			}
		}
		// 攻撃範囲内なら攻撃.
		else
		{
			Debug.Log($"⚔️ 攻撃範囲内！攻撃開始！");
			lastMoveDirection = Vector2.zero;
			Move(Vector2.zero);
			SetMovingAnimation(false);
			Attack();
		}
	}

	/// <summary>
	/// 攻撃力を取得します.
	/// </summary>
	public float GetAttackDamage()
	{
		Debug.Log($"📊 GetAttackDamage呼び出し: attackPower = {attackPower}");
		return attackPower;
	}

	/// <summary>
	/// 敵が何かに衝突した時にジャンプ.
	/// </summary>
	private void OnCollisionEnter2D(Collision2D collision)
	{
		// 敵自身のコライダーを無視.
		if (collision.gameObject == gameObject)
		{
			return;
		}

		Debug.Log($"💥 衝突検出: {collision.gameObject.name}");

		// 衝突した瞬間、地面に接触している状態として扱う
		isGrounded = true;

		// 移動中なら即座にジャンプ.
		if (playerTransform != null)
		{
			Vector2 directionToPlayer = GetDirectionToPlayer();

			if (directionToPlayer.x != 0)
			{
				// ジャンプクールダウンをリセット
				jumpCooldown = 0f;
				TryJump();
				Debug.Log($"🆙 衝突時ジャンプ実行!");
			}
		}
	}

	/// <summary>
	/// ジャンプを試みます.
	/// </summary>
	private void TryJump()
	{
		var rb = GetComponent<Rigidbody2D>();

		if (rb != null)
		{
			Debug.Log($"📊 ジャンプ前: pos={transform.position}, velocity={rb.linearVelocity}");

			// 直接y速度を設定（AddForceではなく）
			rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

			Debug.Log($"📊 ジャンプ後: pos={transform.position}, velocity={rb.linearVelocity}");

			jumpCooldown = 0.5f;
			Debug.Log($"🆙 敵がジャンプ！ジャンプ力設定: {jumpForce}");
		}
		else
		{
			Debug.LogWarning("⚠️ Rigidbody2D が見つかりません");
		}
	}

	/// <summary>
	/// プレイヤーに攻撃します.
	/// </summary>
	private void Attack()
	{
		attackTimer += Time.deltaTime;

		if (attackTimer >= attackCooldown)
		{
			attackTimer = 0f;
			hasHitPlayerThisAttack = false;
			Debug.Log($"💥 近接敵が攻撃！");

			// 攻撃アニメーション再生.
			PlayAttackAnimation();

			// 攻撃コリジョンを有効化.
			EnableAttackCollider();
		}
	}

	/// <summary>
	/// 攻撃アニメーションを再生します.
	/// </summary>
	private void PlayAttackAnimation()
	{
		if (animator != null)
		{
			animator.SetTrigger(attackAnimationTrigger);
			Debug.Log($"🎬 攻撃アニメーション再生");
		}
	}

	/// <summary>
	/// 移動アニメーションを設定します.
	/// </summary>
	private void SetMovingAnimation(bool moving)
	{
		if (animator != null && isMoving != moving)
		{
			isMoving = moving;
			animator.SetBool(moveAnimationParameter, moving);
			Debug.Log($"🚶 移動アニメーション: {moving}");
		}
	}

	/// <summary>
	/// 攻撃コリジョンがプレイヤーに接触した時.
	/// </summary>
	private void OnTriggerEnter2D(Collider2D collision)
	{
		// 攻撃コリジョンからの接触判定のみ処理
		if (attackCollider != null && collision != attackCollider)
		{
			return;
		}

		// プレイヤーに接触したか確認
		if (collision.CompareTag("Player"))
		{
			// 同じ攻撃で複数回ダメージを与えないようにする
			if (!hasHitPlayerThisAttack)
			{
				O_Player player = collision.GetComponent<O_Player>();
				if (player != null)
				{
					player.TakeDamage(attackDamage);
					hasHitPlayerThisAttack = true;
					Debug.Log($"💢 プレイヤーにダメージ！ダメージ量: {attackDamage}");
				}
				else
				{
					Debug.LogWarning($"⚠️ プレイヤースクリプト(O_Player)が見つかりません");
				}
			}
		}
	}

	#endregion

	#region 地面判定.

	/// <summary>
	/// 敵が地面に接触しているかを判定します（デバッグ強化版）.
	/// </summary>
	private void CheckGrounded()
	{
		if (enemyCollider == null)
		{
			Debug.LogWarning("⚠️ enemyColliderがnullです");
			isGrounded = false;
			return;
		}

		// コライダーの下端を基準にレイキャスト.
		float bottomY = enemyCollider.bounds.min.y;
		float leftX = enemyCollider.bounds.min.x + 0.05f;
		float rightX = enemyCollider.bounds.max.x - 0.05f;
		float centerX = (leftX + rightX) / 2f;

		// 3箇所からレイキャスト（左、中央、右）
		Vector2[] raycastOrigins = new Vector2[]
		{
			new Vector2(leftX, bottomY),
			new Vector2(centerX, bottomY),
			new Vector2(rightX, bottomY)
		};

		bool wasGrounded = isGrounded;
		isGrounded = false;

		// LayerMask が設定されていない場合は全レイヤーをチェック
		int layerMaskToUse = (groundLayer.value == 0) ? ~0 : groundLayer;

		Debug.Log($"🔍 CheckGrounded: 敵位置={transform.position}, bottomY={bottomY}");
		Debug.Log($"📍 レイキャスト開始位置: left={leftX}, center={centerX}, right={rightX}");

		foreach (Vector2 origin in raycastOrigins)
		{
			RaycastHit2D hit = Physics2D.Raycast(
				origin,
				Vector2.down,
				0.5f,
				layerMaskToUse
			);

			Debug.DrawRay(origin, Vector2.down * 0.5f, hit.collider != null ? Color.green : Color.red);
			Debug.Log($"🔫 Raycast from {origin}: Hit={hit.collider != null}, Distance={hit.distance}, Collider={hit.collider?.name}");

			// 敵自身のコライダーを無視して地面を検出
			if (hit.collider != null && hit.collider != enemyCollider)
			{
				isGrounded = true;
				Debug.Log($"✅ 地面検出: {hit.collider.name}, Distance={hit.distance}");
				break;
			}
		}

		if (!isGrounded)
		{
			Debug.Log($"❌ 地面が見つかりません");
		}

		if (wasGrounded && !isGrounded)
		{
			Debug.Log($"⚠️ 地面から離れた");
		}
	}

	#endregion

	#region 攻撃コリジョン管理.

	/// <summary>
	/// 攻撃コリジョンを見つけます.
	/// </summary>
	private void FindAttackCollider()
	{
		// 子オブジェクトから攻撃コリジョンを探す.
		foreach (Transform child in transform)
		{
			if (child.name == attackColliderName)
			{
				attackCollider = child.GetComponent<Collider2D>();
				if (attackCollider != null)
				{
					// 初期状態は無効.
					attackCollider.enabled = false;
					// IsTriggerを有効化（重要！）
					attackCollider.isTrigger = true;
					Debug.Log($"✅ 攻撃コリジョン取得: {attackColliderName}");
					return;
				}
			}
		}

		Debug.LogWarning($"⚠️ 攻撃コリジョンが見つかりません: {attackColliderName}");
	}

	/// <summary>
	/// 攻撃コリジョンを有効化します.
	/// </summary>
	private void EnableAttackCollider()
	{
		if (attackCollider != null)
		{
			attackCollider.enabled = true;
			Debug.Log($"🔓 攻撃コリジョン有効化");
			Debug.Log($"📊 Is Trigger: {attackCollider.isTrigger}");
			Debug.Log($"📊 Enabled: {attackCollider.enabled}");
			Debug.Log($"📊 Bounds: {attackCollider.bounds}");

			// 0.5秒後に無効化.
			Invoke(nameof(DisableAttackCollider), 0.5f);
		}
		else
		{
			Debug.LogWarning($"⚠️ attackCollider が null です");
		}
	}

	/// <summary>
	/// 攻撃コリジョンを無効化します.
	/// </summary>
	private void DisableAttackCollider()
	{
		if (attackCollider != null)
		{
			attackCollider.enabled = false;
			Debug.Log($"🔒 攻撃コリジョン無効化");
		}
	}

	#endregion

	#region スプライト反転.

	/// <summary>
	/// 指定された方向に敵を向かせます（スプライト反転対応）.
	/// </summary>
	protected override void UpdateFacingDirection(Vector2 direction)
	{
		if (direction.x != 0)
		{
			// SpriteRendererをFlipXで反転.
			if (spriteRenderer != null)
			{
				spriteRenderer.flipX = direction.x < 0;
			}
		}
	}

	#endregion
}