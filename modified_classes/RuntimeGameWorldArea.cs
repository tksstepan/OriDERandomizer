using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class RuntimeGameWorldArea
{
	public RuntimeGameWorldArea(GameWorldArea area)
	{
		this.Area = area;
		this.Initialize();
	}

	public Vector2 FindCenterPositionOnDiscoveredAreas()
	{
		int num = 0;
		Vector2 a = Vector2.zero;
		Rect[] facesAsRectangles = this.Area.CageStructureTool.FacesAsRectangles;
		for (int i = 0; i < this.Area.CageStructureTool.Faces.Count; i++)
		{
			CageStructureTool.Face face = this.Area.CageStructureTool.Faces[i];
			if (this.IsDiscovered(face))
			{
				Rect rect = facesAsRectangles[i];
				a += rect.center;
				num++;
			}
		}
		if (num > 0)
		{
			return a / (float)num;
		}
		return this.Area.BoundingRect.center;
	}

	public Vector2 FindCenterPositionOnUndiscoveredAreas()
	{
		int num = 0;
		Vector2 a = Vector2.zero;
		Rect[] facesAsRectangles = this.Area.CageStructureTool.FacesAsRectangles;
		for (int i = 0; i < this.Area.CageStructureTool.Faces.Count; i++)
		{
			CageStructureTool.Face face = this.Area.CageStructureTool.Faces[i];
			if (!this.IsDiscovered(face))
			{
				Rect rect = facesAsRectangles[i];
				a += rect.center;
				num++;
			}
		}
		if (num > 0)
		{
			return a / (float)num;
		}
		return this.Area.BoundingRect.center;
	}

	public void Initialize()
	{
		this.m_dirtyCompletionAmount = true;
		this.Icons.Clear();
		this.Icons.Capacity = this.Area.Icons.Count;
		foreach (GameWorldArea.WorldMapIcon icon in this.Area.Icons)
		{
			this.Icons.Add(new RuntimeWorldMapIcon(icon, this));
		}
		this.m_worldAreaStates.Clear();
	}

	public bool AreaDiscovered
	{
		get
		{
			return this.m_worldAreaStates.Count > 0;
		}
	}

	public float CompletionAmount
	{
		get
		{
			if (this.m_dirtyCompletionAmount)
			{
				this.m_dirtyCompletionAmount = false;
				this.UpdateCompletionAmount();
			}
			return this.m_completionAmount;
		}
	}

	public void DirtyCompletionAmount()
	{
		this.m_dirtyCompletionAmount = true;
	}

	public int CompletionPercentage
	{
		get
		{
			return Mathf.RoundToInt(this.CompletionAmount * 100f);
		}
	}

	private bool IconIsCompletionType(WorldMapIconType type)
	{
		switch (type)
		{
		case WorldMapIconType.HealthUpgrade:
		case WorldMapIconType.EnergyUpgrade:
		case WorldMapIconType.AbilityPoint:
		case WorldMapIconType.Experience:
		case WorldMapIconType.MapstonePickup:
			break;
		default:
			if (type != WorldMapIconType.Keystone)
			{
				return false;
			}
			break;
		}
		return true;
	}

	public void UpdateCompletionAmount()
	{
		int total = RandomizerStatsManager.PickupCounts[this.Area.AreaIdentifier];
		int collected = RandomizerStatsManager.GetObtainedPickupCount(this.Area.AreaIdentifier);

		if (RandomizerTrackedDataManager.Pedistals.ContainsKey(this.Area.AreaIdentifier))
		{
			total++;
			if (RandomizerTrackedDataManager.GetMapstone(RandomizerTrackedDataManager.Pedistals[this.Area.AreaIdentifier]))
				collected++;
		}

		this.m_completionAmount = (float)collected / (float)total;
	}

	public void VisitMapAreaAtPosition(Vector3 worldPosition)
	{
		Vector3 position = this.Area.CageStructureTool.transform.InverseTransformPoint(worldPosition);
		CageStructureTool.Face face = this.Area.CageStructureTool.FindFaceAtPositionFaster(position);
		if (face != null)
		{
			WorldMapAreaState worldMapAreaState;
			if (this.m_worldAreaStates.TryGetValue(face.ID, out worldMapAreaState))
			{
				if (worldMapAreaState != WorldMapAreaState.Visited)
				{
					this.m_dirtyCompletionAmount = true;
					this.m_worldAreaStates[face.ID] = WorldMapAreaState.Visited;
				}
			}
			else
			{
				this.m_dirtyCompletionAmount = true;
				this.m_worldAreaStates[face.ID] = WorldMapAreaState.Visited;
			}
		}
	}

	private bool HasSenseAbility
	{
		get
		{
			return Characters.Sein && Characters.Sein.PlayerAbilities.Sense.HasAbility;
		}
	}

	public bool IsHidden(Vector3 worldPosition)
	{
		if (this.HasSenseAbility)
		{
			return false;
		}
		Vector3 position = this.Area.CageStructureTool.transform.InverseTransformPoint(worldPosition);
		CageStructureTool.Face face = this.Area.CageStructureTool.FindFaceAtPositionFaster(position);
		return face == null || this.IsHidden(face);
	}

	public bool IsDiscovered(Vector3 worldPosition)
	{
		Vector3 position = this.Area.CageStructureTool.transform.InverseTransformPoint(worldPosition);
		CageStructureTool.Face face = this.Area.CageStructureTool.FindFaceAtPositionFaster(position);
		return face != null && this.IsDiscovered(face);
	}

	public bool IsHidden(CageStructureTool.Face face)
	{
		return !this.m_worldAreaStates.ContainsKey(face.ID) || this.m_worldAreaStates[face.ID] == WorldMapAreaState.Hidden;
	}

	public bool IsDiscovered(CageStructureTool.Face face)
	{
		return this.m_worldAreaStates.ContainsKey(face.ID) && this.m_worldAreaStates[face.ID] == WorldMapAreaState.Discovered;
	}

	public void Serialize(Archive ar)
	{
		if (ar.Reading)
		{
			this.m_dirtyCompletionAmount = true;
			this.m_worldAreaStates.Clear();
			int num = ar.Serialize(0);
			for (int i = 0; i < num; i++)
			{
				int key = ar.Serialize(0);
				WorldMapAreaState value = (WorldMapAreaState)ar.Serialize(0);
				this.m_worldAreaStates.Add(key, value);
			}
			num = ar.Serialize(0);
			for (int j = 0; j < num; j++)
			{
				MoonGuid guid = MoonGuid.Empty;
				guid.Serialize(ar);
				WorldMapIconType icon = (WorldMapIconType)ar.Serialize(0);
				RuntimeWorldMapIcon runtimeWorldMapIcon = this.Icons.Find((RuntimeWorldMapIcon a) => a.Guid == guid);
				if (runtimeWorldMapIcon != null)
				{
					runtimeWorldMapIcon.Icon = icon;
				}
			}
		}
		else
		{
			ar.Serialize(this.m_worldAreaStates.Count);
			foreach (KeyValuePair<int, WorldMapAreaState> keyValuePair in this.m_worldAreaStates)
			{
				ar.Serialize(keyValuePair.Key);
				ar.Serialize((int)keyValuePair.Value);
			}
			ar.Serialize(this.Icons.Count);
			foreach (RuntimeWorldMapIcon runtimeWorldMapIcon2 in this.Icons)
			{
				runtimeWorldMapIcon2.Guid.Serialize(ar);
				ar.Serialize((int)runtimeWorldMapIcon2.Icon);
			}
		}
	}

	public void DiscoverAllAreas()
	{
		CageStructureTool cageStructureTool = this.Area.CageStructureTool;
		foreach (CageStructureTool.Face face in cageStructureTool.Faces)
		{
			if (!this.m_worldAreaStates.ContainsKey(face.ID))
			{
				this.m_worldAreaStates[face.ID] = WorldMapAreaState.Discovered;
			}
		}
	}

	public void VisitAllAreas()
	{
		this.m_worldAreaStates.Clear();
		CageStructureTool cageStructureTool = this.Area.CageStructureTool;
		foreach (CageStructureTool.Face face in cageStructureTool.Faces)
		{
			this.m_worldAreaStates[face.ID] = WorldMapAreaState.Visited;
		}
	}

	public bool FaceIsDiscoveredOrVisited(int id)
	{
		WorldMapAreaState worldMapAreaState;
		return this.m_worldAreaStates.TryGetValue(id, out worldMapAreaState) && (worldMapAreaState == WorldMapAreaState.Discovered || worldMapAreaState == WorldMapAreaState.Visited);
	}

	public WorldMapAreaState GetFaceState(int id)
	{
		WorldMapAreaState result;
		if (this.m_worldAreaStates.TryGetValue(id, out result))
		{
		}
		return result;
	}

	public GameWorldArea Area;

	public List<RuntimeWorldMapIcon> Icons = new List<RuntimeWorldMapIcon>();

	private readonly Dictionary<int, WorldMapAreaState> m_worldAreaStates = new Dictionary<int, WorldMapAreaState>();

	private float m_completionAmount;

	private bool m_dirtyCompletionAmount;
}
