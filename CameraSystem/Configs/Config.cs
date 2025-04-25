using System.ComponentModel;
using CameraSystem.Enums;
using CameraSystem.Models;
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

    [Description("Preset locations where workstations will be automatically spawned. (Intercom, Nuke)")]
    public Preset[] Presets { get; set; } = new[]
    {
        Preset.Intercom,
        Preset.Nuke
    };

    [Description("List of camera workstation configurations including their positions, rotations, and scales")]
    public WorkstationConfig[] Workstations { get; set; } = new[]
    {
        new WorkstationConfig(Vector3.zero, Vector3.zero, Vector3.one),
        new WorkstationConfig(Vector3.one, Vector3.one, Vector3.one)
    };
}