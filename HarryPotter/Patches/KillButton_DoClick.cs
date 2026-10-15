using HarmonyLib;
using HarryPotter.Classes;
using HarryPotter.Classes.Roles;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
internal class KillButton_DoClick
{
    private static bool Prefix(KillButton __instance)
    {
        if (__instance == HudManager.Instance.KillButton &&
            Main.Instance.GetLocalModdedPlayer()?.Role?.RoleName == "Bellatrix" &&
            ((Bellatrix)Main.Instance.GetLocalModdedPlayer().Role).MindControlledPlayer != null)
        {
            var killer = ((Bellatrix)Main.Instance.GetLocalModdedPlayer().Role).MindControlledPlayer._Object;
            if (HudManager.Instance.KillButton.currentTarget != null && !Main.Instance.ControlKillUsed)
            {
                Main.Instance.ControlKillUsed = true;
                Main.Instance.RpcKillPlayer(killer, HudManager.Instance.KillButton.currentTarget);
            }

            return false;
        }

        foreach (var item in Main.Instance.GetLocalModdedPlayer()?.Inventory)
        {
            if (item.IsSpecial) continue;
            if (__instance != item.Button) continue;
            if (!HudManager._instance.ReportButton.isActiveAndEnabled) break;
            item.Use();
            return false;
        }

        if (!PlayerControl.LocalPlayer.CanMove) return false;
        if (Main.Instance.GetLocalModdedPlayer()?.Role != null)
            return Main.Instance.GetLocalModdedPlayer().Role.DoClick(__instance);
        return true;
    }
}