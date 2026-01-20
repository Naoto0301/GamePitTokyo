using UnityEngine;

/// <summary>
/// ジャンプ力UP用カード（O_Player対応）.
/// </summary>
public class JumpPowerUpCard : StatusUpCard
{
	/// <summary>
	/// プレイヤーのジャンプ力をアップさせます.
	/// </summary>
	public override void ApplyEffect(O_Player player)
	{
		player.IncreaseJumpPower(statUpValue);
		Debug.Log($"⬆️ ジャンプ力がアップ！ +{statUpValue}");
	}
}