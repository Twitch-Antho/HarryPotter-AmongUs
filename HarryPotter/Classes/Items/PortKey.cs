using HarryPotter.Classes.WorldItems;
using UnityEngine;

namespace HarryPotter.Classes.Items;

public class PortKey : Item
{
    public PortKey(ModdedPlayerClass owner)
    {
        Owner = owner;
        ParentInventory = owner.Inventory;
        Id = 2;
        Icon = Main.Instance.Assets.ItemIcons[Id];
        Name = "Port Key";
        Tooltip = ModTranslation.getString("PortKeyTooltip");
    }

    public override void Use()
    {
        if (AmongUsClient.Instance.AmHost)
        {
            PortKeyWorld.HasSpawned = false;
        }
        else
        {
            var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.UseItem);
            writer.Write(Id);
            writer.EndMessage();
        }

        Delete();

        Main.Instance.RpcTeleportPlayer(Owner._Object,
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4
                ? new Vector2(7.620923f, 15.0479f)
                : ShipStatus.Instance.MeetingSpawnCenter);
    }
}