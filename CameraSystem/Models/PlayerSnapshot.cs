using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using UnityEngine;

namespace CameraSystem.Models;
internal sealed class PlayerSnapshot
{
    internal Dictionary<EffectType, (byte Intensity, float Duration)> ActiveEffects { get; }
    internal Dictionary<AmmoType, ushort> Ammo { get; }
    internal float ArtificialHealth { get; }
    internal string CustomInfo { get; }
    internal string CustomName { get; }
    internal EmotionPresetType Emotion { get; }
    internal float Health { get; }
    internal Vector3 Position { get; }
    internal RoleTypeId Role { get; }
    internal Vector3 Scale { get; }

    internal PlayerSnapshot(Player player)
    {
        ActiveEffects = player.ActiveEffects.ToDictionary(
            effect => effect.GetEffectType(),
            effect => (effect.Intensity, effect.Duration)
        );
        Ammo = player.Ammo.ToDictionary(kvp => kvp.Key.GetAmmoType(), kvp => kvp.Value);
        ArtificialHealth = player.ArtificialHealth;
        CustomName = player.CustomName;
        CustomInfo = player.CustomInfo;
        Emotion = player.Emotion;
        Health = player.Health;
        Position = player.Position;
        Role = player.Role.Type;
        Scale = player.Scale;
    }
}