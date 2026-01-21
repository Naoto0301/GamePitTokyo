using UnityEngine;

/// <summary>
/// すべての敵の基底クラス.
/// 敵の共通機能（HP、攻撃力、徘徊、プレイヤー検出）を管理します.
/// </summary>
public class BaseEnemy : MonoBehaviour
{
	#region インスペクター設定.

	[Header("基本ステータス")]
	[SerializeField]
	[Tooltip("敵の最大HP.")]
	protected float maxHP = 30f;

	[SerializeField]
	[Tooltip("敵の攻撃力.")]
	protected float attackPower = 5f;

	[Header("検出設定")]
	[SerializeField]
	[Tooltip("プレイヤーを検出する最大距離.")]
	protected float detectionRange = 10f;

	[SerializeField]
	[Tooltip("視野角（度）.")]
	protected float fieldOfViewAngle = 120f;

	[SerializeField]
	[Tooltip("プレイヤーレイヤー名.")]
	protected string playerTag = "Player";

	[SerializeField]
	[Tooltip("敵が向いている方向（右=1, 左=-1）.")]
	protected int facingDirection = 1;

	[SerializeField]
	[Tooltip("Raycastの分割数.")]
	protected int raycastCount = 5;

	[SerializeField]
	[Tooltip("障害物レイヤー名.")]
	protected LayerMask obstacleLayer;

	[SerializeField]
	[Tooltip("プレイヤーが属するレイヤー.")]
	private LayerMask playerLayer;

	[Header("移動設定")]
	[SerializeField]
	[Tooltip("通常時の移動速度.")]
	protected float moveSpeed = 2f;

	[SerializeField]
	[Tooltip("追尾時の移動速度.")]
	protected float chaseSpeed = 4f;

	[SerializeField]
	[Tooltip("徘徊時の待機時間.")]
	protected float wanderWaitTime = 2f;

	[SerializeField]
	[Tooltip("徘徊時の移動距離.")]
	protected float wanderDistance = 5f;

	[Header("移動タイプ")]
	[SerializeField]
	[Tooltip("敵のタイプ（Ground=地上, Flying=飛行）.")]
	private EnemyType enemyType = EnemyType.Ground;

	#endregion

	#region 列挙型.

	/// <summary>
	/// 敵のタイプ.
	/// </summary>
	protected enum EnemyType
	{
		/// <summary>地上の敵.</summary>
		Ground,
		/// <summary>飛行敵.</summary>
		Flying
	}

	#endregion

	#region 保護された変数.

	protected float currentHP;
	protected Transform playerTransform;
	protected bool isPlayerDetected = false;
	protected Rigidbody2D rb;
	protected Vector2 moveDirection = Vector2.right;
	protected float wanderTimer = 0f;

	#endregion

	#region Unityライフサイクル.

	/// <summary>
	/// 初期化処理.
	/// </summary>
	protected virtual void Start()
	{
		currentHP = maxHP;
		rb = GetComponent<Rigidbody2D>();
		GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
		if (playerObject != null)
		{
			playerTransform = playerObject.transform;
		}

		// 地上敵の場合、Y軸の移動を制限.
		if (enemyType == EnemyType.Ground && rb != null)
		{
			rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
		}
	}

	/// <summary>
	/// 毎フレーム実行される処理.
	/// </summary>
	protected virtual void Update()
	{
		DetectPlayer();

		if (isPlayerDetected)
		{
			OnPlayerDetected();
		}
		else
		{
			Wander();
		}
	}

	#endregion

	#region プレイヤー検出.

	/// <summary>
	/// Raycastを使用して扇状にプレイヤーを検出します.
	/// </summary>
	protected virtual void DetectPlayer()
	{
		// 既に検出されていたら検出処理をスキップ.
		if (isPlayerDetected)
		{
			return;
		}

		if (playerTransform == null)
		{
			isPlayerDetected = false;
			return;
		}

		float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

		// 検出範囲外なら検出しない.
		if (distanceToPlayer > detectionRange)
		{
			isPlayerDetected = false;
			return;
		}

		Debug.Log($"📊 検出範囲内: 距離 = {distanceToPlayer}");

		// プレイヤーの方向を計算.
		Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
		float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

		// 敵の向く方向の角度を計算.
		float enemyFacingAngle = facingDirection > 0 ? 0f : 180f;

		// 視野角内かチェック.
		float angleDifference = Mathf.Abs(Mathf.DeltaAngle(enemyFacingAngle, angleToPlayer));

		Debug.Log($"👁️ 角度差: {angleDifference}度, 視野角: {fieldOfViewAngle}度");

		if (angleDifference > fieldOfViewAngle * 0.5f)
		{
			Debug.Log($"⚠️ 視野角外です");
			isPlayerDetected = false;
			return;
		}

		// Raycastでプレイヤーまでの直線に障害物がないかチェック.
		isPlayerDetected = CastRayToPlayer();
	}

