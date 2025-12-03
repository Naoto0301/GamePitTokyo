using UnityEngine;

/// <summary>
/// プレイヤーを追尾して近接攻撃する敵クラス.
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
	[Tooltip("攻撃時のアニメーションを再生するAnimator.")]
	private Animator animator;

	[SerializeField]
	[Tooltip("攻撃アニメーションパラメータ名.")]
	private string attackAnimationTrigger = "Attack";

	#endregion

	#region プライベート変数.

	private float attackTimer = 0f;
	private bool isAttacking = false;

	#endregion

	#region Unityライフサイクル.

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

		attackTimer = attackCooldown;
	}

	#endregion

	#region 追尾と攻撃.

	/// <summary>
	/// プレイヤーが検出された時、追尾して攻撃します.
	/// </summary>
	protected override void OnPlayerDetected()
	{
		if (playerTransform == null)
			return;

		float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
		Vector2 directionToPlayer = GetDirectionToPlayer();

		// 敵の向きをプレイヤー方向に更新.
		UpdateFacingDirection(directionToPlayer);

		// 攻撃範囲外なら追尾.
		if (distanceToPlayer > attackRange)
		{
			// X軸のみ移動（地上敵用）.
			Move(new Vector2(directionToPlayer.x * chaseSpeed, 0));
			isAttacking = false;
		}
		// 攻撃範囲内なら攻撃.
		else
		{
			Move(Vector2.zero);
			Attack();
		}

		Debug.Log($"プレイヤーを発見！距離: {distanceToPlayer}, 方向: {directionToPlayer}");
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
			isAttacking = true;

			// アニメーション再生.
			PlayAttackAnimation();

			// プレイヤーにダメージを与える.
			DealDamageToPlayer();

			Debug.Log($"近接敵が攻撃！ダメージ: {attackPower}");
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
		}
	}

	/// <summary>
	/// プレイヤーにダメージを与えます.
	/// </summary>
	private void DealDamageToPlayer()
	{
		if (playerTransform == null)
			return;

		//// プレイヤースクリプトの取得を試みる.
		//PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();

		//if (playerHealth != null)
		//{
		//	playerHealth.TakeDamage(attackPower);
		//}
		//else
		//{
		//	Debug.LogWarning("プレイヤーに PlayerHealth スクリプトが見つかりません.");
		//}
	}

	#endregion

	#region 敵の向き更新.

	/// <summary>
	/// 移動方向に敵の向きを更新します.
	/// </summary>
	/// <param name="direction">移動方向.</param>
	private void UpdateFacingDirection(Vector2 direction)
	{
		// 敵を回転させる（移動方向に応じて）.
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
	}

	#endregion
}