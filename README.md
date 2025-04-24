# CameraSystem

A plugin that allows players to connect to the facility's security camera system via special workstations.

## Features

- Customizable camera workstations placed throughout the facility
- Players maintain their original appearance via NPC clones while viewing cameras
- Full integration with SCP-079's camera system
- Blocks inappropriate actions while viewing cameras
- Configurable spawn points and timing

## Installation

1. Place the `CameraSystem.dll` in your `Exiled/Plugins` folder
2. Configure the plugin in `Exiled/Configs`

## Configuration

```yaml
camera_system:
  is_enabled: true
  debug: false
  spawn_event: Generated # Generated or RoundStarted
  workstations:
  - position: {x: 0, y: 0, z: 0}
    rotation: {x: 0, y: 0, z: 0}
    scale: {x: 1, y: 1, z: 1}
```

## Usage

1. Approach a configured workstation
2. Activate it (default: E key)
3. You'll enter camera view while your physical body remains at the workstation
4. Press E again to exit camera view