	/// <summary>
	/// プレイヤーに複数のRayを放ち、障害物チェックを行います.
	/// </summary>
	/// <returns>プレイヤーが検出されたか.</returns>
	private bool CastRayToPlayer()
	{
		Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
		float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

		Debug.Log($"📍 Raycast開始: プレイヤーまでの距離 = {distanceToPlayer}");

		// 複数本のRayを放つ.
		for (int i = 0; i < raycastCount; i++)
		{
			float angle = Mathf.Lerp(
				-fieldOfViewAngle * 0.5f,
				fieldOfViewAngle * 0.5f,
				(float)i / (raycastCount - 1)
			);

			float radians = angle * Mathf.Deg2Rad;
			Vector2 rayDirection = new Vector2(
				Mathf.Cos(radians) * facingDirection,
				Mathf.Sin(radians)
			);

			RaycastHit2D hit = Physics2D.Raycast(
				transform.position,
				rayDirection,
				detectionRange,
				playerLayer
			);

			// Raycast結果の可視化（Debug用）.
			Debug.DrawRay(transform.position, rayDirection * detectionRange, Color.yellow);

			// プレイヤーに命中したか.
			if (hit.collider != null)
			{
				Debug.Log($"🎯 Ray {i}: 何かに命中 - オブジェクト名: {hit.collider.gameObject.name}, タグ: {hit.collider.tag}, 距離: {hit.distance}");

				if (hit.collider.CompareTag(playerTag))
				{
					Debug.DrawRay(transform.position, rayDirection * hit.distance, Color.green);
					Debug.Log($"✅ プレイヤーを検出しました！");
					return true;
				}
			}
			else
			{
				Debug.Log($"🎯 Ray {i}: 何も命中しません");
			}
		}

		return false;
	}

	/// <summary>
	/// 視野の扇状範囲をScene ビューに描画します（デバッグ用）.
	/// </summary>
	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;

		// 視野角の端を描画.
		float halfFOV = fieldOfViewAngle * 0.5f * Mathf.Deg2Rad;

		Vector2 leftRay = new Vector2(
			Mathf.Cos(-halfFOV) * facingDirection,
			Mathf.Sin(-halfFOV)
		);
		Vector2 rightRay = new Vector2(
			Mathf.Cos(halfFOV) * facingDirection,
			Mathf.Sin(halfFOV)
		);

		Gizmos.DrawLine(transform.position, (Vector2)transform.position + leftRay * detectionRange);
		Gizmos.DrawLine(transform.position, (Vector2)transform.position + rightRay * detectionRange);

		// 検出範囲を円で表示.
		Gizmos.color = Color.blue;
		DrawCircle(transform.position, detectionRange, 30);
	}

	/// <summary>
	/// Gizmoで円を描画します.
	/// </summary>
	private void DrawCircle(Vector3 position, float radius, int segments)
	{
		float angle = 360f / segments;
		Vector3 lastPoint = position + new Vector3(radius, 0, 0);

		for (int i = 1; i <= segments; i++)
		{
			float rad = angle * i * Mathf.Deg2Rad;
			Vector3 newPoint = position + new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0);
			Gizmos.DrawLine(lastPoint, newPoint);
			lastPoint = newPoint;
		}
	}

	#endregion

	#region 徘徊.

	/// <summary>
	/// 敵が徘徊します.
	/// </summary>
	protected virtual void Wander()
	{
		wanderTimer += Time.deltaTime;

		if (wanderTimer >= wanderWaitTime)
		{
			wanderTimer = 0f;
			moveDirection = new Vector2(Random.Range(-1f, 1f), 0).normalized;
			UpdateFacingDirection(moveDirection);
		}

		// 地上敵の場合、X軸のみ移動.
		if (enemyType == EnemyType.Ground)
		{
			Move(new Vector2(moveDirection.x * moveSpeed, 0));
		}
		else
		{
			Move(moveDirection * moveSpeed);
		}
	}

	#endregion

	#region 追尾と攻撃.

	/// <summary>
	/// プレイヤーが検出された時の処理.
	/// 派生クラスでオーバーライドして実装します.
	/// </summary>
	protected virtual void OnPlayerDetected()
	{
		// 派生クラスで実装.
	}

	#endregion

	#region 移動.

	/// <summary>
	/// 指定された方向に敵を移動させます.
	/// </summary>
	/// <param name="direction">移動方向と速度.</param>
	protected virtual void Move(Vector2 direction)
	{
		if (rb != null)
		{
			rb.linearVelocity = direction;
		}
	}

	/// <summary>
	/// プレイヤーの方向を取得します.
	/// </summary>
	/// <returns>プレイヤーへの正規化された方向ベクトル.</returns>
	protected Vector2 GetDirectionToPlayer()
	{
		if (playerTransform == null)
			return Vector2.zero;

		return (playerTransform.position - transform.position).normalized;
	}

	#endregion

	#region ダメージと健康.

	/// <summary>
	/// 敵にダメージを与えます.
	/// </summary>
	/// <param name="damage">ダメージ量.</param>
	public virtual void TakeDamage(float damage)
	{
		currentHP -= damage;
			Debug.Log($"💥 {gameObject.name}がダメージを受けた！ ダメージ量: {damage}, 現在HP: {currentHP}/{maxHP}");
		if (currentHP <= 0)
		{
			Die();
		}
	}

	/// <summary>
	/// 敵を破壊します.
	/// </summary>
	protected virtual void Die()
	{
		Debug.Log($"💀 {gameObject.name}が倒された！");
		Destroy(gameObject);
	}

	#endregion

	#region ゲッター.

	/// <summary>
	/// 敵の現在のHPを取得します.
	/// </summary>
	public float GetCurrentHP() => currentHP;

	/// <summary>
	/// 敵の攻撃力を取得します.
	/// </summary>
	public float GetAttackPower() => attackPower;

	#endregion

	#region 敵の向き更新.

	/// <summary>
	/// 移動方向に敵の向きを更新します.
	/// </summary>
	/// <param name="direction">移動方向.</param>
	protected virtual void UpdateFacingDirection(Vector2 direction)
	{
		// X軸の移動方向に応じてfacingDirectionと向きを更新.
		if (direction.x > 0.1f)
		{
			facingDirection = 1;
			transform.rotation = Quaternion.identity;
		}
		else if (direction.x < -0.1f)
		{
			facingDirection = -1;
			transform.rotation = Quaternion.Euler(0, 180, 0);
		}
	}

	#endregion
}