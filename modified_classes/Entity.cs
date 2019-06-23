using System;
using Core;
using Game;
using UnityEngine;

// Token: 0x0200041E RID: 1054
public class Entity : SaveSerialize, IRespawnReciever, IFrustumOptimizable, ISuspendable
{
	// Token: 0x0600176F RID: 5999 RVA: 0x0007B72C File Offset: 0x0007992C
	public Entity()
	{
		this.IsSuspended = false;
	}

	// Token: 0x06001770 RID: 6000 RVA: 0x00014B4E File Offset: 0x00012D4E
	public void OnSceneUnloaded(SceneRoot sceneRoot)
	{
		if (!Scenes.Manager.IsInsideActiveSceneBoundary(base.transform.position))
		{
			InstantiateUtility.Destroy(base.gameObject);
		}
	}

	// Token: 0x06001771 RID: 6001 RVA: 0x00014B75 File Offset: 0x00012D75
	public void ReclaimOwernship(RespawningPlaceholder placeholder)
	{
		base.transform.parent = placeholder.transform.parent;
		Events.Scheduler.OnSceneRootDisabled.Remove(new Action<SceneRoot>(this.OnSceneUnloaded));
		this.m_registeredToSceneRootDisabled = false;
	}

	// Token: 0x06001772 RID: 6002 RVA: 0x00014BAF File Offset: 0x00012DAF
	public void FreeOwnership(RespawningPlaceholder placeholder)
	{
		base.transform.parent = null;
		Events.Scheduler.OnSceneRootDisabled.Add(new Action<SceneRoot>(this.OnSceneUnloaded));
		this.m_registeredToSceneRootDisabled = true;
	}

	// Token: 0x06001773 RID: 6003 RVA: 0x00004AE6 File Offset: 0x00002CE6
	public virtual bool CanBeOptimized()
	{
		return true;
	}

	// Token: 0x170003FB RID: 1019
	// (get) Token: 0x06001774 RID: 6004 RVA: 0x00014BDF File Offset: 0x00012DDF
	public bool IsInWater
	{
		get
		{
			return WaterZone.PositionInWater(this.Position);
		}
	}

	// Token: 0x06001775 RID: 6005 RVA: 0x0007B780 File Offset: 0x00079980
	public void Drown()
	{
		Damage damage = new Damage(1000f, Vector3.zero, this.Position, DamageType.Water, base.gameObject);
		this.DamageReciever.OnRecieveDamage(damage);
	}

	// Token: 0x06001776 RID: 6006 RVA: 0x00014BEC File Offset: 0x00012DEC
	public bool IsOnScreen()
	{
		return UI.Cameras.Current == null || UI.Cameras.Current.IsOnScreen(base.transform.position);
	}

	// Token: 0x06001777 RID: 6007 RVA: 0x0007B7BC File Offset: 0x000799BC
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

	// Token: 0x06001778 RID: 6008 RVA: 0x00014C15 File Offset: 0x00012E15
	public void SetSceneRoot(MoonGuid sceneRoot)
	{
		this.SceneRootGUID = sceneRoot;
	}

	// Token: 0x06001779 RID: 6009 RVA: 0x0007B810 File Offset: 0x00079A10
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

	// Token: 0x0600177A RID: 6010 RVA: 0x00014C1E File Offset: 0x00012E1E
	public override void Serialize(Archive ar)
	{
		this.Position = ar.Serialize(this.Position);
		this.Rotation = ar.Serialize(this.Rotation);
	}

	// Token: 0x0600177B RID: 6011 RVA: 0x00014C44 File Offset: 0x00012E44
	public void Start()
	{
		this.StartPosition = base.transform.position;
	}

	// Token: 0x0600177C RID: 6012
	public void FixedUpdate()
	{
		if (this is Enemy)
			(this as Enemy).Animation.Animator.TextureAnimator.SpeedMultiplier = RandomizerBonusSkill.TimeScale(1f);
		if (this.FrustrumOptimized && !this.m_insideFrustum && this.CanBeOptimized())
		{
			base.gameObject.SetActive(false);
		}
	}

