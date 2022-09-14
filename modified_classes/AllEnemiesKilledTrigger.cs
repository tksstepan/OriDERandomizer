using System;
using System.Collections.Generic;
using Game;

public class AllEnemiesKilledTrigger : Trigger
{
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_counter);
		base.Serialize(ar);
		if (this.ActionOnAwakeTrigger && this.m_counter >= this.TriggerOnCounter)
		{
			this.ActionOnAwakeTrigger.Perform(null);
		}
	}

	public void Increment()
	{
		this.m_counter++;
		if (this.m_counter == this.TriggerOnCounter)
		{
			BingoController.OnPurpleDoor(this.MoonGuid);
			base.DoTrigger(true);
		}
	}

	public new void Awake()
	{
		base.Awake();
		this.RegisterEvent();
	}

	public new void OnDestroy()
	{
		base.OnDestroy();
		this.DeregisterEvent();
	}

	public void Init()
	{
		this.RespawningPlaceholders.Clear();
		for (int i = 0; i < base.GetComponentsInChildren<RespawningPlaceholder>().Length; i++)
		{
			RespawningPlaceholder item = base.GetComponentsInChildren<RespawningPlaceholder>()[i];
			this.RespawningPlaceholders.Add(item);
		}
		this.Entities.Clear();
		for (int j = 0; j < base.GetComponentsInChildren<Entity>().Length; j++)
		{
			Entity item2 = base.GetComponentsInChildren<Entity>()[j];
			this.Entities.Add(item2);
		}
		this.TriggerOnCounter = this.RespawningPlaceholders.Count + this.Entities.Count;
	}

	private void RegisterEvent()
	{
		Action<Damage> action = new Action<Damage>(this.EntityKilled);
		for (int i = 0; i < this.RespawningPlaceholders.Count; i++)
		{
			RespawningPlaceholder respawningPlaceholder = this.RespawningPlaceholders[i];
			respawningPlaceholder.OnCurrentInstanceDeath = (Action<Damage>)Delegate.Combine(respawningPlaceholder.OnCurrentInstanceDeath, action);
		}
		for (int j = 0; j < this.Entities.Count; j++)
		{
			this.Entities[j].DamageReciever.OnDeathEvent.Add(action);
		}
	}

	private void DeregisterEvent()
	{
		Action<Damage> action = new Action<Damage>(this.EntityKilled);
		for (int i = 0; i < this.RespawningPlaceholders.Count; i++)
		{
			RespawningPlaceholder respawningPlaceholder = this.RespawningPlaceholders[i];
			respawningPlaceholder.OnCurrentInstanceDeath = (Action<Damage>)Delegate.Remove(respawningPlaceholder.OnCurrentInstanceDeath, action);
		}
		for (int j = 0; j < this.Entities.Count; j++)
		{
			this.Entities[j].DamageReciever.OnDeathEvent.Remove(action);
		}
	}

	private void EntityKilled(Damage damage)
	{
		this.EnemyKilled();
	}

	private void EnemyKilled()
	{
		if (this.Active)
		{
			this.Increment();
			if (this.m_lastMessageBox)
			{
				this.m_lastMessageBox.HideMessageScreen();
			}
			if (this.ShowMessages)
			{
				int num = this.TriggerOnCounter - this.m_counter - 1;
				if (num >= this.Messages.Count)
				{
					num = this.Messages.Count - 1;
				}
				if (num > 0)
				{
					this.m_lastMessageBox = UI.Hints.Show(this.Messages[num], HintLayer.Gameplay, 1f);
				}
			}
		}
	}

	public List<RespawningPlaceholder> RespawningPlaceholders = new List<RespawningPlaceholder>();

	public List<Entity> Entities = new List<Entity>();

	public List<MessageProvider> Messages = new List<MessageProvider>();

	public bool ShowMessages = true;

	public int TriggerOnCounter;

	private int m_counter;

	private MessageBox m_lastMessageBox;

	public ActionMethod ActionOnAwakeTrigger;
}
