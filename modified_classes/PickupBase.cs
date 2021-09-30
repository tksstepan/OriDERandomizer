using System;
using Core;
using Game;
using UnityEngine;

public abstract class PickupBase : SaveSerialize, IFrustumOptimizable, IPooled, IDynamicGraphicHierarchy
{
	public void OnValidate()
	{
		this.m_onKillRecievers = base.GetComponentsInChildren(typeof(IKillReciever));
		if (this.DestroyTarget == null)
		{
			this.DestroyTarget = base.gameObject;
		}
		this.m_transform = base.transform;
	}

	public void OnPoolSpawned()
	{
		this.OnCollectedEvent = delegate()
		{
		};
		this.IsCollected = false;
		this.m_currentTime = 0f;
	}

	public override void Awake()
	{
		base.Awake();
		this.m_bounds = new Bounds(base.transform.position, Vector3.one * 4f);
	}

	public void FixedUpdate()
	{
		if (this.FrustrumOptimized && !this.m_insideFrustum)
		{
			base.gameObject.SetActive(false);
			return;
		}

		if (!this.IsCollected && RandomizerLocationManager.IsPickupCollected(this.MoonGuid))
		{
			this.IsCollected = true;

			if (this.OnCollectedAction != null)
			{
				this.OnCollectedAction.PerformInstantly(null);
			}

			this.OnCollectedEvent();

			if (this.DestroyOnCollect)
			{
				InstantiateUtility.Destroy(this.DestroyTarget);
			}
			else
			{
				base.gameObject.SetActive(false);
			}
		}

		this.m_currentTime += Time.deltaTime;
		if (this.m_currentTime < this.DelayBeforeCollectable)
		{
			return;
		}
		if (!this.IsCollected && Characters.Sein && Vector3.Distance(this.m_transform.position, Characters.Sein.Position) < this.Radius)
		{
			this.OnCollectorCandidateTouch(Characters.Sein.gameObject);
		}
	}

	public abstract void OnCollectorCandidateTouch(GameObject collector);

	public void SpawnCollectedEffect()
	{
		if (this.CollectedEffect)
		{
			InstantiateUtility.Instantiate(this.CollectedEffect, this.m_transform.position, Quaternion.identity);
		}
	}

	public virtual void Collected()
	{
		this.IsCollected = true;
		this.SpawnCollectedEffect();
		if (this.CollectedSoundProvider != null)
		{
			Sound.Play(this.CollectedSoundProvider.GetSound(null), this.m_transform.position, null);
		}
		for (int i = 0; i < this.m_onKillRecievers.Length; i++)
		{
			if (this.m_onKillRecievers[i])
			{
				((IKillReciever)this.m_onKillRecievers[i]).OnKill();
			}
		}
		if (this.OnCollectedAction != null)
		{
			this.OnCollectedAction.Perform(null);
		}
		this.OnCollectedEvent();
		if (this.DestroyOnCollect)
		{
			InstantiateUtility.Destroy(this.DestroyTarget);
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_currentTime);
		ar.Serialize(ref this.IsCollected);
		if (ar.Reading)
		{
			base.gameObject.SetActive(!this.IsCollected);
		}
	}

	public Bounds Bounds
	{
		get
		{
			this.m_bounds.center = this.m_transform.position;
			return this.m_bounds;
		}
	}

	public void OnFrustumEnter()
	{
		this.m_insideFrustum = true;
		if (!this.IsCollected)
		{
			base.gameObject.SetActive(true);
		}
	}

	public void OnFrustumExit()
	{
		this.m_insideFrustum = false;
	}

	public bool InsideFrustum
	{
		get
		{
			return this.m_insideFrustum;
		}
	}

	public bool IsCollected;

	public SoundProvider CollectedSoundProvider;

	public Action OnCollectedEvent = delegate()
	{
	};

	public ActionMethod OnCollectedAction;

	public float DelayBeforeCollectable;

	public bool DestroyOnCollect;

	public GameObject DestroyTarget;

	public GameObject CollectedEffect;

	public float Radius = 2f;

	public bool FrustrumOptimized;

	[HideInInspector]
	[SerializeField]
	private Component[] m_onKillRecievers;

	[HideInInspector]
	[SerializeField]
	private Transform m_transform;

	private float m_currentTime;

	private Bounds m_bounds;

	private bool m_insideFrustum = true;
}
