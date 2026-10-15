using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using HarryPotter.Classes;
using HarryPotter.Classes.Roles;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(RoleOptionsCollectionV08), nameof(RoleOptionsCollectionV08.GetNumPerGame))]
internal class RoleOptionsDataGetNumPerGamePatch
{
    public static void Postfix(ref int __result)
    {
        if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal &&
            Main.Instance.Config.EnableModRole) __result = 0; // Deactivate Vanilla Roles if the mod roles are active
    }
}

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
internal class RoleManager_SelectRoles
{
    /*static void Prefix()
    {
        if (GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek || !Main.Instance.Config.EnableModRole) return;

        PluginSingleton<Plugin>.Instance.Log.LogMessage("RPC SET ROLE");
        var infected = GameData.Instance.AllPlayers.ToArray().Where(o => o.Role.IsImpostor);
    }*/

    private static void Postfix()
    {
        if (GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek ||
            !Main.Instance.Config.EnableModRole) return;
        PluginSingleton<Plugin>.Instance.Log.LogMessage("RPC SET ROLE");

        var allImp =
            Main.Instance.AllPlayers.Where(x => x._Object.Data.Role.IsImpostor).ToList();

        var allCrew =
            Main.Instance.AllPlayers.Where(x => !x._Object.Data.Role.IsImpostor).ToList();

        var impRolesToAssign = new List<string> { "Voldemort", "Bellatrix" };
        var crewRolesToAssign = new List<string> { "Harry", "Hermione", "Ron" };

        while (allImp.Count > 0 && impRolesToAssign.Count > 0)
        {
            var rolePlayer = allImp.Random();
            allImp.Remove(rolePlayer);

            if (impRolesToAssign.Contains("Voldemort") && Random.Range(0, 101) <= Main.Instance.Config.EnableVoldemort)
            {
                impRolesToAssign.Remove("Voldemort");
                Main.Instance.RpcAssignRole(rolePlayer, new Voldemort(rolePlayer));
                continue;
            }

            if (impRolesToAssign.Contains("Bellatrix") && Random.Range(0, 101) <= Main.Instance.Config.EnableBellatrix)
            {
                impRolesToAssign.Remove("Bellatrix");
                Main.Instance.RpcAssignRole(rolePlayer, new Bellatrix(rolePlayer));
            }
        }

        while (allCrew.Count > 0 && crewRolesToAssign.Count > 0)
        {
            var rolePlayer = allCrew.Random();
            allCrew.Remove(rolePlayer);

            if (crewRolesToAssign.Contains("Harry") && Random.Range(0, 101) <= Main.Instance.Config.EnableHarry)
            {
                crewRolesToAssign.Remove("Harry");
                Main.Instance.RpcAssignRole(rolePlayer, new Harry(rolePlayer));
                continue;
            }

            if (crewRolesToAssign.Contains("Ron") && Random.Range(0, 101) <= Main.Instance.Config.EnableRon)
            {
                crewRolesToAssign.Remove("Ron");
                Main.Instance.RpcAssignRole(rolePlayer, new Ron(rolePlayer));
                continue;
            }

            if (crewRolesToAssign.Contains("Hermione") && Random.Range(0, 101) <= Main.Instance.Config.EnableHermione)
            {
                crewRolesToAssign.Remove("Hermione");
                Main.Instance.RpcAssignRole(rolePlayer, new Hermione(rolePlayer));
            }
        }
    }
}