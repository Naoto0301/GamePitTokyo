using UnityEngine;

/// <summary>
/// 攻撃力UP用カード（O_Player対応）.
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