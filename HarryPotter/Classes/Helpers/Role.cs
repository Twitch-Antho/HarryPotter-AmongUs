using UnityEngine;

namespace HarryPotter.Classes;

public class Role
{
    public string RoleNameTranslation { get; set; }
    public string RoleName { get; set; }
    public string IntroString { get; set; }
    public string TaskText { get; set; }
    public Color RoleColor { get; set; }
    public Color RoleColor2 { get; set; }
    public ModdedPlayerClass Owner { get; set; }

    public virtual void Update()
    {
    }

    public virtual bool DoClick(KillButton __instance)
    {
        return false;
    }

    public virtual void ResetCooldowns()
    {
    }

    public virtual void RemoveCooldowns()
    {
    }

    public virtual bool ShouldDrawCustomButtons()
    {
        return false;
    }
}