	// Token: 0x170003FC RID: 1020
	// (get) Token: 0x0600177D RID: 6013 RVA: 0x0007B860 File Offset: 0x00079A60
	public bool PlayerIsToLeft
	{
		get
		{
			return this.PositionToPlayerPosition.x < 0f;
		}
	}

	// Token: 0x170003FD RID: 1021
	// (get) Token: 0x0600177E RID: 6014 RVA: 0x00014C86 File Offset: 0x00012E86
	public Vector3 PlayerPosition
	{
		get
		{
			return Characters.Sein.PlatformBehaviour.PlatformMovement.Position;
		}
	}

	// Token: 0x170003FE RID: 1022
	// (get) Token: 0x0600177F RID: 6015 RVA: 0x00002AF6 File Offset: 0x00000CF6
	// (set) Token: 0x06001780 RID: 6016 RVA: 0x00002B03 File Offset: 0x00000D03
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

	// Token: 0x170003FF RID: 1023
	// (get) Token: 0x06001781 RID: 6017 RVA: 0x00014C9C File Offset: 0x00012E9C
	// (set) Token: 0x06001782 RID: 6018 RVA: 0x00014CA9 File Offset: 0x00012EA9
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

	// Token: 0x17000400 RID: 1024
	// (get) Token: 0x06001783 RID: 6019 RVA: 0x00014CB7 File Offset: 0x00012EB7
	public Vector3 PositionToPlayerPosition
	{
		get
		{
			return base.transform.InverseTransformDirection(this.PlayerPosition - this.Position);
		}
	}

	// Token: 0x17000401 RID: 1025
	// (get) Token: 0x06001784 RID: 6020 RVA: 0x00014CD5 File Offset: 0x00012ED5
	public Vector3 StartPositionToPlayerPosition
	{
		get
		{
			return this.PlayerPosition - this.StartPosition;
		}
	}

	// Token: 0x17000402 RID: 1026
	// (get) Token: 0x06001785 RID: 6021 RVA: 0x0007B884 File Offset: 0x00079A84
	public bool LeftOfStartPosition
	{
		get
		{
			return this.StartPositionToPlayerPosition.x < 0f;
		}
	}

	// Token: 0x17000403 RID: 1027
	// (get) Token: 0x06001786 RID: 6022 RVA: 0x00014CE8 File Offset: 0x00012EE8
	public Vector3 PositionToStartPosition
	{
		get
		{
			return this.StartPosition - this.Position;
		}
	}

	// Token: 0x17000404 RID: 1028
	// (get) Token: 0x06001787 RID: 6023 RVA: 0x00014CFB File Offset: 0x00012EFB
	// (set) Token: 0x06001788 RID: 6024 RVA: 0x00014D03 File Offset: 0x00012F03
	public Vector3 StartPosition { get; set; }

	// Token: 0x06001789 RID: 6025 RVA: 0x00014242 File Offset: 0x00012442
	public bool AfterTime(float duration)
	{
		return this.Controller.StateMachine.CurrentStateTime > duration;
	}

	// Token: 0x17000405 RID: 1029
	// (get) Token: 0x0600178A RID: 6026 RVA: 0x00014D0C File Offset: 0x00012F0C
	// (set) Token: 0x0600178B RID: 6027 RVA: 0x00014D14 File Offset: 0x00012F14
	public bool IsSuspended { get; set; }

	// Token: 0x0600178C RID: 6028 RVA: 0x000030E7 File Offset: 0x000012E7
	public void OnTimedRespawn()
	{
	}

	// Token: 0x0600178D RID: 6029 RVA: 0x0007B8A8 File Offset: 0x00079AA8
	public void RegisterRespawnDelegate(Action onRespawn)
	{
		this.DamageReciever.OnDeathEvent.Add(delegate(Damage a)
		{
			onRespawn();
		});
	}

	// Token: 0x0600178E RID: 6030 RVA: 0x00014D1D File Offset: 0x00012F1D
	public void PlaySound(SoundSource sound)
	{
		if (sound != null)
		{
			sound.Play();
		}
	}

	// Token: 0x0600178F RID: 6031 RVA: 0x00014D31 File Offset: 0x00012F31
	public void StopSound(SoundSource sound)
	{
		if (sound != null)
		{
			sound.Stop();
		}
	}

