using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using HarryPotter.Classes;
using HarryPotter.Classes.WorldItems;
using InnerNet;
using Reactor.Utilities;
using UnityEngine;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Update))]
internal class InnerNetClient_Update
{
    private static void Postfix(InnerNetClient __instance)
    {
        Coroutines.Start(LateUpdate());
    }

    private static IEnumerator LateUpdate()
    {
        yield return new WaitForEndOfFrame();
        RunUpdate();
    }

    private static void RunUpdate()
    {
        Main.Instance?.Config?.ReloadSettings();

        if (!AmongUsClient.Instance.IsGameStarted && Main.Instance != null)
        {
            foreach (var wItem in Main.Instance.AllItems) wItem.Delete();
            DeluminatorWorld.HasSpawned = false;
            MaraudersMapWorld.HasSpawned = false;
            PortKeyWorld.HasSpawned = false;
            TheGoldenSnitchWorld.HasSpawned = false;
            GhostStoneWorld.HasSpawned = false;
            ButterBeerWorld.HasSpawned = false;
            ElderWandWorld.HasSpawned = false;
            BasWorldItem.HasSpawned = false;
            SortingHatWorld.HasSpawned = false;
            PhiloStoneWorld.HasSpawned = false;
            Main.Instance.CurrentStage = 0;
            Main.Instance.AllItems.Clear();
            Main.Instance.AllPlayers.Clear();
            Main.Instance.PossibleItemPositions = Main.Instance.DefaultItemPositons;
            TaskInfoHandler.Instance.AllInfo.Clear();
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
            if (Main.Instance?.AllPlayers.Where(x => x?._Object == player).ToList().Count == 0)
                Main.Instance?.AllPlayers.Add(new ModdedPlayerClass(player, null, new List<Item>()));

        foreach (var player in Main.Instance?.AllPlayers.ToList())
            if (player == null || player._Object == null || player._Object.Data.Disconnected)
                Main.Instance?.AllPlayers.Remove(player);

        Main.Instance?.GetLocalModdedPlayer().Update();
    }
}