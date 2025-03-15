using System;
using Game;

public class NewGameAction : ActionMethod
{
	public override void Perform(IContext context)
	{
		Game.Checkpoint.SaveGameData = new SaveGameData();
		try {
			Randomizer.initialize();
			Randomizer.showSeedInfo();
			RandomizerStatsManager.Activate();
		} catch(Exception e) {
			Randomizer.LogError("New Game Action: " + e.Message);
		}
		if (Randomizer.OpenMode)
		{
			Game.Checkpoint.SaveGameData.LoadCustomData(Randomizer.GinsoData);
			Game.Checkpoint.SaveGameData.LoadCustomData(Randomizer.ForlornData);
			Game.Checkpoint.SaveGameData.LoadCustomData(Randomizer.HoruData);
		}
		if (Randomizer.OpenWorld)
		{
			Game.Checkpoint.SaveGameData.LoadCustomData(Randomizer.GladesData);
			Game.Checkpoint.SaveGameData.LoadCustomData(Randomizer.ValleyStompDoorData);
		}
		GameController.Instance.RequireInitialValues = true;
		GameStateMachine.Instance.SetToGame();
	}
}
