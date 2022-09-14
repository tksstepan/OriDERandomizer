using System;
using UnityEngine;

public class DashOwlDashState : DashOwlState
{
	public DashOwlDashState(DashOwlEnemy dashOwl) : base(dashOwl)
	{
	}

	public override void OnEnter()
	{
		this.m_dashTargetOffset = (this.DashOwl.Controller.LastSeenSeinPosition - this.DashOwl.transform.position).normalized * this.DashOwl.Settings.DashDistance;
		this.DashOwl.DashSound.Play();
		this.DashOwl.Animation.Play(this.DashOwl.Animations.Dash, 0, null);
		this.DashOwl.SpriteRotation.RotateTowardsTarget(this.DashOwl.PositionToPlayerPosition, this.DashOwl.FaceLeft);
	}

	public override void OnExit()
	{
		this.DashOwl.SpriteRotation.RotateBackToNormal();
	}

	public override void UpdateState()
	{
		this.DashOwl.FlyMovement.Kickback.Stop();
		Vector3 a = this.m_dashTargetOffset * (this.DashOwl.Settings.DashCurve.Evaluate(base.CurrentStateTime + RandomizerBonusSkill.TimeScale(Time.deltaTime)) - this.DashOwl.Settings.DashCurve.Evaluate(base.CurrentStateTime));
		this.DashOwl.FlyMovement.Velocity = ((Time.deltaTime != 0f) ? (a / RandomizerBonusSkill.TimeScale(Time.deltaTime)) : Vector3.zero);
		base.UpdateState();
	}

	private Vector3 m_dashTargetOffset;
}
