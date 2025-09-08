using System.Linq;
using CameraSystem.Managers;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp079Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;
using VoiceChat;
using Logger = LabApi.Features.Console.Logger;

namespace CameraSystem;

internal static class EventHandlers
{
    private static GameObject WorkStationPrefab
    {
        get
        {
            if (_prefab == null)
            {
                _prefab = NetworkClient.prefabs.FirstOrDefault(x => x.Key == 1783091262).Value;
            }

            return _prefab;
        }
    }

    private static GameObject _prefab;

    internal static void Register()
    {
        ServerEvents.MapGenerated += OnMapGenerated;

        Scp079Events.Pinging += OnPinging;
        Scp079Events.Recontaining += OnRecontaining;

        // TODO: Make sure camera cannot do anything (opening doors/sending elevators may be possible)
        Scp079Events.BlackingOutRoom += OnPlayerEvent;
        Scp079Events.BlackingOutZone += OnPlayerEvent;
        Scp079Events.ChangingCamera += OnChangingCamera;
        Scp079Events.LockingDoor += OnPlayerEvent;
        Scp079Events.LockingDownRoom += OnPlayerEvent;
        Scp079Events.GainingExperience += OnPlayerEvent;
        Scp079Events.UsingTesla += OnPlayerEvent;

        PlayerEvents.TriggeringTesla += OnPlayerEvent;
        PlayerEvents.InteractingElevator += OnPlayerEvent;
        PlayerEvents.Hurting += OnHurting;
        PlayerEvents.Cuffing += OnHandcuffing;
        PlayerEvents.Left += OnLeft;
        PlayerEvents.Dying += OnDying;

        PlayerEvents.ReceivingVoiceMessage += OnVoiceChatting;
        PlayerEvents.SendingVoiceMessage += OnVoiceChatting;
    }

    internal static void Unregister()
    {
        ServerEvents.MapGenerated -= OnMapGenerated;

        Scp079Events.BlackingOutRoom -= OnPlayerEvent;
        Scp079Events.BlackingOutZone -= OnPlayerEvent;
        Scp079Events.ChangingCamera -= OnChangingCamera;
        Scp079Events.LockingDoor -= OnPlayerEvent;
        Scp079Events.LockingDownRoom -= OnPlayerEvent;
        Scp079Events.GainingExperience -= OnPlayerEvent;
        Scp079Events.UsingTesla -= OnPlayerEvent;

        PlayerEvents.TriggeringTesla -= OnPlayerEvent;
        PlayerEvents.InteractingElevator -= OnPlayerEvent;
        PlayerEvents.Hurting -= OnHurting;
        PlayerEvents.Cuffing -= OnHandcuffing;
        PlayerEvents.Left -= OnLeft;
        PlayerEvents.Dying -= OnDying;

        PlayerEvents.ReceivingVoiceMessage -= OnVoiceChatting;
        PlayerEvents.SendingVoiceMessage -= OnVoiceChatting;
    }

    private static void OnMapGenerated(MapGeneratedEventArgs _) => SpawnWorkstations();

    private static void SpawnWorkstations()
    {
        if (WorkStationPrefab == null)
        {
            Logger.Error("Failed to find prefab of type WorkstationStructure.");
            return;
        }

        Logger.Debug(
            $"Starting workstation spawn process. Found {CameraSystem.Instance.Config.PresetConfigs.Length} presets " +
            $"and {CameraSystem.Instance.Config.Workstations.Length} custom workstations to spawn.");

        foreach (var presetConfig in CameraSystem.Instance.Config.PresetConfigs)
        {
            var targetRoom = Room.Get(presetConfig.RoomType).FirstOrDefault();
            if (targetRoom == null)
            {
                Logger.Warn($"Room {presetConfig.RoomType} not found for preset workstation.");
                continue;
            }

            CameraManager.Instance.CreateWorkstation(
                WorkStationPrefab,
                targetRoom.Transform.TransformPoint(presetConfig.LocalPosition),
                targetRoom.Transform.rotation * Quaternion.Euler(presetConfig.LocalRotation),
                presetConfig.Scale
            );
        }

        foreach (var workstationConfig in CameraSystem.Instance.Config.Workstations)
        {
            CameraManager.Instance.CreateWorkstation(
                WorkStationPrefab,
                workstationConfig.Position,
                Quaternion.Euler(workstationConfig.Rotation),
                workstationConfig.Scale
            );
        }

        Logger.Debug(
            $"Successfully spawned {CameraSystem.Instance.Config.PresetConfigs.Length + CameraSystem.Instance.Config.Workstations.Length} workstations in total.");
    }

