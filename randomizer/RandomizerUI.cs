using System;
using Core;
using Game;
using UnityEngine;

public class RandomizerUI : MonoBehaviour
{
	public static void Initialize()
	{
		RandomizerUI.Instance = new GameObject("randomizerUI").AddComponent<RandomizerUI>();
	}

	void OnGUI()
	{
		GUIStyle betaStyle = new GUIStyle();
		betaStyle.fontStyle = FontStyle.Bold;
		betaStyle.fontSize = 32;
		betaStyle.alignment = TextAnchor.MiddleLeft;
		betaStyle.normal.textColor = Color.white;
		GUI.Label(new Rect(0f, 0f, 70f, 36f), "BETA", betaStyle);

		if (DebugMenuB.DebugControlsEnabled && Characters.Sein != null && Characters.Sein.Active)
		{
			GUIStyle infoStyle = new GUIStyle();
			infoStyle.fontSize = 16;
			infoStyle.alignment = TextAnchor.LowerLeft;
			infoStyle.normal.textColor = Color.white;
			Camera camera = UI.Cameras.Current.Camera;
			Vector2 cursorPosition = Core.Input.CursorPosition;
			Vector2 cursorWorldPos = camera.ViewportToWorldPoint(new Vector3(cursorPosition.x, cursorPosition.y, -camera.transform.position.z));
			string text = string.Format("Ori (World) X: {0} / Y: {1}\nCursor (World) X {2} / Y: {3}", new object[]
			{
				Characters.Sein.Position.x,
				Characters.Sein.Position.y,
				cursorWorldPos.x,
				cursorWorldPos.y
			});
			GUI.Label(new Rect(4f, GameSettings.Instance.Resolution.y - 54f, 200f, 50f), text, infoStyle);
		}
	}

	public static RandomizerUI Instance;
}