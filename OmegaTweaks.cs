using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Cheats;
using Kingmaker.Controllers.Rest;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.UI._ConsoleUI.GameOver;
using Kingmaker.Kingdom.Rules;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Log;
using OmegaTweaks.ThroneRoomAnorel;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager.ModEntry;
using System;

namespace OmegaTweaks
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        public enum PricePerWeightOverrideTarget
        {
            NOT_USE_THIS,
            Type,
            Price,
            Name,
            Date,
            Weight,
        }

        [Draw("Free Respec")] public bool FreeRespecEnable = true;
        [Draw("No time loss for Respec")] public bool NoTimeLossRespecEnable = true;
        [Draw("Cheep Cost Mercenaries (=250*Lv)")] public bool CheepCostMercenaries = true;
        [Draw("No Penalty Mercenary Advisor")] public bool NoPenaltyCustomCompanionAdvisor = true;
        [Draw("Telepathy Anoriel (*WIP*). Press [P]")] public bool TelepathyAnoriel = false;
        [Draw("Override the original sort type to perform a sort by price per weight")] public PricePerWeightOverrideTarget PricePerWeight = PricePerWeightOverrideTarget.NOT_USE_THIS;
        [Draw("Building full price exchange.")] public bool BuildingFullPriceExchange = false;

        public void OnChange()
        {
            KingdomRoot.Instance.CustomLeaderPenalty = NoPenaltyCustomCompanionAdvisor ? 0 : -4;
            OmegaTweaksModMain.logger.Log("KingdomRoot.Instance.CustomLeaderPenalty = " + KingdomRoot.Instance.CustomLeaderPenalty);
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public class OmegaTweaksModMain
    {
        public static Settings settings;
        public static bool inited = false;
        public static ModLogger logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Harmony harmony2 = new Harmony(modEntry.Info.Id + modEntry.Info.Author);
            harmony2.PatchAll(Assembly.GetExecutingAssembly());
            
            settings = Settings.Load<Settings>(modEntry);
            logger = modEntry.Logger;

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUpdate = OnUpdate;

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                //logger.Log("Scene: " + scene.name + " Mode: " + mode.ToString());
                //logger.Log("AreaGUID: " + Game.Instance.CurrentlyLoadedArea?.AssetGuid + " AreaName: " + Game.Instance.CurrentlyLoadedArea?.name);
                if (!inited && scene.name == SceneName.MainMenu)
                {
                    settings.OnChange();
                    inited = true;
                }
            };

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Space(5);
            settings.Draw(modEntry);
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                TelepathyAnoriel.ShowDialog();
            }
        }
    }

    [HarmonyPatch(typeof(Player))]
    static class FreeRespec
    {
        // Free Respec
        [HarmonyPostfix]
        [HarmonyPatch("GetRespecCost")]
        public static void GetRespecCostPostfix(ref int __result)
        {
            if (OmegaTweaksModMain.settings.FreeRespecEnable)
            {
                __result = 0;
            }
        }
    }

    [HarmonyPatch(typeof(RespecCompanion))]
    static class NoTimeLossForRespec
    {
        // Free Respec
        [HarmonyPrefix]
        [HarmonyPatch("FinishRespecialization")]
        public static bool FinishRespecializationPrefix(ref RespecCompanion __instance)
        {
            if (!OmegaTweaksModMain.settings.NoTimeLossRespecEnable)
            {
                return true;
            }

            Player player = Game.Instance.Player;
            if (!__instance.ForFree)
            {
                player.SpendMoney((long)player.GetRespecCost());
                player.RespecsUsed++;
            }
            // Game.Instance.AdvanceGameTime(1.Days());
            foreach (UnitEntityData unitEntityData in Game.Instance.Player.ControllableCharacters)
            {
                if (!unitEntityData.Descriptor.State.IsFinallyDead)
                {
                    RestController.ApplyRest(unitEntityData.Descriptor);
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Player))]
    static class CheepCostMercenaries
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetCustomCompanionCost")]
        public static void GetCustomCompanionCostPostfix(ref Player __instance, ref int __result)
        {
            if (OmegaTweaksModMain.settings.CheepCostMercenaries)
            {
                // return BlueprintRoot.Instance.CustomCompanionBaseCost * __instance.PartyLevel * __instance.PartyLevel;
                __result = (BlueprintRoot.Instance.CustomCompanionBaseCost * __instance.PartyLevel) / 2;
            }
        }
    }

    static class TelepathyAnoriel
    {
        public static void ShowDialog()
        {
            if (!OmegaTweaksModMain.settings.TelepathyAnoriel)
            {
                return;
            }

            if (Game.Instance.CurrentlyLoadedArea != null)
            {
                var areaGuid = Game.Instance.CurrentlyLoadedArea.AssetGuid;
                if (areaGuid == GUID.Area.CapitalThroneRoom1 ||
                    areaGuid == GUID.Area.CapitalThroneRoom2 ||
                    areaGuid == GUID.Area.DungeonStartHub_Roguelike)
                {
                    var bp = Utilities.GetBlueprintByGuid<BlueprintDialog>(GUID.Dialog.Anorel);
                    Game.Instance.DialogController.StartDialogWithoutTarget(bp, null);
                } else
                {
                    OmegaTweaksModMain.logger.Log("Telepathy prohibited area. [GUID: " + areaGuid + " NAME: " + Game.Instance.CurrentlyLoadedArea.name + "]");
                    OmegaTweaksModMain.logger.Log("Telepathy to Anoriel only works in the capital's throne room or at the start hub in roguelike mode.");
                    Game.Instance.UI.BattleLogManager.LogView.AddLogEntry("[OmegaTweaks] Telepathy to Anoriel only works in the capital's throne room or at the start hub in roguelike mode.", GameLogStrings.Instance.DefaultColor, LogChannel.None);
                }
            }
        }
    }

    [HarmonyPatch(typeof(SettlementState))]
    static class BuildingFullPriceExchange
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetSellPrice")]
        public static void GetSellPricePostfix(BlueprintSettlementBuilding bp, ref SettlementState __instance, ref int __result)
        {
            if (!OmegaTweaksModMain.settings.BuildingFullPriceExchange)
            {
                return;
            }

            __result = Rulebook.Trigger<RuleCalculateBuildingCost>(new RuleCalculateBuildingCost(bp, __instance, true)).Cost;
        }

        [HarmonyPostfix]
        [HarmonyPatch("GetActualCost")]
        [HarmonyPatch(new Type[] { typeof(BlueprintSettlementBuilding) })]
        public static void GetActualCostPostfix(BlueprintSettlementBuilding bp, ref SettlementState __instance, ref int __result)
        {
            if (!OmegaTweaksModMain.settings.BuildingFullPriceExchange)
            {
                return;
            }

            if (__instance.SellDiscountedBuilding == bp)
            {
                __result = Rulebook.Trigger<RuleCalculateBuildingCost>(new RuleCalculateBuildingCost(bp, __instance, true)).Cost;
            }

            return;
        }
    }
}
