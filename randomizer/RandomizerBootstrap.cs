using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class RandomizerBootstrap
{
	public static void Initialize()
	{
		Events.Scheduler.OnSceneRootPreEnabled.Add(new Action<SceneRoot>(RandomizerBootstrap.BootstrapScenePreEnabled));
		Events.Scheduler.OnSceneRootEnabledAfterSerialize.Add(new Action<SceneRoot>(RandomizerBootstrap.BootstrapSceneAfterSerialize));
	}

	public static void FixedUpdate()
	{
		for (int i = 0; i < RandomizerBootstrap.s_bootstrappedScenesPreEnabled.Count;)
		{
			if (Core.Scenes.Manager.GetSceneManagerScene(RandomizerBootstrap.s_bootstrappedScenesPreEnabled[i]) != null)
			{
				i++;
			}
			else
			{
				RandomizerBootstrap.s_bootstrappedScenesPreEnabled.RemoveAt(i);
			}
		}
		for (int i = 0; i < RandomizerBootstrap.s_bootstrappedScenesAfterSerialize.Count;)
		{
			if (Core.Scenes.Manager.GetSceneManagerScene(RandomizerBootstrap.s_bootstrappedScenesAfterSerialize[i]) != null)
			{
				i++;
			}
			else
			{
				RandomizerBootstrap.s_bootstrappedScenesAfterSerialize.RemoveAt(i);
			}
		}
	}

	private static void BootstrapScenePreEnabled(SceneRoot sceneRoot)
	{
		if (RandomizerBootstrap.s_bootstrappedScenesPreEnabled.Contains(sceneRoot.name))
		{
			return;
		}

		if (RandomizerBootstrap.s_bootstrapPreEnabled.ContainsKey(sceneRoot.name))
		{
			RandomizerBootstrap.s_bootstrappedScenesPreEnabled.Add(sceneRoot.name);
			RandomizerBootstrap.s_bootstrapPreEnabled[sceneRoot.name].Invoke(sceneRoot);
		}
	}

	private static void BootstrapSceneAfterSerialize(SceneRoot sceneRoot)
	{
		if (RandomizerBootstrap.s_bootstrappedScenesAfterSerialize.Contains(sceneRoot.name))
		{
			return;
		}

		if (RandomizerBootstrap.s_bootstrapAfterSerialize.ContainsKey(sceneRoot.name))
		{
			RandomizerBootstrap.s_bootstrappedScenesAfterSerialize.Add(sceneRoot.name);
			RandomizerBootstrap.s_bootstrapAfterSerialize[sceneRoot.name].Invoke(sceneRoot);
			// We also need to process these functions after serialisation not caused by
			// scene loading, e.g. after death. So connect those hooks.
			sceneRoot.SaveSceneManager.sceneRoot = sceneRoot;
			sceneRoot.SaveSceneManager.bootstrapHook = RandomizerBootstrap.s_bootstrapAfterSerialize[sceneRoot.name];
		}
	}

	private static void TwiddleGuidAndSave(SceneRoot sceneRoot, GuidOwner owner)
	{
		// this is a horrendous hack but it works well enough to place new, serializable objects without maintaining a giant GUID store
		// MoonGuid is a v4 UUID, which is a 16-byte identifier with two special bit sequences:
		//		* in the 16-bit identifier spanning bytes 6-7, the four most significant bits must be 0100 (4), indicating a v4 UUID
		//		* in byte 8, the two most significant bits must be 10, a special sequence indicating UUID "variant 2"
		// we can ensure "unique" "UUIDs" by just abusing the four version bits, incrementing the "version" by 1 for each clone
		// this results in an invalid UUID but fortunately this literally only matters for differentiating saved object data
		MoonGuid originalGuid = owner.MoonGuid;
		owner.MoonGuid = new MoonGuid(originalGuid.A, originalGuid.B + 268435456, originalGuid.C, originalGuid.D);

		if (owner is SaveSerialize)
		{
			(owner as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		}
	}

	private static void SetGuidAndSave(SceneRoot sceneRoot, GuidOwner owner, MoonGuid guid)
	{
		owner.MoonGuid = guid;

		if (owner is SaveSerialize)
		{
			(owner as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		}
	}

	private static Transform CloneObject(SceneRoot sceneRoot, Transform obj, string name=null, bool sibling=true)
	{
		// temporarily fiddle with the original object's active status to prevent the clone from instantly awaking if it shouldn't
		bool originalActive = obj.gameObject.activeSelf;
		if (!obj.gameObject.activeInHierarchy)
		{
			obj.gameObject.SetActive(false);
		}

		Transform clone = UnityEngine.Object.Instantiate<Transform>(obj);
		if (name != null)
		{
			clone.gameObject.name = name;
		}
		if (sibling)
		{
			clone.parent = obj.parent;
		}

		// reinstate active status after the clone is part of the hierarchy
		obj.gameObject.SetActive(originalActive);
		clone.gameObject.SetActive(originalActive);

		foreach (GuidOwner owner in clone.gameObject.FindComponentsInChildren<GuidOwner>())
		{
			TwiddleGuidAndSave(sceneRoot, owner);
		}

		return clone;
	}

	private static void BootstrapTitleScreen(SceneRoot sceneRoot)
	{
		SaveSlotsItemsUI itemsUI = sceneRoot.transform.FindChild("ui").GetComponent<TitleScreenManager>().SaveSlotsScreen.ItemsUI;
		foreach (SaveSlotUI saveSlotUI in new UnityEngine.Object[2]{ itemsUI.SaveSlotUI, itemsUI.SaveSlotCompletedUI })
		{
			saveSlotUI.EasyTextMessageProvider = RandomizerText.DifficultyOverrides.Easy.NameOverride;
			saveSlotUI.NormalTextMessageProvider = RandomizerText.DifficultyOverrides.Normal.NameOverride;
			saveSlotUI.HardTextMessageProvider = RandomizerText.DifficultyOverrides.Hard.NameOverride;
			saveSlotUI.OneLifeTestMessageProvider = RandomizerText.DifficultyOverrides.OneLife.NameOverride;
		}

		CleverMenuItemSelectionManager difficultyManager = itemsUI.SaveSlotUI.DifficultyScreen.GetComponent<CleverMenuItemSelectionManager>();
		difficultyManager.MenuItems[0].GetComponentInChildren<MessageBox>(true).SetMessageProvider(RandomizerText.DifficultyOverrides.Easy.NameOverride);
		difficultyManager.MenuItems[0].GetComponentInChildren<CleverMenuItemTooltip>(true).Tooltip = RandomizerText.DifficultyOverrides.Easy.DescriptionOverride;
		difficultyManager.MenuItems[1].GetComponentInChildren<MessageBox>(true).SetMessageProvider(RandomizerText.DifficultyOverrides.Normal.NameOverride);
		difficultyManager.MenuItems[1].GetComponentInChildren<CleverMenuItemTooltip>(true).Tooltip = RandomizerText.DifficultyOverrides.Normal.DescriptionOverride;
		difficultyManager.MenuItems[2].GetComponentInChildren<MessageBox>(true).SetMessageProvider(RandomizerText.DifficultyOverrides.Hard.NameOverride);
		difficultyManager.MenuItems[2].GetComponentInChildren<CleverMenuItemTooltip>(true).Tooltip = RandomizerText.DifficultyOverrides.Hard.DescriptionOverride;
		difficultyManager.MenuItems[3].GetComponentInChildren<MessageBox>(true).SetMessageProvider(RandomizerText.DifficultyOverrides.OneLife.NameOverride);
		difficultyManager.MenuItems[3].GetComponentInChildren<CleverMenuItemTooltip>(true).Tooltip = RandomizerText.DifficultyOverrides.OneLife.DescriptionOverride;
		difficultyManager.Index = 0;

		switch (RandomizerSettings.Game.DefaultDifficulty.Value) {
			case RandomizerSettings.Difficulty.Relaxing:
				difficultyManager.Index = 0;
				break;
			case RandomizerSettings.Difficulty.Challenging:
				difficultyManager.Index = 1;
				break;
			case RandomizerSettings.Difficulty.Punishing:
				difficultyManager.Index = 2;
				break;
			case RandomizerSettings.Difficulty.OneLife:
				difficultyManager.Index = 3;
				break;
			default:
				Randomizer.log($"unknown default difficulty {RandomizerSettings.Game.DefaultDifficulty.Value}");
				difficultyManager.Index = 0;				
				break;
		}
	}

	private static void BootstrapBlackrootLanternRoom(SceneRoot sceneRoot)
	{
		Transform darkPlatforms = sceneRoot.transform.FindChild("*lightDarkPlatforms/darkPlatforms");
		Transform physicsManager = darkPlatforms.FindChild("physicsManager");

		// add difficulty condition to the platform container; use this to toggle all the platforms
		DifficultyCondition condition = darkPlatforms.gameObject.AddComponent<DifficultyCondition>();
		condition.Easy = true;
		condition.Normal = false;
		condition.Hard = false;
		condition.OneLife = false;
		
		// make a clone of the physics system and set it as a sibling of the original
		Transform alternateManager = CloneObject(sceneRoot, physicsManager, "physicsManagerRelaxed");

		// activate the clone if the condition is met, else activate the original
		ActivationBasedOnCondition activation = darkPlatforms.gameObject.AddComponent<ActivationBasedOnCondition>();
		activation.Condition = condition;
		activation.TargetTrue = alternateManager.gameObject;
		activation.TargetFalse = physicsManager.gameObject;

		// force the scene to re-validate since we've added things
		sceneRoot.OnValidate();

		// modify platforms in the clone
		foreach (Transform child in alternateManager)
		{
			if (child.position.x < 125f)
			{
				child.rotation *= Quaternion.Euler(0f, 0f, 60f);

				// "unrotate" the swaying movement of the platforms back to Y-axis alignment
				SinMovement componentInChildren = child.GetComponentInChildren<SinMovement>();
				SinMovement.Affect affectorY = componentInChildren.Affectors[0];

				Vector2 rotatedRange = MoonMath.Angle.Unrotate(new Vector2(0f, affectorY.Range / 2f), 60f);
				Vector2 rotatedRangeRandom = MoonMath.Angle.Unrotate(new Vector2(0f, affectorY.Range / 2f), 60f);

				affectorY.Range = rotatedRange.y;
				affectorY.RangeRandom = rotatedRangeRandom.y;

				SinMovement.Affect affectorX = new SinMovement.Affect();
				affectorX.Type = SinMovement.Affect.AffectType.X;
				affectorX.Offset = affectorY.Offset;
				affectorX.OffsetRandom = affectorY.OffsetRandom;
				affectorX.Period = affectorY.Period;
				affectorX.PeriodRandom = affectorY.PeriodRandom;
				affectorX.Range = rotatedRange.x;
				affectorX.RangeRandom = rotatedRangeRandom.x;
				componentInChildren.Affectors.Add(affectorX);
			}
			else if (child.position.x < 135f)
			{
				child.GetComponentInChildren<SinMovement>().Affectors[0].Range /= 2f;
			}
			else
			{
				child.position -= new Vector3(0f, 0.2f, 0f);
				child.GetComponentInChildren<SinMovement>().Affectors[0].Range /= 2f;
			}
		}
	}

	private static void BootstrapSpiritTree(SceneRoot sceneRoot)
	{
		// Unlike most other pickups, which are permanent placeholders that spawn an object with a DestroyOnRestoreCheckpoint component,
		// this one is *just* an object with a DestroyOnRestoreCheckpoint component. Disable that to prevent its untimely demise.
		sceneRoot.transform.FindChild("mediumExpOrb").GetComponent<DestroyOnRestoreCheckpoint>().enabled = false;
		// This checks if it is a grove tp spawn. TODO replace this with something tidier later.
		if (Randomizer.SpawnWith.Contains("-159,-114,force")) {
			Transform transform = sceneRoot.transform.FindChild("*spiritTreeStorySetup");
			ActionSequence sequence = transform.FindChild("container/actionSequences/01. reachSpiritTreeActionSequence").GetComponent<ActionSequence>();
			sequence.Actions.Clear();
			ActionSequence sequence2 = transform.FindChild("container/actionSequences/04. returnCameraToPlayerActionSequence").GetComponent<ActionSequence>();
			ActionMethod action = sequence2.Actions[12];
			sequence.Actions.Add(action);
			ActionMethod action2 = sceneRoot.transform.FindChild("*spiritTreeStorySetup/container/actionSequences/04. returnCameraToPlayerActionSequence/10. Deactivate *seinAbilityRestrictZones").GetComponent<ActionMethod>();
			sequence.Actions.Add(action2);
			sceneRoot.OnValidate();
		}
	}

	private static void BootstrapValleyThreeBirdArea(SceneRoot sceneRoot)
	{
		if (Characters.Sein && Characters.Sein.Inventory && Characters.Sein.Inventory.GetRandomizerItem(800) > 0)
		{
			Transform leverSetup = sceneRoot.transform.FindChild("*leverSetup");

			// Just disconnect the lever from the door; leave the lever itself interact for Ori to play with if they want
			ActionLeverSystem leverSystem = leverSetup.GetComponentInChildren<ActionLeverSystem>();
			leverSystem.LeverLeftAction = null;
			leverSystem.LeverRightAction = null;

			// Force the door open
			LegacyTranslateAnimator doorAnimator = leverSetup.FindChild("platformBranchSetup/sunkenGladesStompTree").GetComponent<LegacyTranslateAnimator>();
			doorAnimator.TimeOffset = doorAnimator.TimeOfLastCurvePoint;
		}
	}

	private static void BootstrapThornfeltSwampMain(SceneRoot sceneRoot)
	{
		// force the music to start up, dang it
		ActionSequence musicSequence = sceneRoot.transform.FindChild("musicZones/musicActivation").GetComponent<ActionSequence>();
		OnSceneStartRunAction runAction = musicSequence.gameObject.AddComponent<OnSceneStartRunAction>();
		runAction.ActionToRun = musicSequence;
		runAction.TriggerOnce = true;
		RandomizerBootstrap.SetGuidAndSave(sceneRoot, runAction, new MoonGuid(560691571, 1097907217, -1524861543, 276788056));

		// patch the post-Ginso cutscene to fix softlock when Sein's dialogue is auto-skipped
		ActionSequence seinAnimationSequence = sceneRoot.transform.FindChild("*objectiveSetup/objectiveSetupTrigger/seinSpriteAction").GetComponent<ActionSequence>();
		WaitAction waitAction = seinAnimationSequence.Actions[1] as WaitAction;
		waitAction.Duration = 5.0f;
	}

	private static void BootstrapMoonGrottoBridge(SceneRoot sceneRoot)
	{
		if (RandomizerSettings.Game.FixGrottoBridgeDrop)
		{
			// add an ActionSequenceSerializer to the bridge so that the sequence continues and activates the final colliders even after glitching it,
			// but delay that activation so the skip acts more like the vanilla skip.
			GameObject bridgeSequenceGameObject = sceneRoot.transform.FindChild("*gumoBridgeSetup/group/action").gameObject;
			ActionSequenceSerializer serializer = bridgeSequenceGameObject.AddComponent<ActionSequenceSerializer>();
			ActionSequence bridgeSequence = sceneRoot.transform.FindChild("*gumoBridgeSetup/group/action").GetComponent<ActionSequence>();
			WaitAction waitAction = bridgeSequence.gameObject.AddComponent<WaitAction>();
			waitAction.Duration = 10f;
			bridgeSequence.Actions.Insert(16, waitAction);
			RandomizerBootstrap.SetGuidAndSave(sceneRoot, waitAction, new MoonGuid(705566895, 1206307123, -626862952, 223115723));
			serializer.OnValidate();
			RandomizerBootstrap.SetGuidAndSave(sceneRoot, serializer, new MoonGuid(1360931587, 1176121670, -1051255642, 855352030));
		}
	}

	private static void BootstrapMountHoruHub(SceneRoot sceneRoot)
	{
		// add randomized pickup actions for each end of room cutscene
		Transform lavaDrainParent = sceneRoot.transform.FindChild("*doorSetups/lavaDrainSetups");

		// door1LavaDrain - (L3) mountHoruBreakyPathTop
		ActionSequence doorSequence = lavaDrainParent.FindChild("*door1LavaDrains/*door1LavaDrain").GetComponent<ActionSequence>();
		RandomizerPickupAction pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruL3");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door2LavaDrain - (R1) mountHoruStomperSystemsR
		doorSequence = lavaDrainParent.FindChild("*door2LavaDrains/*door2LavaDrain").GetComponent<ActionSequence>();
		pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruR1");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door3LavaDrain - (R2) mountHoruProjectileCorridor
		doorSequence = lavaDrainParent.FindChild("*door3LavaDrains/*door3LavaDrain").GetComponent<ActionSequence>();
		pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruR2");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door5LavaDrain - (R3) mountHoruMovingPlatform
		doorSequence = lavaDrainParent.FindChild("*door5LavaDrains/*door5LavaDrain").GetComponent<ActionSequence>();
		pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruR3");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door7LavaDrain - (L2) mountHoruBigPushBlock
		doorSequence = lavaDrainParent.FindChild("*door7LavaDrains/*door7LavaDrain").GetComponent<ActionSequence>();
		pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruL2");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door8LavaDrain - (L1) mountHoruBlockableLasers
		doorSequence = lavaDrainParent.FindChild("*door8LavaDrains/*door8LavaDrain").GetComponent<ActionSequence>();
		pickupAction = RandomizerLocationManager.AddPickupAction(doorSequence.gameObject, "HoruL1");
		(pickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);
		doorSequence.Actions.Insert(3, pickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// special cases for L4/R4
		RandomizerPickupAction leftPickupAction = RandomizerLocationManager.AddPickupAction(lavaDrainParent.gameObject, "HoruL4", "giveLeftPickup");
		(leftPickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);

		RandomizerPickupAction rightPickupAction = RandomizerLocationManager.AddPickupAction(lavaDrainParent.gameObject, "HoruR4", "giveRightPickup");
		(rightPickupAction as SaveSerialize).RegisterToSaveSceneManager(sceneRoot.SaveSceneManager);

		// door4LavaDrain - L4/R4, whichever comes first
		doorSequence = lavaDrainParent.FindChild("*door4LavaDrains/*door4LavaDrain").GetComponent<ActionSequence>();
		GameObject obj = new GameObject("pickupAction");
		obj.transform.parent = doorSequence.transform;

		RunActionCondition conditionPickupAction = obj.AddComponent<RunActionCondition>();
		RandomizerBootstrap.SetGuidAndSave(sceneRoot, conditionPickupAction, new MoonGuid(-1261986975, 1336041250, 1663544246, -817715174));
		conditionPickupAction.Action = leftPickupAction;
		conditionPickupAction.ElseAction = rightPickupAction;
		conditionPickupAction.Condition = (doorSequence.Actions[2] as RunActionCondition).Condition;

		doorSequence.Actions.Insert(3, conditionPickupAction);
		ActionSequence.Rename(doorSequence.Actions);

		// door6LavaDrain - L4/R4, whichever comes second
		doorSequence = lavaDrainParent.FindChild("*door6LavaDrains/*door6LavaDrain").GetComponent<ActionSequence>();
		obj = new GameObject("pickupAction");
		obj.transform.parent = doorSequence.transform;

		conditionPickupAction = obj.AddComponent<RunActionCondition>();
		RandomizerBootstrap.SetGuidAndSave(sceneRoot, conditionPickupAction, new MoonGuid(-300318401, 1327879929, 1536957364, -1500614911));
		conditionPickupAction.Action = rightPickupAction;
		conditionPickupAction.ElseAction = leftPickupAction;
		conditionPickupAction.Condition = (doorSequence.Actions[2] as RunActionCondition).Condition;

		doorSequence.Actions.Insert(3, conditionPickupAction);
		ActionSequence.Rename(doorSequence.Actions);
	}

	private static void BootstrapSunkenGladesSpiritWell(SceneRoot sceneRoot)
	{
		// forcibly deactivate the collision trigger for the spirit well intro cutscene
		sceneRoot.transform.FindChild("*activatedBySpiritFlame/activated/*spiritWellHintSetup/objectiveSetupTrigger").GetComponent<PlayerCollisionTrigger>().Active = false;
	}

	private static void BootstrapMountHoruLaserPuzzle(SceneRoot sceneRoot)
	{
		GameObject obj = new GameObject("deactivateSequence");
		obj.transform.parent = sceneRoot.transform.FindChild("laserPuzzle");
		
		ActionSequence sequence = obj.AddComponent<ActionSequence>();
		RandomizerBootstrap.SetGuidAndSave(sceneRoot, sequence, new MoonGuid(-217873041, 1228699831, -192933462, 1616173080));

		TriggerByString trigger = obj.AddComponent<TriggerByString>();
		trigger.Data = new TriggerByString.StringTriggerData{ String = "horuLaserPuzzleSolved", TriggerEvent = TriggerByString.TriggerEvent.Always };
		trigger.TriggerOnce = true;
		trigger.ActionToRun = sequence;
		RandomizerBootstrap.SetGuidAndSave(sceneRoot, trigger, new MoonGuid(-1643625622, 1244944140, -1378018126, -449882576));

		foreach (Transform child in sceneRoot.transform.FindChild("laserPuzzle/enemyStoppers"))
		{
			if (child.name == "blockableLaser")
			{
				GameObject newAction = new GameObject("action");
				newAction.transform.parent = obj.transform;

				ActivateAction activate = newAction.AddComponent<ActivateAction>();
				activate.Activate = false;
				activate.Target = child.gameObject;
				sequence.Actions.Add(activate);

				if (child.position.x < 265f)
				{
					RandomizerBootstrap.SetGuidAndSave(sceneRoot, activate, new MoonGuid(296308939, 1211527480, -1445804128, 1888526783));
				}
				else
				{
					RandomizerBootstrap.SetGuidAndSave(sceneRoot, activate, new MoonGuid(83562839, 1305673046, 1379750071, 220123169));
				}
			}
		}

		ActionSequence.Rename(sequence.Actions);
	}

	private static void BootstrapSunkenGladesRunaway(SceneRoot sceneRoot)
	{
		// This checks if it is a non-default spawn. TODO replace this with something tidier later.
		if (!Randomizer.SpawnWith.Contains("WS")) {
			return;
		}
		int wsLocation = Randomizer.SpawnWith.IndexOf("WS");
		int offset = 2;
		if (Randomizer.SpawnWith.Contains("WS/")) {
			offset = 3;
		}
		string[] pieces = Randomizer.SpawnWith.Substring(wsLocation + offset).Split(',');
		int warpX;
		int.TryParse(pieces[0], out warpX);
		int warpY;
		int.TryParse(pieces[1], out warpY);
		Vector3 position = new Vector3(warpX, warpY, 0);
		// This only takes a position, and loads scenes at that position. Doesn't require the metadata.
		// Definitely not as nice as adding a load to the action sequence, but significantly easier.
		Core.Scenes.Manager.AdditivelyLoadScenesAtPosition(position, true, false, true);
		
		ActionSequence actionSequence = sceneRoot.transform.FindChild("*objectiveSetup/objectiveSetupTrigger/objectiveSetupAction").GetComponent<ActionSequence>();
		List<ActionMethod> original_list = new List<ActionMethod>(actionSequence.Actions);
		// Remove from "09. Wait 4 seconds" and onwards.
		actionSequence.Actions.RemoveRange(8, 9);	
		// Hide letterboxes
		actionSequence.Actions.Add(original_list[11]);
		// Show UI
		actionSequence.Actions.Add(original_list[15]);
		// Unlock player input
		actionSequence.Actions.Add(original_list[10]);
		// Warp
		SetCharacterPosition setPosition = actionSequence.gameObject.AddComponent<SetCharacterPosition>();
		setPosition.transform.position = position;
		setPosition.Position = setPosition.transform;
		RandomizerBootstrap.SetGuidAndSave(sceneRoot, setPosition, new MoonGuid(2033807637, 1102752838, 351348109, 1564353675));
		actionSequence.Actions.Add(setPosition);
		// create checkpoint -- should be immediately after warp.
		actionSequence.Actions.Add(original_list[14]);
		// Wait 4 seconds
		actionSequence.Actions.Add(original_list[8]);		
		// wait 3.3 sceonds
		actionSequence.Actions.Add(original_list[12]);
		// play sound
		actionSequence.Actions.Add(original_list[13]);
		// Set user status action.
		actionSequence.Actions.Add(original_list[16]);
		sceneRoot.OnValidate();
	}

	private static void BootstrapWallJumpTreeHint(SceneRoot sceneRoot)
	{
		// This adds a return-to-start hint to the tree animation.
		ActionSequence treeSequence = sceneRoot.transform.FindChild("*abilityPedestalWallJump/pedestal/actionSequence").GetComponent<ActionSequence>();	
		ShowHintAction hint = treeSequence.gameObject.AddComponent<ShowHintAction>();
		RandomizerMessageProvider message = ScriptableObject.CreateInstance<RandomizerMessageProvider>();
		string text = "Stuck? You can use Return To Start (" + RandomizerRebinding.ReturnToStart.FirstBindName() + ") to go somewhere useful!";
		message.SetMessage(text);
		hint.HintMessage = message;
		hint.Duration = 5f;
		// The hint only shows when we don't have a casual skill set able to get out.
		RandomizerWallJumpHintCondition condition = treeSequence.gameObject.AddComponent<RandomizerWallJumpHintCondition>();
		RunActionCondition action = treeSequence.gameObject.AddComponent<RunActionCondition>();
		action.Action = hint;
		action.Condition = condition;
		treeSequence.Actions.Add(action);
	}

	private static void BootstrapSeinRoomHint(SceneRoot sceneRoot)
	{
		// This adds an alt-r hint into the getting-sein animation.
		ActionSequence getSeinSequence = sceneRoot.transform.FindChild("*setups/*story/findingOri/seinInterestZone/trigger/activateSequence").GetComponent<ActionSequence>();	
		ShowHintAction hint = getSeinSequence.gameObject.AddComponent<ShowHintAction>();
		RandomizerMessageProvider message = ScriptableObject.CreateInstance<RandomizerMessageProvider>();
		string text = "Tip: You can use Return To Start (" + RandomizerRebinding.ReturnToStart.FirstBindName() + ") to skip this fight!";
		message.SetMessage(text);
		hint.HintMessage = message;
		hint.Duration = 5f;
		getSeinSequence.Actions.Insert(17, hint);
	}

	private static void BootstrapMoonGrottoMiniboss(SceneRoot sceneRoot)
	{
		// This function makes it so you don't soft-lock if you alt-r out
		// of the moon grotto miniboss room.
		// Check disable alt-r soft-lock fixes.
		if (Characters.Sein.Inventory.GetRandomizerItem(1103) != 0) {
			return;
		}

		PlayerCollisionTrigger firstDoorTrigger = sceneRoot.transform.FindChild("*gumoAnimationSummonEnemy/enemyPuzzles/doorASetup/triggerCollider").GetComponent<PlayerCollisionTrigger>();
		LegacyTranslateAnimator firstDoorAnimator = sceneRoot.transform.FindChild("*gumoAnimationSummonEnemy/enemyPuzzles/doorASetup/moonGrottoBlockingDoorB").GetComponent<LegacyTranslateAnimator>();		
		LegacyTranslateAnimator secondDoorAnimator = sceneRoot.transform.FindChild("*gumoAnimationSummonEnemy/enemyPuzzles/enemyPuzzle/doorSetup/sidewaysDoor/puzzleDoorLeft").GetComponent<LegacyTranslateAnimator>();
		CameraWideScreenZone cameraZone = sceneRoot.transform.FindChild("*gumoAnimationSummonEnemy/cameraWideScreenZone").GetComponent<CameraWideScreenZone>();

		bool firstDoorShut = !firstDoorAnimator.AtStart;
		bool secondDoorOpen = !secondDoorAnimator.AtStart;
		if (secondDoorOpen) {
			// Note: I don't believe this is required as the other logic should suffice
			// by itself, but it is here just in case.
			// Open the door and disable the trigger and camera zone.
			firstDoorAnimator.Stopped = true;
			firstDoorAnimator.Reversed = false;
			firstDoorAnimator.CurrentTime = 0f;
			firstDoorAnimator.Sample(firstDoorAnimator.CurrentTime);
			firstDoorTrigger.gameObject.active = false;
			firstDoorTrigger.Active = false;
			cameraZone.gameObject.active = false;
			return;
		}
		
		Rect minibossRoom = Rect.MinMaxRect(558f, -423f, 628f, -390f); 
		bool isInRoom = minibossRoom.Contains(Characters.Sein.Position);		
		if (firstDoorShut && !isInRoom) {
			// Open the door and enable the trigger and camera zone.
			firstDoorAnimator.Stopped = true;
			firstDoorAnimator.Reversed = false;
			firstDoorAnimator.CurrentTime = 0f;
			firstDoorAnimator.Sample(firstDoorAnimator.CurrentTime);
			firstDoorTrigger.gameObject.active = true;
			firstDoorTrigger.Active = true;
			cameraZone.gameObject.active = true;
		}
	}

	private static Dictionary<string, Action<SceneRoot>> s_bootstrapPreEnabled = new Dictionary<string, Action<SceneRoot>>
	{
		{ "moonGrottoRopeBridge", new Action<SceneRoot>(RandomizerBootstrap.BootstrapMoonGrottoBridge) },
		{ "mountHoruHubMid", new Action<SceneRoot>(RandomizerBootstrap.BootstrapMountHoruHub) },
		{ "mountHoruLaserPuzzle", new Action<SceneRoot>(RandomizerBootstrap.BootstrapMountHoruLaserPuzzle) },
		{ "northMangroveFallsLanternIntro", new Action<SceneRoot>(RandomizerBootstrap.BootstrapBlackrootLanternRoom) },
		{ "spiritTreeRefined", new Action<SceneRoot>(RandomizerBootstrap.BootstrapSpiritTree) },
		{ "sunkenGladesIntroSplitB", new Action<SceneRoot>(RandomizerBootstrap.BootstrapSunkenGladesSpiritWell) },
		{ "thornfeltSwampActTwoStart", new Action<SceneRoot>(RandomizerBootstrap.BootstrapThornfeltSwampMain) },
		{ "titleScreenSwallowsNest", new Action<SceneRoot>(RandomizerBootstrap.BootstrapTitleScreen) },
		{ "westGladesFireflyAreaA", new Action<SceneRoot>(RandomizerBootstrap.BootstrapValleyThreeBirdArea) },
		{ "sunkenGladesRunaway", new Action<SceneRoot>(RandomizerBootstrap.BootstrapSunkenGladesRunaway) },
		{ "sunkenGladesSpiritCavernWalljumpB", new Action<SceneRoot>(RandomizerBootstrap.BootstrapWallJumpTreeHint) },
		{ "sunkenGladesOriRoom", new Action<SceneRoot>(RandomizerBootstrap.BootstrapSeinRoomHint) },
	};

	private static List<string> s_bootstrappedScenesPreEnabled = new List<string>();

	// Generally prefer PreEnabled over AfterSerialize. These functions are run after *every* 
	// serialisation of the scene, so after every death and not just the initial load. So don't e.g.
	// unconditionally add things to the scene in these functions, as they will repeat. But if
	// you need to do things that alter or depend on serialised parts of the scene, this is the 
	// place. Things altered here may be serialised (saved) by the scene. If you want to make
	// new serialised scene elements you'll need to use PreEnabled.
	private static Dictionary<string, Action<SceneRoot>> s_bootstrapAfterSerialize = new Dictionary<string, Action<SceneRoot>>
	{
		{ "moonGrottoEnemyPuzzle", new Action<SceneRoot>(RandomizerBootstrap.BootstrapMoonGrottoMiniboss) },
	};

	private static List<string> s_bootstrappedScenesAfterSerialize = new List<string>();
}
