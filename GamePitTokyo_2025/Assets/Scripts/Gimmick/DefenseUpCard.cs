using UnityEngine;

/// <summary>
/// 防御力UP用カード（O_Player対応）.
/// </summary>
public class DefenseUpCard : StatusUpCard
{
	/// <summary>
	/// プレイヤーの防御力をアップさせます.
	/// </summary>
	public override void ApplyEffect(O_Player player)
	{
		player.IncreaseDefense(statUpValue);
		Debug.Log($"🛡️ 防御力がアップ！ +{statUpValue}");
	}
}

