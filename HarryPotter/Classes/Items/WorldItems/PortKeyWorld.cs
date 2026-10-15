using System.Linq;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Random = System.Random;

namespace HarryPotter.Classes.WorldItems;

public class PortKeyWorld : WorldItem
{
    public PortKeyWorld(Vector2 position)
    {
        Position = position;
        Id = 2;
        Icon = Main.Instance.Assets.WorldItemIcons[Id];
        Name = "Port Key";
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
        Main.Instance.RpcSpawnItem(2, pos);
        HasSpawned = true;
    }

    public static bool CanSpawn()
    {
        if (Main.Instance.AllItems.Where(x => x.Id == 2).ToList().Count > 0) return false;
        if (MeetingHud.Instance) return false;
        if (!AmongUsClient.Instance.IsGameStarted) return false;
        if (ItemRandom.Next(0, 100000) > ItemSpawnChance) return false;
        if (HasSpawned) return false;

        return true;
    }
}