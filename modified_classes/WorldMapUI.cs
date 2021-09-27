using System;
using Game;
using UnityEngine;

public class WorldMapUI : MonoBehaviour
{
	public static bool IsReady
	{
		get
		{
			return WorldMapUI.Instance != null;
		}
	}

	public static bool UseCameraSettings
	{
		get
		{
			return !(WorldMapUI.Instance == null) && WorldMapUI.Instance.m_enabled;
		}
	}

	public void OnEnable()
	{
		this.m_enabled = true;
	}

	public void OnDisable()
	{
		this.m_enabled = false;
	}

	public static CameraSettings CameraSettings
	{
		get
		{
			if (WorldMapUI.Instance == null)
			{
				return null;
			}
			if (WorldMapUI.Instance.m_cameraSettings == null)
			{
				WorldMapUI.Instance.m_cameraSettings = new CameraSettings(WorldMapUI.Instance.CameraSettingsAsset, WorldMapUI.Instance.Fog);
			}
			return WorldMapUI.Instance.m_cameraSettings;
		}
	}

	public Transform FadeOutGroup
	{
		get
		{
			return this.CrossFade.transform;
		}
	}

	public void Awake()
	{
		WorldMapUI.Instance = this;
		base.transform.parent = GameMapUI.Instance.Group;
		base.transform.position = Vector3.zero;
		this.NavigationManager.OptionChangeCallback = (Action)Delegate.Combine(this.NavigationManager.OptionChangeCallback, new Action(this.OnMenuItemChange));
	}

	public void OnMenuItemChange()
	{
		if (this.m_ignoreNavigationMenuItemChange)
		{
			return;
		}
		WorldMapOverworldArea currentArea = this.CurrentArea;
		AreaMapUI.Instance.Navigation.ScrollPosition = currentArea.ScrollPosition;
		GameMapUI.Instance.CurrentHighlightedArea = GameWorld.Instance.FindRuntimeArea(currentArea.Area);
	}

	public void OnDestroy()
	{
		if (WorldMapUI.Instance == this)
		{
			UnityEngine.Object.DestroyObject(WorldMapUI.Instance);
		}
		this.NavigationManager.OptionChangeCallback = (Action)Delegate.Remove(this.NavigationManager.OptionChangeCallback, new Action(this.OnMenuItemChange));
	}

	public void Activate()
	{
		base.gameObject.SetActive(true);
		if (!GameMapUI.Instance.ShowingTeleporters)
		{
			this.ShowAreaSelection();
		}
		this.m_ignoreNavigationMenuItemChange = true;
		foreach (WorldMapOverworldArea worldMapOverworldArea in this.NavigationManager.GetComponentsInChildren<WorldMapOverworldArea>())
		{
			if (worldMapOverworldArea.Area == GameMapUI.Instance.CurrentHighlightedArea.Area)
			{
				this.NavigationManager.SetCurrentMenuItem(worldMapOverworldArea.GetComponent<CleverMenuItem>());
			}
		}
		this.m_ignoreNavigationMenuItemChange = false;
	}

	public void Deactivate()
	{
		base.gameObject.SetActive(false);
	}

	public float ZoomTime
	{
		get
		{
			return AreaMapUI.Instance.Navigation.ZoomTime;
		}
	}

	public Vector3 ScrollPosition
	{
		get
		{
			return AreaMapUI.Instance.Navigation.ScrollPosition;
		}
	}

	public Vector3 WorldToProjectedPosition(Vector3 position)
	{
		return WorldMapOverworldLogic.Instance.WorldToOverworld(position);
	}

	public Vector3 WorldToUIPosition(Vector3 position)
	{
		return this.WorldToScreenToUI(this.WorldToProjectedPosition(position));
	}

	public Vector3 ClosePosition
	{
		get
		{
			return this.WorldToProjectedPosition(this.ScrollPosition) + Vector3.back * this.CloseZoom;
		}
	}

	public Vector3 FarPosition
	{
		get
		{
			return Vector3.back * this.FullZoom + this.CameraOffset;
		}
	}

	public WorldMapOverworldArea CurrentArea
	{
		get
		{
			return this.NavigationManager.CurrentMenuItem.GetComponent<WorldMapOverworldArea>();
		}
	}

