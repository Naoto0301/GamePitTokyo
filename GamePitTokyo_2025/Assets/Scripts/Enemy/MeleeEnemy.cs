using UnityEngine;

/// <summary>
/// プレイヤーを追尾して近接攻撃する敵クラス.
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

	#endregion


	#region プライベート変数.

	private float attackTimer = 0f;
	private Collider2D attackCollider;

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

		// 攻撃コリジョンを取得.
		FindAttackCollider();

		Debug.Log($"✅ MeleeEnemy初期化完了");
	}

	#endregion

	#region 追尾と攻撃.

	/// <summary>
	/// プレイヤーが検出された時、追尾して攻撃します.
	/// </summary>
	protected override void OnPlayerDetected()
	{
		Debug.Log("🎯 OnPlayerDetected() 実行!");

		if (playerTransform == null)
		{
			Debug.Log("⚠️ playerTransformがnullです");
			return;
		}

		float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
		Vector2 directionToPlayer = GetDirectionToPlayer();

		Debug.Log($"📍 プレイヤー距離: {distanceToPlayer}, 方向: {directionToPlayer}");

		// 敵の向きをプレイヤー方向に更新.
		UpdateFacingDirection(directionToPlayer);

		// 攻撃範囲外なら追尾.
		if (distanceToPlayer > attackRange)
		{
			Debug.Log($"🚶 プレイヤーに追尾中...");
			// X軸のみ移動（地上敵用）.
			Move(new Vector2(directionToPlayer.x * chaseSpeed, 0));
		}
		// 攻撃範囲内なら攻撃.
		else
		{
			Debug.Log($"⚔️ 攻撃範囲内！攻撃開始！");
			Move(Vector2.zero);
			Attack();
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

			// 0.5秒後に無効化.
			Invoke(nameof(DisableAttackCollider), 0.5f);
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


}