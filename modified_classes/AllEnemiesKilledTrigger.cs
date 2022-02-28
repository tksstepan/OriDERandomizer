using System;
using System.Collections.Generic;
using Game;

// Token: 0x02000294 RID: 660
public class AllEnemiesKilledTrigger : Trigger
{
	// Token: 0x06000CC4 RID: 3268
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_counter);
		base.Serialize(ar);
		if (this.ActionOnAwakeTrigger && this.m_counter >= this.TriggerOnCounter)
		{
			this.ActionOnAwakeTrigger.Perform(null);
		}
	}

	// Token: 0x06000CC5 RID: 3269
	public void Increment()
	{
		this.m_counter++;
		if (this.m_counter == this.TriggerOnCounter)
		{
			BingoController.OnPurpleDoor(this.MoonGuid);
			base.DoTrigger(true);
		}
	}

	// Token: 0x06000CC6 RID: 3270
	public new void Awake()
	{
		base.Awake();
		this.RegisterEvent();
	}

	// Token: 0x06000CC7 RID: 3271
	public new void OnDestroy()
	{
		base.OnDestroy();
		this.DeregisterEvent();
	}

	// Token: 0x06000CC8 RID: 3272
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

	// Token: 0x06000CC9 RID: 3273
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

	// Token: 0x06000CCA RID: 3274
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

	// Token: 0x06000CCB RID: 3275
	private void EntityKilled(Damage damage)
	{
		this.EnemyKilled();
	}

	// Token: 0x06000CCC RID: 3276
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

	// Token: 0x04000BFD RID: 3069
	public List<RespawningPlaceholder> RespawningPlaceholders = new List<RespawningPlaceholder>();

	// Token: 0x04000BFE RID: 3070
	public List<Entity> Entities = new List<Entity>();

	// Token: 0x04000BFF RID: 3071
	public List<MessageProvider> Messages = new List<MessageProvider>();

	// Token: 0x04000C00 RID: 3072
	public bool ShowMessages = true;

	// Token: 0x04000C01 RID: 3073
	public int TriggerOnCounter;

	// Token: 0x04000C02 RID: 3074
	private int m_counter;

	// Token: 0x04000C03 RID: 3075
	private MessageBox m_lastMessageBox;

	// Token: 0x04000C04 RID: 3076
	public ActionMethod ActionOnAwakeTrigger;
}
