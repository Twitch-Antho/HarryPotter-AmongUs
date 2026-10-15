using System;
using System.Collections.Generic;
using AmongUs.Data.Player;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using HarryPotter.Classes;
using HarryPotter.Classes.UI;
using InnerNet;
using Reactor;
using TMPro;
using UnityEngine;

namespace HarryPotter;

[BepInPlugin(Id, "Harry Potter", VersionString)]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public class Plugin : BasePlugin
{
    public const string Id = "harry.potter.mod";
    public const string VersionString = "2.3.0";
    public static Version Version = Version.Parse(VersionString);
    public static Plugin Instance;
    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        Main.Instance = new Main();
        Instance = this;
        ModTranslation.Load();
        TaskInfoHandler.Instance = new TaskInfoHandler { AllInfo = new List<ImportantTextTask>() };
        PopupTMPHandler.Instance = new PopupTMPHandler { AllPopups = new List<TextMeshPro>() };
        Classes.Config.LoadOption();
        AddComponent<ModUpdater>();
        Harmony.PatchAll();
        CustomOption.Patches.SettingsUpdate.ImportSlot("HarryPotterSettings");
    }
}

[HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.IsBanned), MethodType.Getter)]
public static class PlayerBanData_IsBanned
{
    private static void Postfix(out bool __result)
    {
        __result = false;
    }
}

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
public static class PingTracker_Update
{
    private static void Postfix(PingTracker __instance)
    {
        var position = __instance.GetComponent<AspectPosition>();
        var text2 = AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started
            ? ModTranslation.getString("PingTracker_Update")
              + $"\n{__instance.text.text}"
            : "";
        __instance.text.text =
            $"<size=130%><color=#FFF319>Harry Potter</color> v{Plugin.Version.ToString()}</size>\n{text2}";
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
        {
            __instance.text.alignment = TextAlignmentOptions.Top;
            position.Alignment = AspectPosition.EdgeAlignments.Top;
            position.DistanceFromEdge = new Vector3(1.5f, 0.11f, 0);
        }
        else
        {
            position.Alignment = AspectPosition.EdgeAlignments.LeftTop;
            __instance.text.alignment = TextAlignmentOptions.TopLeft;
            position.DistanceFromEdge = new Vector3(0.5f, 0.11f);
        }
    }
}