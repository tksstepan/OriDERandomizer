using System;
using Game;

// Token: 0x020007D3 RID: 2003
public class NewGameAction : ActionMethod
{
	// Token: 0x06002B2F RID: 11055 RVA: 0x00003D17 File Offset: 0x00001F17
	public NewGameAction()
	{
	}

	// Token: 0x06002B30 RID: 11056
	public override void Perform(IContext context)
	{
		Game.Checkpoint.SaveGameData = new SaveGameData();
		if (Randomizer.OpenMode)
		{
			Game.Checkpoint.SaveGameData.LoadCustomData(Randomizer.GladesData);
			Game.Checkpoint.SaveGameData.LoadCustomData(Randomizer.GinsoData);
			Game.Checkpoint.SaveGameData.LoadCustomData(Randomizer.ForlornData);
			Game.Checkpoint.SaveGameData.LoadCustomData(Randomizer.HoruData);
		}
		GameController.Instance.RequireInitialValues = true;
		GameStateMachine.Instance.SetToGame();
	}
}
