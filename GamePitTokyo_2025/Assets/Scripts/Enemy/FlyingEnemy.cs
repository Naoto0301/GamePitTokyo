using UnityEngine;

/// <summary>
/// 空を飛びながら弾を発射する敵クラス.
/// </summary>
public class FlyingEnemy : BaseEnemy
{
	#region インスペクター設定.

	[Header("飛行敵設定")]
	[SerializeField]
	[Tooltip("攻撃可能な範囲.")]
	private float attackRange = 8f;

	[SerializeField]
	[Tooltip("攻撃間隔（秒）.")]
	private float shootCooldown = 2f;

	[SerializeField]
	[Tooltip("発射する弾のプリファブ.")]
	private GameObject bulletPrefab;

	[SerializeField]
	[Tooltip("弾の発射位置.")]
	private Transform shootPoint;

	[SerializeField]
	[Tooltip("弾の速度.")]
	private float bulletSpeed = 5f;

	[SerializeField]
	[Tooltip("ホバリング時のプレイヤーとの距離.")]
	private float hoverDistance = 3f;

	[SerializeField]
	[Tooltip("ホバリングの円運動速度.")]
	private float hoverOrbitSpeed = 2f;

	[SerializeField]
	[Tooltip("射撃アニメーションを再生するAnimator.")]
	private Animator animator;

	[SerializeField]
	[Tooltip("射撃アニメーションパラメータ名.")]
	private string shootAnimationTrigger = "Shoot";

	#endregion

	#region プライベート変数.

	private float shootTimer = 0f;
	private Vector3 hoverPosition;
	private float orbitAngle = 0f;

	#endregion

	#region Unityイベント.

	/// <summary>
	/// 初期化処理.
	/// </summary>
	protected override void Start()
	{
		base.Start();
		hoverPosition = transform.position;
		orbitAngle = 0f;

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
	/// プレイヤーが検出された時、ホバリングしながら攻撃します.
	/// </summary>
	protected override void OnPlayerDetected()
	{
		if (playerTransform == null)
			return;

		float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

		// プレイヤーの周りをホバリング.
		HoverAroundPlayer();

		// 攻撃範囲内なら攻撃.
		if (distanceToPlayer <= attackRange)
		{
			Shoot();
		}

		// 敵の向きをプレイヤー方向に更新.
		Vector2 directionToPlayer = GetDirectionToPlayer();
		UpdateFacingDirection(directionToPlayer);
	}

	/// <summary>
	/// プレイヤーの周りを円を描くようにホバリングします.
	/// </summary>
	private void HoverAroundPlayer()
	{
		if (playerTransform == null)
			return;

		// 軌道角度を更新.
		orbitAngle += hoverOrbitSpeed * Time.deltaTime;

		// ホバリング位置を計算.
		float xOffset = Mathf.Cos(orbitAngle) * hoverDistance;
		float yOffset = Mathf.Sin(orbitAngle) * hoverDistance;

		hoverPosition = playerTransform.position + new Vector3(xOffset, yOffset, 0);

		// ホバリング位置へ移動.
		Vector2 directionToHover = (hoverPosition - transform.position).normalized;
		Move(directionToHover * moveSpeed);
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

			Debug.Log($"飛行敵が発射！ダメージ: {attackPower}");
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
			bulletRb.linearVelocity = directionToPlayer * bulletSpeed;
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

	#region 徘徊.

	/// <summary>
	/// 敵が徘徊します（飛行敵版）.
	/// </summary>
	protected override void Wander()
	{
		wanderTimer += Time.deltaTime;

		if (wanderTimer >= wanderWaitTime)
		{
			wanderTimer = 0f;
			hoverPosition = (Vector3)transform.position + Random.insideUnitSphere * wanderDistance;
			hoverPosition.z = 0;
		}

		// ホバリング位置へ移動.
		Vector2 directionToWander = (hoverPosition - transform.position).normalized;
		Move(directionToWander * moveSpeed * 0.7f);
	}

	#endregion

	#region デバッグ用.

	/// <summary>
	/// Scene ビューに攻撃範囲を描画します.
	/// </summary>
	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();

		// 攻撃範囲を描画.
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRange);

		// ホバリング距離を描画.
		if (playerTransform != null)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(playerTransform.position, hoverDistance);
		}
	}

	#endregion
}