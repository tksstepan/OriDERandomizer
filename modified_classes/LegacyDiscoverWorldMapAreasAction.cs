using System;
using System.Collections;
using UnityEngine;

[Category("World Map")]
public class LegacyDiscoverWorldMapAreasAction : ActionMethod
{
	public override void Perform(IContext context)
	{
	}

	public IEnumerator ShowWorldMap()
	{
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
