using HarmonyLib;
using Kingmaker;
using Kingmaker.AreaLogic;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Cheats;
using Kingmaker.Controllers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TinyJson;
using UnityEngine;
using static Kingmaker.Blueprints.Root.CheatRoot;
using static Kingmaker.Controllers.EntityCreationController;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace OmegaTweaks.ThroneRoomAnorel
{
    static class GUID
    {
        public static class Area
        {
            public const string CapitalThroneRoom1 = "173c1547502bb7243ad94ef8eec980d0";
            public const string CapitalThroneRoom2 = "c39ed0e2ceb98404b811b13cb5325092";
            public const string CapitalTavern = "5c3935c8ab777f04f83f272425b750f9";
            public const string DungeonStartHub_Roguelike = "c49315fe499f0e5468af6f19242499a2";
        }
        public static class Unit
        {
            public const string Anorel = "c839b384295534144b4ae6e5141efab5";
        }

        public static class Dialog
        {
            public const string Anorel = "512beff56185aaf4c9043540e0c2d787";
        }
    }

    static class GroupId
    {
        public static class Unit
        {
            public const string CapitalTavernOriginalAnorel = "c78c13e6-cf2f-400a-849f-bf9572ebaa02";
        }
    }

    //[HarmonyPatch(typeof(GameModesFactory), "Initialize")]
    //static class AddNewController
    //{
    //    public static void Postfix()
    //    {
    //        MethodInfo dynMethod = typeof(GameModesFactory)
    //            .GetMethod(
    //                "Register",
    //                BindingFlags.Static | BindingFlags.NonPublic);

    //        dynMethod.Invoke(null, new object[] { new ThroneRoomAnorel(), new[] { GameModeType.Default } });
    //    }
    //}

    //[HarmonyPatch(typeof(UnitEntityData), "SelectClickInteraction")]
    //static class Test
    //{
    //    public static void Prefix(UnitEntityData initiator, ref UnitEntityData __instance)
    //    {
    //        if (__instance.GroupId.Equals(GroupId.Unit.CapitalTavernOriginalAnorel))
    //        {
    //            // __instance(this)=アノリエル　initiator=プレイヤー

    //            var anorel = __instance;
    //            var player = initiator;

    //            UnitPartInteractions unitPartInteractions = anorel.Get<UnitPartInteractions>();
    //            OmegaTweaksModMain.logger.Log("initiator[" + player.CharacterName + "] : this[" + anorel.CharacterName + "]");
    //            if (unitPartInteractions == null)
    //            {
    //                OmegaTweaksModMain.logger.Log("SelectClickInteraction : null.");
    //            } else
    //            {
    //                OmegaTweaksModMain.logger.Log("SelectClickInteraction : found.");
    //                //OmegaTweaksModMain.logger.Log("SelectClickInteraction : " + Newtonsoft.Json.JsonConvert.SerializeObject(unitPartInteractions));
    //            }
    //        }
    //    }
    //}

    //public class ThroneRoomAnorel : IController, IAreaLoadingStagesHandler
    //{
    //    public void Activate()
    //    {
    //    }

    //    public void Deactivate()
    //    {
    //    }

    //    public void OnAreaLoadingComplete()
    //    {
    //        Core();
    //    }

    //    public void OnAreaScenesLoaded()
    //    {
    //    }

    //    public void Tick()
    //    {
    //    }

    //    private void Core()
    //    {
    //        GameModeType currentMode = Game.Instance.CurrentMode;
    //        if (!(currentMode == GameModeType.Default || currentMode == GameModeType.Pause))
    //        {
    //            return;
    //        }

    //        OmegaTweaksModMain.logger.Log("OnAreaLoadingComplete : " + Game.Instance.CurrentlyLoadedArea.AssetGuid + " : " + Game.Instance.CurrentlyLoadedArea.name);

    //        var areaGuid = Game.Instance.CurrentlyLoadedArea.AssetGuid;
    //        if (areaGuid.Equals(GUID.Area.CapitalThroneRoom1) ||
    //            areaGuid.Equals(GUID.Area.CapitalThroneRoom2))
    //        {
    //            DespawnAnorel();
    //            SpawnAnorel(new Vector3(-13.0f, 6.6f, 7.2f), new Vector3(-0.46f, 0, -0.88f));
    //        }
    //        else if (areaGuid.Equals(GUID.Area.CapitalTavern))
    //        {
    //            DespawnAnorel();
    //            SpawnAnorel(new Vector3(-6.6f, 0.06f, 11.8f), new Vector3(-0.46f, 0, -0.88f));
    //        }
    //    }

    //    private UnitEntityData SpawnAnorel(Vector3 position, Vector3 rotation)
    //    {
    //        UnitEntityData anorelEntityData = Game.Instance.EntityCreator.SpawnUnit(
    //            (BlueprintUnit)Utilities.GetBlueprintByGuid<BlueprintUnit>(GUID.Unit.Anorel),
    //            position,
    //            Quaternion.LookRotation(rotation),
    //            //Game.Instance.CurrentScene.MainState
    //            Game.Instance.State.LoadedAreaState.MainState
    //            );
    //        //anorelEntityData.View.CreateEntityData(true);
    //        //UnitDescriptor descriptor2 = new UnitDescriptor(anorelEntityData.View.Blueprint, anorelEntityData);
    //        //anorelEntityData.Descriptor = descriptor2;
    //        foreach (UnitEntityData unit in Game.Instance.State.Units)
    //        {
    //            if (unit.GroupId.Equals(GroupId.Unit.CapitalTavernOriginalAnorel))
    //            {
    //                //unit.CutceneControlledUnit = null;
    //                //OmegaTweaksModMain.logger.Log(Newtonsoft.Json.JsonConvert.SerializeObject(unit));

    //                //var bp = Utilities.GetBlueprintByGuid<BlueprintDialog>(GUID.Dialog.Anorel);
    //                //Game.Instance.DialogController.StartDialogWithoutTarget(bp, null);

    //                //OmegaTweaksModMain.logger.Log("Loop1 Start.");
    //                //List<UnitInteractionComponent> list = unit.Blueprint.GetComponents<UnitInteractionComponent>().ToList<UnitInteractionComponent>();
    //                //foreach (UnitInteractionComponent component in list)
    //                //{
    //                //    OmegaTweaksModMain.logger.Log("------");
    //                //    OmegaTweaksModMain.logger.Log(component.ToString());
    //                //}
    //                //OmegaTweaksModMain.logger.Log("Loop1 End.");

    //                //UnitPartInteractions unitPartInteractions = unit.Ensure<UnitPartInteractions>();
    //                //OmegaTweaksModMain.logger.Log(Newtonsoft.Json.JsonConvert.SerializeObject(unitPartInteractions));

    //                break;
    //            }
    //        }
    //        //anorelEntityData.CutceneControlledUnit = new CutsceneControlledUnit(anorelEntityData);
    //        //anorelEntityData.CutceneControlledUnit = cutsceneControlledUnit;
    //        //OmegaTweaksModMain.logger.Log(Newtonsoft.Json.JsonConvert.SerializeObject(anorelEntityData));
    //        UnitPartInteractions.SetupBlueprintInteractions(anorelEntityData);
    //        return anorelEntityData;
    //    }

    //    private void DespawnAnorel()
    //    {
    //        foreach (UnitEntityData unit in Game.Instance.State.Units)
    //        {
    //            if (
    //                unit.Blueprint.AssetGuid.Equals(GUID.Unit.Anorel) &&
    //                !unit.GroupId.Equals(GroupId.Unit.CapitalTavernOriginalAnorel)
    //            )
    //            {
    //                unit.Destroy();
    //            }
    //        }
    //    }
    //}
}
