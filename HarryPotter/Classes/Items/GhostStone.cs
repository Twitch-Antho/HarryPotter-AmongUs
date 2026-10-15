namespace HarryPotter.Classes.Items;

public class GhostStone : Item
{
    public GhostStone(ModdedPlayerClass owner)
    {
        Owner = owner;
        ParentInventory = owner.Inventory;
        Id = 4;
        Icon = Main.Instance.Assets.WorldItemIcons[Id];
        Name = "Resurrection Stone";
        Tooltip = ModTranslation.getString("GhostStoneTooltip");
        IsSpecial = true;
    }
}