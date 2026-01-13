using UnityEngine;

/// <summary>
/// 宝箱システム.
/// </summary>
public class TreasureChest : MonoBehaviour
{
	#region 定数.
	private const int MAX_SELECTABLE_CARDS = 3;
	#endregion

	#region インスペクター設定.
	[Header("カード選択")]
	[SerializeField]
	[Tooltip("攻撃力UP用カード（宝箱に入れるカード）.")]
	private AttackUpCard attackUpCardPrefab;
	[SerializeField]
	[Tooltip("防御力UP用カード（宝箱に入れるカード）.")]
	private DefenseUpCard defenseUpCardPrefab;
	[SerializeField]
	[Tooltip("ジャンプ力UP用カード（宝箱に入れるカード）.")]
	private JumpPowerUpCard jumpPowerUpCardPrefab;

	[Header("宝箱設定")]
	[SerializeField]
	[Tooltip("宝箱が開いているかどうか.")]
	private bool isOpened = false;
	#endregion

	#region プライベート変数.
	private StatusUpCard[] availableCards;
	private int selectedCardIndex = -1;
	private Playerkari targetPlayer;
	private bool cardSelectionActive = false;
	private bool interactKeyPressed = false;
	#endregion

	#region Unityライフサイクル.
	/// <summary>
	/// 初期化処理.
	/// </summary>
	private void Start()
	{
		// このGameObjectに"Treasure"タグを設定.
		gameObject.tag = "Treasure";
		Debug.Log("✅ 宝箱初期化完了");
	}

	/// <summary>
	/// 毎フレーム実行される処理.
	/// </summary>
	private void Update()
	{
		// インタラクションキー（E、Space）の入力を検出
		if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
		{
			interactKeyPressed = true;
			Debug.Log("🔑 インタラクションキーが押されました");
		}
	}
	#endregion

	#region 宝箱の開封.
	/// <summary>
	/// 宝箱を開封してカードを出現させます.
	/// </summary>
	public void Open(Playerkari player)
	{
		if (isOpened)
		{
			Debug.Log("⚠️ この宝箱は既に開いています");
			return;
		}

		isOpened = true;
		targetPlayer = player;
		availableCards = new StatusUpCard[MAX_SELECTABLE_CARDS]
		{
			Instantiate(attackUpCardPrefab),
			Instantiate(defenseUpCardPrefab),
			Instantiate(jumpPowerUpCardPrefab)
		};

		// カードの位置を横一列に配置
		float cardSpacing = 3f; // カード間の距離
		float startX = transform.position.x - (cardSpacing * (MAX_SELECTABLE_CARDS - 1)) / 2f;

		for (int i = 0; i < availableCards.Length; i++)
		{
			// カードのPositionを設定
			Vector3 cardPosition = new Vector3(
				startX + (i * cardSpacing),
				transform.position.y + 3f,
				0f
			);
			availableCards[i].transform.position = cardPosition;
			Debug.Log($"カード{i + 1}の位置: {cardPosition}");
		}

		cardSelectionActive = true;
		Debug.Log("💰 宝箱を開封した！カードを選択してください");
		DisplayCardOptions();
	}

	/// <summary>
	/// カード選択オプションを表示します.
	/// </summary>
	private void DisplayCardOptions()
	{
		// UI Panel で表示（宝箱参照も渡す）
		CardUIPanelDisplay.ShowCardSelection(availableCards, this);

		Debug.Log("\n========== カード選択 ==========");
		for (int i = 0; i < availableCards.Length; i++)
		{
			Debug.Log($"\n【カード {i + 1}】");
			Debug.Log(availableCards[i].GetCardInfo());
		}
		Debug.Log("\n=================================");
		Debug.Log("数字キー（1、2、3）またはクリックでカードを選択してください");
		Debug.Log("=================================\n");
	}

	/// <summary>
	/// カードを選択して効果を適用します.
	/// </summary>
	/// <param name="cardIndex">選択したカードのインデックス.</param>
	public void SelectCard(int cardIndex)
	{
		if (!cardSelectionActive)
		{
			Debug.Log("⚠️ 現在カード選択中ではありません");
			return;
		}

		if (cardIndex < 0 || cardIndex >= availableCards.Length)
		{
			Debug.Log("⚠️ 無効なカード選択です");
			return;
		}

		selectedCardIndex = cardIndex;
		StatusUpCard selectedCard = availableCards[selectedCardIndex];

		// プレイヤーにカードの効果を適用
		selectedCard.ApplyEffect(targetPlayer);

		// 使用済みカードをクリーンアップ
		foreach (var card in availableCards)
		{
			if (card != null)
			{
				Destroy(card.gameObject);
			}
		}

		cardSelectionActive = false;

		// UI を非表示
		CardUIPanelDisplay.HideCardSelection();

		Debug.Log($"✅ {selectedCard.GetCardName()}を選択しました！");
	}
	#endregion

	#region トリガーイベント.
	/// <summary>
	/// プレイヤーが宝箱に接触した時の処理.
	/// </summary>
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			Playerkari player = collision.GetComponent<Playerkari>();
			if (player != null && !isOpened)
			{
				Debug.Log("💬 「Eキー（またはSpaceキー）で宝箱を開ける」");
			}
		}
	}

	/// <summary>
	/// プレイヤーが宝箱に接触し続けている間の処理.
	/// </summary>
	private void OnTriggerStay2D(Collider2D collision)
	{
		Debug.Log($"📍 OnTriggerStay2D呼び出し: {collision.gameObject.name}");
		Debug.Log($"   CompareTag('Player'): {collision.CompareTag("Player")}");
		Debug.Log($"   isOpened: {isOpened}");
		Debug.Log($"   interactKeyPressed: {interactKeyPressed}");

		if (collision.CompareTag("Player"))
		{
			Debug.Log("✅ Playerタグが確認されました");

			if (!isOpened)
			{
				Debug.Log("✅ 宝箱がまだ開いていません");

				if (interactKeyPressed)
				{
					Debug.Log("✅ インタラクションキーが押されています");
					Playerkari player = collision.GetComponent<Playerkari>();
					Debug.Log($"   プレイヤーコンポーネント: {player}");

					if (player != null)
					{
						Debug.Log("🎉 すべての条件を満たしました。宝箱を開きます！");
						Open(player);
						interactKeyPressed = false; // フラグをリセット
					}
				}
				else
				{
					Debug.Log("❌ interactKeyPressed = false");
				}
			}
			else
			{
				Debug.Log("❌ 宝箱は既に開いています");
			}
		}
		else
		{
			Debug.Log("❌ Playerタグが見つかりません");
		}
	}
	#endregion
}