using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Game;
using UnityEngine;

public class AreaMapUI : MonoBehaviour, ISuspendable
{
	public GameObject PlayerPositionMarker { get; set; }

	public GameObject SoulFlamePositionMarker { get; set; }

	public AreaMapDebugNavigation DebugNavigation { get; set; }

	public AreaMapNavigation Navigation { get; set; }

	public AreaMapIconManager IconManager { get; set; }

	public Transform FadeOutGroup
	{
		get
		{
			return this.FadeOutAnimator.transform;
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	public void ResetMaps()
	{
		foreach (AreaMapCanvas areaMapCanvas in this.Canvases)
		{
			areaMapCanvas.ResetMap();
		}
		foreach (AreaMapCanvasOverlay areaMapCanvasOverlay in base.GetComponentsInChildren<AreaMapCanvasOverlay>(true))
		{
			areaMapCanvasOverlay.ApplyMasks();
		}
	}

	public void Awake()
	{
		AreaMapUI.Instance = this;
		this.DebugNavigation = base.GetComponent<AreaMapDebugNavigation>();
		this.Navigation = base.GetComponent<AreaMapNavigation>();
		this.IconManager = base.GetComponent<AreaMapIconManager>();
		SuspensionManager.Register(this);
		this.AreaMapLegend.HideSilently();
		if (this.PlayerPositionMarker == null)
		{
			this.PlayerPositionMarker = UnityEngine.Object.Instantiate<GameObject>(this.PlayerPositionMarkerPrefab);
			this.PlayerPositionMarker.transform.parent = this.FadeOutGroup;
			TransparencyAnimator.Register(this.PlayerPositionMarker.transform);
		}
		if (this.SoulFlamePositionMarker == null)
		{
			this.SoulFlamePositionMarker = UnityEngine.Object.Instantiate<GameObject>(this.SoulFlamePositionMarkerPrefab);
			this.SoulFlamePositionMarker.transform.parent = this.FadeOutGroup;
			TransparencyAnimator.Register(this.SoulFlamePositionMarker.transform);
		}
	}

	public void OnDestroy()
	{
		SuspensionManager.Unregister(this);
		AreaMapUI.Instance = null;
	}

	public AreaMapCanvas FindCanvas(GameWorldArea area)
	{
		return this.Canvases.FirstOrDefault((AreaMapCanvas canvas) => canvas.Area == area);
	}

	public void Init()
	{
		this.ResetMaps();
		this.IconManager.ShowAreaIcons();
		this.Navigation.Advance();
		this.Navigation.UpdateScrollLimits();
		this.PlayerPositionOffset = Vector2.zero;
		this.Navigation.Init();
	}

	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		if (!GameMapUI.Instance.IsVisible)
		{
			return;
		}
		this.Navigation.Advance();
		this.DebugNavigation.Advance();
		this.UpdatePlayerPositionMarker();
		this.UpdateSoulFlamePositionMarker();
		this.UpdateCurrentArea();
		if (!GameMapUI.Instance.ShowingObjective)
		{
			this.ObjectiveText.SetMessage(new MessageDescriptor(string.Concat(new object[]
			{
				"#",
				this.ObjectiveMessageProvider,
				"#: ",
				RandomizerText.GetObjectiveText()
			})));
			this.ObjectiveText.gameObject.SetActive(true);
		}
		else
		{
			this.ObjectiveText.gameObject.SetActive(false);
		}
		if (GameMapTransitionManager.Instance.InAreaMapMode && Core.Input.Legend.OnPressed)
		{
			this.AreaMapLegend.Toggle();
		}
	}

	public void UpdateCurrentArea()
	{
		Vector2 scrollPosition = this.Navigation.ScrollPosition;
		foreach (RuntimeGameWorldArea runtimeGameWorldArea in GameWorld.Instance.RuntimeAreas)
		{
			if ((runtimeGameWorldArea.AreaDiscovered || this.DebugNavigation.UndiscoveredMapVisible) && runtimeGameWorldArea.Area.BoundaryCage.FindFaceAtPositionFaster(scrollPosition) != null)
			{
				if (GameMapUI.Instance.CurrentHighlightedArea != runtimeGameWorldArea && this.ChangeSelectedAreaSound)
				{
					Sound.Play(this.ChangeSelectedAreaSound.GetSound(null), base.transform.position, null);
				}
				GameMapUI.Instance.CurrentHighlightedArea = runtimeGameWorldArea;
				break;
			}
		}
	}

	public Vector3 PlayerMarkerWorldPosition
	{
		get
		{
			Transform target = UI.Cameras.Current.Target;
			return target.position + this.PlayerPositionOffset + Vector3.up;
		}
	}

	public Vector3 SoulFlameMarkerWorldPosition
	{
		get
		{
			return Characters.Sein.SoulFlame.SoulFlamePosition + this.PlayerPositionOffset + Vector3.up;
		}
	}

	private void UpdatePlayerPositionMarker()
	{
		if (this.PlayerPositionMarker)
		{
			this.PlayerPositionMarker.transform.localPosition = this.Navigation.WorldToMapPosition(this.PlayerMarkerWorldPosition);
		}
	}

	private void UpdateSoulFlamePositionMarker()
	{
		if (this.SoulFlamePositionMarker == null)
		{
			return;
		}
		if (Characters.Sein)
		{
			if (Characters.Sein.SoulFlame.SoulFlameExists)
			{
				this.SoulFlamePositionMarker.SetActive(true);
				this.SoulFlamePositionMarker.transform.localPosition = this.Navigation.WorldToMapPosition(this.SoulFlameMarkerWorldPosition);
			}
			else
			{
				this.SoulFlamePositionMarker.SetActive(false);
			}
		}
	}

	public bool IsSuspended { get; set; }

	public static AreaMapUI Instance;

	public List<AreaMapCanvas> Canvases = new List<AreaMapCanvas>();

	public GameObject PlayerPositionMarkerPrefab;

	public GameObject SoulFlamePositionMarkerPrefab;

	public GameObject TeleportPrefab;

	public GameObject ObjectivePrefab;

	public GameObject IconPrefab;

	public SoundProvider OpenSound;

	public SoundProvider CloseSound;

	public SoundProvider ChangeSelectedAreaSound;

	public MessageBox ObjectiveText;

	public TransparencyAnimator FadeOutAnimator;

	public AreaMapLegend AreaMapLegend;

	public MessageProvider ObjectiveMessageProvider;

	public MessageProvider CompletedMessageProvider;

	public Vector3 PlayerPositionOffset;
}