	public void UpdateCameraPosition()
	{
		if (!GameMapUI.Instance.ShowingObjective)
		{
			if (!GameMapUI.Instance.RevealingMap)
			{
				if (GameMapUI.Instance.ShowingTeleporters)
				{
					Vector3 b = GameMapUI.Instance.Teleporters.SelectedTeleporter.WorldProjectedPositon;
					b.z = -2f;
					b.x *= 0.3f;
					b.y *= 0.1f;
					b.y -= 3f;
					this.CameraOffset = Vector3.Lerp(this.CameraOffset, b, 0.03f);
				}
				else if (this.NavigationManager.gameObject.activeSelf)
				{
					Vector3 position = this.CurrentArea.transform.position;
					position.z = 0f;
					Vector3 b2 = position * 0.2f;
					this.CameraOffset = Vector3.Lerp(this.CameraOffset, b2, 0.03f);
				}
			}
		}
		Vector3 vector = this.FarPosition + Vector3.right * 0.3f * Mathf.Sin(6.2831855f * Time.time / 4.2f) + Vector3.up * 0.2f * Mathf.Cos(6.2831855f * Time.time / 7.3f);
		Vector3 closePosition = this.ClosePosition;
		float zoomTime = this.ZoomTime;
		Vector3 position2;
		position2.x = Mathf.Lerp(vector.x, closePosition.x, this.ZoomXYCurve.Evaluate(zoomTime));
		position2.y = Mathf.Lerp(vector.y, closePosition.y, this.ZoomXYCurve.Evaluate(zoomTime));
		position2.z = Mathf.Lerp(vector.z, closePosition.z, this.ZoomZCurve.Evaluate(zoomTime));
		this.Camera.transform.position = position2;
	}

	public void FixedUpdate()
	{
		if (!GameMapUI.Instance.IsVisible)
		{
			return;
		}
		this.NavigationManager.IsActive = GameMapTransitionManager.Instance.InWorldMapMode;
		this.UpdateCameraPosition();
		if (Characters.Sein)
		{
			this.PlayerMarker.position = this.WorldToUIPosition(Characters.Sein.Position);
		}
	}

	public Vector3 WorldToScreenToUI(Vector3 position)
	{
		Vector2 v = this.Camera.WorldToScreenPoint(position);
		Camera camera = UI.Cameras.System.GUICamera.Camera;
		Vector3 result = camera.ScreenToWorldPoint(v);
		result.z = 0f;
		return result;
	}

	public void ShowAreaSelection()
	{
		this.NavigationManager.gameObject.SetActive(true);
	}

	public void HideAreaSelection()
	{
		this.NavigationManager.gameObject.SetActive(false);
	}

	public static void Initialize()
	{
		if (WorldMapUI.m_isLoadingWorldMapScene)
		{
			WorldMapUI.m_cancelLoading = false;
		}
		else
		{
			WorldMapUI.m_isLoadingWorldMapScene = true;
			Application.LoadLevelAdditiveAsync("worldMapScene");
		}
	}

	public static void OnFinishedLoading(SceneRoot sceneRoot)
	{
		if (WorldMapUI.m_cancelLoading)
		{
			UnityEngine.Object.DestroyObject(sceneRoot.gameObject);
		}
		else
		{
			sceneRoot.EarlyStart();
			UnityEngine.Object.DestroyObject(sceneRoot.GetComponent<SaveSceneManager>());
			UnityEngine.Object.DestroyObject(sceneRoot.GetComponent<SceneSettingsComponent>());
			UnityEngine.Object.DestroyObject(sceneRoot);
			sceneRoot.gameObject.SetActive(true);
		}
		WorldMapUI.m_isLoadingWorldMapScene = false;
		WorldMapUI.m_cancelLoading = false;
	}

	public static void CancelLoading()
	{
		if (WorldMapUI.m_isLoadingWorldMapScene)
		{
			WorldMapUI.m_cancelLoading = true;
		}
	}

	public static WorldMapUI Instance;

	public Transform ProjectionPlane;

	public Transform PlayerMarker;

	public bool Activated;

	public float FullZoom = 20f;

	public float CloseZoom = 10f;

	public Camera Camera;

	public TransparencyAnimator CrossFade;

	public CleverMenuItemSelectionManager NavigationManager;

	public CameraSettingsAsset CameraSettingsAsset;

	public FogGradientController Fog;

	public AnimationCurve ZoomXYCurve;

	public AnimationCurve ZoomZCurve;

	public Vector3 CameraOffset;

	private CameraSettings m_cameraSettings;

	private bool m_enabled;

	private bool m_ignoreNavigationMenuItemChange;

	private static bool m_isLoadingWorldMapScene;

	private static bool m_cancelLoading;
}
