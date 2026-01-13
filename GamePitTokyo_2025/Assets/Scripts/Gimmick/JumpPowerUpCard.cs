using UnityEngine;

/// <summary>
/// ジャンプ力UP用カード.
/// </summary>
public class JumpPowerUpCard : StatusUpCard
{
	/// <summary>
	/// プレイヤーのジャンプ力をアップさせます.
	/// </summary>
	public override void ApplyEffect(Playerkari player)
	{
		player.IncreaseJumpPower(statUpValue);
		Debug.Log($"⬆️ ジャンプ力がアップ！ +{statUpValue}");
	}
}