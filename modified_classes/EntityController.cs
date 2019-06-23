using System;
using fsm;
using fsm.triggers;
using Game;
using UnityEngine;

// Token: 0x02000429 RID: 1065
public class EntityController : SaveSerialize, INearSeinReceiver, IDamageReciever
{
	// Token: 0x17000408 RID: 1032
	// (get) Token: 0x060017A9 RID: 6057 RVA: 0x00014E91 File Offset: 0x00013091
	private SpriteEntity SpriteEntity
	{
		get
		{
			return this.Entity as SpriteEntity;
		}
	}

	// Token: 0x060017AA RID: 6058 RVA: 0x00014E9E File Offset: 0x0001309E
	public void OnValidate()
	{
		this.Entity = base.transform.FindComponentUpwards<Entity>();
		this.Entity.Controller = this;
	}

	// Token: 0x060017AB RID: 6059 RVA: 0x0007B994 File Offset: 0x00079B94
	public new void Awake()
	{
		base.Awake();
		if (this.Entity == null)
		{
			this.OnValidate();
		}
		if (this.SpriteEntity && this.SpriteEntity.Animation)
		{
			this.SpriteEntity.Animation.Animator.OnAnimationEndEvent += this.OnAnimationEnd;
		}
	}

	// Token: 0x060017AC RID: 6060
	public void FixedUpdate()
	{
		if (this.Entity.IsSuspended)
		{
			return;
		}
		if (this.m_transManager == null)
		{
			this.m_transManager = this.StateMachine.GetTransistionManager<OnFixedUpdate>();
		}
		if (this.m_transManager == null)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		if (this.Entity is Enemy)
		{
			deltaTime = RandomizerBonusSkill.TimeScale(deltaTime);
		}
		this.StateMachine.UpdateState(deltaTime);
		this.StateMachine.CurrentTrigger = null;
		this.m_transManager.Process(this.StateMachine);
	}

	// Token: 0x060017AD RID: 6061 RVA: 0x00014EBD File Offset: 0x000130BD
	public void OnAnimationEnd(TextureAnimation anim)
	{
		this.StateMachine.Trigger<OnAnimationOrTransitionEnded>();
		if (!this.SpriteEntity.Animation.Animator.IsTransitionPlaying)
		{
			this.StateMachine.Trigger<OnAnimationEnded>();
		}
	}

	// Token: 0x060017AE RID: 6062 RVA: 0x00014EEF File Offset: 0x000130EF
	public void OnCollisionEnter(Collision collision)
	{
		this.StateMachine.Trigger(new OnCollisionEnter(collision));
	}

	// Token: 0x060017AF RID: 6063 RVA: 0x00014F02 File Offset: 0x00013102
	public void OnCollisionStay(Collision collision)
	{
		this.StateMachine.Trigger(new OnCollisionStay(collision));
	}

	// Token: 0x060017B0 RID: 6064 RVA: 0x00014F15 File Offset: 0x00013115
	public void OnCollisionExit(Collision collision)
	{
		this.StateMachine.Trigger(new OnCollisionExit(collision));
	}

	// Token: 0x060017B1 RID: 6065 RVA: 0x00014F28 File Offset: 0x00013128
	public void OnRecieveDamage(Damage damage)
	{
		if (this.OnReceiveDamage != null)
		{
			this.OnReceiveDamage(damage);
		}
		this.StateMachine.Trigger(new OnReceiveDamage(damage));
	}

	// Token: 0x060017B2 RID: 6066 RVA: 0x00014F52 File Offset: 0x00013152
	public void OnNearSeinEnter()
	{
		this.m_nearSein = true;
	}

	// Token: 0x060017B3 RID: 6067 RVA: 0x00014F5B File Offset: 0x0001315B
	public void OnNearSeinExit()
	{
		this.m_nearSein = false;
	}

	// Token: 0x17000409 RID: 1033
	// (get) Token: 0x060017B4 RID: 6068 RVA: 0x00014F64 File Offset: 0x00013164
	public bool NearSein
	{
		get
		{
			return this.m_nearSein && Characters.Sein.Controller.CanMove;
		}
	}

	// Token: 0x060017B5 RID: 6069 RVA: 0x00014F83 File Offset: 0x00013183
	public bool IsNearSein()
	{
		return this.NearSein;
	}

	// Token: 0x060017B6 RID: 6070 RVA: 0x00014F8B File Offset: 0x0001318B
	public void OnSeinNearStay()
	{
		this.LastSeenSeinPosition = Characters.Sein.Position;
	}

	// Token: 0x1700040A RID: 1034
	// (get) Token: 0x060017B7 RID: 6071 RVA: 0x00014F9D File Offset: 0x0001319D
	// (set) Token: 0x060017B8 RID: 6072 RVA: 0x00014FA5 File Offset: 0x000131A5
	public Vector3 LastSeenSeinPosition { get; private set; }

	// Token: 0x060017B9 RID: 6073 RVA: 0x000030E7 File Offset: 0x000012E7
	[ContextMenu("Current state class name")]
	public void ShowCurrentStateClassName()
	{
	}

	// Token: 0x060017BA RID: 6074 RVA: 0x00014FAE File Offset: 0x000131AE
	public override void Serialize(Archive ar)
	{
		this.StateMachine.Serialize(ar);
	}

	// Token: 0x040014FA RID: 5370
	public Entity Entity;

	// Token: 0x040014FB RID: 5371
	public StateMachine StateMachine = new StateMachine();

	// Token: 0x040014FC RID: 5372
	public Action<Damage> OnReceiveDamage;

	// Token: 0x040014FD RID: 5373
	private TransitionManager m_transManager;

	// Token: 0x040014FE RID: 5374
	private bool m_nearSein;
}
