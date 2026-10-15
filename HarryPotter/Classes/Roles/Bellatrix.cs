using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HarryPotter.Classes.Roles;

public class Bellatrix : Role
{
    public Bellatrix(ModdedPlayerClass owner)
    {
        RoleNameTranslation = "RoleNameBellatrix";
        RoleName = "Bellatrix";
        RoleColor = Palette.ImpostorRed;
        RoleColor2 = Palette.ImpostorRed;
        IntroString = ModTranslation.getString("IntroStringBellatrix");
        TaskText = $"{ModTranslation.getString("CrucioTooltipText")}\n" +
                   $"{ModTranslation.getString("ImperioCrucioTooltipText")}";
        Owner = owner;
        MarkedPlayers = new List<PlayerControl>();

        if (!Owner._Object.AmOwner)
            return;

        CrucioButton = Object.Instantiate(HudManager.Instance.KillButton);
        CrucioButton.graphic.enabled = true;

        MindControlButton = Object.Instantiate(HudManager.Instance.KillButton);
        MindControlButton.graphic.enabled = true;

        //MarkButton = UnityEngine.Object.Instantiate(HudManager.Instance.KillButton);
        //MarkButton.graphic.enabled = true;

        ControlMenu = new MindControlMenu();
    }

    public KillButton CrucioButton { get; set; }

    public KillButton MindControlButton { get; set; }

    //public KillButton MarkButton { get; set; }
    public MindControlMenu ControlMenu { get; set; }
    public ModdedPlayerClass MindControlledPlayer { get; set; }
    public List<PlayerControl> MarkedPlayers { get; set; }
    public DateTime LastCrucio { get; set; }
    public DateTime LastMark { get; set; }

    public override void RemoveCooldowns()
    {
        LastCrucio = DateTime.UtcNow.AddSeconds(Main.Instance.Config.CrucioCooldown * -1);
        LastMark = DateTime.UtcNow.AddSeconds(10 * -1);
        Owner._Object.SetKillTimer(0);
    }

    public override void ResetCooldowns()
    {
        LastCrucio = DateTime.UtcNow;
        LastMark = DateTime.UtcNow;
    }

    public override void Update()
    {
        if (!Owner._Object.AmOwner)
            return;

        if (!HudManager.Instance)
            return;

        if (Owner._Object.inVent || Owner._Object.Data.IsDead || MeetingHud.Instance ||
            !MindControlButton.isActiveAndEnabled || MindControlButton.isCoolingDown)
            ControlMenu?.CloseMenu();

        MarkedPlayers.RemoveAll(x => x.Data.IsDead || x.Data.Disconnected);
        foreach (var player in MarkedPlayers)
        {
            player.cosmetics?.currentBodySprite?.BodySprite?.material?.SetFloat("_Outline", 1f);
            player.cosmetics?.currentBodySprite?.BodySprite?.material?.SetColor("_OutlineColor", Color.yellow);
        }

        DrawButtons();

        if (MindControlledPlayer != null)
            return;

        ControlMenu.Update();

        if (Input.GetMouseButtonDown(1)) CastCrucio();
    }

    public override bool ShouldDrawCustomButtons()
    {
        return HudManager.Instance.ReportButton.isActiveAndEnabled && MindControlledPlayer == null;
    }

    public override bool DoClick(KillButton __instance)
    {
        if (__instance == CrucioButton)
            return false;

        if (__instance == MindControlButton)
            ToggleMindControlMenu();
        //else if (__instance == MarkButton)
        //    MarkPlayer();
        else
            return true;

        return false;
    }

    public void ToggleMindControlMenu()
    {
        if (MindControlButton.isCoolingDown)
            return;

        if (!MindControlButton.isActiveAndEnabled)
            return;

        if (Owner._Object.Data.IsDead)
            return;

        if (Owner._Object.inVent)
            return;

        ControlMenu?.ToggleMenu();
    }

    /*public void MarkPlayer()
    {
        if (MarkButton.isCoolingDown)
            return;

        if (!MarkButton.isActiveAndEnabled)
            return;

        if (Owner._Object.Data.IsDead)
            return;

        if (MarkButton.currentTarget == null)
            return;

        if (MarkedPlayers.Contains(MarkButton.currentTarget))
            return;

        LastMark = DateTime.UtcNow;
        MarkedPlayers.Add(MarkButton.currentTarget);
    }*/

    public void CastCrucio()
    {
        if (CrucioButton.isCoolingDown)
            return;

        if (!CrucioButton.isActiveAndEnabled)
            return;

        if (Owner._Object.Data.IsDead)
            return;

        if (Owner._Object.inVent && !Main.Instance.Config.SpellsInVents)
            return;

        LastCrucio = DateTime.UtcNow;
        var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Main.Instance.RpcCreateCrucio(mouseWorld, Owner);
    }

    public void DrawButtons()
    {
        Vector2 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0));

        CrucioButton.gameObject.SetActive(ShouldDrawCustomButtons());
        CrucioButton.graphic.sprite = Main.Instance.Assets.AbilityIcons[1];
        CrucioButton.buttonLabelText.text = ModTranslation.getString("ButtonTextCrucio");
        CrucioButton.transform.position = new Vector2(bottomLeft.x + 0.75f, bottomLeft.y + 0.75f);
        CrucioButton.SetTarget(null);
        CrucioButton.SetCoolDown(
            Main.Instance.Config.CrucioCooldown - (float)(DateTime.UtcNow - LastCrucio).TotalSeconds,
            Main.Instance.Config.CrucioCooldown);

        MindControlButton.gameObject.SetActive(ShouldDrawCustomButtons());
        MindControlButton.graphic.sprite = Main.Instance.Assets.AbilityIcons[2];
        MindControlButton.buttonLabelText.text = ModTranslation.getString("ButtonTextImperio");
        MindControlButton.transform.position = new Vector2(bottomLeft.x + MindControlButton.graphic.size.x + 0.75f,
            bottomLeft.y + 0.75f);
        MindControlButton.SetTarget(null);
        MindControlButton.SetCoolDown(Owner._Object.killTimer,
            GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown);

        /*MarkButton.gameObject.SetActive(ShouldDrawCustomButtons());
        MarkButton.graphic.sprite = Main.Instance.Assets.AbilityIcons[6];
        MarkButton.buttonLabelText.text = ModTranslation.getString("ButtonTextMarkVictim");
        MarkButton.transform.position = new Vector2(bottomLeft.x + MindControlButton.graphic.size.x + MarkButton.graphic.size.x + 0.75f, bottomLeft.y + 0.75f);
        MarkButton.SetTarget(Main.Instance.GetClosestTarget(Owner._Object, true, MarkedPlayers.ToArray()));
        MarkButton.SetCoolDown(10f - (float)(DateTime.UtcNow - LastMark).TotalSeconds, 10f);*/

        var isDead = Owner._Object.Data.IsDead;
        if (isDead)
            MindControlButton.SetCoolDown(0, 1);

        if (!MindControlButton.isCoolingDown && !isDead)
        {
            MindControlButton.graphic.material.SetFloat("_Desat", 0f);
            MindControlButton.graphic.color = Palette.EnabledColor;
            MindControlButton.buttonLabelText.color = Palette.EnabledColor;
        }

        if (!CrucioButton.isCoolingDown && !isDead)
        {
            CrucioButton.graphic.material.SetFloat("_Desat", 0f);
            CrucioButton.graphic.color = Palette.EnabledColor;
            CrucioButton.buttonLabelText.color = Palette.EnabledColor;
        }
    }
}