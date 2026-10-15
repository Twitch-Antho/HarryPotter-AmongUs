using System.Linq;
using HarmonyLib;
using HarryPotter.Classes;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
public static class RoleManager_AssignRoleOnDeath
{
    public static bool Prefix([HarmonyArgument(0)] PlayerControl player, bool specialRolesAllowed)
    {
        if (Main.Instance.AllPlayers.FindAll(p => p._Object.PlayerId == player.PlayerId).FirstOrDefault().Role
                .RoleName == "Hermione" && Main.Instance.isActivateHourglass
                || Main.Instance.AllPlayers.FindAll(p => p._Object.PlayerId == player.PlayerId).FirstOrDefault().ShouldRevive)
            return false;
        return true;
    }
}