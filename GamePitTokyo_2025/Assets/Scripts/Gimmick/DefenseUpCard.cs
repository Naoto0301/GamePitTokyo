using UnityEngine;

/// <summary>
/// 防御力UP用カード.
/// </summary>
public class DefenseUpCard : StatusUpCard
{
	/// <summary>
	/// プレイヤーの防御力をアップさせます.
	/// </summary>
	public override void ApplyEffect(Playerkari player)
	{
		player.IncreaseDefense(statUpValue);
		Debug.Log($"🛡️ 防御力がアップ！ +{statUpValue}");
	}
}
