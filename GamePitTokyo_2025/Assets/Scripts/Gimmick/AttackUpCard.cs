using UnityEngine;

/// <summary>
/// 攻撃力UP用カード.
/// </summary>
public class AttackUpCard : StatusUpCard
{
	/// <summary>
	/// プレイヤーの攻撃力をアップさせます.
	/// </summary>
	public override void ApplyEffect(O_Player player)
	{
		player.IncreaseAttack(statUpValue);
		Debug.Log($"🔥 攻撃力がアップ！ +{statUpValue}");
	}
}