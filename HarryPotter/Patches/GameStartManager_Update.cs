using HarmonyLib;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class GameStartManager_Update
{
    private static void Postfix(GameStartManager __instance)
    {
        //CustomOverlay.resetOverlays();
        //__instance.GameRoomName.transform.localPosition = new Vector3(0.75f, 4.15f);
        //__instance.HostPublicButton.transform.position = new Vector3(__instance.HostPublicButton.transform.position.x, __instance.HostPublicButton.transform.position.y, 0);
        //__instance.PlayerCounter.transform.localPosition = new Vector3(0.3f, -1.1f);
        //__instance.StartButton.transform.localPosition = new Vector3(0f, -0.4f);
    }
}