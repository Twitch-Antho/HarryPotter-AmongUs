using HarmonyLib;
using HarryPotter.Classes;
using HarryPotter.Classes.Roles;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
internal class PlayerPhysics_FixedUpdate
{
    private static bool Prefix(PlayerPhysics __instance)
    {
        var moddedController = Main.Instance.ModdedPlayerById(__instance.myPlayer.PlayerId);
        if (__instance.myPlayer != PlayerControl.LocalPlayer || moddedController?.Role?.RoleName != "Bellatrix" ||
            ((Bellatrix)moddedController.Role).MindControlledPlayer == null) return true;
        var controlledPlayer = ((Bellatrix)moddedController.Role).MindControlledPlayer._Object.MyPhysics;
        var vel = (HudManager.Instance.joystick.DeltaL + HudManager.Instance.joystick.DeltaR) * __instance.TrueSpeed;
        controlledPlayer.body.velocity = vel;
        var writer =
            AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.MoveControlledPlayer);
        writer.Write(controlledPlayer.myPlayer.PlayerId);
        writer.Write(vel.x);
        writer.Write(vel.y);
        writer.Write(controlledPlayer.body.position.x);
        writer.Write(controlledPlayer.body.position.y);
        writer.EndMessage();
        return false;
    }

    private static void Postfix(PlayerPhysics __instance)
    {
        if (__instance.AmOwner && Main.Instance.GetLocalModdedPlayer() != null)
        {
            __instance.body.velocity *= Main.Instance.GetLocalModdedPlayer().ReverseDirectionalControls ? -1f : 1f;
            __instance.body.velocity *= Main.Instance.GetLocalModdedPlayer().SpeedMultiplier;
        }
    }
}