using System;
using UnityEngine;

// Token: 0x020003E1 RID: 993
public class DashOwlDashState : DashOwlState
{
	// Token: 0x06001694 RID: 5780 RVA: 0x0001410A File Offset: 0x0001230A
	public DashOwlDashState(DashOwlEnemy dashOwl) : base(dashOwl)
	{
	}

	// Token: 0x06001695 RID: 5781 RVA: 0x00078940 File Offset: 0x00076B40
	public override void OnEnter()
	{
		this.m_dashTargetOffset = (this.DashOwl.Controller.LastSeenSeinPosition - this.DashOwl.transform.position).normalized * this.DashOwl.Settings.DashDistance;
		this.DashOwl.DashSound.Play();
		this.DashOwl.Animation.Play(this.DashOwl.Animations.Dash, 0, null);
		this.DashOwl.SpriteRotation.RotateTowardsTarget(this.DashOwl.PositionToPlayerPosition, this.DashOwl.FaceLeft);
	}

	// Token: 0x06001696 RID: 5782 RVA: 0x0001418A File Offset: 0x0001238A
	public override void OnExit()
	{
		this.DashOwl.SpriteRotation.RotateBackToNormal();
	}

	// Token: 0x06001697 RID: 5783
	public override void UpdateState()
	{
		this.DashOwl.FlyMovement.Kickback.Stop();
		Vector3 a = this.m_dashTargetOffset * (this.DashOwl.Settings.DashCurve.Evaluate(base.CurrentStateTime + RandomizerBonusSkill.TimeScale(Time.deltaTime)) - this.DashOwl.Settings.DashCurve.Evaluate(base.CurrentStateTime));
		this.DashOwl.FlyMovement.Velocity = ((Time.deltaTime != 0f) ? (a / RandomizerBonusSkill.TimeScale(Time.deltaTime)) : Vector3.zero);
		base.UpdateState();
	}

	// Token: 0x0400140C RID: 5132
	private Vector3 m_dashTargetOffset;
}
