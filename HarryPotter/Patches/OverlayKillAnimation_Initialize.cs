using HarmonyLib;
using HarryPotter.Classes;
using UnityEngine;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.Initialize))]
public class OverlayKillAnimation_Begin
{
    private static void Prefix(OverlayKillAnimation __instance, KillOverlayInitData initData)
    {
        if (initData.victimOutfit == null || initData.killerOutfit == null || __instance == null) return;
        if (initData.victimOutfit != initData.killerOutfit) return;
        var harry = Main.Instance.FindPlayerOfRole("Harry");
        if (harry == null) return;

        initData.killerOutfit = harry._Object.Data.DefaultOutfit;
        __instance.killerParts.cosmetics.hat.transform.localScale = new Vector3(0.4f, 0.4f);
        __instance.killerParts.cosmetics.skin.transform.localScale = new Vector3(0.4f, 0.4f);
        __instance.killerParts.cosmetics.currentBodySprite.BodySprite.transform.localScale = new Vector3(0.4f, 0.4f);
    }
}