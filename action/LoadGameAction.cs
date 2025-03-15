using System;

public class LoadGameAction : ActionMethod
{
	public override void Perform(IContext context)
	{
		SaveSlotBackupsManager.ResetBackupDelay();
		InstantLoadScenesController.Instance.LockFinishingLoading = true;
		RandomizerStatsManager.Active = true;
		GameStateMachine.Instance.SetToGame();
		if (!GameController.Instance.SaveGameController.PerformLoad())
		{
		}
		RandomizerBonusSkill.DelayDrainUpdate();
	}
}
