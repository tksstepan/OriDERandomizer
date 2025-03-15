using System;
using System.Collections;
using Game;
using UnityEngine;

[Category("World Map")]
public class LegacyDiscoverWorldMapAreasAction : ActionMethod
{
	public override void Perform(IContext context)
	{
		this.m_isInstant = false;
		base.StartCoroutine(this.ShowWorldMap());
	}

	public override void PerformInstantly(IContext context)
	{
		this.m_isInstant = true;
		base.StartCoroutine(this.ShowWorldMap());
	}

	public IEnumerator ShowWorldMap()
	{
		yield return new WaitForFixedUpdate();
		RuntimeGameWorldArea currentArea = World.CurrentArea;
		if (currentArea != null)
		{
			currentArea.DiscoverAllAreas();
			AreaMapCanvas canvas = AreaMapUI.Instance.FindCanvas(currentArea.Area);
			canvas.UpdateAreaMaskTextureB();
			AreaMapUI.Instance.Navigation.UpdateScrollLimits();
			AreaMapUI.Instance.IconManager.ShowAreaIcons();
			base.StartCoroutine(this.ReleaseTexture(canvas));
		}
		if (this.OnClosedAction)
		{
			if (this.m_isInstant)
			{
				this.OnClosedAction.PerformInstantly(null);
			}
			else
			{
				this.OnClosedAction.Perform(null);
			}
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

	private bool m_isInstant;
}
