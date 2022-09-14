using System;
using Core;
using Game;
using UnityEngine;

public class Entity : SaveSerialize, IRespawnReciever, IFrustumOptimizable, ISuspendable
{
	public Entity()
	{
		this.IsSuspended = false;
	}

	public void OnSceneUnloaded(SceneRoot sceneRoot)
	{
		if (!Scenes.Manager.IsInsideActiveSceneBoundary(base.transform.position))
		{
			InstantiateUtility.Destroy(base.gameObject);
		}
	}

	public void ReclaimOwernship(RespawningPlaceholder placeholder)
	{
		base.transform.parent = placeholder.transform.parent;
		Events.Scheduler.OnSceneRootDisabled.Remove(new Action<SceneRoot>(this.OnSceneUnloaded));
		this.m_registeredToSceneRootDisabled = false;
	}

	public void FreeOwnership(RespawningPlaceholder placeholder)
	{
		base.transform.parent = null;
		Events.Scheduler.OnSceneRootDisabled.Add(new Action<SceneRoot>(this.OnSceneUnloaded));
		this.m_registeredToSceneRootDisabled = true;
	}

	public virtual bool CanBeOptimized()
	{
		return true;
	}

	public bool IsInWater
	{
		get
		{
			return WaterZone.PositionInWater(this.Position);
		}
	}

	public void Drown()
	{
		Damage damage = new Damage(1000f, Vector3.zero, this.Position, DamageType.Water, base.gameObject);
		this.DamageReciever.OnRecieveDamage(damage);
	}

	public bool IsOnScreen()
	{
		return UI.Cameras.Current == null || UI.Cameras.Current.IsOnScreen(base.transform.position);
	}

	public override void Awake()
	{
		SuspensionManager.Register(this);
		if (this.FrustrumOptimized)
		{
			CameraFrustumOptimizer.Register(this);
		}
		SceneRoot sceneRoot = SceneRoot.FindFromTransform(base.transform);
		if (sceneRoot != null)
		{
			this.SceneRootGUID = sceneRoot.MetaData.SceneMoonGuid;
		}
		base.Awake();
	}

	public void SetSceneRoot(MoonGuid sceneRoot)
	{
		this.SceneRootGUID = sceneRoot;
	}

	public override void OnDestroy()
	{
		SuspensionManager.Unregister(this);
		if (this.FrustrumOptimized)
		{
			CameraFrustumOptimizer.Unregister(this);
		}
		if (this.m_registeredToSceneRootDisabled)
		{
			Events.Scheduler.OnSceneRootDisabled.Remove(new Action<SceneRoot>(this.OnSceneUnloaded));
		}
		base.OnDestroy();
	}

	public override void Serialize(Archive ar)
	{
		this.Position = ar.Serialize(this.Position);
		this.Rotation = ar.Serialize(this.Rotation);
	}

	public void Start()
	{
		this.StartPosition = base.transform.position;
	}

	public void FixedUpdate()
	{
		if (this is Enemy)
			(this as Enemy).Animation.Animator.TextureAnimator.SpeedMultiplier = RandomizerBonusSkill.TimeScale(1f);
		if (this.FrustrumOptimized && !this.m_insideFrustum && this.CanBeOptimized())
		{
			base.gameObject.SetActive(false);
		}
	}

	public bool PlayerIsToLeft
	{
		get
		{
			return this.PositionToPlayerPosition.x < 0f;
		}
	}

	public Vector3 PlayerPosition
	{
		get
		{
			return Characters.Sein.PlatformBehaviour.PlatformMovement.Position;
		}
	}

	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public Quaternion Rotation
	{
		get
		{
			return base.transform.rotation;
		}
		set
		{
			base.transform.rotation = value;
		}
	}

	public Vector3 PositionToPlayerPosition
	{
		get
		{
			return base.transform.InverseTransformDirection(this.PlayerPosition - this.Position);
		}
	}

	public Vector3 StartPositionToPlayerPosition
	{
		get
		{
			return this.PlayerPosition - this.StartPosition;
		}
	}

	public bool LeftOfStartPosition
	{
		get
		{
			return this.StartPositionToPlayerPosition.x < 0f;
		}
	}

	public Vector3 PositionToStartPosition
	{
		get
		{
			return this.StartPosition - this.Position;
		}
	}

	public Vector3 StartPosition { get; set; }

	public bool AfterTime(float duration)
	{
		return this.Controller.StateMachine.CurrentStateTime > duration;
	}

	public bool IsSuspended { get; set; }

	public void OnTimedRespawn()
	{
	}

	public void RegisterRespawnDelegate(Action onRespawn)
	{
		this.DamageReciever.OnDeathEvent.Add(delegate(Damage a)
		{
			onRespawn();
		});
	}

	public void PlaySound(SoundSource sound)
	{
		if (sound != null)
		{
			sound.Play();
		}
	}

	public void StopSound(SoundSource sound)
	{
		if (sound != null)
		{
			sound.Stop();
		}
	}

	public void PlaySound(SoundProvider sound)
	{
		if (sound != null)
		{
			Sound.Play(sound.GetSound(null), this.Position, null);
		}
	}

	public void SpawnPrefab(PrefabSpawner prefabSpawner)
	{
		if (prefabSpawner != null)
		{
			prefabSpawner.Spawn(null);
		}
	}

	public void SpawnPrefab(GameObject prefab)
	{
		if (prefab != null)
		{
			InstantiateUtility.Instantiate(prefab, this.Position, base.transform.rotation);
		}
	}

	public void DestroyPrefab(PrefabSpawner prefabSpawner)
	{
		if (prefabSpawner != null)
		{
			prefabSpawner.DestroyInstance();
		}
	}

	public void ActivateDamageDealer()
	{
		this.DamageDealer.Activated = true;
	}

	public void DeactivateDamageDealer()
	{
		this.DamageDealer.Activated = false;
	}

	public void ActivateTargetting()
	{
		this.Targetting.Activated = true;
	}

	public void DeactivateTargetting()
	{
		this.Targetting.Activated = false;
	}

	public void OnFrustumEnter()
	{
		this.m_insideFrustum = true;
		if (!this.DamageReciever || !this.DamageReciever.NoHealthLeft)
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

	public Bounds Bounds
	{
		get
		{
			Vector3 size = new Vector3(this.BoundingBox.width, this.BoundingBox.height, 0f);
			Vector3 vector = base.transform.position;
			vector += new Vector3(this.BoundingBox.center.x, this.BoundingBox.center.y, 0f);
			return new Bounds(vector, size);
		}
	}

	public bool PlayerInsideSameScene()
	{
		RuntimeSceneMetaData currentScene = Scenes.Manager.CurrentScene;
		return currentScene != null && currentScene.SceneMoonGuid == this.SceneRootGUID;
	}

	public EntityController Controller;

	public EntityDamageReciever DamageReciever;

	public EntityDamageDealer DamageDealer;

	public EntityTargetting Targetting;

	protected MoonGuid SceneRootGUID;

	public Rect BoundingBox = new Rect
	{
		width = 4f,
		height = 4f,
		center = Vector2.zero
	};

	public bool FrustrumOptimized;

	private bool m_registeredToSceneRootDisabled;

	private bool m_insideFrustum = true;
}
