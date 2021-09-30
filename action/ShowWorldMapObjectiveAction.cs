using System;

public class ShowWorldMapObjectiveAction : PerformingAction
{
	public override void Perform(IContext context)
	{
	}

	public override void Stop()
	{
	}

	public override bool IsPerforming
	{
		get
		{
			return this.m_isPerforming;
		}
	}

	public void OnFinish()
	{
		this.m_isPerforming = false;
	}

	public Objective Objective;

	private bool m_isPerforming;
}
