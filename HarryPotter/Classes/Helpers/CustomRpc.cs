using System.Linq;
using HarryPotter.Classes.Roles;
using HarryPotter.Classes.WorldItems;
using HarryPotter.CustomOption;
using Hazel;
using Reactor.Utilities;
using UnityEngine;

namespace HarryPotter.Classes;

public enum Packets
{
    AssignRole = 70,
    FixLightsRpc = 71,
    ForceAllVotes = 72,
    CreateCurse = 73,
    DestroyCurse = 74,
    KillPlayerUnsafe = 75,
    DeactivatePlayer = 76,
    DestroyCrucio = 77,
    CreateCrucio = 78,
    StartControlling = 79,
    MoveControlledPlayer = 80,
    InvisPlayer = 81,
    DefensiveDuelist = 82,
    RevivePlayer = 83,
    TeleportPlayer = 84,
    SpawnItem = 85,
    TryPickupItem = 86,
    GiveItem = 87,
    DestroyItem = 88,
    UseItem = 89,
    UpdateSpeedMultiplier = 90,
    RevealRole = 91,
    FakeKill = 92,
    FinallyDie = 93,
    RequestRole = 94,
    SyncCustomSettings = 95
}

public class CustomRpc
{
    public void Handle(byte packetId, MessageReader reader)
    {
        switch (packetId)
        {
            case (byte)Packets.AssignRole:
                var playerId = reader.ReadByte();
                var roleName = reader.ReadString();
                var rolePlayer = Main.Instance.ModdedPlayerById(playerId);
                switch (roleName)
                {
                    case "Voldemort":
                        rolePlayer.Role = new Voldemort(rolePlayer);
                        break;
                    case "Bellatrix":
                        rolePlayer.Role = new Bellatrix(rolePlayer);
                        break;
                    case "Harry":
                        rolePlayer.Role = new Harry(rolePlayer);
                        break;
                    case "Hermione":
                        rolePlayer.Role = new Hermione(rolePlayer);
                        break;
                    case "Ron":
                        rolePlayer.Role = new Ron(rolePlayer);
                        break;
                }

                break;
            case (byte)Packets.RequestRole:
                if (AmongUsClient.Instance.AmHost)
                {
                    var requesterId = reader.ReadByte();
                    var requestedRole = reader.ReadString();

                    if (Main.Instance.PlayersWithRequestedRoles.All(x => x.Item1.PlayerId != requesterId))
                        Main.Instance.PlayersWithRequestedRoles.Add(
                            new Pair<PlayerControl, string>(GameData.Instance.GetPlayerById(requesterId).Object,
                                requestedRole));
                }

                break;
            case (byte)Packets.FinallyDie:
                var finallyDeadId = reader.ReadByte();
                Main.Instance.PlayerDie(Main.Instance.ModdedPlayerById(finallyDeadId)._Object);
                break;
            case (byte)Packets.FakeKill:
                var fakeKilledId = reader.ReadByte();
                Coroutines.Start(Main.Instance.CoFakeKill(Main.Instance.ModdedPlayerById(fakeKilledId)._Object));
                break;
            case (byte)Packets.FixLightsRpc:
                var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
                break;
            case (byte)Packets.ForceAllVotes:
                var forcePlayer = reader.ReadByte();
                Main.Instance.ForceAllVotes(forcePlayer);
                break;
            case (byte)Packets.CreateCurse:
                var casterId = reader.ReadByte();
                var direction = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                Main.Instance.CreateCurse(direction, Main.Instance.ModdedPlayerById(casterId));
                break;
            case (byte)Packets.CreateCrucio:
                var blinderId = reader.ReadByte();
                var crucioDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                Main.Instance.CreateCrucio(crucioDirection, Main.Instance.ModdedPlayerById(blinderId));
                break;
            case (byte)Packets.DestroyCurse:
                Main.Instance.DestroySpell("_curse");
                break;
            case (byte)Packets.DestroyCrucio:
                Main.Instance.DestroySpell("_crucio");
                break;
            case (byte)Packets.KillPlayerUnsafe:
                var killerId = reader.ReadByte();
                var targetId = reader.ReadByte();
                var isCurseKill = reader.ReadBoolean();
                var forceAnim = reader.ReadBoolean();
                var target = Main.Instance.ModdedPlayerById(targetId);
                var killer = Main.Instance.ModdedPlayerById(killerId);
                Main.Instance.KillPlayer(killer._Object, target._Object, isCurseKill, forceAnim);
                break;
            case (byte)Packets.DeactivatePlayer:
                var blindId = reader.ReadByte();
                var blind = Main.Instance.ModdedPlayerById(blindId);
                Main.Instance.CrucioBlind(blind._Object);
                break;
            case (byte)Packets.StartControlling:
                var controllerId = reader.ReadByte();
                var controlledId = reader.ReadByte();
                var controller = Main.Instance.ModdedPlayerById(controllerId);
                var controlled = Main.Instance.ModdedPlayerById(controlledId);
                Main.Instance.ControlPlayer(controller._Object, controlled._Object);
                break;
            case (byte)Packets.MoveControlledPlayer:
                var moveId = reader.ReadByte();
                var newVel = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                var newPos = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                var movePlayer = Main.Instance.ModdedPlayerById(moveId)._Object;
                if (movePlayer.AmOwner)
                {
                    movePlayer.transform.position = newPos;
                    movePlayer.MyPhysics.body.position = newPos;
                    movePlayer.MyPhysics.body.velocity = newVel;
                    System.Console.WriteLine("MoveControlledPlayer");
                }

                break;
            case (byte)Packets.InvisPlayer:
                var invisId = reader.ReadByte();
                var invisPlayer = Main.Instance.ModdedPlayerById(invisId)._Object;
                Main.Instance.InvisPlayer(invisPlayer);
                break;
            case (byte)Packets.DefensiveDuelist:
                var ddId = reader.ReadByte();
                var ddPlayer = Main.Instance.ModdedPlayerById(ddId)._Object;
                Main.Instance.DefensiveDuelist(ddPlayer);
                break;
            case (byte)Packets.RevivePlayer:
                var reviveId = reader.ReadByte();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.PlayerId != reviveId)
                        continue;
                    if (!player.Data.IsDead)
                        continue;

                    player.ModRevive();
                    foreach (var body in Object.FindObjectsOfType<DeadBody>())
                        if (body.ParentId == reviveId)
                            Object.Destroy(body.gameObject);
                }

