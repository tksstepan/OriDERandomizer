using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

// Token: 0x020008E5 RID: 2277
public class ProjectileSpawner : SaveSerialize, ISuspendable
{
	// Token: 0x170007D7 RID: 2007
	// (get) Token: 0x060031E3 RID: 12771
	public Vector3 Position
	{
		get
		{
			return this.m_transform.position;
		}
	}

	// Token: 0x170007D8 RID: 2008
	// (get) Token: 0x060031E4 RID: 12772
	// (set) Token: 0x060031E5 RID: 12773
	public float TimeSinceLastShot { get; set; }

	// Token: 0x060031E6 RID: 12774
	public override void Awake()
	{
		this.TimeSinceLastShot = float.MaxValue;
		base.Awake();
		SuspensionManager.Register(this);
	}

	// Token: 0x060031E7 RID: 12775
	public override void OnDestroy()
	{
		base.OnDestroy();
		SuspensionManager.Unregister(this);
	}

	// Token: 0x060031E8 RID: 12776
	public void Start()
	{
		this.m_timedTrigger = base.GetComponent<TimedTrigger>();
		if (this.m_timedTrigger != null)
		{
			this.trueTimedDuration = new float?(this.m_timedTrigger.Duration);
		}
		this.m_transform = base.transform;
	}

	// Token: 0x170007D9 RID: 2009
	// (get) Token: 0x060031E9 RID: 12777
	// (set) Token: 0x060031EA RID: 12778
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

	// Token: 0x060031EB RID: 12779
	public void OnDisable()
	{
		this.TimerPaused = false;
	}

	// Token: 0x060031EC RID: 12780
	public void OnTimedTrigger()
	{
		this.SpawnProjectile();
	}

	// Token: 0x060031ED RID: 12781
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

	// Token: 0x060031EE RID: 12782
	public void AimAt(Transform target)
	{
		this.Direction = (target.position - this.m_transform.position).normalized;
	}

	// Token: 0x060031EF RID: 12783
	public override void Serialize(Archive ar)
	{
	}

	// Token: 0x060031F0 RID: 12784
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

	// Token: 0x170007DA RID: 2010
	// (get) Token: 0x060031F1 RID: 12785
	// (set) Token: 0x060031F2 RID: 12786
	public bool IsSuspended { get; set; }

	// Token: 0x04002D29 RID: 11561
	public float Speed;

	// Token: 0x04002D2A RID: 11562
	public Vector3 Direction = Vector3.zero;

	// Token: 0x04002D2B RID: 11563
	public float Gravity;

	// Token: 0x04002D2C RID: 11564
	public GameObject Projectile;

	// Token: 0x04002D2D RID: 11565
	public List<Collider> CollidersToIgnore;

	// Token: 0x04002D2E RID: 11566
	public GameObject Owner;

	// Token: 0x04002D2F RID: 11567
	public bool WaitForProjectileToBeDestroyed;

	// Token: 0x04002D30 RID: 11568
	public AudioClip SpawnSound;

	// Token: 0x04002D31 RID: 11569
	public float SpawnSoundVolume = 0.3f;

	// Token: 0x04002D32 RID: 11570
	protected TimedTrigger m_timedTrigger;

	// Token: 0x04002D33 RID: 11571
	private GameObject m_lastProjectile;

	// Token: 0x04002D34 RID: 11572
	private Transform m_transform;

	// Token: 0x040035B6 RID: 13750
	private float? trueTimedDuration;
}
