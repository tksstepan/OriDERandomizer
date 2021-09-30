using System;
using Game;
using UnityEngine;

public class CollectablePlaceholder : SaveSerialize, ISuspendable, IDynamicGraphic
{
	public override void Awake()
	{
		CollectablePlaceholder.All.Add(this);
		if (this.Prefab == null)
		{
			InstantiateUtility.Destroy(base.gameObject);
			return;
		}
		base.Awake();
		base.GetComponent<Renderer>().enabled = false;
		SuspensionManager.Register(this);
	}

	public override void OnDestroy()
	{
		SuspensionManager.Unregister(this);
		base.OnDestroy();
		CollectablePlaceholder.All.Remove(this);
	}

	public void Spawn()
	{
		if (!InstantiateUtility.IsDestroyed(this.m_instance))
		{
			InstantiateUtility.Destroy(this.m_instance);
			this.m_instance = null;
		}
		this.Instantiate();
	}

	public void OnCollect()
	{
		this.m_collected = true;
		this.m_remainingRespawnTime = this.RespawnTime;
	}

	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}

		if (!this.m_collected && RandomizerLocationManager.IsPickupCollected(this.MoonGuid))
		{
			// only do anything if the pickup isn't spawned; if it's spawned, PickupBase will mark itself collected
			if (this.m_instance == null)
			{
				this.OnCollect();
			}
		}

		if (this.m_remainingRespawnTime > 0f)
		{
			this.m_remainingRespawnTime -= Time.deltaTime;
			this.m_collected = false;
		}
		if (this.m_instance == null && !this.m_collected && UI.Cameras.Current.IsOnScreenPadded(base.transform.position, 5f))
		{
			this.Instantiate();
		}
	}

	public void Instantiate()
	{
		this.m_instance = (InstantiateUtility.Instantiate(this.Prefab, base.transform.position, base.transform.rotation) as GameObject);
		UberPoolManager.Instance.AddOnDestroyed(this.m_instance, delegate
		{
			this.m_instance = null;
		});

		PickupBase pickupBase = this.m_instance.GetComponentInChildren<PickupBase>();
		pickupBase.MoonGuid = this.MoonGuid;
		pickupBase.OnCollectedEvent = (Action)Delegate.Combine(pickupBase.OnCollectedEvent, new Action(this.OnCollect));

		if (this.m_instance.GetComponent<DestroyOnRestoreCheckpoint>() == null)
		{
			this.m_instance.AddComponent<DestroyOnRestoreCheckpoint>();
		}

		if (base.GetComponent<VisibleOnWorldMap>() && this.m_instance.GetComponent<VisibleOnWorldMap>())
		{
			this.m_instance.GetComponent<VisibleOnWorldMap>().MoonGuid = base.GetComponent<VisibleOnWorldMap>().MoonGuid;
		}

		this.m_instance.transform.parent = base.transform.parent;
		this.m_instance.name = this.Prefab.name;
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_collected);
		ar.Serialize(ref this.m_remainingRespawnTime);
	}

	public bool Collected
	{
		get
		{
			return this.m_collected;
		}
	}

	public bool IsSuspended { get; set; }

	public float RespawnTime;

	public GameObject Prefab;

	public static AllContainer<CollectablePlaceholder> All = new AllContainer<CollectablePlaceholder>();

	public bool UseDebug;

	private float m_remainingRespawnTime;

	private GameObject m_instance;

	private bool m_collected;
}
