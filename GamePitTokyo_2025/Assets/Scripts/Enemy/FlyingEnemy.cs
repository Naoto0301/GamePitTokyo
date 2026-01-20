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

	#endregion

	#region プレイヤー検出.

	/// <summary>
	/// 一度検出されたら、ずっと追尾を続けます.
	/// </summary>
	protected override void DetectPlayer()
	{
		// 既に検出されていたら検出処理をスキップ.
		if (isPlayerDetected)
		{
			return;
		}

		base.DetectPlayer();
	}

	#endregion

	#region プライベート変数.

	private float shootTimer = 0f;
	private Vector3 hoverPosition;
	private float orbitAngle = 0f;

	#endregion

	#region Unityライフサイクル.

	/// <summary>
	/// 初期化処理.
	/// </summary>
	protected override void Start()
	{
		base.Start();
		hoverPosition = transform.position;
		orbitAngle = 0f;
		shootTimer = shootCooldown;
		Debug.Log($"✅ FlyingEnemy初期化完了");
	}

	/// <summary>
	/// 毎フレーム実行される処理.
	/// </summary>
	protected override void Update()
	{
		base.Update();

		// 攻撃タイマーを更新.
		if (isPlayerDetected)
		{
			shootTimer += Time.deltaTime;
		}
	}

	#endregion

	#region 追尾と攻撃.

	/// <summary>
	/// プレイヤーが検出された時、ホバリングしながら攻撃します.
	/// </summary>
	protected override void OnPlayerDetected()
	{
		Debug.Log($"🎯 FlyingEnemy OnPlayerDetected() 実行!");

		if (playerTransform == null)
			return;

		float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

		Debug.Log($"📍 プレイヤー距離: {distanceToPlayer}, 攻撃範囲: {attackRange}");

		// プレイヤーの周りをホバリング.
		HoverAroundPlayer();

		// 攻撃範囲内なら攻撃.
		if (distanceToPlayer <= attackRange)
		{
			Debug.Log($"🔫 攻撃範囲内！Shoot()を呼び出し");
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
		Debug.Log($"⏰ shootTimer: {shootTimer}, shootCooldown: {shootCooldown}");

		if (shootTimer >= shootCooldown && bulletPrefab != null)
		{
			shootTimer = 0f;

			// 弾を発射.
			FireBullet();

			Debug.Log($"💥 飛行敵が発射！ダメージ: {attackPower}");
		}
		else if (bulletPrefab == null)
		{
			Debug.LogWarning($"⚠️ Bullet Prefabが指定されていません！");
		}
	}

	/// <summary>
	/// 弾を発射します.
	/// </summary>
	private void FireBullet()
	{
		Vector2 directionToPlayer = GetDirectionToPlayer();
		Vector3 spawnPosition = shootPoint != null ? shootPoint.position : transform.position;

		Debug.Log($"🎯 弾発射: 位置={spawnPosition}, 方向={directionToPlayer}");

		GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
		Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

		if (bulletRb != null)
		{
			// 敵の向きに関係なく、プレイヤー方向に発射.
			bulletRb.linearVelocity = directionToPlayer * bulletSpeed;
			Debug.Log($"✅ 弾の速度設定: {directionToPlayer * bulletSpeed}");
		}

		// 敵の向きに応じて弾を回転.
		RotateBullet(bullet, directionToPlayer);

		// 弾スクリプトにダメージを設定.
		SetBulletDamage(bullet);
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
		Bullet bulletScript = bullet.GetComponent<Bullet>();

		if (bulletScript != null)
		{
			bulletScript.SetDamage(attackPower);
			Debug.Log($"✅ 弾にダメージ設定: {attackPower}");
		}
	}

	#endregion

	#region 敵の向き更新.

	/// 移動方向に敵の向きを更新します.
	/// </summary>
	/// <param name="direction">移動方向.</param>
	private void UpdateFacingDirection(Vector2 direction)
	{
		// 敵を回転させる(移動方向に応じて).
		if (direction.x > 0.1f)
		{
			facingDirection = 1;
			transform.rotation = Quaternion.Euler(0, 180, 0);
		}
		else if (direction.x < -0.1f)
		{
			facingDirection = -1;
			transform.rotation = Quaternion.identity;
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
}