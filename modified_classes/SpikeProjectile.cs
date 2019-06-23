using System;
using UnityEngine;

// Token: 0x020004B3 RID: 1203
public class SpikeProjectile : Projectile
{
	// Token: 0x06001A54 RID: 6740
	public new void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.IsSuspended)
		{
			this.Rigidbody.velocity = RandomizerBonusSkill.TimeScale(this.SpeedOverTimeCurve.Evaluate(this.CurrentTime) * base.Direction * base.Speed);
		}
	}

	// Token: 0x06001A55 RID: 6741
	public override bool CanBeBashed()
	{
		return false;
	}

	// Token: 0x0400175E RID: 5982
	public AnimationCurve SpeedOverTimeCurve;
}
