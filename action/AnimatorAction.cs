using System;
using UnityEngine;

[Category("Animator")]
public class AnimatorAction : ActionMethod
{
	public new void Start()
	{
		base.Start();
		if (this.Target == null)
		{
			base.enabled = false;
			return;
		}
		if (this.AnimatorsMode == AnimatorAction.FindAnimatorsMode.GameObject)
		{
			this.Animators = this.Target.GetComponents<LegacyAnimator>();
		}
		if (this.AnimatorsMode == AnimatorAction.FindAnimatorsMode.GameObjectAndChildren)
		{
			this.Animators = this.Target.GetComponentsInChildren<LegacyAnimator>();
		}
	}

	public override void Perform(IContext context)
	{
		for (int i = 0; i < this.Animators.Length; i++)
		{
			LegacyAnimator legacyAnimator = this.Animators[i];
			if (legacyAnimator.enabled)
			{
				switch (this.Command)
				{
				case AnimatorAction.PlayMode.Restart:
					legacyAnimator.Restart();
					break;
				case AnimatorAction.PlayMode.RestartReversed:
					legacyAnimator.RestartReverse();
					break;
				case AnimatorAction.PlayMode.Reverse:
					legacyAnimator.Reverse();
					break;
				case AnimatorAction.PlayMode.Stop:
					legacyAnimator.Stop();
					break;
				case AnimatorAction.PlayMode.Continue:
					legacyAnimator.Continue();
					break;
				case AnimatorAction.PlayMode.ContinueForward:
					legacyAnimator.Reversed = false;
					legacyAnimator.Continue();
					break;
				case AnimatorAction.PlayMode.ContinueReversed:
					legacyAnimator.Reversed = true;
					legacyAnimator.Continue();
					break;
				case AnimatorAction.PlayMode.StopAtStart:
					legacyAnimator.Restart();
					legacyAnimator.Stop();
					break;
				case AnimatorAction.PlayMode.StopAtEnd:
					legacyAnimator.RestartReverse();
					legacyAnimator.Stop();
					break;
				}
				legacyAnimator.Sample(legacyAnimator.CurrentTime);
			}
		}
	}

	public override void PerformInstantly(IContext context)
	{
		foreach (LegacyAnimator legacyAnimator in this.Animators)
		{
			if (legacyAnimator.enabled)
			{
				switch (this.Command)
				{
				case AnimatorAction.PlayMode.Restart:
					legacyAnimator.StopAndSampleAtEnd();
					break;
				case AnimatorAction.PlayMode.RestartReversed:
					legacyAnimator.StopAndSampleAtStart();
					break;
				case AnimatorAction.PlayMode.Reverse:
					legacyAnimator.StopAndSampleAtStart();
					break;
				case AnimatorAction.PlayMode.Stop:
					legacyAnimator.Stop();
					break;
				case AnimatorAction.PlayMode.Continue:
					if (legacyAnimator.Reversed)
					{
						legacyAnimator.StopAndSampleAtStart();
					}
					else
					{
						legacyAnimator.StopAndSampleAtEnd();
					}
					break;
				case AnimatorAction.PlayMode.ContinueForward:
					legacyAnimator.StopAndSampleAtEnd();
					break;
				case AnimatorAction.PlayMode.ContinueReversed:
					legacyAnimator.StopAndSampleAtStart();
					break;
				case AnimatorAction.PlayMode.StopAtStart:
					legacyAnimator.StopAndSampleAtStart();
					break;
				case AnimatorAction.PlayMode.StopAtEnd:
					legacyAnimator.StopAndSampleAtEnd();
					break;
				}
			}
		}
	}

	private string TargetName
	{
		get
		{
			return (this.AnimatorsMode != AnimatorAction.FindAnimatorsMode.SpecifyAnimators) ? ((!this.Target) ? "unkown" : this.Target.name) : ((this.Animators.Length <= 0 || !this.Animators[0]) ? "unkown" : this.Animators[0].name);
		}
	}

	public override string GetNiceName()
	{
		switch (this.Command)
		{
		case AnimatorAction.PlayMode.Restart:
			return "Restart " + this.TargetName + " animator";
		case AnimatorAction.PlayMode.RestartReversed:
			return "Restart reversed " + this.TargetName + " animator";
		case AnimatorAction.PlayMode.Reverse:
			return "Reverse " + this.TargetName + " animator";
		case AnimatorAction.PlayMode.Stop:
			return "Stop " + this.TargetName + " animator";
		case AnimatorAction.PlayMode.Continue:
			return "Continue " + this.TargetName + " animator";
		case AnimatorAction.PlayMode.ContinueForward:
			return "Continue " + this.TargetName + " animator forward";
		case AnimatorAction.PlayMode.ContinueReversed:
			return "Continue " + this.TargetName + " animator reversed";
		case AnimatorAction.PlayMode.StopAtStart:
			return "Stop " + this.TargetName + " animator at start";
		case AnimatorAction.PlayMode.StopAtEnd:
			return "Stop " + this.TargetName + " animator at end";
		default:
			return base.GetNiceName();
		}
	}

	[NotNull]
	public GameObject Target;

	public AnimatorAction.FindAnimatorsMode AnimatorsMode;

	public AnimatorAction.PlayMode Command;

	public LegacyAnimator[] Animators;

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
