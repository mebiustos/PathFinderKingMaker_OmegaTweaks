using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Rest;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Kingdom.Blueprints;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

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
}
