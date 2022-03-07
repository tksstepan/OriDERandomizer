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
			if (this.Name.GetType() == typeof(RandomizerMessageProvider))
			{
				Renderer[] componentsInChildren = this.m_worldMapIconGameObject.GetComponentsInChildren<Renderer>();
				int[] multiplicative = new int[] {0, 10, 11, 12};
				int[] others = new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
				foreach (int index in multiplicative)
				{
					Color originalColor = componentsInChildren[index].material.color;
					Color newColor = new Color(RandomizerSettings.Customization.WarpTeleporterColor.Value.r * originalColor.r, 
						RandomizerSettings.Customization.WarpTeleporterColor.Value.g * originalColor.g, 
						RandomizerSettings.Customization.WarpTeleporterColor.Value.b * originalColor.b, 
						originalColor.a);
					componentsInChildren[index].material.color = newColor;
				}
				foreach (int index2 in others)
				{
					Color originalColor2 = componentsInChildren[index2].material.color;
					Color newColor2 = new Color(RandomizerSettings.Customization.WarpTeleporterColor.Value.r, 
						RandomizerSettings.Customization.WarpTeleporterColor.Value.g, 
						RandomizerSettings.Customization.WarpTeleporterColor.Value.b, 
						originalColor2.a);
					componentsInChildren[index2].material.color = newColor2;
				}
			}
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
			if (this.Name.GetType() == typeof(RandomizerMessageProvider))
			{
				Renderer[] componentsInChildren2 = this.m_areaMapIconGameObject.GetComponentsInChildren<Renderer>();
				int[] multiplicative2 = new int[] {0, 10, 11, 12};
				int[] others2 = new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
				foreach (int index3 in multiplicative2)
				{
					Color originalColor3 = componentsInChildren2[index3].material.color;
					Color newColor3 = new Color(RandomizerSettings.Customization.WarpTeleporterColor.Value.r * originalColor3.r, 
						RandomizerSettings.Customization.WarpTeleporterColor.Value.g * originalColor3.g, 
						RandomizerSettings.Customization.WarpTeleporterColor.Value.b * originalColor3.b, 
						originalColor3.a);
					componentsInChildren2[index3].material.color = newColor3;
				}
				foreach (int index4 in others2)
				{
					Color originalColor4 = componentsInChildren2[index4].material.color;
					Color newColor4 = new Color(RandomizerSettings.Customization.WarpTeleporterColor.Value.r, 
						RandomizerSettings.Customization.WarpTeleporterColor.Value.g, 
						RandomizerSettings.Customization.WarpTeleporterColor.Value.b, 
						originalColor4.a);
					componentsInChildren2[index4].material.color = newColor4;
				}
			}
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
		this.Identifier = name;
		this.WorldPosition = new Vector3(x, y, 0f);
		this.Activated = false;
		RandomizerMessageProvider randomizerMessageProvider = (RandomizerMessageProvider)ScriptableObject.CreateInstance(typeof(RandomizerMessageProvider));
		randomizerMessageProvider.SetMessage(name);
		this.Name = randomizerMessageProvider;
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
