using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
	private static void SuspendGameplay()
	{
		SuspensionManager.GetSuspendables(DebugMenu.SuspendablesToIgnoreForGameplay, UI.Cameras.Current.GameObject);
		SuspensionManager.SuspendExcluding(DebugMenu.SuspendablesToIgnoreForGameplay);
	}

	public static bool DashOrGrenadeEnabled
	{
		get
		{
			return Characters.Sein && (Characters.Sein.PlayerAbilities.Dash.HasAbility || Characters.Sein.PlayerAbilities.Grenade.HasAbility);
		}
	}

	private static void ResumeGameplay()
	{
		SuspensionManager.ResumeExcluding(DebugMenu.SuspendablesToIgnoreForGameplay);
		DebugMenu.SuspendablesToIgnoreForGameplay.Clear();
	}

	public void FixedUpdate()
	{
		if (XboxLiveController.IsContentPackage)
		{
		}
		if (GameController.FreezeFixedUpdate)
		{
			return;
		}
		if (Characters.Current as Component && !UI.MainMenuVisible && !GameController.Instance.GameInTitleScreen)
		{
			if (DebugMenuB.DebugControlsEnabled && !Core.Input.RightShoulder.Used && Core.Input.RightShoulder.IsPressed && !DebugMenu.DashOrGrenadeEnabled && !DebugMenuB.Active)
			{
				if (!this.m_noClipParamsEnabled)
				{
					this.m_noClipGhost = (GameObject)InstantiateUtility.Instantiate(this.NoClipGhostPrefab);
					this.m_noClipGhost.transform.position = Characters.Current.Position;
					UI.Cameras.Current.ChangeTarget(this.m_noClipGhost.transform);
					DebugMenu.SuspendGameplay();
					this.m_noClipParamsEnabled = true;
					if (UberPostProcess.Instance != null)
					{
						this.m_doMotionBlur = UberPostProcess.Instance.DoMotionBlur;
						UberPostProcess.Instance.DoMotionBlur = false;
					}
				}
				Vector2 vector = MoonMath.Vector.ApplyRectangleDeadzone(Core.Input.Axis, 0.15f, 0.15f);
				this.m_noClipGhost.transform.position += (Vector3) vector.normalized * this.AxisToSpeedCurve.Evaluate(vector.magnitude) * Time.deltaTime;
			}
			if (this.m_noClipParamsEnabled && !Core.Input.RightShoulder.IsPressed)
			{
				Characters.Current.Position = this.m_noClipGhost.transform.position;
				Characters.Current.Speed = Vector2.zero;
				if (Characters.Ori)
				{
					Characters.Ori.MoveOriBackToPlayer();
				}
				UI.Cameras.Current.ChangeTargetToCurrentCharacter();
				InstantiateUtility.Destroy(this.m_noClipGhost);
				DebugMenu.ResumeGameplay();
				this.m_noClipParamsEnabled = false;
				if (UberPostProcess.Instance != null)
				{
					UberPostProcess.Instance.DoMotionBlur = this.m_doMotionBlur;
				}
			}
		}
	}

	public GameObject NoClipGhostPrefab;

	public AnimationCurve AxisToSpeedCurve;

	private GameObject m_noClipGhost;

	private bool m_noClipParamsEnabled;

	private static readonly HashSet<ISuspendable> SuspendablesToIgnoreForGameplay = new HashSet<ISuspendable>();

	private bool m_doMotionBlur;
}
