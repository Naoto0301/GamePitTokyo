using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManeger : MonoBehaviour
{
	// タイトル → ゲーム
	public void GoToGameScene()
	{
		SceneManager.LoadScene("GameScene");
	}

	// クリア → タイトル
	public void GoToTitleScene()
	{
		SceneManager.LoadScene("TitleScene");
	}
}
