using System.ComponentModel;

namespace CameraSystem.Configs;

public class Translation
{
    [Description("The text appended to a player's custom info when they're watching cameras")]
    public string WatchingCamerasPostfix { get; set; } = "\nWatching Security Cameras";

    [Description("The message shown when a player successfully connects to the camera system")]
    public string ConnectionSuccessMessage { get; set; } =
        "\n\n<color=#FAFF86><size=21><b>You have successfully connected to the security camera system.\nPress E to exit.</b></size></color>";

    [Description("The message shown when a player disconnects from the camera system")]
    public string DisconnectionMessage { get; set; } =
        "\n\n<color=#FAFF86><size=21><b>You have disconnected from the security camera system.</b></size></color>";

    [Description("The message shown when a player connects but the camera system is disabled")]
    public string CameraSystemDisabledMessage { get; set; } =
        "\n\n<color=#FAFF86><size=21><b>Unable to establish connection...</color>\n<color=#C50000>ERROR: Camera network unreachable. System currently disabled.</color></b></size>";

    [Description("The message shown when a prohibited role tries to use cameras")]
    public string ProhibitedRoleMessage { get; set; } =
        "\n\n<color=#FAFF86><size=21><b>Your role cannot interact with the camera system.</b></size></color>";

    [Description("Usage hint for command")]
    public string InvalidArguments { get; set; } =
        "To execute this command provide at least {0} arguments!\nUsage: {1}";

    // Parent command messages
    [Description("Description for the parent camera system command")]
    public string CameraSystemCommandDescription { get; set; } = "Parent command for managing the camera system";

    [Description("The message shown when executing the parent command without subcommands")]
    public string ParentCommandHeader { get; set; } = "Please enter a valid subcommand:";

    [Description("Format for listing available commands (command, aliases, description)")]
    public string ParentCommandFormat { get; set; } =
        "\n\n<color=FAFF86><b>- {0} ({1})</b></color>\n<color=white>{2}</color>";

    // Shared
    [Description("Message shown when player lacks permission for the command")]
    public string NoPermission { get; set; } = "You do not have permission to execute this command.";

    [Description("Message shown when command for players used not by player")]
    public string NotPlayer { get; set; } = "You must be a player to use this command!";

    [Description("Message shown when no valid players found")]
    public string DisconnectNoPlayersFound { get; set; } = "No valid players found!";

    // ToggleWorkstationUsage command messages
    [Description("Description for the toggle workstation usage command")]
    public string ToggleWorkstationUsageDescription { get; set; } =
        "Toggles workstation in/out of the managed list and provides preset data";

    [Description("Message shown when no workstation is found in view")]
    public string NoWorkstationFound { get; set; } = "No workstation found in view.";

    [Description("Message shown when workstation is removed from managed list")]
    public string WorkstationRemoved { get; set; } = "Workstation removed from managed list.";

    // DisconnectCommand command messages
    [Description("Description for the disconnect command")]
    public string DisconnectCommandDescription { get; set; } = "Disconnects specified players from the cameras";

    [Description("Success message after disconnecting players")]
    public string DisconnectSuccess { get; set; } = "Successfully disconnected player(s)";

    // Toggle command messages
    [Description("Description for the toggle command")]
    public string ToggleCommandDescription { get; set; } = "Toggles the camera system";

    [Description("Message shown when camera system is disabled")]
    public string ToggleDisabled { get; set; } = "Camera system has been disabled.";

    [Description("Message shown when camera system is enabled")]
    public string ToggleEnabled { get; set; } = "Camera system has been enabled.";
}