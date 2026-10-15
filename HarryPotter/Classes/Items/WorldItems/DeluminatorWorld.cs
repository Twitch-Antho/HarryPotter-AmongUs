using System;
using System.Linq;
using Reactor.Utilities.Extensions;
using Vector2 = UnityEngine.Vector2;

namespace HarryPotter.Classes.WorldItems;

public class DeluminatorWorld : WorldItem
{
    public DeluminatorWorld(Vector2 position)
    {
        Position = position;
        Id = 0;
        Icon = Main.Instance.Assets.WorldItemIcons[Id];
        Name = "Deluminator";
    }

    public static Random ItemRandom { get; set; } = new();
    public static float ItemSpawnChance { get; set; } = 30;
    public static bool HasSpawned { get; set; }

    public static void WorldSpawn()
    {
        if (!CanSpawn())
            return;

        if (!ShipStatus.Instance)
            return;

        var pos = Main.Instance.GetAllApplicableItemPositions().Random();
        Main.Instance.RpcSpawnItem(0, pos);
        HasSpawned = true;
    }

    public static bool CanSpawn()
    {
        if (Main.Instance.AllItems.Where(x => x.Id == 0).ToList().Count > 0) return false;
        if (MeetingHud.Instance) return false;
        if (!AmongUsClient.Instance.IsGameStarted) return false;
        if (ItemRandom.Next(0, 100000) > ItemSpawnChance) return false;
        if (HasSpawned) return false;
        if (ModHelpers.isFungle()) return false;

        return true;
    }
}