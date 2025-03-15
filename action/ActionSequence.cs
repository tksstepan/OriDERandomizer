using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class ActionSequence : PerformingAction, IPooled, ISuspendable
{
	public bool IsRunning
	{
		get
		{
			return this.m_isRunning;
		}
		set
		{
			this.m_isRunning = value;
		}
	}

	public int Index
	{
		get
		{
			return this.m_index;
		}
		set
		{
			this.m_index = value;
		}
	}

	public void OnPoolSpawned()
	{
		this.Stop();
		this.m_isSuspended = false;
	}

	public override void Awake()
	{
		SuspensionManager.Register(this);
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
		Events.Scheduler.OnGameReset.Add(new Action(this.OnGameReset));
	}

	public override void OnDestroy()
	{
		SuspensionManager.Unregister(this);
		base.OnDestroy();
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
		Events.Scheduler.OnGameReset.Remove(new Action(this.OnGameReset));
	}

	private void OnGameReset()
	{
		if (this.m_isRunning)
		{
			this.Stop();
		}
	}

	public void OnRestoreCheckpoint()
	{
		ActionSequenceSerializer component = base.GetComponent<ActionSequenceSerializer>();
		if (component)
		{
			return;
		}
		
		this.Stop();
	}

	public void FindActions()
	{
		this.Actions.Clear();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			foreach (ActionMethod item in child.GetComponents<ActionMethod>())
			{
				this.Actions.Add(item);
			}
		}
		this.Actions.Sort((ActionMethod a, ActionMethod b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
	}

	public override void Perform(IContext context)
	{
		this.Perform(context, false);
	}

	public override void PerformInstantly(IContext context)
	{
		this.Perform(context, true);
	}

	public void Perform(IContext context, bool instant)
	{
		if (!base.enabled)
		{
			return;
		}

		if (this.Actions == null)
		{
			this.FindActions();
		}

		if (this.Actions.Count == 0)
		{
			return;
		}

		this.m_isRunning = true;
		this.m_isInstant = instant;
		this.m_index = 0;
		this.m_context = context;
		this.RunAction(this.Actions[this.m_index]);
		this.UpdateActions();
	}

	public void RunAction(ActionMethod action)
	{
		if (action)
		{
			if (this.m_isInstant)
			{
				action.PerformInstantly(this.m_context);
			}
			else
			{
				action.Perform(this.m_context);
			}
		}
	}

	public void FixedUpdate()
	{
		if (this.m_isSuspended)
		{
			return;
		}
		this.UpdateActions();
	}

	public void UpdateActions()
	{
		if (!this.m_isRunning)
		{
			return;
		}
		int count = this.Actions.Count;
		while (this.m_index < count)
		{
			ActionMethod actionMethod = this.Actions[this.m_index];
			if (actionMethod != null && actionMethod is WaitAction)
			{
				WaitAction waitAction = actionMethod as WaitAction;
				if (waitAction.IsPerforming)
				{
					return;
				}
			}
			this.m_index++;
			if (this.m_index == count)
			{
				this.m_isRunning = false;
				return;
			}
			this.RunAction(this.Actions[this.m_index]);
		}
	}

	public static void Rename(List<ActionMethod> actions)
	{
		int num = 0;
		for (int i = 0; i < actions.Count; i++)
		{
			ActionMethod actionMethod = actions[i];
			num++;
			string niceName = actionMethod.GetNiceName();
			actionMethod.name = ActionSequence.FormatName(num, niceName);
		}
	}

	public static string FormatName(int number, string name)
	{
		return string.Format("{0:00}", number) + ". " + name;
	}

	public static string UnformatName(string name)
	{
		return name.Remove(0, 4);
	}

	public void RefreshNames()
	{
		this.FindActions();
		ActionSequence.Rename(this.Actions);
	}

	public override string GetNiceName()
	{
		return base.gameObject.name;
	}

	public bool IsSuspended
	{
		get
		{
			return this.m_isSuspended;
		}
		set
		{
			this.m_isSuspended = value;
		}
	}

	public override void Stop()
	{
		this.m_isRunning = false;
		this.m_isInstant = false;
		this.m_index = 0;
		this.m_context = null;
	}

	public override bool IsPerforming
	{
		get
		{
			return this.m_isRunning;
		}
	}

	public override void Serialize(Archive ar)
	{
		ActionSequenceSerializer component = base.GetComponent<ActionSequenceSerializer>();
		if (component)
		{
			return;
		}
		if (ar.Reading)
		{
			this.Stop();
		}
		base.Serialize(ar);
	}

	private bool m_isRunning;

	private int m_index;

	private IContext m_context;

	private bool m_isSuspended;

	public List<ActionMethod> Actions = new List<ActionMethod>();

	private bool m_isInstant;
}
