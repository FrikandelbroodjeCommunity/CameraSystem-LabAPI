using System;
using CameraSystem.Configs;
using CameraSystem.Managers;
using Exiled.API.Features;
using HarmonyLib;

namespace CameraSystem;
public class CameraSystem : Plugin<Config, Translation>
{
    public override string Author => "Jiraya";
    public override string Name => "CameraSystem";
    public override Version Version { get; } = new(1, 1, 3);

    internal static CameraSystem Instance { get; private set; } = null!;
    internal CameraManager CameraManager { get; private set; } = null!;

    private Harmony _harmony = null!;

    public override void OnEnabled()
    {
        Instance = this;

        CameraManager = new();
        EventHandlers.Register();

        _harmony = new($"com.{Author}.{Name}.{DateTime.Now.Ticks}");
        _harmony.PatchAll();

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        _harmony.UnpatchAll();
        _harmony = null!;

        EventHandlers.Unregister();

        CameraManager.Dispose();
        CameraManager = null!;

        Instance = null!;

        base.OnDisabled();
    }
}
