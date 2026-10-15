namespace HarryPotter.Classes.Items;

public class PhiloStone : Item
{
    public PhiloStone(ModdedPlayerClass owner)
    {
        Owner = owner;
        ParentInventory = owner.Inventory;
        Id = 9;
        Icon = Main.Instance.Assets.WorldItemIcons[Id];
        Name = "Philosopher's Stone";
        Tooltip = ModTranslation.getString("PhiloStoneTooltip");
        IsSpecial = true;
    }
}