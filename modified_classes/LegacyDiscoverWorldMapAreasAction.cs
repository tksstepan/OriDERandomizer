using System;
using System.Collections;
using Game;
using UnityEngine;

[Category("World Map")]
public class LegacyDiscoverWorldMapAreasAction : ActionMethod
{
	public override void Perform(IContext context)
	{
		base.StartCoroutine(this.ShowWorldMap());
	}

	public IEnumerator ShowWorldMap()
	{
		yield return new WaitForFixedUpdate();
		GameMapUI.Instance.SetRevealingMap();
		RuntimeGameWorldArea currentArea = World.CurrentArea;
		if (currentArea != null)
		{
			currentArea.DiscoverAllAreas();
			AreaMapCanvas canvas = AreaMapUI.Instance.FindCanvas(currentArea.Area);
			canvas.UpdateAreaMaskTextureB();
			AreaMapUI.Instance.Navigation.UpdateScrollLimits();
			GameMapUI.Instance.SetNormal();
			AreaMapUI.Instance.IconManager.ShowAreaIcons();
			base.StartCoroutine(this.ReleaseTexture(canvas));
		}
		if (this.OnClosedAction)
		{
			this.OnClosedAction.Perform(null);
		}
		yield break;
	}

	public IEnumerator ReleaseTexture(AreaMapCanvas canvas)
	{
		yield return new WaitForSeconds(1f);
		canvas.ReleaseAreaMaskTextureB();
		yield break;
	}

	public ActionMethod OnClosedAction;

	public float FadeDelay;

	public float MoveDuration = 1f;

	public float FadeDuration;

	public SoundProvider RevealSound;

	public Transform RevealPosition;
}
