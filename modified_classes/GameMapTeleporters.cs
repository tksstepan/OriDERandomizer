using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class GameMapTeleporters : MonoBehaviour
{
	public List<GameMapTeleporter> Teleporters
	{
		get
		{
			return TeleporterController.Instance.Teleporters;
		}
	}

	[ContextMenu("Show teleporters")]
	public void ShowTeleporters()
	{
		foreach (GameMapTeleporter gameMapTeleporter in this.Teleporters)
		{
			if (gameMapTeleporter.Activated)
			{
				gameMapTeleporter.Show();
			}
		}
	}

	public void HideTeleporters()
	{
		foreach (GameMapTeleporter gameMapTeleporter in this.Teleporters)
		{
			gameMapTeleporter.Hide();
		}
	}

	private void ChangeSelection(int index)
	{
		if (this.SelectedIndex == index)
		{
			return;
		}
		this.SetIndex(index);
		if (this.SwitchTeleporterSelectionSound)
		{
			Sound.Play(this.SwitchTeleporterSelectionSound.GetSound(null), base.transform.position, null);
		}
		if (GameMapTransitionManager.Instance.InWorldMapMode)
		{
			AreaMapUI.Instance.Navigation.ScrollPosition = this.Teleporters[index].WorldPosition;
		}
	}

	private int TeleporterUnderMouse()
	{
		int result = -1;
        if (this.Teleporters.Count <= 12) {
            // There are no custom teleporters, so use the default behaviour.
            if (GameMapTransitionManager.Instance.InWorldMapMode)
            {
                for (int i = 0; i < this.Teleporters.Count; i++)
                {
                    GameMapTeleporter gameMapTeleporter = this.Teleporters[i];
                    if (gameMapTeleporter.Activated && Vector3.Distance(Core.Input.CursorPositionUI, gameMapTeleporter.WorldMapIconPosition) < 1f)
                    {
                        result = i;
                    }
                }
            }
            if (GameMapTransitionManager.Instance.InAreaMapMode)
            {
                for (int j = 0; j < this.Teleporters.Count; j++)
                {
                    GameMapTeleporter gameMapTeleporter2 = this.Teleporters[j];
                    if (gameMapTeleporter2.Activated && Vector3.Distance(Core.Input.CursorPositionUI, gameMapTeleporter2.AreaMapIconPosition) < 1f)
                    {
                        result = j;
                    }
                }
            }
        } 
        else
        {
            // There are custom teleporters, so use our mouse algorithm.
            // The default algorithm only finds the *last* teleporter within 1f, we find the closest.
            // The gameMapTeleporter.WorldMapIconPosition is centered left and right, but is at the
            // top of the teleporter icon, so we remove roughly half the height to centre it.
			float minimum = 1f;
			for (int k = 0; k < this.Teleporters.Count; k++)
			{
				GameMapTeleporter gameMapTeleporter3 = this.Teleporters[k];
				if (gameMapTeleporter3.Activated)
				{
					Vector2 teleporterCenter;
					if (GameMapTransitionManager.Instance.InWorldMapMode)
					{
						teleporterCenter = new Vector2(gameMapTeleporter3.WorldMapIconPosition.x, gameMapTeleporter3.WorldMapIconPosition.y - 0.125f);
					}
					else
					{
						teleporterCenter = new Vector2(gameMapTeleporter3.AreaMapIconPosition.x, gameMapTeleporter3.AreaMapIconPosition.y - 0.125f);
					}
					float distance = Vector3.Distance(Core.Input.CursorPositionUI, teleporterCenter);
					if (distance < minimum)
					{
						result = k;
						minimum = distance;
					}
				}
			}
        }
		return result;
	}

	private void AdvanceWorldMap()
	{
		this.m_flyBackTime = 0f;
		if (Core.Input.Axis.magnitude < 0.5f)
		{
			this.m_released = true;
		}
		if (Core.Input.CursorMoved)
		{
			int num = this.TeleporterUnderMouse();
			if (num != -1)
			{
				this.ChangeSelection(num);
			}
		}
		if (Core.Input.Axis.magnitude > 0.5f && this.m_released)
		{
			Vector2 normalized = Core.Input.Axis.normalized;
			Vector2 worldMapIconPosition = this.SelectedTeleporter.WorldMapIconPosition;
			int num2 = -1;
			float num3 = float.MaxValue;
			for (int i = 0; i < this.Teleporters.Count; i++)
			{
				GameMapTeleporter gameMapTeleporter = this.Teleporters[i];
				if (gameMapTeleporter.Activated)
				{
					Vector2 vector = gameMapTeleporter.WorldMapIconPosition - worldMapIconPosition;
					if (vector.magnitude < num3 && Vector3.Dot(vector.normalized, normalized) > 0.707f)
					{
						num3 = vector.magnitude;
						num2 = i;
					}
				}
			}
			if (num2 != -1)
			{
				this.m_released = false;
				this.ChangeSelection(num2);
			}
		}
	}

	private void AdvanceAreaMap()
	{
		if (Core.Input.CursorMoved)
		{
			int num = this.TeleporterUnderMouse();
			if (num != -1)
			{
				this.ChangeSelection(num);
			}
		}
		if (AreaMapUI.Instance.Navigation.ScrollingSensitivityCurve.Evaluate(Core.Input.Axis.magnitude) > 0f)
		{
			this.m_flyBackTime = 1.1f;
			this.m_previousScrollPosition = AreaMapUI.Instance.Navigation.ScrollPosition;
			float num2 = 9f;
			int index = this.SelectedIndex;
			for (int i = 0; i < this.Teleporters.Count; i++)
			{
				GameMapTeleporter gameMapTeleporter = this.Teleporters[i];
				if (gameMapTeleporter.Activated)
				{
					float magnitude = gameMapTeleporter.AreaMapIconPosition.magnitude;
					if (magnitude < num2)
					{
						index = i;
						num2 = magnitude;
					}
				}
			}
			this.ChangeSelection(index);
		}
		else
		{
			this.m_flyBackTime -= Time.deltaTime;
			if (this.m_flyBackTime < 1f && this.m_flyBackTime > 0f)
			{
				AreaMapUI.Instance.Navigation.ScrollPosition = Vector2.Lerp(this.m_previousScrollPosition, this.Teleporters[this.SelectedIndex].WorldPosition, 1f - Mathf.SmoothStep(0f, 1f, this.m_flyBackTime));
			}
		}
	}

	public void Advance()
	{
		if (!GameMapUI.Instance.ShowingTeleporters)
		{
			return;
		}
		foreach (GameMapTeleporter gameMapTeleporter in this.Teleporters)
		{
			gameMapTeleporter.Update();
		}
		if (GameMapTransitionManager.Instance.InWorldMapMode)
		{
			this.AdvanceWorldMap();
		}
		if (GameMapTransitionManager.Instance.InAreaMapMode)
		{
			this.AdvanceAreaMap();
		}
		if (Core.Input.LeftClick.OnPressed)
		{
			this.m_clickedPosition = Core.Input.CursorPositionUI;
		}
		bool flag = Core.Input.LeftClick.OnReleased && Vector2.Distance(Core.Input.CursorPositionUI, this.m_clickedPosition) < 0.01f && this.TeleporterUnderMouse() != -1;
		if (Core.Input.ActionButtonA.OnPressed || flag)
		{
			UI.Menu.HideMenuScreen(false);
			if (this.SelectTeleporterSound)
			{
				Sound.Play(this.SelectTeleporterSound.GetSound(null), base.transform.position, null);
			}
			TeleporterController.BeginTeleportation(this.SelectedTeleporter);
		}
	}

	public void OnDisable()
	{
		this.HideTeleporters();
		if (GameMapUI.Instance.ShowingTeleporters)
		{
			TeleporterController.OnClose();
		}
	}

	public GameMapTeleporter SelectedTeleporter
	{
		get
		{
			return this.Teleporters[this.SelectedIndex];
		}
	}

	public void Select(string identifier)
	{
		int num = this.Teleporters.FindIndex((GameMapTeleporter a) => a.Identifier == identifier);
		if (num != -1)
		{
			this.SetIndex(num);
		}
	}

	public void SetIndex(int index)
	{
		this.SelectedTeleporter.Dehighlight();
		this.SelectedIndex = index;
		this.SelectedTeleporter.Highlight();
		GameWorldArea area = GameWorld.Instance.FindAreaFromPosition(this.SelectedTeleporter.WorldPosition);
		GameMapUI.Instance.CurrentHighlightedArea = GameWorld.Instance.FindRuntimeArea(area);
	}

	public SoundProvider SelectTeleporterSound;

	public SoundProvider SwitchTeleporterSelectionSound;

	public SoundProvider StartTeleportingSound;

	public SoundProvider ReachDestinationTeleporterSound;

	public SoundProvider OpenWindowSound;

	public SoundProvider CloseWindowSound;

	public int SelectedIndex;

	private bool m_released = true;

	private Vector2 m_previousScrollPosition;

	private float m_flyBackTime;

	private Vector2 m_clickedPosition;
}