    internal static bool OnActivatingWorkstation(Player player, WorkstationController controller)
    {
        if (!CameraManager.Instance.WorkstationControllers.Contains(controller))
        {
            return true;
        }

        if (CameraSystem.Instance.Config.ProhibitedRoles.Contains(player.Role))
        {
            player.SendHint(CameraSystem.Instance.Config.Translations.ProhibitedRoleMessage, 7);
            return false;
        }

        if (!CameraManager.Instance.IsCameraSystemEnabled)
        {
            player.SendHint(CameraSystem.Instance.Config.Translations.CameraSystemDisabledMessage, 7);
            return false;
        }

        CameraManager.Instance.Connect(player);
        return false;
    }

    private static void OnDying(PlayerDyingEventArgs ev)
    {
        if (!CameraManager.Instance.TryGetWatcher(ev.Player, out var watcher))
        {
            return;
        }

        ev.IsAllowed = false;

        if (watcher.Player.IsOnline)
        {
            CameraManager.Instance.Disconnect(watcher.Player, ev.DamageHandler);
        }
    }

    private static void OnPinging(Scp079PingingEventArgs ev)
    {
        if (!CameraManager.Instance.IsWatching(ev.Player))
            return;

        ev.IsAllowed = false;

        CameraManager.Instance.Disconnect(ev.Player);
    }

    private static void OnPlayerEvent(IPlayerEvent ev)
    {
        if (!CameraManager.Instance.IsWatching(ev.Player) || ev is not ICancellableEvent cancellableEvent)
            return;

        cancellableEvent.IsAllowed = false;
    }

    private static void OnChangingCamera(Scp079ChangingCameraEventArgs ev)
    {
        if (!ev.Camera.IsBeingUsed)
            return;

        ev.IsAllowed = false;
    }

    private static void OnRecontaining(Scp079RecontainingEventArgs ev)
    {
        CameraManager.Instance.DisconnectAll();
    }

    private static void OnHurting(PlayerHurtingEventArgs ev)
    {
        if (!CameraManager.Instance.TryGetWatcher(ev.Player, out var watcher) || ev.Player.ReferenceHub != watcher.Npc)
        {
            return;
        }

        CameraManager.Instance.Disconnect(watcher.Player, ev.DamageHandler);
        ev.IsAllowed = false;
    }

    private static void OnHandcuffing(PlayerCuffingEventArgs ev)
    {
        if (!CameraManager.Instance.TryGetWatcher(ev.Target, out var watcher) || ev.Target.ReferenceHub != watcher.Npc)
        {
            return;
        }

        CameraManager.Instance.Disconnect(watcher.Player);
        watcher.Player.DisarmedBy = ev.Player;
        ev.IsAllowed = false;
    }

    private static void OnLeft(PlayerLeftEventArgs ev)
    {
        if (!CameraManager.Instance.IsWatching(ev.Player))
            return;

        CameraManager.Instance.ForceDisconnect(ev.Player);
    }

    private static void OnVoiceChatting(IPlayerEvent ev)
    {
        if (ev is IVoiceMessageEvent voiceMessageEvent and ICancellableEvent cancellableEvent &&
            CameraManager.Instance.IsWatching(ev.Player) &&
            voiceMessageEvent.Message.Channel == VoiceChatChannel.ScpChat)
        {
            cancellableEvent.IsAllowed = false;
        }
    }
}