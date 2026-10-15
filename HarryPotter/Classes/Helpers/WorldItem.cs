using Reactor.Utilities.Extensions;
using UnityEngine;

namespace HarryPotter.Classes;

public class WorldItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Sprite Icon { get; set; }
    public Vector2 Position { get; set; }
    public GameObject ItemWorldObject { get; set; }
    public bool IsPickedUp { get; set; }

    public void Update()
    {
        if (ItemWorldObject == null) return;
        if (PlayerControl.LocalPlayer.Data.IsDead) return;
        if (PlayerControl.LocalPlayer.Data.Disconnected) return;
        if (Main.Instance.GetLocalModdedPlayer().HasItem(Id)) return;
        if (!ItemWorldObject.GetComponent<SpriteRenderer>().bounds
                .Intersects(PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.bounds)) return;

        PickUp();
    }

    public virtual void DrawWorldIcon()
    {
        if (ItemWorldObject == null)
        {
            ItemWorldObject = new GameObject();
            ItemWorldObject.AddComponent<SpriteRenderer>();
            ItemWorldObject.SetActive(true);
        }

        var itemRender = ItemWorldObject.GetComponent<SpriteRenderer>();
        itemRender.enabled = true;
        itemRender.sprite = Icon;
        itemRender.transform.localScale = new Vector2(0.5f, 0.5f);
        ItemWorldObject.transform.position = Position;
    }

    public void PickUp()
    {
        if (AmongUsClient.Instance.AmHost)
        {
            Main.Instance.GiveGrabbedItem(Id);
        }
        else
        {
            Delete();
            var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.TryPickupItem);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(Id);
            writer.EndMessage();
        }
    }

    public void Delete()
    {
        IsPickedUp = true;
        if (ItemWorldObject != null) ItemWorldObject.Destroy();

        if (AmongUsClient.Instance.AmHost)
        {
            var writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.DestroyItem);
            writer.Write(Id);
            writer.EndMessage();
        }
    }
}