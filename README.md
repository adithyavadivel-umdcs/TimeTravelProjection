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
4. Open `Assets/Scenes/MainScene.unity`, press **Play**, and click the **Game** view.

Controls: **WASD** to move, **mouse** to look, **T** to cycle Past → Present → Future → Past,
**E** to interact, and **Escape** to release or capture the cursor.

## Run as a macOS application

1. On a Mac, open MainScene and exit Play mode.
2. Select **Tools → Time Travel → Set Up Phase 6 Projection Settings** once to
   add the display settings if they are missing. Select `ProjectionSettings` to
   configure resolution, fullscreen, and field of view, then save the scene.
3. Select **Tools → Time Travel → Build macOS Prototype** and save scene changes
   when prompted. Later builds replace generated output in `Builds/macOS`.
4. After the build succeeds, open `Builds/macOS/TimeTravelProjection.app` in Finder.

The build includes Intel and Apple silicon support. Use **Command+Q** to quit.
Controls match the Editor version. Test starting in Past, switching with T,
interacting with E in Past, and seeing the consequence in Future.
`MainScene` contains `PastWorld`, `PresentWorld`, and `FutureWorld`; add each
period’s content under its matching root. Present includes `PresentTestCube`.
The Phase 2 setup menu also creates and registers all three worlds. Fullscreen and
physical display output need to be checked in the standalone app.
