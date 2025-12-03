using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Assets.CoreScripts;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace HarryPotter
{
    [BepInPlugin(Id, "Harry Potter Mod", "1.0.0")]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(HunterPlugin.Id)]
    public class Plugin : BasePlugin
    {
        public const string Id = "harry.potter.mod";
        public Harmony Harmony { get; } = new Harmony(Id);

        public override void Load()
        {
            // Obligatoire avec Il2CppInterop
            ClassInjector.RegisterTypeInIl2Cpp<Main>();

            Main.Instance = new Main();

            InitializeHats();
            ConfigureHunterHud();
            Harmony.PatchAll();

            if (Main.Instance.Config.UseCustomRegion)
                SetupCustomRegion2025();
        }

        private void InitializeHats()
        {
            // La liste Hat.AllHats n'existe plus en 2025 -> injection du catalogue moddé
            if (HatManager.Instance == null)
            {
                Debug.LogError("[HarryPotter] HatManager not initialized yet.");
                return;
            }

            var hatList = new List<HatData>();

            void addHat(string resName)
            {
                var hat = ScriptableObject.CreateInstance<HatData>();
                hat.name = resName;
                hat.ProductId = resName;
                hat.StoreName = resName;
                hat.MainImage = null;
                hatList.Add(hat);
            }

            addHat("Scarf1");
            addHat("Scarf2");
            addHat("Scarf3");
            addHat("Scarf4");
            addHat("Hair1");
            addHat("Hair2");
            addHat("Hair3");
            addHat("Hair4");
            addHat("Ears1");
            addHat("Ears2");
            addHat("Devil");
            addHat("Fire");
            addHat("Glitch");
            addHat("WizardGlitch");
            addHat("Pinkee");
            addHat("Raccoon");
            addHat("Snake");
            addHat("Wizard");
            addHat("Penguin");
            addHat("Elephant");
            addHat("PiratePanda");
            addHat("Flower");

            HatManager.Instance.allHats._items.AddRange(hatList);
        }

        private void ConfigureHunterHud()
        {
            HunterPlugin.DrawHudString = false;
            HunterPlugin.HudScale = 0.8f;
        }

        // Nouveau système de régions 2025
        private void SetupCustomRegion2025()
        {
            var region = new RegionInfoV2(
                name: "Private",
                serverName: "Private-1",
                defaultIp: "51.222.158.63",
                port: 22023,
                useDtls: false
            );

            var arr = new Il2CppReferenceArray<RegionInfoV2>(1);
            arr[0] = region;

            ServerManager.Instance.regions = arr;
            ServerManager.Instance.SaveServers();
            ServerManager.Instance.ReselectRegion();
        }
    }


    // ============ PATCHES ============


    // Anti-ban (signature confirmée 2025)
    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public static class StatsManagerPatch
    {
        public static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }


    // Nouveau système Game Options (ToHudString supprimé en 2025)
    [HarmonyPatch(typeof(GameOptionsMenuController), nameof(GameOptionsMenuController.SetGameOptions))]
    public class GameOptionsPatch
    {
        public static void Postfix(GameOptionsMenuController __instance)
        {
            if (!Main.Instance.Config.ShowPopups) return;

            __instance.titleText.text += "\n<size=2><#EEFFB3FF>Mod settings enabled</size>";
        }
    }


    // Nouveau PingTracker HUD (2025)
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingTrackerPatch
    {
        public static void Postfix(PingTracker __instance)
        {
            if (__instance.text == null) return;

            var text = __instance.text;

            if (Main.Instance.Config.SimplerWatermark)
            {
                text.fontSize = 1.8f;
                text.rectTransform.anchoredPosition = new Vector2(140f, -40f);

                text.text += "\n<#7289DAFF>Hunter101#1337";
                text.text += "\n<#00DDFFFF>www.computable.us";
            }
            else
            {
                text.fontSize = 1.9f;
                text.alignment = TextAlignmentOptions.TopRight;
                text.rectTransform.anchoredPosition = new Vector2(180f, -40f);

                text.text += "\nCreated by <#7289DAFF>Hunter101#1337";
                text.text += "\nDownload: <#00DDFFFF>www.computable.us";
                text.text += "\nDesign: <#88FF00FF>npc & friends";
                text.text += "\nArt: <#E67E22FF>PhasmoFireGod";
            }
        }
    }
}
