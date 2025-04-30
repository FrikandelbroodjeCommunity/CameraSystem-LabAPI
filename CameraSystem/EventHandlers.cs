using CameraSystem.Enums;
using CameraSystem.Managers;
using CameraSystem.Models;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp079;
using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using UnityEngine;

namespace CameraSystem;
internal static class EventHandlers
{
    internal static void Register()
    {
        switch (Plugin.Instance.Config.SpawnEvent)
        {
            case SpawnEvent.Generated:
                Exiled.Events.Handlers.Map.Generated += SpawnWorkstations;
                break;
            case SpawnEvent.RoundStarted:
                Exiled.Events.Handlers.Server.RoundStarted += SpawnWorkstations;
                break;
            default:
                Log.Warn($"Invalid spawn event type \"{Plugin.Instance.Config.SpawnEvent}\". " +
                         $"Defaulting to \"{SpawnEvent.Generated}\".");
                Exiled.Events.Handlers.Map.Generated += SpawnWorkstations;
                break;
        }

        Exiled.Events.Handlers.Scp079.Pinging += OnPinging;
        Exiled.Events.Handlers.Scp079.Recontaining += OnPlayerEvent;

        Exiled.Events.Handlers.Scp079.GainingExperience += OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.LockingDown += OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.LosingSignal += OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.RoomBlackout += OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.ZoneBlackout += OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.ChangingSpeakerStatus += OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.TriggeringDoor += OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.InteractingTesla += OnPlayerEvent;

        Exiled.Events.Handlers.Player.ActivatingWorkstation += OnActivatingWorkstation;
        Exiled.Events.Handlers.Player.TriggeringTesla += OnPlayerEvent;
        Exiled.Events.Handlers.Player.InteractingElevator += OnPlayerEvent;
        Exiled.Events.Handlers.Player.Hurting += OnHurting;
        Exiled.Events.Handlers.Player.Left += OnLeft;
        Exiled.Events.Handlers.Player.Dying += OnDying;
    }

    internal static void Unregister()
    {
        Exiled.Events.Handlers.Map.Generated -= SpawnWorkstations;
        Exiled.Events.Handlers.Server.RoundStarted -= SpawnWorkstations;

        Exiled.Events.Handlers.Scp079.Pinging -= OnPinging;
        Exiled.Events.Handlers.Scp079.Recontaining -= OnPlayerEvent;

        Exiled.Events.Handlers.Scp079.GainingExperience -= OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.LockingDown -= OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.LosingSignal -= OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.RoomBlackout -= OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.ZoneBlackout -= OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.ChangingSpeakerStatus -= OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.TriggeringDoor -= OnPlayerEvent;
        Exiled.Events.Handlers.Scp079.InteractingTesla -= OnPlayerEvent;

        Exiled.Events.Handlers.Player.ActivatingWorkstation -= OnActivatingWorkstation;
        Exiled.Events.Handlers.Player.TriggeringTesla -= OnPlayerEvent;
        Exiled.Events.Handlers.Player.InteractingElevator -= OnPlayerEvent;
        Exiled.Events.Handlers.Player.Hurting -= OnHurting;
        Exiled.Events.Handlers.Player.Left -= OnLeft;
        Exiled.Events.Handlers.Player.Dying -= OnDying;
    }

    private static void SpawnWorkstations()
    {
        if (!PrefabHelper.TryGetPrefab(PrefabType.WorkstationStructure, out GameObject prefab))
        {
            Log.Error("Failed to find prefab of type WorkstationStructure.");
            return;
        }

        Log.Debug($"Starting workstation spawn process. Found {Plugin.Instance.Config.PresetConfigs.Length} presets " +
                  $"and {Plugin.Instance.Config.Workstations.Length} custom workstations to spawn.");
        foreach (PresetConfig presetConfig in Plugin.Instance.Config.PresetConfigs)
        {
            Room targetRoom = Room.Get(presetConfig.RoomType);

            SpawnWorkstation(prefab,
                targetRoom.WorldPosition(presetConfig.LocalPosition),
                targetRoom.transform.rotation * Quaternion.Euler(presetConfig.LocalRotation),
                presetConfig.Scale);
        }

        foreach (WorkstationConfig workstationConfig in Plugin.Instance.Config.Workstations)
        {
            SpawnWorkstation(prefab,
                workstationConfig.Position,
                Quaternion.Euler(workstationConfig.Rotation),
                workstationConfig.Scale);
        }

        Log.Debug($"Successfully spawned {CameraManager.Instance.WorkstationControllers.Count} workstations in total.");
    }

    private static void SpawnWorkstation(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject workstation = Object.Instantiate(prefab, position, rotation);
        workstation.transform.localScale = scale;
        NetworkServer.Spawn(workstation);

        if (!workstation.TryGetComponent(out WorkstationController workstationController))
        {
            Log.Error($"WorkstationController missing on spawned workstation (ID: {workstation.GetInstanceID()}).");
            return;
        }

        CameraManager.Instance.WorkstationControllers.Add(workstationController);
        Log.Debug($"Successfully spawned workstation (ID: {workstation.GetInstanceID()}).");
    }

    private static void OnActivatingWorkstation(ActivatingWorkstationEventArgs ev)
    {
        if (!CameraManager.Instance.WorkstationControllers.Contains(ev.WorkstationController))
            return;

        ev.IsAllowed = false;

        CameraManager.Instance.Connect(ev.Player);
    }

    private static void OnDying(DyingEventArgs ev)
    {
        Npc npc = Npc.Get(ev.Player.ReferenceHub);
        if (npc == null || !CameraManager.Instance.TryGetWatcher(npc, out Watcher watcher))
            return;

        ev.IsAllowed = false;

        if (watcher.Player != null)
        {
            CameraManager.Instance.Disconnect(watcher.Player, ev.DamageHandler);
        }
    }

    private static void OnPinging(PingingEventArgs ev)
    {
        if (!CameraManager.Instance.IsWatching(ev.Player))
            return;

        ev.IsAllowed = false;

        CameraManager.Instance.Disconnect(ev.Player);
    }

    private static void OnPlayerEvent(IPlayerEvent ev)
    {
        if (!CameraManager.Instance.IsWatching(ev.Player))
            return;

        ((IDeniableEvent)ev).IsAllowed = false;
    }

    private static void OnHurting(HurtingEventArgs ev)
    {
        Npc npc = Npc.Get(ev.Player.ReferenceHub);
        if (npc == null || !CameraManager.Instance.IsWatching(npc))
            return;

        ev.IsAllowed = false;

        if (CameraManager.Instance.TryGetWatcher(npc, out Watcher watcher))
        {
            CameraManager.Instance.Disconnect(watcher.Player, ev.DamageHandler);
        }
    }

    private static void OnLeft(LeftEventArgs ev)
    {
        if (!CameraManager.Instance.IsWatching(ev.Player))
            return;

        CameraManager.Instance.ForceDisconnect(ev.Player);
    }
}