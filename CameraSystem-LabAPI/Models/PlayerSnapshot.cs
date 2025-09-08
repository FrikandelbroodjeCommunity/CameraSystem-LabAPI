using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using UnityEngine;

namespace CameraSystem.Models;

internal class PlayerSnapshot
{
    internal Dictionary<StatusEffectBase, (byte Intensity, float Duration)> ActiveEffects { get; }
    internal Dictionary<ItemType, ushort> Ammo { get; }
    internal float ArtificialHealth { get; }
    internal string CustomInfo { get; }
    internal string CustomName { get; }
    internal EmotionPresetType Emotion { get; }
    internal float Health { get; }
    internal Vector3 Position { get; }
    internal RoleTypeId Role { get; }
    internal Vector3 Scale { get; }
    internal Quaternion Rotation { get; }
    internal string Nickname { get; }

    internal PlayerSnapshot(Player player)
    {
        ActiveEffects = player.ActiveEffects.ToDictionary(
            effect => effect,
            effect => (effect.Intensity, effect.Duration)
        );
        Ammo = player.Ammo.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        ArtificialHealth = player.ArtificialHealth;
        CustomName = player.DisplayName;
        CustomInfo = player.CustomInfo;
        Emotion = EmotionSync.Database[player.ReferenceHub];
        Health = player.Health;
        Position = player.Position;
        Role = player.Role;
        Scale = player.Scale;
        Rotation = player.Rotation;
        Nickname = player.Nickname;
    }
}