using UnityEngine;
/// <summary>
/// テスト用プレイヤースクリプト.
/// 敵システムのテストに使用します.
/// </summary>
public class Playerkari : MonoBehaviour
{
	#region インスペクター設定.
	[Header("移動設定")]
	[SerializeField]
	[Tooltip("プレイヤーの移動速度.")]
	private float moveSpeed = 5f;
	[SerializeField]
	[Tooltip("プレイヤーのHP.")]
	private float maxHP = 100f;

	[Header("ステータス設定")]
	[SerializeField]
	[Tooltip("プレイヤーの攻撃力.")]
	private float attack = 10f;
	[SerializeField]
	[Tooltip("プレイヤーの防御力.")]
	private float defense = 5f;
	[SerializeField]
	[Tooltip("プレイヤーのジャンプ力.")]
	private float jumpPower = 10f;
	#endregion

	#region プライベート変数.
	private float currentHP;
	private Rigidbody2D rb;
	private Vector2 moveDirection = Vector2.zero;
	#endregion

	#region Unityライフサイクル.
	/// <summary>
	/// 初期化処理.
	/// </summary>
	private void Start()
	{
		currentHP = maxHP;
		rb = GetComponent<Rigidbody2D>();
		// このGameObjectに"Player"タグを設定.
		gameObject.tag = "Player";
		Debug.Log("✅ プレイヤー初期化完了");
		DisplayStats();
	}

	/// <summary>
	/// 毎フレーム実行される処理.
	/// </summary>
	private void Update()
	{
		HandleInput();
	}

	/// <summary>
	/// 物理演算フレーム実行処理.
	/// </summary>
	private void FixedUpdate()
	{
		Move();
	}
	#endregion

	#region 入力処理.
	/// <summary>
	/// プレイヤーの入力を処理します.
	/// </summary>
	private void HandleInput()
	{
		moveDirection = Vector2.zero;
		// 左右移動.
		if (Input.GetKey(KeyCode.A))
		{
			moveDirection.x = -1f;
		}
		if (Input.GetKey(KeyCode.D))
		{
			moveDirection.x = 1f;
		}
		// 上下移動（テスト用）.
		if (Input.GetKey(KeyCode.W))
		{
			moveDirection.y = 1f;
		}
		if (Input.GetKey(KeyCode.S))
		{
			moveDirection.y = -1f;
		}
	}
	#endregion

	#region 移動.
	/// <summary>
	/// プレイヤーを移動させます.
	/// </summary>
	private void Move()
	{
		if (rb != null)
		{
			rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
		}
	}
	#endregion

	#region ダメージと健康.
	/// <summary>
	/// プレイヤーがダメージを受けます.
	/// </summary>
	/// <param name="damage">ダメージ量.</param>
	public void TakeDamage(float damage)
	{
		// 防御力によるダメージ軽減
		float actualDamage = Mathf.Max(1f, damage - (defense * 0.1f));
		currentHP -= actualDamage;
		Debug.Log($"💔 プレイヤーがダメージを受けた！ダメージ: {actualDamage}, 残りHP: {currentHP}");
		if (currentHP <= 0)
		{
			Die();
		}
	}

	/// <summary>
	/// プレイヤーが死亡します.
	/// </summary>
	private void Die()
	{
		Debug.Log("💀 プレイヤーが死亡しました");
		Destroy(gameObject);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.name == "AttackCollider")
		{
			Debug.Log($"敵の攻撃が当たった！");
			// ダメージはMeleeEnemyから呼び出される
		}
	}
	#endregion

	#region ステータスアップ.
	/// <summary>
	/// 攻撃力をアップさせます.
	/// </summary>
	/// <param name="value">アップさせる値.</param>
	public void IncreaseAttack(float value)
	{
		attack += value;
		Debug.Log($"攻撃力が{value}上昇しました。現在の攻撃力: {attack}");
		DisplayStats();
	}

	/// <summary>
	/// 防御力をアップさせます.
	/// </summary>
	/// <param name="value">アップさせる値.</param>
	public void IncreaseDefense(float value)
	{
		defense += value;
		Debug.Log($"防御力が{value}上昇しました。現在の防御力: {defense}");
		DisplayStats();
	}

	/// <summary>
	/// ジャンプ力をアップさせます.
	/// </summary>
	/// <param name="value">アップさせる値.</param>
	public void IncreaseJumpPower(float value)
	{
		jumpPower += value;
		Debug.Log($"ジャンプ力が{value}上昇しました。現在のジャンプ力: {jumpPower}");
		DisplayStats();
	}

	/// <summary>
	/// 現在のステータスを表示します.
	/// </summary>
	private void DisplayStats()
	{
		Debug.Log($"\n=== ステータス ===\n攻撃力: {attack}\n防御力: {defense}\nジャンプ力: {jumpPower}\n================");
	}
	#endregion

	#region ゲッター.
	/// <summary>
	/// プレイヤーの現在のHPを取得します.
	/// </summary>
	public float GetCurrentHP() => currentHP;

	/// <summary>
	/// プレイヤーの最大HPを取得します.
	/// </summary>
	public float GetMaxHP() => maxHP;

	/// <summary>
	/// プレイヤーの攻撃力を取得します.
	/// </summary>
	public float GetAttack() => attack;

	/// <summary>
	/// プレイヤーの防御力を取得します.
	/// </summary>
	public float GetDefense() => defense;

	/// <summary>
	/// プレイヤーのジャンプ力を取得します.
	/// </summary>
	public float GetJumpPower() => jumpPower;
	#endregion
}