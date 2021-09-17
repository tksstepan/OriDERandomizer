using System;

public class ReturnToTitleScreenAction : ActionMethod
{
	public override void Perform(IContext context)
	{
		try
		{
			Randomizer.Returning = false;
			Randomizer.Warping = 0;
			RandomizerStatsManager.OnReturnToMenu();
		}
		catch (Exception e)
		{
			Randomizer.LogError("ReturnToTitleScreenAction: " + e.Message);
		}
		GameController.Instance.RestartGame();
	}
}
