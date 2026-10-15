using System.Collections.Generic;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace HarryPotter.Classes;

public class TaskInfoHandler
{
    public static TaskInfoHandler Instance { get; set; }
    public List<ImportantTextTask> AllInfo { get; set; }

    public void Update()
    {
        if (HudManager.Instance.taskDirtyTimer != 0f)
            return;

        if (AllInfo.Count == 0 && PlayerControl.LocalPlayer.myTasks.Count > 0)
        {
            var roleName = Main.Instance.GetLocalModdedPlayer().Role == null
                ? PlayerControl.LocalPlayer.Data.Role.IsImpostor
                    ? ModTranslation.getString("TeamImpostor")
                    : ModTranslation.getString("TeamMuggle")
                : ModTranslation.getString(Main.Instance.GetLocalModdedPlayer().Role.RoleNameTranslation);
            AddNewItem(0,
                string.Format(ModTranslation.getString("taskTextRoleText"), GetRoleHexColor(PlayerControl.LocalPlayer),
                    roleName));
            if (Main.Instance.GetLocalModdedPlayer().Role == null) return;
            AddNewItem(1,
                string.Format(ModTranslation.getString("taskTextRoleIntroText"),
                    GetRoleHexColor(PlayerControl.LocalPlayer), Main.Instance.GetLocalModdedPlayer().Role.TaskText) +
                $"\n{"taskTextRoleIntroText1".Translate()}");
        }
    }

    public ImportantTextTask AddNewItem(int index, string text)
    {
        var roleTextObj = new GameObject();
        var textTask = roleTextObj.AddComponent<ImportantTextTask>();
        textTask.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
        textTask.Text = text;
        textTask.Index = 0;
        PlayerControl.LocalPlayer.myTasks.Insert(index, textTask);
        AllInfo.Add(textTask);
        return textTask;
    }

    public void RemoveItem(ImportantTextTask item)
    {
        item.Destroy();
        PlayerControl.LocalPlayer.myTasks.Remove(item);
    }

    public string GetRoleHexColor(PlayerControl player)
    {
        var moddedPlayer = Main.Instance.ModdedPlayerById(player.PlayerId);
        if (moddedPlayer.Role == null)
            return "<#FFFFFF>";

        return moddedPlayer.Role.RoleColor.ToTextColor();
    }
}