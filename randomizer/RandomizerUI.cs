using System;
using UnityEngine;

public class RandomizerUI : MonoBehaviour
{
	void OnGUI()
	{
		GUIStyle guiStyle = new GUIStyle();
		guiStyle.fontStyle = FontStyle.Bold;
		guiStyle.fontSize = 32;
		guiStyle.alignment = TextAnchor.MiddleLeft;
		guiStyle.normal.textColor = Color.white;
		GUI.Label(new Rect(0f, 0f, 70f, 36f), "BETA", guiStyle);
	}
}