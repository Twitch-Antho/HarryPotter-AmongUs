using Hazel;
using System;
using HarryPotter.Classes.WorldItems;

namespace HarryPotter.Classes.Items;

public class Deluminator : Item
{
    public Deluminator(ModdedPlayerClass owner)
    {
        Owner = owner;
        ParentInventory = owner.Inventory;
        Id = 0;
        Icon = Main.Instance.Assets.ItemIcons[Id];
        Name = "Deluminator";
        Tooltip = ModTranslation.getString("DeluminatorTooltip");
    }

    public override void Use()
    {
        if (AmongUsClient.Instance.AmHost)
        {
            DeluminatorWorld.HasSpawned = false;
        }
        else
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)Packets.UseItem, SendOption.Reliable);
            writer.Write(Id);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        Delete();

        switch (Main.Instance.IsLightsSabotaged())
        {
            case true:
                var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                var writer =
                    AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)Packets.FixLightsRpc, SendOption.Reliable);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                break;
            case false:
                byte b = 4;
                for (var i = 0; i < 5; i++)
                    if (new Random().Next(0, 2) == 0)
                        b |= (byte)(1 << i);
                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)(b | 128));
                break;
        }
    }
}