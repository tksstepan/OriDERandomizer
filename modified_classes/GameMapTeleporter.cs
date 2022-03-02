using System;
using UnityEngine;

[Serializable]
public class GameMapTeleporter
{
	public GameMapTeleporter(SceneMetaData.Teleporter teleporter, SceneMetaData sceneMetaData)
	{
		this.Identifier = teleporter.Identifier;
		this.WorldPosition = teleporter.SceneLocalPosition + sceneMetaData.RootPosition;
	}

	public void Show()
	{
		AreaMapUI instance = AreaMapUI.Instance;
		if (this.m_worldMapIconGameObject)
		{
			this.m_worldMapIconGameObject.SetActive(true);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(instance.TeleportPrefab);
			this.m_worldMapIconTransform = gameObject.transform;
			this.m_worldMapIconGameObject = this.m_worldMapIconTransform.gameObject;
			this.m_worldMapIconHighlightAnimator = this.m_worldMapIconGameObject.transform.FindChild("highlight").GetComponentInChildren<TransparencyAnimator>();
			this.m_worldMapIconTransform.position = WorldMapUI.Instance.WorldToUIPosition(this.WorldPosition);
			this.m_worldMapIconTransform.parent = WorldMapUI.Instance.FadeOutGroup;
			TransparencyAnimator.Register(this.m_worldMapIconTransform);
		}
		if (this.m_areaMapIconGameObject)
		{
			this.m_areaMapIconGameObject.SetActive(true);
		}
		else
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(instance.TeleportPrefab);
			this.m_areaMapIconTransform = gameObject2.transform;
			this.m_areaMapIconGameObject = this.m_areaMapIconTransform.gameObject;
			this.m_areaMapIconHighlightAnimator = this.m_areaMapIconGameObject.transform.FindChild("highlight").GetComponentInChildren<TransparencyAnimator>();
			this.m_areaMapIconTransform.position = AreaMapUI.Instance.Navigation.WorldToMapPosition(this.WorldPosition + Vector3.up * 4f);
			this.m_areaMapIconTransform.parent = AreaMapUI.Instance.FadeOutGroup;
			TransparencyAnimator.Register(this.m_areaMapIconTransform);
		}
	}

	public void Update()
	{
		if (this.m_worldMapIconTransform)
		{
			this.m_worldMapIconTransform.position = WorldMapUI.Instance.WorldToUIPosition(this.WorldPosition);
		}
		if (this.m_areaMapIconTransform)
		{
			this.m_areaMapIconTransform.position = AreaMapUI.Instance.Navigation.WorldToMapPosition(this.WorldPosition + Vector3.up * 4f);
		}
	}

	public Vector2 WorldMapIconPosition
	{
		get
		{
			return this.m_worldMapIconTransform.position;
		}
	}

	public Vector2 AreaMapIconPosition
	{
		get
		{
			return this.m_areaMapIconTransform.position;
		}
	}

	public Vector2 WorldProjectedPositon
	{
		get
		{
			return WorldMapUI.Instance.WorldToProjectedPosition(this.WorldPosition);
		}
	}

	public RuntimeGameWorldArea Area
	{
		get
		{
			return GameWorld.Instance.FindRuntimeArea(GameWorld.Instance.FindAreaFromPosition(this.WorldPosition));
		}
	}

	public void Hide()
	{
		if (this.m_worldMapIconGameObject)
		{
			this.m_worldMapIconGameObject.SetActive(false);
		}
		if (this.m_areaMapIconGameObject)
		{
			this.m_areaMapIconGameObject.SetActive(false);
		}
	}

	public void Highlight()
	{
		if (this.m_worldMapIconHighlightAnimator)
		{
			this.m_worldMapIconHighlightAnimator.AnimatorDriver.ContinueForward();
		}
		if (this.m_areaMapIconHighlightAnimator)
		{
			this.m_areaMapIconHighlightAnimator.AnimatorDriver.ContinueForward();
		}
	}

	public void Dehighlight()
	{
		if (this.m_worldMapIconHighlightAnimator)
		{
			this.m_worldMapIconHighlightAnimator.AnimatorDriver.ContinueBackwards();
		}
		if (this.m_areaMapIconHighlightAnimator)
		{
			this.m_areaMapIconHighlightAnimator.AnimatorDriver.ContinueBackwards();
		}
	}

    public GameMapTeleporter(string name, float x, float y)
	{
		Vector3 worldPosition = new Vector3(x, y, 0f);
		GameMapTeleporter(name, worldPosition, false);
	}

	public GameMapTeleporter(string name, Vector3 position, bool activated)
	{
		this.Identifier = name;
		this.WorldPosition = position;
		this.Activated = activated;
		RandomizerMessageProvider randomizerMessageProvider = (RandomizerMessageProvider)ScriptableObject.CreateInstance(typeof(RandomizerMessageProvider));
		randomizerMessageProvider.SetMessage(name);
		this.Name = randomizerMessageProvider;
	}

	public void SetInfo(string name, Vector3 position, bool activated)
	{
		if (this.Identifier != name) {
			this.Identifier = name;
			RandomizerMessageProvider randomizerMessageProvider = (RandomizerMessageProvider)ScriptableObject.CreateInstance(typeof(RandomizerMessageProvider));
			randomizerMessageProvider.SetMessage(name);
			this.Name = randomizerMessageProvider;
		}
		this.WorldPosition = position;
		this.Activated = activated;
	}


	public string Identifier;

	public Vector3 WorldPosition;

	public bool Activated;

	public MessageProvider Name;

	private TransparencyAnimator m_worldMapIconHighlightAnimator;

	private Transform m_worldMapIconTransform;

	private GameObject m_worldMapIconGameObject;

	private TransparencyAnimator m_areaMapIconHighlightAnimator;

	private Transform m_areaMapIconTransform;

	private GameObject m_areaMapIconGameObject;
}
