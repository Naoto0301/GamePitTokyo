using UnityEngine;

/// <summary>
/// 地上で弾を発射する敵クラス.
/// プレイヤーとの距離を保ちながら攻撃します.
/// </summary>
public class GroundRangeEnemy : BaseEnemy
{
	#region インスペクター設定.

	[Header("地上射撃敵設定")]
	[SerializeField]
	[Tooltip("攻撃可能な範囲.")]
	private float attackRange = 7f;

	[SerializeField]
	[Tooltip("攻撃間隔（秒）.")]
	private float shootCooldown = 1.5f;

	[SerializeField]
	[Tooltip("発射する弾のプリファブ.")]
	private GameObject bulletPrefab;

	[SerializeField]
	[Tooltip("弾の発射位置.")]
	private Transform shootPoint;

	[SerializeField]
	[Tooltip("弾の速度.")]
	private float bulletSpeed = 6f;

	[SerializeField]
	[Tooltip("プレイヤーから保つ距離.")]
	private float keepDistance = 4f;

	[SerializeField]
	[Tooltip("距離調整の許容範囲.")]
	private float distanceTolerance = 1f;

	[SerializeField]
	[Tooltip("射撃アニメーションを再生するAnimator.")]
	private Animator animator;

	[SerializeField]
	[Tooltip("射撃アニメーションパラメータ名.")]
	private string shootAnimationTrigger = "Shoot";

	[SerializeField]
	[Tooltip("移動アニメーションパラメータ名.")]
	private string moveAnimationParameter = "IsMoving";

	#endregion

	#region プライベート変数.

	private float shootTimer = 0f;
	private bool isMoving = false;

	#endregion

	#region Unityイベント.

	/// <summary>
	/// 初期化処理.
	/// </summary>
	protected override void Start()
	{
		base.Start();

		// Animatorが指定されていない場合は自動取得.
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}

		shootTimer = shootCooldown;
	}

	#endregion

	#region 追尾と攻撃.

	/// <summary>
	/// プレイヤーが検出された時、距離を保ちながら攻撃します.
	/// </summary>
	protected override void OnPlayerDetected()
	{
		if (playerTransform == null)
			return;

		float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
		Vector2 directionToPlayer = GetDirectionToPlayer();

		// プレイヤーとの距離を調整.
		AdjustDistanceToPlayer(distanceToPlayer, directionToPlayer);

		// 攻撃範囲内なら攻撃.
		if (distanceToPlayer <= attackRange)
		{
			Shoot();
		}

		// 敵の向きをプレイヤー方向に更新.
		UpdateFacingDirection(directionToPlayer);
	}

	/// <summary>
	/// プレイヤーとの距離を調整します.
	/// </summary>
	/// <param name="distanceToPlayer">プレイヤーまでの距離.</param>
	/// <param name="directionToPlayer">プレイヤーへの方向.</param>
	private void AdjustDistanceToPlayer(float distanceToPlayer, Vector2 directionToPlayer)
	{
		// 保つべき距離より遠い場合はプレイヤーに近づく.
		if (distanceToPlayer > keepDistance + distanceTolerance)
		{
			Move(directionToPlayer * chaseSpeed);
			isMoving = true;
		}
		// 保つべき距離より近い場合はプレイヤーから遠ざかる.
		else if (distanceToPlayer < keepDistance - distanceTolerance)
		{
			Move(-directionToPlayer * chaseSpeed);
			isMoving = true;
		}
		// 保つべき距離内なら停止.
		else
		{
			Move(Vector2.zero);
			isMoving = false;
		}

		// 移動アニメーション更新.
		UpdateMoveAnimation();
	}

	/// <summary>
	/// プレイヤーに向かって弾を発射します.
	/// </summary>
	private void Shoot()
	{
		shootTimer += Time.deltaTime;

		if (shootTimer >= shootCooldown && bulletPrefab != null)
		{
			shootTimer = 0f;

			// 射撃アニメーション再生.
			PlayShootAnimation();

			// 弾を発射.
			FireBullet();

			Debug.Log($"地上射撃敵が発射！ダメージ: {attackPower}");
		}
	}

	/// <summary>
	/// 弾を発射します.
	/// </summary>
	private void FireBullet()
	{
		Vector2 directionToPlayer = GetDirectionToPlayer();
		Vector3 spawnPosition = shootPoint != null ? shootPoint.position : transform.position;

		GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
		Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

		if (bulletRb != null)
		{
			//bulletRb.velocity = directionToPlayer * bulletSpeed;
		}

		// 敵の向きに応じて弾を回転.
		RotateBullet(bullet, directionToPlayer);

		// 弾スクリプトにダメージを設定.
		SetBulletDamage(bullet);
	}

	/// <summary>
	/// 射撃アニメーションを再生します.
	/// </summary>
	private void PlayShootAnimation()
	{
		if (animator != null)
		{
			animator.SetTrigger(shootAnimationTrigger);
		}
	}

	/// <summary>
	/// 弾を発射方向に向けて回転させます.
	/// </summary>
	/// <param name="bullet">回転させる弾.</param>
	/// <param name="direction">弾の進行方向.</param>
	private void RotateBullet(GameObject bullet, Vector2 direction)
	{
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	/// <summary>
	/// 弾スクリプトにダメージを設定します.
	/// </summary>
	/// <param name="bullet">ダメージを設定する弾.</param>
	private void SetBulletDamage(GameObject bullet)
	{
		//Bullet bulletScript = bullet.GetComponent<Bullet>();

		//if (bulletScript != null)
		//{
		//	bulletScript.SetDamage(attackPower);
		//}
	}

	#endregion

	#region 敵の向き更新.

	/// <summary>
	/// プレイヤー方向に敵の向きを更新します.
	/// </summary>
	/// <param name="direction">プレイヤーへの方向.</param>
	private void UpdateFacingDirection(Vector2 direction)
	{
		if (direction.x > 0)
		{
			facingDirection = 1;
			transform.localScale = new Vector3(1, 1, 1);
		}
		else if (direction.x < 0)
		{
			facingDirection = -1;
			transform.localScale = new Vector3(-1, 1, 1);
		}
	}

	#endregion

	#region アニメーション.

	/// <summary>
	/// 移動アニメーションを更新します.
	/// </summary>
	private void UpdateMoveAnimation()
	{
		if (animator != null)
		{
			animator.SetBool(moveAnimationParameter, isMoving);
		}
	}

	#endregion

	#region デバッグ用.

	/// <summary>
	/// Scene ビューに攻撃範囲と保持距離を描画します.
	/// </summary>
	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();

		// 攻撃範囲を赤で描画.
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRange);

		// 保つ距離を緑で描画.
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, keepDistance);

		// 許容範囲を黄色で描画.
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, keepDistance + distanceTolerance);
		Gizmos.DrawWireSphere(transform.position, keepDistance - distanceTolerance);
	}

	#endregion
}