using AmongUs.GameOptions;
using HarmonyLib;
using HarryPotter.Classes;
using UnityEngine;

namespace HarryPotter.Patches;

[HarmonyPatch(typeof(IntroCutscene._ShowTeam_d__38), nameof(IntroCutscene._ShowTeam_d__38.MoveNext))]
public static class IntroCutscene_ShowTeam__d_MoveNext
{
    public static void Prefix(IntroCutscene._ShowTeam_d__38 __instance)
    {
        if (GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek ||
            !Main.Instance.Config.EnableModRole) return;
        var localPlayer = Main.Instance.GetLocalModdedPlayer();
        //if (!localPlayer._Object.Data.Role.IsImpostor) __instance.__4__this.TeamTitle.text = ModTranslation.getString("TeamMuggle");
        __instance.__4__this.ImpostorText.text = ModTranslation.getString("ImpostorTextHP");
        __instance.__4__this.ImpostorText.gameObject.SetActive(true);
        __instance.__4__this.ImpostorText.transform.localScale = new Vector3(0.7f, 0.7f);
    }
}

[HarmonyPatch(typeof(IntroCutscene._ShowRole_d__41), nameof(IntroCutscene._ShowRole_d__41.MoveNext))]
public static class IntroCutscene_ShowRole_d__24
{
    public static void Postfix(IntroCutscene._ShowRole_d__41 __instance)
    {
        if (GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek ||
            !Main.Instance.Config.EnableModRole) return;
        var localPlayer = Main.Instance.GetLocalModdedPlayer();
        if (localPlayer.Role == null) return;
        localPlayer.Role.ResetCooldowns();
        __instance.__4__this.RoleBlurbText.transform.localScale = new Vector3(0.7f, 0.7f);
        __instance.__4__this.RoleText.text = ModTranslation.getString(localPlayer.Role.RoleNameTranslation);
        __instance.__4__this.RoleText.color = localPlayer.Role.RoleColor;
        __instance.__4__this.RoleBlurbText.text = localPlayer.Role.IntroString;
        __instance.__4__this.RoleBlurbText.color = localPlayer.Role.RoleColor;
        __instance.__4__this.YouAreText.color = Color.white;
        __instance.__4__this.BackgroundBar.material.color = localPlayer.Role.RoleColor2;
    }
}