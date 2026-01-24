using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManeger : MonoBehaviour
{
	// タイトル → ゲーム
	public void GoToGameScene()
	{
		SceneManager.LoadScene("Stage");
	}

	// クリア → タイトル
	public void GoToTitleScene()
	{
		SceneManager.LoadScene("TitleScene");
	}
	// イグジットボタン
	public void ExitGame()
	{
		Application.Quit();

#if UNITY_EDITOR
		// Unityエディタ上で終了を確認するため
		UnityEditor.EditorApplication.isPlaying = false;
#endif
	}
}
