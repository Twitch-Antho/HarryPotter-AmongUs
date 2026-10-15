using System;
using System.Linq;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Random = System.Random;

namespace HarryPotter.Classes.WorldItems;

public class TheGoldenSnitchWorld : WorldItem
{
    private double _time;

    public TheGoldenSnitchWorld(Vector2 position, Vector2 velocity)
    {
        Position = position;
        Velocity = velocity;
        Id = 3;
        Icon = Main.Instance.Assets.WorldItemIcons[Id];
        Name = "The Golden Snitch";
    }

    public static Random ItemRandom { get; set; } = new();
    public static float ItemSpawnChance { get; set; } = 30;
    public static bool HasSpawned { get; set; }
    public Vector2 Velocity { get; set; }

    public override void DrawWorldIcon()
    {
        if (ItemWorldObject == null)
        {
            System.Console.WriteLine("Creating new Item: " + Name);
            ItemWorldObject = new GameObject();
            ItemWorldObject.AddComponent<SpriteRenderer>();
            ItemWorldObject.SetActive(true);

            var itemRender = ItemWorldObject.GetComponent<SpriteRenderer>();
            itemRender.enabled = true;
            itemRender.sprite = Icon;
            itemRender.transform.localScale = new Vector2(0.5f, 0.5f);
            ItemWorldObject.transform.position = Position;

            var itemRigid = ItemWorldObject.AddComponent<Rigidbody2D>();
            var itemCollider = ItemWorldObject.AddComponent<BoxCollider2D>();

            itemCollider.autoTiling = false;
            itemCollider.edgeRadius = 0;
            itemCollider.size = Icon.bounds.size * 0.5f;
            itemCollider.sharedMaterial = Main.Instance.Assets.SnitchMaterial;
            ItemWorldObject.layer = 8;

            itemRigid.velocity = Velocity;
        }

        var itemRigid2 = ItemWorldObject.GetComponent<Rigidbody2D>();
        itemRigid2.fixedAngle = true;
        itemRigid2.drag = 0;
        itemRigid2.angularDrag = 0;
        itemRigid2.inertia = 0;
        itemRigid2.gravityScale = 0;

        _time += Time.deltaTime;

        var angle = (float)(25 + 25 * Math.Sin(_time));
        itemRigid2.rotation = angle - 32.5f;
    }

    public static void WorldSpawn()
    {
        if (!CanSpawn())
            return;

        if (!ShipStatus.Instance)
            return;

        var pos = Main.Instance.GetAllApplicableItemPositions().Random();
        Main.Instance.RpcSpawnItem(3, pos);
        HasSpawned = true;
    }

    public static bool CanSpawn()
    {
        if (Main.Instance.AllItems.Where(x => x.Id == 3).ToList().Count > 0) return false;
        if (MeetingHud.Instance) return false;
        if (!AmongUsClient.Instance.IsGameStarted) return false;
        if (ItemRandom.Next(0, 100000) > ItemSpawnChance) return false;
        if (Main.Instance.CurrentStage < 2) return false;
        if (HasSpawned) return false;

        return true;
    }
}