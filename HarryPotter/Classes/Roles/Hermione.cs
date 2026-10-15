using System;
using UnityEngine;

namespace HarryPotter.Classes.Roles;

public class Hermione : Role
{
    public Hermione(ModdedPlayerClass owner)
    {
        RoleNameTranslation = "RoleNameHermione";
        RoleName = "Hermione";
        RoleColor = Palette.Orange;
        RoleColor2 = Palette.Orange;
        IntroString = ModTranslation.getString("IntroStringHermione");
        TaskText = string.Format(ModTranslation.getString("HermioneTooltipText"), Main.Instance.Config.HourglassTimer);
        Owner = owner;

        if (!Owner._Object.AmOwner)
            return;

        HourglassButton = KillButton.Instantiate(HudManager.Instance.KillButton);
        HourglassButton.graphic.enabled = true;
    }

    public KillButton HourglassButton { get; set; }
    public DateTime LastHourglass { get; set; }

    public override void ResetCooldowns()
    {
        LastHourglass = DateTime.UtcNow;
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
        if (__instance == HourglassButton)
        {
            Main.Instance.isActivateHourglass = true;
            ActivateHourglass();
        }
        else
        {
            return true;
        }

        return false;
    }

    public void ActivateHourglass()
    {
        if (HourglassButton.isCoolingDown)
            return;

        if (!HourglassButton.isActiveAndEnabled)
            return;

        if (Owner._Object.Data.IsDead)
            return;

        if (Owner.ControllerOverride != null)
            return;

        Main.Instance.UseHourglass(Owner._Object);
    }

    public void DrawButtons()
    {
        Vector2 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0));

        HourglassButton.gameObject.SetActive(HudManager.Instance.ReportButton.isActiveAndEnabled);
        HourglassButton.graphic.sprite = Main.Instance.Assets.AbilityIcons[5];
        HourglassButton.buttonLabelText.text = ModTranslation.getString("ButtonTextTimeTurner");
        HourglassButton.transform.position = new Vector2(bottomLeft.x + 0.75f, bottomLeft.y + 0.75f);
        HourglassButton.SetTarget(null);
        HourglassButton.SetCoolDown(
            Main.Instance.Config.HourglassCooldown - (float)(DateTime.UtcNow - LastHourglass).TotalSeconds,
            Main.Instance.Config.HourglassCooldown);

        var isDead = Owner._Object.Data.IsDead;
        if (isDead)
            HourglassButton.SetCoolDown(0, 1);

        if (!HourglassButton.isCoolingDown && !isDead)
        {
            HourglassButton.graphic.material.SetFloat("_Desat", 0f);
            HourglassButton.graphic.color = Palette.EnabledColor;
            HourglassButton.buttonLabelText.color = Palette.EnabledColor;
        }
    }
}