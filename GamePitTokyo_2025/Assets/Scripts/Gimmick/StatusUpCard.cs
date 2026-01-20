using UnityEngine;

/// <summary>
/// ステータスアップカードの基底クラス（O_Player対応）.
/// </summary>
public abstract class StatusUpCard : MonoBehaviour
{
	#region インスペクター設定.
	[Header("カード設定")]
	[SerializeField]
	[Tooltip("カードの名前.")]
	protected string cardName = "ステータスアップカード";
	[SerializeField]
	[Tooltip("カードの説明.")]
	protected string cardDescription = "ステータスをアップさせるカード";
	[SerializeField]
	[Tooltip("カードのスプライト.")]
	protected Sprite cardSprite;
	[SerializeField]
	[Tooltip("アップするステータスの値.")]
	protected float statUpValue = 0f;
	#endregion

	/// <summary>
	/// プレイヤーにステータスアップを適用します.
	/// </summary>
	public abstract void ApplyEffect(O_Player player);

	/// <summary>
	/// カード情報を取得します.
	/// </summary>
	public string GetCardInfo()
	{
		return $"{cardName}\n{cardDescription}\n効果値: +{statUpValue}";
	}

	/// <summary>
	/// カード名を取得します.
	/// </summary>
	public string GetCardName() => cardName;

	/// <summary>
	/// カードスプライトを取得します.
	/// </summary>
	public Sprite GetCardSprite() => cardSprite;
}
