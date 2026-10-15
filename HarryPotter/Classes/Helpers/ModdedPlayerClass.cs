using System.Collections.Generic;
using System.Linq;
using HarryPotter.Classes.Items;
using HarryPotter.Classes.UI;
using HarryPotter.Classes.WorldItems;
using UnityEngine;

namespace HarryPotter.Classes;

public class ModdedPlayerClass
{
    public ModdedPlayerClass(PlayerControl orgPlayer, Role role, List<Item> inventory)
    {
        _Object = orgPlayer;
        Role = role;
        Inventory = inventory;
    }

    public PlayerControl _Object { get; set; }
    public Role Role { get; set; }
    public ModdedPlayerClass ControllerOverride { get; set; }
    public List<Item> Inventory { get; set; }
    public bool Immortal { get; set; }
    public bool KilledByCurse { get; set; }
    public bool CanUseConsoles { get; set; } = true;
    public bool CanSeeAllRolesOveridden { get; set; }
    public bool ReverseDirectionalControls { get; set; }
    public float SpeedMultiplier { get; set; } = 1f;
    public bool VigilanteShotEnabled { get; set; }
    public bool ShouldRevive { get; set; }

    public void Update()
    {
        if (ExileController.Instance != null)
            Role.ResetCooldowns();

        if (_Object.Data.IsDead)
            ClearItems();

        /*if (Input.GetKeyDown(KeyCode.I))
        {
            for (var i = 0; i < 5; i++) GiveItem(i);
            GiveItem(6);
            GiveItem(8);
            GiveItem(9);
        }

        if (Input.GetKeyDown(KeyCode.O)) GiveItem(5);
        if (Input.GetKeyDown(KeyCode.P)) GiveItem(7);*/

        if (HasItem(4))
            foreach (var player in PlayerControl.AllPlayerControls)
                if (player.Data.IsDead)
                    player.Visible = true;

        ShouldRevive = HasItem(9);

        TaskInfoHandler.Instance.Update();
        HandleNameColors();
        PopulateButtons();
        Role?.Update();

        if (VigilanteShotEnabled)
        {
            HudManager.Instance.KillButton.gameObject.SetActive(HudManager.Instance.ReportButton.isActiveAndEnabled);
            HudManager.Instance.KillButton.SetTarget(Main.Instance.GetClosestTarget(_Object, false));
            HudManager.Instance.KillButton.SetCoolDown(0f, 1f);
        }

        if (Input.GetKeyDown(KeyCode.Q) && VigilanteShotEnabled) HudManager.Instance.KillButton.DoClick();

        if (AmongUsClient.Instance.AmHost)
        {
            DeluminatorWorld.WorldSpawn();
            MaraudersMapWorld.WorldSpawn();
            PortKeyWorld.WorldSpawn();
            TheGoldenSnitchWorld.WorldSpawn();
            GhostStoneWorld.WorldSpawn();
            ButterBeerWorld.WorldSpawn();
            ElderWandWorld.WorldSpawn();
            BasWorldItem.WorldSpawn();
            SortingHatWorld.WorldSpawn();
            PhiloStoneWorld.WorldSpawn();

            if (Main.Instance.Config.OrderOfTheImp)
                if (Main.Instance.AllPlayers.Any(x =>
                        Main.Instance.IsPlayerRole(x, "Harry") &&
                        (x._Object.Data.IsDead || x._Object.Data.Disconnected)) &&
                    Main.Instance.AllPlayers.Any(x =>
                        Main.Instance.IsPlayerRole(x, "Hermione") &&
                        (x._Object.Data.IsDead || x._Object.Data.Disconnected)) &&
                    Main.Instance.AllPlayers.Any(x =>
                        Main.Instance.IsPlayerRole(x, "Ron") && (x._Object.Data.IsDead || x._Object.Data.Disconnected)))
                    GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByKill, false);
        }
    }

    public void HandleNameColors()
    {
        /*if (_Object.Data.IsDead && !CanSeeAllRolesOveridden)
        {
            foreach (ModdedPlayerClass moddedPlayer in Main.Instance.AllPlayers)
            {
                if (moddedPlayer.Role == null) continue;

                Main.Instance.SetNameColor(moddedPlayer._Object, moddedPlayer.Role.RoleColor);
                moddedPlayer._Object.cosmetics.nameText.transform.position = new Vector3(
                    moddedPlayer._Object.cosmetics.nameText.transform.position.x,
                    moddedPlayer._Object.transform.position.y + 0.8f,
                    moddedPlayer._Object.cosmetics.nameText.transform.position.z);
                moddedPlayer._Object.cosmetics.nameText.text =
                    moddedPlayer._Object.Data.PlayerName + "\n" + moddedPlayer.Role.RoleName;
            }
        }*/

        if (Role == null)
        {
            _Object.cosmetics.nameText.text = _Object.Data.PlayerName + "\n" + (_Object.Data.Role.IsImpostor
                ? ModTranslation.getString("TeamImpostor")
                : ModTranslation.getString("TeamMuggle"));
            _Object.cosmetics.nameText.transform.position = new Vector3(
                _Object.cosmetics.nameText.transform.position.x,
                _Object.transform.position.y + 0.8f,
                _Object.cosmetics.nameText.transform.position.z);
            return;
        }

        Main.Instance.SetNameColor(_Object, Role.RoleColor);
        _Object.cosmetics.nameText.text =
            _Object.Data.PlayerName + "\n" + ModTranslation.getString(Role.RoleNameTranslation);
        _Object.cosmetics.nameText.transform.position = new Vector3(
            _Object.cosmetics.nameText.transform.position.x,
            _Object.transform.position.y + 0.8f,
            _Object.cosmetics.nameText.transform.position.z);

        if (_Object.Data.Role.IsImpostor)
            foreach (var moddedPlayer in Main.Instance.AllPlayers)
            {
                if (moddedPlayer._Object.AmOwner)
                    continue;

                if (!moddedPlayer._Object.Data.Role.IsImpostor)
                    continue;

                if (moddedPlayer.Role == null)
                    continue;

                moddedPlayer._Object.cosmetics.nameText.transform.position = new Vector3(
                    moddedPlayer._Object.cosmetics.nameText.transform.position.x,
                    moddedPlayer._Object.transform.position.y + 0.8f,
                    moddedPlayer._Object.cosmetics.nameText.transform.position.z);
                moddedPlayer._Object.cosmetics.nameText.text =
                    moddedPlayer._Object.Data.PlayerName + "\n" +
                    ModTranslation.getString(moddedPlayer.Role.RoleNameTranslation);
            }
    }

    public void PopulateButtons()
    {
        float itemCount = 0;
        foreach (var item in Inventory)
        {
            //if (item.IsSpecial) continue;
            item.DrawIcon(
                PlayerControl.LocalPlayer.Data.Role.IsImpostor
                    ? HudManager.Instance.SabotageButton.graphic.bounds.max.x - 0.375f - itemCount * 0.6f
                    : HudManager.Instance.ReportButton.graphic.bounds.max.x - 0.375f - itemCount * 0.6f,
                PlayerControl.LocalPlayer.Data.Role.IsImpostor
                    ? HudManager.Instance.SabotageButton.graphic.bounds.max.y + 0.375f
                    : HudManager.Instance.ReportButton.graphic.bounds.max.y + 0.375f,
                HudManager.Instance.KillButton.transform.position.z);
            itemCount++;
        }
    }

    public bool HasItem(int id)
    {
        return Inventory.FindAll(x => x.Id == id).Count > 0;
    }

    public void GiveItem(int id)
    {
        Item item = null;

        switch (id)
        {
            case 0:
                item = new Deluminator(this);
                Inventory.Add(item);
                break;
            case 1:
                item = new MaraudersMap(this);
                Inventory.Add(item);
                break;
            case 2:
                item = new PortKey(this);
                Inventory.Add(item);
                break;
            case 3:
                item = new TheGoldenSnitch(this);
                Inventory.Add(item);
                break;
            case 4:
                item = new GhostStone(this);
                Inventory.Add(item);
                break;
            case 5:
                item = new ButterBeer(this);
                Inventory.Add(item);
                break;
            case 6:
                item = new ElderWand(this);
                Inventory.Add(item);
                break;
            case 7:
                item = new BasItem(this);
                Inventory.Add(item);
                break;
            case 8:
                item = new SortingHat(this);
                Inventory.Add(item);
                break;
            case 9:
                item = new PhiloStone(this);
                Inventory.Add(item);
                break;
        }

        if (item == null) return;
        if (item.IsTrap) item.Use();

        var trapText = ModTranslation.getString("trapText");
        var normalText = ModTranslation.getString("normalText");

        PopupTMPHandler.Instance.CreatePopup(item.IsTrap ? trapText : normalText, Color.white, Color.black);
    }

    public void ClearItems()
    {
        while (Inventory.Count > 0)
            Inventory[0].Delete();
    }
}