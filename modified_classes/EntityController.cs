using System;
using fsm;
using fsm.triggers;
using Game;
using UnityEngine;

public class EntityController : SaveSerialize, INearSeinReceiver, IDamageReciever
{
	private SpriteEntity SpriteEntity
	{
		get
		{
			return this.Entity as SpriteEntity;
		}
	}

	public void OnValidate()
	{
		this.Entity = base.transform.FindComponentUpwards<Entity>();
		this.Entity.Controller = this;
	}

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

	public void OnAnimationEnd(TextureAnimation anim)
	{
		this.StateMachine.Trigger<OnAnimationOrTransitionEnded>();
		if (!this.SpriteEntity.Animation.Animator.IsTransitionPlaying)
		{
			this.StateMachine.Trigger<OnAnimationEnded>();
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		this.StateMachine.Trigger(new OnCollisionEnter(collision));
	}

	public void OnCollisionStay(Collision collision)
	{
		this.StateMachine.Trigger(new OnCollisionStay(collision));
	}

	public void OnCollisionExit(Collision collision)
	{
		this.StateMachine.Trigger(new OnCollisionExit(collision));
	}

	public void OnRecieveDamage(Damage damage)
	{
		if (this.OnReceiveDamage != null)
		{
			this.OnReceiveDamage(damage);
		}
		this.StateMachine.Trigger(new OnReceiveDamage(damage));
	}

	public void OnNearSeinEnter()
	{
		this.m_nearSein = true;
	}

	public void OnNearSeinExit()
	{
		this.m_nearSein = false;
	}

	public bool NearSein
	{
		get
		{
			return this.m_nearSein && Characters.Sein.Controller.CanMove;
		}
	}

	public bool IsNearSein()
	{
		return this.NearSein;
	}

	public void OnSeinNearStay()
	{
		this.LastSeenSeinPosition = Characters.Sein.Position;
	}

	public Vector3 LastSeenSeinPosition { get; private set; }

	[ContextMenu("Current state class name")]
	public void ShowCurrentStateClassName()
	{
	}

	public override void Serialize(Archive ar)
	{
		this.StateMachine.Serialize(ar);
	}

	public Entity Entity;

	public StateMachine StateMachine = new StateMachine();

	public Action<Damage> OnReceiveDamage;

	private TransitionManager m_transManager;

	private bool m_nearSein;
}
