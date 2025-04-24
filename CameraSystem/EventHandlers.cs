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
                Log.Warn("");
                Exiled.Events.Handlers.Map.Generated += SpawnWorkstations;
                break;
        }

        Exiled.Events.Handlers.Scp079.Pinging += OnPinging;
        Exiled.Events.Handlers.Scp079.Recontaining += OnRecontaining;

        Exiled.Events.Handlers.Scp079.GainingExperience += OnScp079Event;
        Exiled.Events.Handlers.Scp079.LockingDown += OnScp079Event;
        Exiled.Events.Handlers.Scp079.LosingSignal += OnScp079Event;
        Exiled.Events.Handlers.Scp079.RoomBlackout += OnScp079Event;
        Exiled.Events.Handlers.Scp079.TriggeringDoor += OnScp079Event;
        Exiled.Events.Handlers.Scp079.InteractingTesla += OnScp079Event;

        Exiled.Events.Handlers.Player.ActivatingWorkstation += OnActivatingWorkstation;
        Exiled.Events.Handlers.Player.TriggeringTesla += OnTriggeringTesla;
        Exiled.Events.Handlers.Player.Hurting += OnHurting;
        Exiled.Events.Handlers.Player.Left += OnLeft;
        Exiled.Events.Handlers.Player.Dying += OnDying;
    }

    internal static void Unregister()
    {
        Exiled.Events.Handlers.Map.Generated -= SpawnWorkstations;
        Exiled.Events.Handlers.Server.RoundStarted -= SpawnWorkstations;

        Exiled.Events.Handlers.Scp079.Pinging -= OnPinging;
        Exiled.Events.Handlers.Scp079.Recontaining -= OnRecontaining;

        Exiled.Events.Handlers.Scp079.GainingExperience -= OnScp079Event;
        Exiled.Events.Handlers.Scp079.LockingDown -= OnScp079Event;
        Exiled.Events.Handlers.Scp079.LosingSignal -= OnScp079Event;
        Exiled.Events.Handlers.Scp079.RoomBlackout -= OnScp079Event;
        Exiled.Events.Handlers.Scp079.TriggeringDoor -= OnScp079Event;
        Exiled.Events.Handlers.Scp079.InteractingTesla -= OnScp079Event;

        Exiled.Events.Handlers.Player.ActivatingWorkstation -= OnActivatingWorkstation;
        Exiled.Events.Handlers.Player.TriggeringTesla -= OnTriggeringTesla;
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

        foreach (WorkstationConfig workstationConfig in Plugin.Instance.Config.Workstations)
        {
            GameObject gameObject = Object.Instantiate(
                prefab,
                workstationConfig.Position,
                Quaternion.Euler(workstationConfig.Rotation)
            );

            gameObject.transform.localScale = workstationConfig.Scale;
            NetworkServer.Spawn(gameObject);

            if (!gameObject.TryGetComponent(out WorkstationController workstationController))
            {
                Log.Error($"WorkstationController component is missing on the instantiated prefab: {gameObject.name} ({gameObject.GetInstanceID()}).");
                continue;
            }

            CameraManager.Instance.WorkstationControllers.Add(workstationController);
            Log.Debug($"Workstation {gameObject.GetInstanceID()} instantiated successfully.");
        }

        Log.Debug("All workstations instantiated successfully.");
    }

    private static void OnRecontaining(RecontainingEventArgs ev)
    {
        if (!CameraManager.Instance.IsWatching(ev.Player))
            return;

        ev.IsAllowed = false;
    }

    private static void OnTriggeringTesla(TriggeringTeslaEventArgs ev)
    {
        if (!CameraManager.Instance.IsWatching(ev.Player))
            return;

        ev.IsAllowed = false;
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
        if (npc == null || !CameraManager.Instance.IsWatching(npc))
            return;

        ev.IsAllowed = false;

        CameraManager.Instance.TryGetWatcher(npc, out Watcher watcher);
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

    private static void OnScp079Event(IScp079Event ev)
    {
        if (CameraManager.Instance.IsWatching(ev.Player))
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