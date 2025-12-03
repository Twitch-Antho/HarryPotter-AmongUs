using HarmonyLib;
using HarryPotter.Classes;
using HarryPotter.Classes.UI;
using UnityEngine;

namespace HarryPotter.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
    public static class PlayerControl_RpcMurderPlayer
    {
        static bool Prefix(PlayerControl __instance, PlayerControl __0)
        {
            if (__instance == null || __0 == null)
                return true; // fallback to original method

            var attacker = Main.Instance.ModdedPlayerById(__instance.PlayerId);
            var target = Main.Instance.ModdedPlayerById(__0.PlayerId);

            if (attacker == null)
                return true;

            // Vigilante shot
            if (attacker.VigilanteShotEnabled)
            {
                attacker.VigilanteShotEnabled = false;
                if (HudManager.Instance?.KillButton != null)
                    HudManager.Instance.KillButton.gameObject.SetActive(false);
            }

            // Target immortal
            if (target != null && target.Immortal)
            {
                if (PopupTMPHandler.Instance != null)
                    PopupTMPHandler.Instance.CreatePopup(
                        "When using his ability, Ron cannot be killed.\nYour cooldown was reset.",
                        Color.white,
                        Color.black
                    );

                PlayerControl.LocalPlayer?.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
                attacker.Role?.ResetCooldowns();
                return false; // block native kill
            }

            // Kill player manually
            __instance.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
            if (target != null)
                Main.Instance.RpcKillPlayer(__instance, __0, false, true);

            return false; // block native method
        }
    }
}
