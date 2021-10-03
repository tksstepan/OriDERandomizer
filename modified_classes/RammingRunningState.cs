using System;
using UnityEngine;

public class RammingRunningState : RammingEnemyState
{
	public RammingRunningState(RammingEnemy rammingEnemy) : base(rammingEnemy)
	{
	}

	public override void OnEnter()
	{
		this.GroundEnemy.Animation.PlayLoop(this.RammingEnemy.Animations.Running, 0, null, false);
		if (this.GroundEnemy.gameObject.activeInHierarchy)
		{
			this.GroundEnemy.PlaySound(this.RammingEnemy.Sounds.Run);
		}
	}

	public override void OnExit()
	{
		this.GroundEnemy.StopSound(this.RammingEnemy.Sounds.Run);
	}

	public override void UpdateState()
	{
		float accelerationDuration = this.RammingEnemy.Settings.AccelerationDuration;
		AnimationCurve runningSpeedMultipliedOverTime = this.RammingEnemy.Settings.RunningSpeedMultipliedOverTime;
		float runSpeed = this.RammingEnemy.Settings.RunSpeed;
		this.GroundEnemy.PlatformMovement.LocalSpeedX = RandomizerBonusSkill.TimeScale((float)((!this.GroundEnemy.FaceLeft) ? 1 : -1) * runSpeed * runningSpeedMultipliedOverTime.Evaluate(base.CurrentStateTime / accelerationDuration));
	}
}
