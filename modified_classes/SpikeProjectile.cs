using System;
using UnityEngine;

public class SpikeProjectile : Projectile
{
	public new void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.IsSuspended)
		{
			this.Rigidbody.velocity = RandomizerBonusSkill.TimeScale(this.SpeedOverTimeCurve.Evaluate(this.CurrentTime) * base.Direction * base.Speed);
		}
	}

	public override bool CanBeBashed()
	{
		return false;
	}

	public AnimationCurve SpeedOverTimeCurve;
}
