# VREF
VREF is a 3D graphing environment with multi-user synchronization capabilities for students and lecturers. Multi-user VREF sessions require the [VREF-Server synchronization server](https://github.com/TheIcyStar/VREF-Server), and solo sessions do not require a server.

# Screenshots
<img width="1517" height="917" alt="Untitled" src="https://github.com/user-attachments/assets/0c73a211-11d2-4a03-9ff0-d59f63898394" />

# Usage
No binaries are currently available. Clone this repository and open the project using the Unity Editor. Use the latest Unity LTS version `6000.0.x` to open the project.

## Emulate a VR headset
You can still run VREF in the Unity Editor without a VR headset. Enable the emulator by going to `Edit > Project Settings > XR interaction Toolkit` and enable `Use XR Device Simulator in scenes`

## Graphing
Press X (or B if using the VR Emulator) to bring up the graph input UI. Currently, equations with only one dependant variable on the left hand side are supported (For exmaple: `X=...`). 2D (two variables) and 3D (three variables) are currently supported. Be sure to add parentheses after trig functions, the UI does not add them for you (example: `sin()`).

You can add as many equations to the visualization as you'd like. Each equation can have a different visualization material to help visualize the graphed result.

#### Where did the name come from?
The name "VREF" came from combining VR (Virtual Reality) with REF (Row Echelon Form).
