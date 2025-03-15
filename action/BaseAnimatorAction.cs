using System;
using UnityEngine;

[Category("BaseAnimator")]
public class BaseAnimatorAction : ActionMethod
{
	public new void Start()
	{
		base.Start();
		if (this.AnimatorsMode == BaseAnimatorAction.FindAnimatorsMode.GameObject)
		{
			this.Animators = this.Target.GetComponents<BaseAnimator>();
		}
		if (this.AnimatorsMode == BaseAnimatorAction.FindAnimatorsMode.GameObjectAndChildren)
		{
			this.Animators = this.Target.GetComponentsInChildren<BaseAnimator>();
		}
	}

	public override void Perform(IContext context)
	{
		for (int i = 0; i < this.Animators.Length; i++)
		{
			BaseAnimator baseAnimator = this.Animators[i];
			if (baseAnimator.enabled)
			{
				baseAnimator.Initialize();
				switch (this.Command)
				{
				case BaseAnimatorAction.PlayMode.Restart:
					baseAnimator.Initialize();
					baseAnimator.AnimatorDriver.SetForward();
					baseAnimator.AnimatorDriver.Restart();
					break;
				case BaseAnimatorAction.PlayMode.RestartReversed:
					baseAnimator.Initialize();
					baseAnimator.AnimatorDriver.SetBackwards();
					baseAnimator.AnimatorDriver.Restart();
					break;
				case BaseAnimatorAction.PlayMode.Reverse:
					baseAnimator.Initialize();
					baseAnimator.AnimatorDriver.Reverse();
					break;
				case BaseAnimatorAction.PlayMode.Stop:
					baseAnimator.Initialize();
					baseAnimator.AnimatorDriver.Stop();
					break;
				case BaseAnimatorAction.PlayMode.Continue:
					baseAnimator.Initialize();
					baseAnimator.AnimatorDriver.Resume();
					break;
				case BaseAnimatorAction.PlayMode.ContinueForward:
					baseAnimator.Initialize();
					baseAnimator.AnimatorDriver.SetForward();
					baseAnimator.AnimatorDriver.Resume();
					break;
				case BaseAnimatorAction.PlayMode.ContinueReversed:
					baseAnimator.Initialize();
					baseAnimator.AnimatorDriver.SetBackwards();
					baseAnimator.AnimatorDriver.Resume();
					break;
				case BaseAnimatorAction.PlayMode.StopAtStart:
					baseAnimator.Initialize();
					baseAnimator.AnimatorDriver.Pause();
					baseAnimator.AnimatorDriver.GoToStart();
					break;
				case BaseAnimatorAction.PlayMode.StopAtEnd:
					baseAnimator.Initialize();
					baseAnimator.AnimatorDriver.Pause();
					baseAnimator.AnimatorDriver.GoToEnd();
					break;
				}
			}
		}
	}

	public override void PerformInstantly(IContext context)
	{
		foreach (BaseAnimator baseAnimator in this.Animators)
		{
			if (baseAnimator.enabled)
			{
				baseAnimator.Initialize();
				switch (this.Command)
				{
				case BaseAnimatorAction.PlayMode.Restart:
					baseAnimator.AnimatorDriver.GoToEnd();
					break;
				case BaseAnimatorAction.PlayMode.RestartReversed:
					baseAnimator.AnimatorDriver.GoToStart();
					break;
				case BaseAnimatorAction.PlayMode.Reverse:
					baseAnimator.AnimatorDriver.GoToStart();
					break;
				case BaseAnimatorAction.PlayMode.Stop:
					baseAnimator.AnimatorDriver.Stop();
					break;
				case BaseAnimatorAction.PlayMode.Continue:
					if (baseAnimator.AnimatorDriver.IsReversed)
					{
						baseAnimator.AnimatorDriver.GoToStart();
					}
					else
					{
						baseAnimator.AnimatorDriver.GoToEnd();
					}
					break;
				case BaseAnimatorAction.PlayMode.ContinueForward:
					baseAnimator.AnimatorDriver.GoToEnd();
					break;
				case BaseAnimatorAction.PlayMode.ContinueReversed:
					baseAnimator.AnimatorDriver.GoToStart();
					break;
				case BaseAnimatorAction.PlayMode.StopAtStart:
					baseAnimator.AnimatorDriver.GoToStart();
					break;
				case BaseAnimatorAction.PlayMode.StopAtEnd:
					baseAnimator.AnimatorDriver.GoToEnd();
					break;
				}
			}
		}
	}

	private string TargetName
	{
		get
		{
			return (this.AnimatorsMode != BaseAnimatorAction.FindAnimatorsMode.SpecifyAnimators) ? ((!this.Target) ? "unkown" : this.Target.name) : ((this.Animators.Length <= 0 || !this.Animators[0]) ? "unkown" : this.Animators[0].name);
		}
	}

	public override string GetNiceName()
	{
		switch (this.Command)
		{
		case BaseAnimatorAction.PlayMode.Restart:
			return "Restart " + this.TargetName + " BaseAnimator";
		case BaseAnimatorAction.PlayMode.RestartReversed:
			return "Restart reversed " + this.TargetName + " BaseAnimator";
		case BaseAnimatorAction.PlayMode.Reverse:
			return "Reverse " + this.TargetName + " BaseAnimator";
		case BaseAnimatorAction.PlayMode.Stop:
			return "Stop " + this.TargetName + " BaseAnimator";
		case BaseAnimatorAction.PlayMode.Continue:
			return "Continue " + this.TargetName + " BaseAnimator";
		case BaseAnimatorAction.PlayMode.ContinueForward:
			return "Continue forward " + this.TargetName + " BaseAnimator";
		case BaseAnimatorAction.PlayMode.ContinueReversed:
			return "Continue reversed " + this.TargetName + " BaseAnimator";
		case BaseAnimatorAction.PlayMode.StopAtStart:
			return "Stop at start " + this.TargetName + " BaseAnimator";
		case BaseAnimatorAction.PlayMode.StopAtEnd:
			return "Stop at end " + this.TargetName + " BaseAnimator";
		default:
			return base.GetNiceName();
		}
	}

	[NotNull]
	public GameObject Target;

	public BaseAnimatorAction.FindAnimatorsMode AnimatorsMode;

	public BaseAnimatorAction.PlayMode Command;

	public BaseAnimator[] Animators;

	public enum PlayMode
	{
		Restart,
		RestartReversed,
		Reverse,
		Stop,
		Continue,
		ContinueForward,
		ContinueReversed,
		StopAtStart,
		StopAtEnd
	}

	public enum FindAnimatorsMode
	{
		GameObject,
		GameObjectAndChildren,
		SpecifyAnimators
	}
}
