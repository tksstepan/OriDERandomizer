using System;
using Core;
using Game;
using UnityEngine;

public class AreaMapNavigation : MonoBehaviour
{
	public float ZoomTime
	{
		get
		{
			return GameMapTransitionManager.Instance.ZoomTime;
		}
	}

	public float Zoom
	{
		get
		{
			if (this.ZoomTime < 1f)
			{
				return 1f / Mathf.Lerp(50f / this.WorldMapZoomLevel, 50f / this.AreaMapZoomLevel, Mathf.SmoothStep(0f, 1f, this.ZoomTime));
			}
			return 1f / Mathf.Lerp(50f / this.AreaMapZoomLevel, 50f / this.AreaMapCloseZoomLevel, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(1f, 2f, this.ZoomTime)));
		}
	}

	public bool BoxIsInsideVisibleCanvas(Rect bound)
	{
		foreach (RuntimeGameWorldArea runtimeGameWorldArea in GameWorld.Instance.RuntimeAreas)
		{
			CageStructureTool cageStructureTool = runtimeGameWorldArea.Area.CageStructureTool;
			Rect[] facesAsRectangles = cageStructureTool.FacesAsRectangles;
			for (int i = 0; i < facesAsRectangles.Length; i++)
			{
				if (facesAsRectangles[i].Overlaps(bound))
				{
					int id = cageStructureTool.Faces[i].ID;
					if (runtimeGameWorldArea.FaceIsDiscoveredOrVisited(id))
					{
						return true;
					}
					if (this.m_areaMapUi.DebugNavigation.UndiscoveredMapVisible)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void Awake()
	{
		this.m_areaMapUi = base.GetComponent<AreaMapUI>();
		this.m_scrollLimits = base.GetComponentsInChildren<AreaMapScrollLimit>();
		this.AreaMapZoomLevel = 0.65f;
	}

	public void OnDisable()
	{
		this.ScrollSound.Stop();
	}

	public Bounds Bounds { get; set; }

	public Vector2 MapPlanePosition
	{
		get
		{
			return this.MapPivot.localPosition;
		}
		set
		{
			this.MapPivot.localPosition = value;
		}
	}

	public Vector2 MapPlaneSize
	{
		get
		{
			return this.MapPivot.localScale;
		}
		set
		{
			Vector3 localScale = this.MapPivot.localScale;
			localScale.x = value.x;
			localScale.y = value.y;
			this.MapPivot.localScale = localScale;
		}
	}

	public void Advance()
	{
		this.HandleMapScrolling();
		this.UpdatePlane();
		this.HandleObjectiveFocus();
		this.HandleRandomizerTooltip();
	}

	public void HandleObjectiveFocus()
	{
		bool isTransitioning = GameMapTransitionManager.Instance.IsTransitioning;
		this.m_focusTime = Mathf.Clamp01(this.m_focusTime - 2f * Time.deltaTime);
		if (this.m_focusTime > 0f)
		{
			this.ScrollPosition = Vector2.Lerp(this.m_fromPosition, this.m_toPosition, Mathf.SmoothStep(1f, 0f, this.m_focusTime));
			this.m_scrollTime = 0f;
		}
		if (!isTransitioning && this.m_focusTime == 0f && Core.Input.Focus.OnPressed && !Core.Input.Focus.Used)
		{
			Core.Input.Focus.Used = true;
			this.m_focusTime = 1f;
			this.m_fromPosition = this.ScrollPosition;
			if (Objectives.All.Count == 0)
			{
				this.m_toggleToPlayer = true;
			}
			this.m_toPosition = ((!this.m_toggleToPlayer) ? Objectives.All[0].Position : (Vector2)Characters.Current.Position);
			this.m_toggleToPlayer = !this.m_toggleToPlayer;
			if (this.FocusSound)
			{
				this.FocusSound.Play();
			}
		}
	}

	public void Init()
	{
		this.m_toggleToPlayer = false;
	}

	public void UpdatePlane()
	{
		this.MapPlaneSize = Vector2.one * this.Zoom;
		this.MapPivot.position = -this.ScrollPosition * this.Zoom;
	}

	public void CenterMapOnWorldPosition(Vector3 position)
	{
		this.m_scrollTime = 0f;
		this.ScrollPosition = position;
	}

	public Vector3 WorldToMapPosition(Vector2 position)
	{
		return this.MapPivot.TransformPoint(position);
	}

	public Vector3 MapToWorldPosition(Vector2 position)
	{
		Vector2 v = position - this.MapPlanePosition;
		v.x /= this.MapPlaneSize.x;
		v.y /= this.MapPlaneSize.y;
		return v;
	}

	private void HandleMapScrolling()
	{
		if (!GameMapTransitionManager.Instance.InAreaMapMode)
		{
			return;
		}
		if (GameMapUI.Instance.ShowingObjective || GameMapUI.Instance.RevealingMap)
		{
			return;
		}
		Vector2 vector = Vector2.zero;
		Vector2 cursorPositionUI = Core.Input.CursorPositionUI;
		cursorPositionUI.x /= this.MapPlaneSize.x;
		cursorPositionUI.y /= this.MapPlaneSize.y;
		if (Core.Input.LeftClick.OnPressed)
		{
			this.m_lastDragPosition = cursorPositionUI;
		}
		if (Core.Input.LeftClick.Pressed && Core.Input.CursorMoved)
		{
			vector += this.m_lastDragPosition - cursorPositionUI;
			this.m_lastDragPosition = cursorPositionUI;
		}
		if ((double)Core.Input.Axis.magnitude < 0.02)
		{
			this.m_scrollTime = 0f;
		}
		else
		{
			this.m_scrollTime = Mathf.Clamp01(this.m_scrollTime + Time.deltaTime * 4f);
			vector = Core.Input.Axis.normalized * this.ScrollingSensitivityCurve.Evaluate(Core.Input.Axis.magnitude) * this.m_scrollTime;
			vector *= Time.deltaTime * 150f / this.Zoom;
		}
		if (vector.magnitude > 0f)
		{
			if (vector.x < 0f && this.ScrollPosition.x <= this.m_scrollAreaLimit.xMin)
			{
				vector.x = 0f;
			}
			if (vector.x > 0f && this.ScrollPosition.x >= this.m_scrollAreaLimit.xMax)
			{
				vector.x = 0f;
			}
			if (vector.y < 0f && this.ScrollPosition.y <= this.m_scrollAreaLimit.yMin)
			{
				vector.y = 0f;
			}
			if (vector.y > 0f && this.ScrollPosition.y >= this.m_scrollAreaLimit.yMax)
			{
				vector.y = 0f;
			}
			this.ScrollPosition += vector;
			if (this.ScrollSound && !this.ScrollSound.IsPlaying && (double)vector.magnitude >= 0.3)
			{
				this.ScrollSound.Play();
				return;
			}
			if (this.ScrollSound && this.ScrollSound.IsPlaying && (double)vector.magnitude < 0.3)
			{
				this.ScrollSound.StopAndFadeOut(0f);
				return;
			}
		}
		else if (this.ScrollSound && this.ScrollSound.IsPlaying)
		{
			this.ScrollSound.StopAndFadeOut(0f);
		}
	}

	public Vector3 ConstrainWorldPositionByBounds(Vector3 worldPosition)
	{
		Bounds bounds = this.Bounds;
		worldPosition.x = Mathf.Clamp(worldPosition.x, bounds.min.x, bounds.max.x);
		worldPosition.y = Mathf.Clamp(worldPosition.y, bounds.min.y, bounds.max.y);
		return worldPosition;
	}

	public void UpdateScrollLimits()
	{
		bool flag = false;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		foreach (RuntimeGameWorldArea runtimeGameWorldArea in GameWorld.Instance.RuntimeAreas)
		{
			GameWorldArea area = runtimeGameWorldArea.Area;
			Rect[] facesAsRectangles = area.CageStructureTool.FacesAsRectangles;
			for (int i = 0; i < area.CageStructureTool.Faces.Count; i++)
			{
				Rect rect = facesAsRectangles[i];
				int id = area.CageStructureTool.Faces[i].ID;
				if (flag)
				{
					num = Mathf.Min(num, rect.xMin);
					num2 = Mathf.Min(num2, rect.yMin);
					num3 = Mathf.Max(num3, rect.xMax);
					num4 = Mathf.Max(num4, rect.yMax);
				}
				else
				{
					flag = true;
					num = rect.xMin;
					num2 = rect.yMin;
					num3 = rect.xMax;
					num4 = rect.yMax;
				}
			}
		}
		for (int j = 0; j < Objectives.All.Count; j++)
		{
			Vector2 position = Objectives.All[j].Position;
			num = Mathf.Min(num, position.x);
			num2 = Mathf.Min(num2, position.y);
			num3 = Mathf.Max(num3, position.x);
			num4 = Mathf.Max(num4, position.y);
		}
		if (Characters.Sein)
		{
			Vector2 vector = Characters.Sein.Position;
			num = Mathf.Min(num, vector.x);
			num2 = Mathf.Min(num2, vector.y);
			num3 = Mathf.Max(num3, vector.x);
			num4 = Mathf.Max(num4, vector.y);
		}
		this.m_scrollAreaLimit.xMin = num;
		this.m_scrollAreaLimit.yMin = num2;
		this.m_scrollAreaLimit.xMax = num3;
		this.m_scrollAreaLimit.yMax = num4;
	}

	public void HandleRandomizerTooltip()
	{
		Vector2 cursorPositionWorld = (Vector2)this.MapToWorldPosition(Core.Input.CursorPositionUI);
		RuntimeWorldMapIcon candidate = null;
		string candidateArea = null;
		float candidateDistance = Mathf.Infinity;

		foreach (RuntimeGameWorldArea runtimeArea in GameWorld.Instance.RuntimeAreas)
		{
			foreach (RuntimeWorldMapIcon runtimeIcon in runtimeArea.Icons)
			{
				if (!runtimeIcon.IsVisible(this.m_areaMapUi) || runtimeIcon.Icon == WorldMapIconType.Invisible)
				{
					continue;
				}

				if (Mathf.Abs(runtimeIcon.Position.x - cursorPositionWorld.x) > 12f || Mathf.Abs(runtimeIcon.Position.y - cursorPositionWorld.y) > 12f)
				{
					continue;
				}

				if (!RandomizerLocationManager.LocationsByWorldMapGuid.ContainsKey(runtimeIcon.Guid))
				{
					continue;
				}

				float distance = Vector2.Distance(runtimeIcon.Position, cursorPositionWorld);

				if (distance > 12f || distance > candidateDistance)
				{
					continue;
				}

				candidateDistance = distance;
				candidateArea = runtimeArea.Area.AreaIdentifier;
				candidate = runtimeIcon;
			}
		}

		if (candidate == null)
		{
			AreaMapUI.Instance.RandomizerTooltip.gameObject.SetActive(false);
			return;
		}

		Vector3 candidatePosition = this.WorldToMapPosition(candidate.Position);
		candidatePosition.y -= 0.35f;
		AreaMapUI.Instance.RandomizerTooltip.transform.position = candidatePosition;

		RandomizerLocationManager.Location pickupLocation = RandomizerLocationManager.LocationsByWorldMapGuid[candidate.Guid];
		AreaMapUI.Instance.RandomizerTooltip.OverrideText = pickupLocation.FriendlyName;
		AreaMapUI.Instance.RandomizerTooltip.gameObject.SetActive(true);

		if (DebugMenuB.DebugControlsEnabled && (MoonInput.GetKey(KeyCode.LeftShift) || MoonInput.GetKey(KeyCode.RightShift)) && Core.Input.RightClick.OnPressed)
		{
			candidate.Hide();
			RandomizerLocationManager.GivePickupByWorldMapGuid(candidate.Guid);
		}
	}

	public Transform MapPivot;

	public float AreaMapZoomLevel = 1f;

	public float WorldMapZoomLevel = 0.5f;

	public float AreaMapCloseZoomLevel = 3f;

	private float m_scrollTime;

	public AnimationCurve ScrollingSensitivityCurve;

	private Vector2 m_lastDragPosition;

	public SoundSource ScrollSound;

	public SoundSource FocusSound;

	public Vector2 ScrollPosition;

	private AreaMapUI m_areaMapUi;

	private AreaMapScrollLimit[] m_scrollLimits;

	private Vector2 m_fromPosition;

	private Vector2 m_toPosition;

	private float m_focusTime;

	private bool m_toggleToPlayer;

	private Rect m_scrollAreaLimit;
}
