using System;
using UnityEngine;

namespace HarryPotter.Classes.Roles;

public class Ron : Role
{
    public Ron(ModdedPlayerClass owner)
    {
        RoleNameTranslation = "RoleNameRon";
        RoleName = "Ron";
        RoleColor = Palette.Orange;
        RoleColor2 = Palette.Orange;
        IntroString = ModTranslation.getString("IntroStringRon");
        TaskText = string.Format(ModTranslation.getString("DefensiveDuelistTooltipText"),
            Main.Instance.Config.DefensiveDuelistDuration);
        Owner = owner;

        if (!Owner._Object.AmOwner)
            return;

        DDButton = KillButton.Instantiate(HudManager.Instance.KillButton);
        DDButton.graphic.enabled = true;
    }

    public KillButton DDButton { get; set; }
    public DateTime LastCloak { get; set; }

    public override void ResetCooldowns()
    {
        LastCloak = DateTime.UtcNow;
    }

    public override void Update()
    {
        if (!Owner._Object.AmOwner)
            return;

        if (!HudManager.Instance)
            return;

        DrawButtons();
    }

    public override bool DoClick(KillButton __instance)
    {
        if (__instance == DDButton)
            ActivateDefensiveDuelist();
        else
            return true;

        return false;
    }

    public void ActivateDefensiveDuelist()
    {
        if (DDButton.isCoolingDown)
            return;

        if (!DDButton.isActiveAndEnabled)
            return;

        if (Owner._Object.Data.IsDead)
            return;

        if (Owner.ControllerOverride != null)
            return;

        ResetCooldowns();
        Main.Instance.RpcDefensiveDuelist(Owner._Object);
    }

    public void DrawButtons()
    {
        Vector2 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0));

        DDButton.gameObject.SetActive(HudManager.Instance.ReportButton.isActiveAndEnabled);
        DDButton.graphic.sprite = Main.Instance.Assets.AbilityIcons[3];
        DDButton.buttonLabelText.text = ModTranslation.getString("ButtonTextDefense");
        DDButton.transform.position = new Vector2(bottomLeft.x + 0.75f, bottomLeft.y + 0.75f);
        DDButton.SetTarget(null);
        DDButton.SetCoolDown(
            Main.Instance.Config.DefensiveDuelistCooldown - (float)(DateTime.UtcNow - LastCloak).TotalSeconds,
            Main.Instance.Config.DefensiveDuelistCooldown);

        var isDead = Owner._Object.Data.IsDead;
        if (isDead)
            DDButton.SetCoolDown(0, 1);

        if (!DDButton.isCoolingDown && !isDead)
        {
            DDButton.graphic.material.SetFloat("_Desat", 0f);
            DDButton.graphic.color = Palette.EnabledColor;
            DDButton.buttonLabelText.color = Palette.EnabledColor;
        }
    }
}