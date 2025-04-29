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

        Log.Debug($"Starting workstation spawn process. Found {Plugin.Instance.Config.Presets.Length} presets and " +
                  $"{Plugin.Instance.Config.Workstations.Length} custom workstations to spawn.");
        foreach (Preset preset in Plugin.Instance.Config.Presets)
        {
            Room targetRoom;
            Vector3 localPosition;
            Vector3 localRotation;
            Vector3 scale;

            switch (preset)
            {
                case Preset.HczArmory:
                    targetRoom = Room.Get(RoomType.HczArmory);
                    localPosition = new Vector3(1.1f, 0f, 2.1f);
                    localRotation = new Vector3(0f, 180f, 0f);
                    scale = Vector3.one;
                    break;
                case Preset.Intercom:
                    targetRoom = Room.Get(RoomType.EzIntercom);
                    localPosition = new Vector3(-5.4f, 0f, -1.8f);
                    localRotation = Vector3.zero;
                    scale = Vector3.one;
                    break;
                case Preset.Intercom2:
                    targetRoom = Room.Get(RoomType.EzIntercom);
                    localPosition = new Vector3(-6.9f, -5.8f, 1.2f);
                    localRotation = new Vector3(0f, 90f, 0f);
                    scale = new Vector3(1f, 1f, 0.7f);
                    break;
                case Preset.Nuke:
                    targetRoom = Room.Get(RoomType.HczNuke);
                    localPosition = new Vector3(2f, -72.4f, 8.5f);
                    localRotation = Vector3.zero;
                    scale = Vector3.one;
                    break;
                default:
                    Log.Warn($"Unknown preset type \"{preset}\". Skipping...");
                    continue;
            }

            if (targetRoom == null)
            {
                Log.Warn($"Failed to find room for preset \"{preset}\". Skipping...");
                continue;
            }

            Vector3 worldPosition = targetRoom.WorldPosition(localPosition);
            Quaternion worldRotation = targetRoom.transform.rotation * Quaternion.Euler(localRotation);

            SpawnWorkstation(prefab, worldPosition, worldRotation, scale);
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