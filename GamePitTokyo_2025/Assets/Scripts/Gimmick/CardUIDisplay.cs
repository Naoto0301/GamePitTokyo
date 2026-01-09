using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カード選択UI パネルを管理するスクリプト.
/// </summary>
public class CardUIPanelDisplay : MonoBehaviour
{
	#region インスペクター設定.
	[Header("UI要素")]
	[SerializeField]
	[Tooltip("カード選択パネル.")]
	private CanvasGroup panelCanvasGroup;
	[SerializeField]
	[Tooltip("カード1の画像")]
	private Image card1Image;
	[SerializeField]
	[Tooltip("カード1の説明テキスト")]
	private Text card1Text;
	[SerializeField]
	[Tooltip("カード2の画像")]
	private Image card2Image;
	[SerializeField]
	[Tooltip("カード2の説明テキスト")]
	private Text card2Text;
	[SerializeField]
	[Tooltip("カード3の画像")]
	private Image card3Image;
	[SerializeField]
	[Tooltip("カード3の説明テキスト")]
	private Text card3Text;
	#endregion

	#region プライベート変数.
	private static CardUIPanelDisplay instance;
	private Image[] cardImages;
	private Text[] cardTexts;
	private TreasureChest treasureChest;
	#endregion

	#region Unityライフサイクル.
	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}

		// 配列を初期化
		cardImages = new Image[] { card1Image, card2Image, card3Image };
		cardTexts = new Text[] { card1Text, card2Text, card3Text };

		// 最初は非表示
		if (panelCanvasGroup != null)
		{
			panelCanvasGroup.alpha = 0f;
		}
	}

	private void Update()
	{
		// パネルが表示されている時だけ数字キー入力を受け付ける
		if (panelCanvasGroup != null && panelCanvasGroup.alpha > 0f)
		{
			HandleKeyInput();
		}
	}
	#endregion

	#region 入力処理.
	/// <summary>
	/// 数字キー入力を処理します.
	/// </summary>
	private void HandleKeyInput()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			Debug.Log("🔑 キー1が押されました");
			SelectCard(0);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			Debug.Log("🔑 キー2が押されました");
			SelectCard(1);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			Debug.Log("🔑 キー3が押されました");
			SelectCard(2);
		}
	}

	/// <summary>
	/// カードがクリックされた時の処理.
	/// </summary>
	/// <param name="cardIndex">クリックされたカードのインデックス.</param>
	public void OnCardClicked(int cardIndex)
	{
		Debug.Log($"🖱️ カード{cardIndex + 1}がクリックされました");
		SelectCard(cardIndex);
	}

	/// <summary>
	/// カードを選択します.
	/// </summary>
	private void SelectCard(int cardIndex)
	{
		if (treasureChest != null)
		{
			treasureChest.SelectCard(cardIndex);
		}
	}
	#endregion

	#region UI表示管理.
	/// <summary>
	/// カード選択UIを表示します.
	/// </summary>
	/// <param name="cards">表示するカード配列.</param>
	/// <param name="chest">宝箱スクリプト参照.</param>
	public static void ShowCardSelection(StatusUpCard[] cards, TreasureChest chest)
	{
		if (instance == null)
		{
			Debug.LogError("CardUIPanelDisplay がシーンに存在しません！");
			return;
		}

		instance.treasureChest = chest;

		// 各カードの情報をUI に設定
		for (int i = 0; i < cards.Length && i < 3; i++)
		{
			// スプライトを設定
			if (instance.cardImages[i] != null)
			{
				instance.cardImages[i].sprite = cards[i].GetCardSprite();
				Debug.Log($"カード{i + 1}の画像を設定しました");
			}

			// 説明文を設定
			if (instance.cardTexts[i] != null)
			{
				instance.cardTexts[i].text = cards[i].GetCardInfo();
				Debug.Log($"カード{i + 1}の説明を設定しました");
			}
		}

		// パネルを表示
		if (instance.panelCanvasGroup != null)
		{
			instance.panelCanvasGroup.alpha = 1f;
			instance.panelCanvasGroup.interactable = true;
			instance.panelCanvasGroup.blocksRaycasts = true;
		}

		Debug.Log("✅ カード選択UIを表示しました");
	}

	/// <summary>
	/// カード選択UIを非表示にします.
	/// </summary>
	public static void HideCardSelection()
	{
		if (instance == null) return;

		if (instance.panelCanvasGroup != null)
		{
			instance.panelCanvasGroup.alpha = 0f;
			instance.panelCanvasGroup.interactable = false;
			instance.panelCanvasGroup.blocksRaycasts = false;
		}

		Debug.Log("✅ カード選択UIを非表示にしました");
	}
	#endregion
}