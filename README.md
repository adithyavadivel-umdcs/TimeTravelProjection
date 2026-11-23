# TimeTravelProjection

A Unity C# immersive projection prototype. Run it in Unity Editor on macOS or Windows.

## Clone the repository

With Git installed, open Terminal on macOS or PowerShell on Windows and run:

```sh
git clone https://github.com/adithyavadivel-umdcs/TimeTravelProjection.git
cd TimeTravelProjection
```

The project downloads into a `TimeTravelProjection` folder in your current directory.

## Run in Unity

1. Install Unity **6000.6.8f1** using Unity Hub.
2. In Hub, select **Add → Add project from disk** and choose the cloned folder.
3. Open the project and wait for imports and compilation to finish.
4. Press **Play** and click the **Game** view. Unity automatically starts
   `Assets/Scenes/MainScene.unity` in Past, even if another scene is open for editing.

- **WASD** or **arrow keys** to move, **mouse** or **I/J/K/L** to look around.
- **F** to toggle flight. While flying, hold **Space** to rise or **Ctrl** to descend.
- **T** to cycle Past → Present → Future → Past, **E** to interact, and
  **Escape** to release or capture the cursor.

## Run as a macOS application

1. On a Mac, open MainScene and exit Play mode.
2. Select **Tools → Time Travel → Build macOS Prototype** and save scene changes
   when prompted. Later builds replace generated output in `Builds/macOS`.
3. After the build succeeds, open `Builds/macOS/TimeTravelProjection.app` in Finder.

The build includes Intel and Apple silicon support. Use **Command+Q** to quit.
Controls match the Editor version. `MainScene` contains the merged
`PastWorld`, `PresentWorld`, and `FutureWorld`. Edit each period under its
matching root. Fullscreen and physical display output can be checked in the
standalone app.

The seven imported Future geometry prefabs use Unity’s binary format to keep
the mesh data compact. They are still editable in ProBuilder. Keep Asset
Serialization set to **Mixed** so the project can retain these alongside its
text scene files.

## Run the immersive projection build

In Unity, select **Tools → Time Travel → Build macOS Projection** or
**Build Windows Projection**. These release builds include the three merged
timelines from **MainScene** and write to `Builds/macOS Projection` or
`Builds/Windows Projection`.

Connect the projector before launching. If it mirrors the computer's display,
use the default primary output. For a projector set up as the second extended
display, use `-projection-display 2`. If Unity cannot see the second display, it
falls back to the primary display. On macOS the second display uses its native
mode as configured in System Settings.

```sh
open -n "Builds/macOS Projection/TimeTravelProjection.app" --args -projection-display 2
```

```powershell
& ".\Builds\Windows Projection\TimeTravelProjection.exe" -projection-display 2
```

For a windowed computer rehearsal, use `-projection-display 1 -projection-windowed`.
The default uses the project's full render scale. If the projector stutters,
try `-projection-render-scale 0.85` and check legibility from the audience position.
This renders fewer pixels internally while keeping the display output mode.
Adjust framing with **ProjectionSettings → Field Of View**, then rebuild.
Press **T** once after launch to enter Present.
