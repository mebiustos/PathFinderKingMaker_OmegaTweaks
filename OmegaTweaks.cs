using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Rest;
using Kingmaker.Controllers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.UnitLogic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using UnityEngine.SceneManagement;
using System.Threading;

namespace OmegaTweaks
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Free Respec")] public bool FreeRespecEnable = true;
        [Draw("No time loss for Respec")] public bool NoTimeLossRespecEnable = true;
        [Draw("Cheep Cost Mercenaries (=250*Lv)")] public bool CheepCostMercenaries = true;
        [Draw("No Penalty Mercenary Advisor")] public bool NoPenaltyCustomCompanionAdvisor = true;

        public void OnChange()
        {
            KingdomRoot.Instance.CustomLeaderPenalty = NoPenaltyCustomCompanionAdvisor ? 0 : -4;
            Debug.Log("KingdomRoot.Instance.CustomLeaderPenalty = " + KingdomRoot.Instance.CustomLeaderPenalty);
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

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Harmony harmony2 = new Harmony(modEntry.Info.Id + modEntry.Info.Author);
            harmony2.PatchAll(Assembly.GetExecutingAssembly());
            
            settings = Settings.Load<Settings>(modEntry);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (!inited && scene.name == "MainMenu")
                {
                    settings.OnChange();
                    inited = true;
                }
            };
            //while (KingdomRoot.Instance == null)
            //{
            //    Thread.Sleep(200);
            //}
            //KingdomRoot.Instance.CustomLeaderPenalty = settings.NoPenaltyCustomCompanionAdvisor ? 0 : -4;

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

    }

    [HarmonyPatch(typeof(Player))]
    static class OmegaTweaks
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

    //[HarmonyPatch(typeof(LeaderState))]
    //static class MyLeaderState
    //{
    //    [HarmonyPostfix]
    //    [HarmonyPatch("GetCharacterStat")]
    //    public static void GetCharacterStatPostfix(LeaderState.Leader unit, bool withPenalty, ref int __result)
    //    {
    //        if (FreeRespec.settings.NoPenaltyCustomCompanionAdvisor)
    //        {
    //            if (unit == null || unit.Empty)
    //            {
    //                return;
    //            }
    //            if (unit.Blueprint.Faction == Game.Instance.BlueprintRoot.PlayerFaction)
    //            {
    //                UnitEntityData unitEntityData = Game.Instance.Player.AllCrossSceneUnits.FirstOrDefault((UnitEntityData u) => unit.IsSameUnit(u));
    //                if (withPenalty && unitEntityData.IsCustomCompanion())
    //                {
    //                    __result -= KingdomRoot.Instance.CustomLeaderPenalty;
    //                }
    //            }
    //        }
    //    }
    //}
}
