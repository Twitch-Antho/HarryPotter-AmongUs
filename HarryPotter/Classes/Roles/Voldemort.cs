using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HarryPotter.Classes.Roles;

public class Voldemort : Role
{
    public Voldemort(ModdedPlayerClass owner)
    {
        RoleNameTranslation = "RoleNameVoldemort";
        RoleName = "Voldemort";
        RoleColor = Palette.ImpostorRed;
        RoleColor2 = Palette.ImpostorRed;
        IntroString = ModTranslation.getString("IntroStringVoldemort");
        TaskText = ModTranslation.getString("TheKillingCurseTooltipText");
        Owner = owner;

        if (!Owner._Object.AmOwner)
            return;

        CurseButton = Object.Instantiate(HudManager.Instance.KillButton);
        CurseButton.graphic.enabled = true;
    }

    public KillButton CurseButton { get; set; }
    public DateTime LastCurse { get; set; }

    public override void Update()
    {
        if (!Owner._Object.AmOwner)
            return;

        if (!HudManager.Instance)
            return;

        Vector2 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0));

        CurseButton.gameObject.SetActive(HudManager.Instance.KillButton.isActiveAndEnabled);
        CurseButton.graphic.sprite = Main.Instance.Assets.AbilityIcons[0];
        CurseButton.buttonLabelText.text = ModTranslation.getString("ButtonTextAvadacadavra");
        CurseButton.transform.position = new Vector2(bottomLeft.x + 0.75f, bottomLeft.y + 0.75f);
        CurseButton.SetTarget(null);
        if (Main.Instance.Config.SeparateCooldowns)
            CurseButton.SetCoolDown(
                GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown -
                (float)(DateTime.UtcNow - LastCurse).TotalSeconds,
                GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);
        else
            CurseButton.SetCoolDown(Owner._Object.killTimer,
                GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);

        var isDead = Owner._Object.Data.IsDead;
        if (isDead)
            CurseButton.SetCoolDown(0, 1);

        if (!CurseButton.isCoolingDown && !isDead)
        {
            CurseButton.graphic.material.SetFloat("_Desat", 0f);
            CurseButton.graphic.color = Palette.EnabledColor;
            CurseButton.buttonLabelText.color = Palette.EnabledColor;
        }

        if (Input.GetMouseButtonDown(1))
            ShootCurse();
    }

    public override void RemoveCooldowns()
    {
        //LastCurse = DateTime.UtcNow.AddSeconds(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * -1);
        //Owner._Object.SetKillTimer(0);

        if (Main.Instance.Config.SeparateCooldowns)
            LastCurse = DateTime.UtcNow.AddSeconds(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown *
                                                   -1);
        else
            Owner._Object.SetKillTimer(0);
    }

    public override void ResetCooldowns()
    {
        if (Main.Instance.Config.SeparateCooldowns)
            LastCurse = DateTime.UtcNow;
        else
            Owner._Object.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);
    }

    public void ShootCurse()
    {
        if (CurseButton.isCoolingDown)
            return;

        if (!CurseButton.isActiveAndEnabled)
            return;

        if (Owner._Object.Data.IsDead)
            return;

        if (Owner._Object.inVent && !Main.Instance.Config.SpellsInVents)
            return;

        ResetCooldowns();
        var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Main.Instance.RpcCreateCurse(mouseWorld, Owner);
    }

    public override bool DoClick(KillButton __instance)
    {
        return __instance != CurseButton;
    }
}