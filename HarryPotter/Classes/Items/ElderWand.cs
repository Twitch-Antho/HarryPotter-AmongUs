namespace HarryPotter.Classes.Items;

public class ElderWand : Item
{
    public ElderWand(ModdedPlayerClass owner)
    {
        Owner = owner;
        ParentInventory = owner.Inventory;
        Id = 6;
        Icon = Main.Instance.Assets.ItemIcons[Id];
        Name = "Elder Wand";
        Tooltip = ModTranslation.getString("ElderWandTooltip");
    }

    public override void Use()
    {
        Delete();

        if (Owner._Object.Data.Role.IsImpostor)
            Owner.Role?.RemoveCooldowns();
        else
            Owner.VigilanteShotEnabled = true;
    }
}