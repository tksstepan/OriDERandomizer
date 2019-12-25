using System;

// Token: 0x02000153 RID: 339
public class LoadGameAction : ActionMethod
{
	// Token: 0x060007D0 RID: 2000 RVA: 0x000040B3 File Offset: 0x000022B3
	public LoadGameAction()
	{
	}

	// Token: 0x060007D1 RID: 2001 RVA: 0x0000879A File Offset: 0x0000699A
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
