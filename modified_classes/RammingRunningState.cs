using System;
using UnityEngine;

// Token: 0x0200048D RID: 1165
public class RammingRunningState : RammingEnemyState
{
	// Token: 0x060019CA RID: 6602 RVA: 0x0001698A File Offset: 0x00014B8A
	public RammingRunningState(RammingEnemy rammingEnemy) : base(rammingEnemy)
	{
	}

	// Token: 0x060019CB RID: 6603 RVA: 0x000811C8 File Offset: 0x0007F3C8
	public override void OnEnter()
	{
		this.GroundEnemy.Animation.PlayLoop(this.RammingEnemy.Animations.Running, 0, null, false);
		if (this.GroundEnemy.gameObject.activeInHierarchy)
		{
			this.GroundEnemy.PlaySound(this.RammingEnemy.Sounds.Run);
		}
	}

	// Token: 0x060019CC RID: 6604 RVA: 0x000169FA File Offset: 0x00014BFA
	public override void OnExit()
	{
		this.GroundEnemy.StopSound(this.RammingEnemy.Sounds.Run);
	}

	// Token: 0x060019CD RID: 6605
	public override void UpdateState()
	{
		float accelerationDuration = this.RammingEnemy.Settings.AccelerationDuration;
		AnimationCurve runningSpeedMultipliedOverTime = this.RammingEnemy.Settings.RunningSpeedMultipliedOverTime;
		float runSpeed = this.RammingEnemy.Settings.RunSpeed;
		this.GroundEnemy.PlatformMovement.LocalSpeedX = RandomizerBonusSkill.TimeScale((float)((!this.GroundEnemy.FaceLeft) ? 1 : -1) * runSpeed * runningSpeedMultipliedOverTime.Evaluate(base.CurrentStateTime / accelerationDuration));
	}
}
