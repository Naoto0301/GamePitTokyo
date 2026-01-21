using UnityEngine;

/// <summary>
/// 敵が発射する弾クラス.
/// プレイヤーに衝突するとダメージを与えます.
/// </summary>
public class Bullet : MonoBehaviour
{
	#region インスペクター設定.

	[Header("弾の設定")]
	[SerializeField]
	[Tooltip("弾のダメージ.")]
	private float damage = 10f;

	[SerializeField]
	[Tooltip("弾の生存時間（秒）.")]
	private float lifeTime = 10f;

	[SerializeField]
	[Tooltip("プレイヤーレイヤー名.")]
	private string playerTag = "Player";

	[SerializeField]
	[Tooltip("弾が消える時のエフェクト.")]
	private GameObject destroyEffectPrefab;

	[SerializeField]
	[Tooltip("弾が消える時の音.")]
	private AudioClip destroySoundClip;

	#endregion

	#region プライベート変数.

	private float elapsedTime = 0f;
	private Rigidbody2D rb;
	private bool hasHit = false;

	#endregion

	#region Unityライフサイクル.

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		elapsedTime = 0f;
		hasHit = false;

		Debug.Log($"🎯 Bullet Start()実行");

		if (rb == null)
		{
			Debug.LogError($"❌ Rigidbody2Dが見つかりません！弾に付けてください");
			return;
		}

		Debug.Log($"🎯 弾生成: 速度={rb.linearVelocity}, 位置={transform.position}");
	}

	private void Update()
	{
		// 生存時間をカウント.
		elapsedTime += Time.deltaTime;

		// 生存時間を超えたら弾を破壊.
		if (elapsedTime >= lifeTime)
		{
			DestroyBullet();
		}

		// 毎フレーム速度をログ出力（デバッグ用）
		if (rb != null)
		{
			Debug.Log($"⚡ 弾の速度: {rb.linearVelocity}");
		}
	}

	#endregion

	#region 衝突判定.

	/// <summary>
	/// 衝突時の処理.
	/// </summary>
	private void OnTriggerEnter2D(Collider2D collision)
	{
		// 既に衝突していたら処理しない.
		if (hasHit)
			return;

		// プレイヤーに衝突したか判定.
		if (collision.CompareTag(playerTag))
		{
			HitPlayer(collision);
		}
		// その他のオブジェクトに衝突したか判定.
		else if (!collision.CompareTag("Enemy"))
		{
			DestroyBullet();
		}
	}

	/// <summary>
	/// 衝突判定（2Dではない場合）.
	/// </summary>
	private void OnTriggerStay2D(Collider2D collision)
	{
		// 既に衝突していたら処理しない.
		if (hasHit)
			return;

		// プレイヤーに衝突したか判定.
		if (collision.CompareTag(playerTag))
		{
			HitPlayer(collision);
		}
	}

	#endregion

	#region 衝突処理.

	/// <summary>
	/// プレイヤーに衝突した時の処理.
	/// </summary>
	/// <param name="playerCollider">プレイヤーのコライダー.</param>
	// <summary>
	/// プレイヤーに衝突した時の処理.
	/// </summary>
	/// <param name="playerCollider">プレイヤーのコライダー.</param>
	private void HitPlayer(Collider2D playerCollider)
	{
		hasHit = true;

		Debug.Log($"🎯 弾がプレイヤーに衝突！playerCollider: {playerCollider.gameObject.name}");

		// プレイヤーのスクリプトを取得
		// 注意: Playerkari ではなく O_Player を使う
		O_Player player = playerCollider.GetComponent<O_Player>();

		if (player != null)
		{
			Debug.Log($"✅ O_Player を取得しました");
			player.TakeDamage(damage);
			Debug.Log($"💥 プレイヤーが弾に衝突！ダメージ: {damage}");
		}
		else
		{
			Debug.LogWarning($"⚠️ プレイヤーに O_Player スクリプトが見つかりません");
			Debug.Log($"📍 playerCollider.gameObject: {playerCollider.gameObject.name}");
			Debug.Log($"📍 playerCollider.gameObject.tag: {playerCollider.gameObject.tag}");
		}

		// 弾を破壊
		DestroyBullet();
	}

	#endregion

	#region 弾の破壊.

	/// <summary>
	/// 弾を破壊します.
	/// </summary>
	private void DestroyBullet()
	{
		// エフェクトを再生.
		PlayDestroyEffect();

		// 音を再生.
		PlayDestroySound();

		// オブジェクトを削除.
		Destroy(gameObject);
	}

	/// <summary>
	/// 弾が消える時のエフェクトを再生します.
	/// </summary>
	private void PlayDestroyEffect()
	{
		if (destroyEffectPrefab != null)
		{
			Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
		}
	}

	/// <summary>
	/// 弾が消える時の音を再生します.
	/// </summary>
	private void PlayDestroySound()
	{
		if (destroySoundClip != null)
		{
			AudioSource.PlayClipAtPoint(destroySoundClip, transform.position);
		}
	}

	#endregion

	#region セッター.

	/// <summary>
	/// 弾のダメージを設定します.
	/// </summary>
	/// <param name="newDamage">新しいダメージ値.</param>
	public void SetDamage(float newDamage)
	{
		damage = newDamage;
	}

	/// <summary>
	/// 弾の生存時間を設定します.
	/// </summary>
	/// <param name="newLifeTime">新しい生存時間（秒）.</param>
	public void SetLifeTime(float newLifeTime)
	{
		lifeTime = newLifeTime;
	}

	/// <summary>
	/// 弾の速度を設定します.
	/// </summary>
	/// <param name="velocity">新しい速度ベクトル.</param>
	public void SetVelocity(Vector2 velocity)
	{
		if (rb != null)
		{
			rb.linearVelocity = velocity;
		}
	}

	#endregion

	#region ゲッター.

	/// <summary>
	/// 弾のダメージを取得します.
	/// </summary>
	public float GetDamage() => damage;

	/// <summary>
	/// 弾がプレイヤーに衝突したかを取得します.
	/// </summary>
	public bool HasHit() => hasHit;

	#endregion
}