                break;
            case (byte)Packets.TeleportPlayer:
                var teleportId = reader.ReadByte();
                var teleportPos = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                foreach (var player in PlayerControl.AllPlayerControls)
                    if (teleportId == player.PlayerId)
                        player.NetTransform.SnapTo(teleportPos);
                break;
            case (byte)Packets.SpawnItem:
                var itemId = reader.ReadInt32();
                var itemPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                var velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                Main.Instance.SpawnItem(itemId, itemPosition, velocity);
                break;
            case (byte)Packets.TryPickupItem:
                if (!AmongUsClient.Instance.AmHost)
                    return;

                var targetPlayer = reader.ReadByte();
                var pickupId = reader.ReadInt32();
                if (Main.Instance.AllItems.Any(x => x.Id == pickupId))
                {
                    var allMatches = Main.Instance.AllItems.FindAll(x => x.Id == pickupId);
                    foreach (var item in allMatches) item.Delete();
                    Main.Instance.AllItems.RemoveAll(x => x.IsPickedUp);

                    var writer =
                        AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)Packets.GiveItem);
                    writer.Write(targetPlayer);
                    writer.Write(pickupId);
                    writer.EndMessage();
                }

                break;
            case (byte)Packets.GiveItem:
                var targetPlayer2 = reader.ReadByte();
                var pickupId2 = reader.ReadInt32();

                if (targetPlayer2 != PlayerControl.LocalPlayer.PlayerId)
                    return;

                if (Main.Instance.GetLocalModdedPlayer().HasItem(pickupId2))
                    return;

                Main.Instance.GiveGrabbedItem(pickupId2);
                Main.Instance.AllItems.RemoveAll(x => x.IsPickedUp);
                break;
            case (byte)Packets.DestroyItem:
                if (!AmongUsClient.Instance.AmHost)
                {
                    var targetItemId = reader.ReadInt32();
                    var allMatches = Main.Instance.AllItems.FindAll(x => x.Id == targetItemId);
                    foreach (var item in allMatches)
                        item.Delete();
                    Main.Instance.AllItems.RemoveAll(x => x.IsPickedUp);
                }

                break;
            case (byte)Packets.UseItem:
                if (!AmongUsClient.Instance.AmHost) return;
                var usedItemId = reader.ReadInt32();
                switch (usedItemId)
                {
                    case 0:
                        DeluminatorWorld.HasSpawned = false;
                        break;
                    case 1:
                        MaraudersMapWorld.HasSpawned = false;
                        break;
                    case 2:
                        PortKeyWorld.HasSpawned = false;
                        break;
                    case 5:
                        ButterBeerWorld.HasSpawned = false;
                        break;
                }

                break;
            case (byte)Packets.UpdateSpeedMultiplier:
                var readerId = reader.ReadByte();
                var newSpeed = reader.ReadSingle();
                Main.Instance.ModdedPlayerById(readerId).SpeedMultiplier = newSpeed;
                break;
            case (byte)Packets.RevealRole:
                var revealId = reader.ReadByte();
                Main.Instance.RevealRole(revealId);
                break;
            case (byte)Packets.SyncCustomSettings:
                Rpc.ReceiveRpc(reader);
                break;
        }
    }
}