	// Token: 0x06001790 RID: 6032 RVA: 0x00014D45 File Offset: 0x00012F45
	public void PlaySound(SoundProvider sound)
	{
		if (sound != null)
		{
			Sound.Play(sound.GetSound(null), this.Position, null);
		}
	}

	// Token: 0x06001791 RID: 6033 RVA: 0x00014D67 File Offset: 0x00012F67
	public void SpawnPrefab(PrefabSpawner prefabSpawner)
	{
		if (prefabSpawner != null)
		{
			prefabSpawner.Spawn(null);
		}
	}

	// Token: 0x06001792 RID: 6034 RVA: 0x00014D7D File Offset: 0x00012F7D
	public void SpawnPrefab(GameObject prefab)
	{
		if (prefab != null)
		{
			InstantiateUtility.Instantiate(prefab, this.Position, base.transform.rotation);
		}
	}

	// Token: 0x06001793 RID: 6035 RVA: 0x00014DA3 File Offset: 0x00012FA3
	public void DestroyPrefab(PrefabSpawner prefabSpawner)
	{
		if (prefabSpawner != null)
		{
			prefabSpawner.DestroyInstance();
		}
	}

	// Token: 0x06001794 RID: 6036 RVA: 0x00014DB7 File Offset: 0x00012FB7
	public void ActivateDamageDealer()
	{
		this.DamageDealer.Activated = true;
	}

	// Token: 0x06001795 RID: 6037 RVA: 0x00014DC5 File Offset: 0x00012FC5
	public void DeactivateDamageDealer()
	{
		this.DamageDealer.Activated = false;
	}

	// Token: 0x06001796 RID: 6038 RVA: 0x00014DD3 File Offset: 0x00012FD3
	public void ActivateTargetting()
	{
		this.Targetting.Activated = true;
	}

	// Token: 0x06001797 RID: 6039 RVA: 0x00014DE1 File Offset: 0x00012FE1
	public void DeactivateTargetting()
	{
		this.Targetting.Activated = false;
	}

	// Token: 0x06001798 RID: 6040 RVA: 0x00014DEF File Offset: 0x00012FEF
	public void OnFrustumEnter()
	{
		this.m_insideFrustum = true;
		if (!this.DamageReciever || !this.DamageReciever.NoHealthLeft)
		{
			base.gameObject.SetActive(true);
		}
	}

	// Token: 0x06001799 RID: 6041 RVA: 0x00014E24 File Offset: 0x00013024
	public void OnFrustumExit()
	{
		this.m_insideFrustum = false;
	}

	// Token: 0x17000406 RID: 1030
	// (get) Token: 0x0600179A RID: 6042 RVA: 0x00014E2D File Offset: 0x0001302D
	public bool InsideFrustum
	{
		get
		{
			return this.m_insideFrustum;
		}
	}

	// Token: 0x17000407 RID: 1031
	// (get) Token: 0x0600179B RID: 6043 RVA: 0x0007B8E0 File Offset: 0x00079AE0
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

	// Token: 0x0600179C RID: 6044 RVA: 0x0007B95C File Offset: 0x00079B5C
	public bool PlayerInsideSameScene()
	{
		RuntimeSceneMetaData currentScene = Scenes.Manager.CurrentScene;
		return currentScene != null && currentScene.SceneMoonGuid == this.SceneRootGUID;
	}

	// Token: 0x040014EA RID: 5354
	public EntityController Controller;

	// Token: 0x040014EB RID: 5355
	public EntityDamageReciever DamageReciever;

	// Token: 0x040014EC RID: 5356
	public EntityDamageDealer DamageDealer;

	// Token: 0x040014ED RID: 5357
	public EntityTargetting Targetting;

	// Token: 0x040014EE RID: 5358
	protected MoonGuid SceneRootGUID;

	// Token: 0x040014EF RID: 5359
	public Rect BoundingBox = new Rect
	{
		width = 4f,
		height = 4f,
		center = Vector2.zero
	};

	// Token: 0x040014F0 RID: 5360
	public bool FrustrumOptimized;

	// Token: 0x040014F1 RID: 5361
	private bool m_registeredToSceneRootDisabled;

	// Token: 0x040014F2 RID: 5362
	private bool m_insideFrustum = true;
}
