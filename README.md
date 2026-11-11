# TimeTravelProjection

A Unity C# prototype for an immersive projection class project. It uses Unity
**6000.6.8f1**, URP, and the Input System. The target application is Windows
x86-64; no VR or AR hardware is required.

The cubes and floor are temporary technical test objects. The student group
creates the project's artistic assets independently.

## Open the project

1. Clone this repository or download and extract its source ZIP.
2. In Unity Hub, select **Add → Add project from disk**.
3. Select the project folder containing `Assets`, `Packages`, and `ProjectSettings`.
4. Open with Unity **6000.6.8f1** and wait for package imports and compilation.
5. Open `Assets/Scenes/MainScene.unity` and press **Play**.

The saved scene contains the test setup. The commands under **Tools → Time
Travel** can create the phase-specific test objects if needed; they are not
required every time the project opens.

## Controls

| Input | Action |
| --- | --- |
| W/A/S/D | Move |
| Mouse | Look around |
| T | Switch timelines |
| E | Interact with an object at the center of the view, within 3 metres |
| Escape | Release or capture the cursor |

In the Editor, click the **Game** view so it receives keyboard input. Movement
and mouse look require cursor capture.

## Test time-travel consequences

1. Start a new Play session or launch the application. It starts in **Past**.
2. Before moving or pressing T, press **E** to interact with the nearby test cube.
3. Press **T** to enter **Future**. The larger consequence cube should appear.
4. Switch repeatedly. The consequence should remain whenever you enter Future.

The interaction is available in Past only. Flags persist across timeline switches
in the current run, but reset when a new session starts.

## Build for Windows

On the development Mac:

1. Save the scene and close Unity Editor.
2. In Unity Hub, open **Installs → 6000.6.8f1 → Add modules**.
3. Install **Windows Build Support (Mono)**.
4. Reopen the project, exit Play mode, and wait for compilation.
5. Select **Tools → Time Travel → Build Windows Prototype**.

The command selects Windows x86-64 with Mono and builds `MainScene` as a
Development Build. Unity may need time to import assets for the Windows target.
Check the Console for a success message or build errors.

Output:

```text
Builds/Windows/TimeTravelProjection.exe
```

Keep **all** files and folders generated beside the executable. Later builds
replace generated files in this output folder. Build output is excluded from Git.
The Windows executable cannot run directly on macOS.

## Send the application to the Windows teammate

After a successful build:

1. On the Mac, right-click the **Windows** folder inside `Builds` and select
   **Compress “Windows”**. Compress the whole folder, not just the executable.
2. Rename the resulting archive to **TimeTravelProjection.zip** and send it to
   the Windows teammate.
3. The teammate saves that ZIP in their **Downloads** folder, opens
   **PowerShell**, and pastes this command:

```powershell
Expand-Archive "$HOME\Downloads\TimeTravelProjection.zip" "$env:LOCALAPPDATA\TimeTravelProjection" -Force; Start-Process "$env:LOCALAPPDATA\TimeTravelProjection\Windows\TimeTravelProjection.exe"
```

This extracts the portable application and launches it. Unity Editor and
administrator access are not required. The command expects a `Windows` folder
at the top level of the ZIP and replaces previously extracted files of the same
name. To launch again, open
`%LOCALAPPDATA%\TimeTravelProjection\Windows\TimeTravelProjection.exe`.

The repository's **Download ZIP** contains source files; it is not the Windows
application ZIP described above.

## Projection settings and standalone checks

Select `ProjectionSettings` in MainScene to configure resolution, fullscreen,
field of view, VSync, and optional second-display output. Save the scene before
building. For a second monitor or projector, use Windows **Extend** display mode
and enable **Use Second Display**. Restart the application after changing
secondary-display activation settings.

On a Windows computer, verify:

- The application starts in Past.
- Movement, mouse look, Escape, T, and E work.
- The interaction changes the future and survives repeated timeline switches.
- Fullscreen, resolution, and any connected projector behave as expected.

Windows build, performance, and physical display checks must be performed on
the actual Windows computer. Successful Editor tests do not verify those checks.

If the application fails, inspect this log (using the current Company Name and
Product Name settings):

```text
%USERPROFILE%\AppData\LocalLow\Unity Technologies\TimeTravelProjection\Player.log
```

## Code reading order

Runtime scripts are in `Assets/Scripts`:

1. `TimeTravelManager.cs` — activates the selected timeline's world root.
2. `TimelineDependentObject.cs` — applies an object's timeline-specific state.
3. `TimelineInteraction.cs` — records an interaction in the required timeline.
4. `TimeTravelState.cs` — remembers interaction keys during the current session.
5. `TimelineConsequence.cs` — changes a target based on a recorded key.
6. `PlayerInteractor.cs` — checks the camera ray when E is pressed.
7. `PlayerTestControls.cs` — handles movement, mouse look, gravity, and cursor capture.
8. `ProjectionDisplaySettings.cs` — applies display and performance settings.

Editor-only setup and build tools are in `Assets/Editor`. They are not included
in the standalone application.
