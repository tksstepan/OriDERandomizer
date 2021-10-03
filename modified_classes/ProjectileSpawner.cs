using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class ProjectileSpawner : SaveSerialize, ISuspendable
{
	public Vector3 Position
	{
		get
		{
			return this.m_transform.position;
		}
	}

	public float TimeSinceLastShot { get; set; }

	public override void Awake()
	{
		this.TimeSinceLastShot = float.MaxValue;
		base.Awake();
		SuspensionManager.Register(this);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		SuspensionManager.Unregister(this);
	}

	public void Start()
	{
		this.m_timedTrigger = base.GetComponent<TimedTrigger>();
		if (this.m_timedTrigger != null)
		{
			this.trueTimedDuration = new float?(this.m_timedTrigger.Duration);
		}
		this.m_transform = base.transform;
	}

	private bool TimerPaused
	{
		get
		{
			return this.m_timedTrigger && this.m_timedTrigger.Paused;
		}
		set
		{
			if (this.m_timedTrigger)
			{
				this.m_timedTrigger.Paused = value;
			}
		}
	}

	public void OnDisable()
	{
		this.TimerPaused = false;
	}

	public void OnTimedTrigger()
	{
		this.SpawnProjectile();
	}

	public Projectile SpawnProjectile()
	{
		this.TimeSinceLastShot = 0f;
		GameObject gameObject = InstantiateUtility.Instantiate(this.Projectile) as GameObject;
		gameObject.transform.SetParentMaintainingLocalTransform(base.transform.root);
		this.m_lastProjectile = gameObject;
		gameObject.transform.position = base.transform.position;
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Speed = this.Speed;
		component.Direction = this.Direction;
		if (this.Direction == Vector3.zero)
		{
			component.Direction = base.transform.up;
		}
		component.Gravity = this.Gravity;
		if (this.Owner)
		{
			component.Owner = this.Owner;
		}
		if (this.SpawnSound)
		{
			Sound.Play(this.SpawnSound, base.transform.position, null, this.SpawnSoundVolume, null);
		}
		return component;
	}

	public void AimAt(Transform target)
	{
		this.Direction = (target.position - this.m_transform.position).normalized;
	}

	public override void Serialize(Archive ar)
	{
	}

	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		if (this.trueTimedDuration != null)
		{
			this.m_timedTrigger.Duration = this.trueTimedDuration.Value / RandomizerBonusSkill.TimeScale(1f);
		}
		if (InstantiateUtility.IsDestroyed(this.m_lastProjectile))
		{
			this.m_lastProjectile = null;
		}
		if (this.WaitForProjectileToBeDestroyed && !this.TimerPaused && this.m_lastProjectile != null)
		{
			this.TimerPaused = true;
		}
		if (this.WaitForProjectileToBeDestroyed && this.TimerPaused && this.m_lastProjectile == null)
		{
			this.TimerPaused = false;
		}
		this.TimeSinceLastShot += Time.deltaTime;
	}

	public bool IsSuspended { get; set; }

	public float Speed;

	public Vector3 Direction = Vector3.zero;

	public float Gravity;

	public GameObject Projectile;

	public List<Collider> CollidersToIgnore;

	public GameObject Owner;

	public bool WaitForProjectileToBeDestroyed;

	public AudioClip SpawnSound;

	public float SpawnSoundVolume = 0.3f;

	protected TimedTrigger m_timedTrigger;

	private GameObject m_lastProjectile;

	private Transform m_transform;

	private float? trueTimedDuration;
}
