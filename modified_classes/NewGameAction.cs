using System;
using Game;

// Token: 0x020007D6 RID: 2006
public class NewGameAction : ActionMethod
{
	// Token: 0x06002B33 RID: 11059 RVA: 0x000040B3 File Offset: 0x000022B3
	public NewGameAction()
	{
	}

	// Token: 0x06002B34 RID: 11060
	public override void Perform(IContext context)
	{
		Game.Checkpoint.SaveGameData = new SaveGameData();
		try {
			Randomizer.initialize();
			Randomizer.showSeedInfo();
			Randomizer.JustSpawned = true;
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
