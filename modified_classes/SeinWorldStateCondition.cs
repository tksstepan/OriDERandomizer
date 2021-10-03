using System;
using Sein.World;
using UnityEngine;

public class SeinWorldStateCondition : Condition
{
    public override bool Validate(IContext context)
    {
        switch (this.State)
        {
        case WorldState.WaterPurified:
            if (this.overrideEvent == SeinWorldStateCondition.OverrideEvents.None)
            {
                return Events.WaterPurified == this.IsTrue;
            }
            if (this.overrideEvent == SeinWorldStateCondition.OverrideEvents.GinsoDoor)
            {
                return false;
            }
            if (this.overrideEvent == SeinWorldStateCondition.OverrideEvents.WaterEscapeExit)
            {
                this.surfaceColliders.SetActive(Events.WarmthReturned);
                this.blockingWall.SetActive(Events.WarmthReturned);
                if (Events.WarmthReturned)
                {
                    return Events.WarmthReturned == this.IsTrue;
                }
                return Events.WaterPurified == this.IsTrue;
            }
            else
            {
                if (this.overrideEvent == SeinWorldStateCondition.OverrideEvents.FinishEscapeTrigger)
                {
                    return Events.WarmthReturned;
                }
                return this.overrideEvent != SeinWorldStateCondition.OverrideEvents.False && Events.WaterPurified == this.IsTrue;
            }
            break;
        case WorldState.GumoFree:
            return Events.GumoFree == this.IsTrue;
        case WorldState.SpiritTreeReached:
            return Events.SpiritTreeReached == this.IsTrue;
        case WorldState.GinsoTreeKey:
            return Keys.GinsoTree == this.IsTrue;
        case WorldState.WindRestored:
            return Randomizer.WindRestored() == this.IsTrue;
        case WorldState.GravityActivated:
            return Events.GravityActivated == this.IsTrue;
        case WorldState.MistLifted:
            return Events.MistLifted == this.IsTrue;
        case WorldState.ForlornRuinsKey:
            return Keys.ForlornRuins == this.IsTrue;
        case WorldState.MountHoruKey:
            return Keys.MountHoru == this.IsTrue;
        case WorldState.WarmthReturned:
            return Events.WarmthReturned == this.IsTrue;
        case WorldState.DarknessLifted:
            return Events.DarknessLifted == this.IsTrue;
        }
        return false;
    }

    private void Awake()
    {
        if (base.gameObject.name == "openingGinsoTree")
        {
            this.overrideEvent = SeinWorldStateCondition.OverrideEvents.GinsoDoor;
            return;
        }
        if (base.gameObject.name == "artAfter")
        {
            Transform transform4 = base.transform.FindChild("artAfter");
            Transform transform2 = transform4.FindChild("surfaceColliders");
            Transform transform3 = transform4.FindChild("blockingWall");
            if (transform2 && transform3)
            {
                this.overrideEvent = SeinWorldStateCondition.OverrideEvents.WaterEscapeExit;
                this.surfaceColliders = transform2.gameObject;
                this.blockingWall = transform3.gameObject;
                return;
            }
        }
        else
        {
            if (base.gameObject.name == "artBefore" && base.transform.parent && base.transform.parent.name == "ginsoTreeWaterRisingEnd")
            {
                this.overrideEvent = SeinWorldStateCondition.OverrideEvents.WaterEscapeExit;
                return;
            }
            if (base.name == "objectiveSetupTrigger" && base.transform.parent && base.transform.parent.name == "*objectiveSetup" && base.transform.parent.parent && base.transform.parent.parent.name == "thornfeltSwampActTwoStart")
            {
                this.overrideEvent = SeinWorldStateCondition.OverrideEvents.False;
                return;
            }
            if (base.name == "musiczones" && (base.transform.Find("musicZoneDuringRising") != null || (base.transform.parent && base.transform.parent.name == "ginsoTreeWaterRisingEnd")))
            {
                this.overrideEvent = SeinWorldStateCondition.OverrideEvents.FinishEscapeTrigger;
                return;
            }
            if (base.name == "activator")
            {
                if (base.transform.childCount == 1 && base.transform.GetChild(0).name == "container" && base.transform.GetChild(0).childCount == 1 && base.transform.GetChild(0).GetChild(0).name == "musicZoneHeartWaterRising")
                {
                    this.overrideEvent = SeinWorldStateCondition.OverrideEvents.FinishEscapeTrigger;
                    return;
                }
                if (base.transform.childCount == 1 && base.transform.GetChild(0).name == "musicZoneWaterCleansed")
                {
                    this.overrideEvent = SeinWorldStateCondition.OverrideEvents.FinishEscapeTrigger;
                    return;
                }
                if (base.transform.parent && base.transform.parent.name == "restoringHeartWaterRising")
                {
                    this.overrideEvent = SeinWorldStateCondition.OverrideEvents.FinishEscapeTrigger;
                }
            }
        }
    }

    public WorldState State;

    public bool IsTrue = true;

    private SeinWorldStateCondition.OverrideEvents overrideEvent;

    private GameObject surfaceColliders;

    private GameObject blockingWall;

    private enum OverrideEvents
    {
        None,
        GinsoDoor,
        WaterEscapeExit,
        FinishEscapeTrigger,
        False
    }
}
