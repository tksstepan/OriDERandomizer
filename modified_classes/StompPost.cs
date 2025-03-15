using System;
using Core;
using Game;
using UnityEngine;

public class StompPost : SaveSerialize, IDamageReciever, IAttackable, IStompAttackable, ISuspendable, IDynamicGraphicHierarchy
{
	public new void Awake()
	{
		base.Awake();
		SuspensionManager.Register(this);
		this.m_transform = base.transform;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		SuspensionManager.Unregister(this);
	}

	public void Start()
	{
		this.m_distanceStompedIntoGround = 0f;
		this.m_startLocalPosition = base.transform.localPosition;
	}

	public void OnRecieveDamage(Damage damage)
	{
		if (damage.Type == DamageType.Stomp && Vector3.Dot(base.transform.rotation * Vector3.down, Characters.Sein.PlatformBehaviour.PlatformMovement.GravityDirection) > Mathf.Cos(0.17453292f) && !this.m_activated)
		{
			this.m_distanceStompedIntoGround = Mathf.Min(this.StompIntoGroundAmount, this.m_distanceStompedIntoGround + this.StompIntoGroundAmount / (float)this.NumberOfStomps);
			this.m_remainingRiseDelayTime = this.RisingDelay;
			if (Mathf.Approximately(this.m_distanceStompedIntoGround, this.StompIntoGroundAmount))
			{
				BingoController.OnStompPost(this.MoonGuid);
				this.m_activated = true;
				if (this.AllTheWayInAction)
				{
					this.AllTheWayInAction.Perform(null);
				}
				if (this.AllTheWayInSound)
				{
					Sound.Play(this.AllTheWayInSound.GetSound(null), this.m_transform.position, null);
					return;
				}
			}
			else if (this.StompSound)
			{
				Sound.Play(this.StompSound.GetSound(null), this.m_transform.position, null);
			}
		}
	}

	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		if (this.m_remainingRiseDelayTime > 0f)
		{
			this.m_remainingRiseDelayTime -= Time.deltaTime;
			if (this.m_remainingRiseDelayTime < 0f)
			{
				this.m_remainingRiseDelayTime = 0f;
			}
		}
		if (!this.m_activated && this.m_remainingRiseDelayTime < 0f)
		{
			this.m_distanceStompedIntoGround -= Time.deltaTime * this.RiseSpeed;
			if (this.m_distanceStompedIntoGround < 0f)
			{
				this.m_distanceStompedIntoGround = 0f;
			}
		}
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, this.m_startLocalPosition + Vector3.down * this.m_distanceStompedIntoGround, 0.3f);
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_activated);
		ar.Serialize(ref this.m_distanceStompedIntoGround);
		ar.Serialize(ref this.m_remainingRiseDelayTime);
	}

	public bool IsSuspended { get; set; }

	public Vector3 Position
	{
		get
		{
			return this.m_transform.position;
		}
	}

	public bool CanBeChargeFlamed()
	{
		return false;
	}

	public bool CanBeChargeDashed()
	{
		return false;
	}

	public bool CanBeGrenaded()
	{
		return false;
	}

	public bool CanBeStomped()
	{
		return true;
	}

	public bool CanBeBashed()
	{
		return false;
	}

	public bool CanBeSpiritFlamed()
	{
		return false;
	}

	public bool IsStompBouncable()
	{
		return false;
	}

	public bool CanBeLevelUpBlasted()
	{
		return false;
	}

	public bool CountsTowardsSuperJumpAchievement()
	{
		return false;
	}

	public bool IsDead()
	{
		return false;
	}

	public int NumberOfStomps = 3;

	public float StompIntoGroundAmount = 0.1f;

	public float RisingDelay = 8f;

	public float RiseSpeed = 1f;

	public SoundProvider StompSound;

	public SoundProvider AllTheWayInSound;

	public ActionMethod AllTheWayInAction;

	private Vector3 m_startLocalPosition;

	private Transform m_transform;

	private float m_distanceStompedIntoGround;

	private float m_remainingRiseDelayTime;

	private bool m_activated;
}
