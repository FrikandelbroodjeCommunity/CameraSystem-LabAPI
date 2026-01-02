using System;
using CameraSystem.Configs;
using CameraSystem.Managers;
using HarmonyLib;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;

namespace CameraSystem;

public class CameraSystem : Plugin<Config>
{
    public override string Name => "CameraSystem";
    public override string Author => "Jiraya";
    public override string Description => "A plugin that allows players to connect to the facility's security camera system via special workstations.";
    public override Version Version => new(1, 1, 4);
    public override Version RequiredApiVersion => new(LabApiProperties.CompiledVersion);

    internal static CameraSystem Instance { get; private set; }
    internal CameraManager CameraManager { get; private set; }

    private Harmony _harmony;

    public override void Enable()
    {
        Instance = this;

        CameraManager = new CameraManager();
        EventHandlers.Register();

        _harmony = new Harmony($"com.{Author}.{Name}.{DateTime.Now.Ticks}");
        _harmony.PatchAll();
    }

    public override void Disable()
    {
        _harmony.UnpatchAll();
        _harmony = null!;

        EventHandlers.Unregister();

        CameraManager.Dispose();
        CameraManager = null;
    }
}