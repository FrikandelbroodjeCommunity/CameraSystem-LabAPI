using System.ComponentModel;
using CameraSystem.Enums;
using CameraSystem.Models;
using Exiled.API.Enums;
using Exiled.API.Interfaces;
using UnityEngine;

namespace CameraSystem.Configs;
public sealed class Config : IConfig
{
    [Description("Whether the CameraSystem plugin is enabled")]
    public bool IsEnabled { get; set; } = true;

    [Description("Whether debug messages should be shown in the server console")]
    public bool Debug { get; set; } = false;

    [Description("When the camera workstations should be spawned. (Generated = during map generation, RoundStarted = when round starts)")]
    public SpawnEvent SpawnEvent { get; set; } = SpawnEvent.Generated;

    [Description("Whether the camera system is enabled by default when the plugin loads")]
    public bool IsCameraSystemEnabledByDefault { get; set; } = true;

    [Description("Preset locations where workstations will be automatically spawned")]
    public PresetConfig[] PresetConfigs { get; set; } = new[]
    {
        new PresetConfig(RoomType.HczArmory, new Vector3(1.1f, 0f, 2.1f), new Vector3(0f, 180f, 0f), Vector3.one),
        new PresetConfig(RoomType.EzIntercom, new Vector3(-5.4f, 0f, -1.8f), Vector3.zero, Vector3.one),
        new PresetConfig(RoomType.EzIntercom, new Vector3(-6.9f, -5.8f, 1.2f), new Vector3(0f, 90f, 0f), new Vector3(1f, 1f, 0.7f)),
        new PresetConfig(RoomType.HczNuke, new Vector3(2f, -72.4f, 8.5f), Vector3.zero, Vector3.one),
        new PresetConfig(RoomType.Lcz914, new Vector3(-1.9f, 0f, 5.5f), new Vector3(0f, 90f, 0f), Vector3.one),
        new PresetConfig(RoomType.Lcz914, new Vector3(-6.2f, 0f, 3.1f), new Vector3(0f, 180f, 0f), Vector3.one)
    };

    [Description("List of camera workstation configurations including their positions, rotations, and scales")]
    public WorkstationConfig[] Workstations { get; set; } = new[]
    {
        new WorkstationConfig(Vector3.zero, Vector3.zero, Vector3.one),
        new WorkstationConfig(Vector3.one, Vector3.one, Vector3.one)
    };
}