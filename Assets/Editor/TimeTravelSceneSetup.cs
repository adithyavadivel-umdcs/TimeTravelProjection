using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// EDITOR TOOL: this file adds the setup buttons under Tools > Time Travel.
// It runs only in Unity's Editor, not in the Windows application.
// Gameplay code is in Assets/Scripts. Start reading TimeTravelManager.cs there.
// Undo records allow Ctrl+Z/Cmd+Z to reverse scene setup changes.
public static class TimeTravelSceneSetup
{
    private const string ScenePath = "Assets/Scenes/MainScene.unity";
    private const string MenuPath = "Tools/Time Travel/Set Up Phase 2 Test Scene";

    [MenuItem("Tools/Time Travel/Set Up Phase 6 Projection Settings")]
    public static void SetUpProjectionSettings()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != ScenePath)
        {
            Debug.LogError("Time Travel setup: Exit Play mode and open MainScene first.");
            return;
        }

        Camera camera = Camera.main;
        if (camera == null || camera.gameObject.scene != scene)
        {
            Debug.LogError("Time Travel setup: MainScene needs an active MainCamera-tagged camera.");
            return;
        }

        GameObject root;
        try
        {
            root = FindRoot(scene, "ProjectionSettings");
        }
        catch (InvalidOperationException error)
        {
            Debug.LogError(error.Message);
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Set Up Projection Settings");
        if (root == null)
        {
            root = CreateRoot(scene, "ProjectionSettings");
        }
        ProjectionDisplaySettings settings = root.GetComponent<ProjectionDisplaySettings>();
        if (settings == null)
        {
            settings = Undo.AddComponent<ProjectionDisplaySettings>(root);
            SetReference(settings, "viewCamera", camera);
            SerializedObject serialized = new SerializedObject(settings);
            serialized.FindProperty("fieldOfView").floatValue = camera.fieldOfView;
            serialized.ApplyModifiedProperties();
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = root;
        if (EditorSceneManager.SaveScene(scene))
        {
            Debug.Log("Time Travel setup: Projection settings saved. Configure resolution, FOV, VSync, and optional second display in the Inspector.");
        }
        else
        {
            Debug.LogError("Time Travel setup: Save MainScene manually; automatic save failed.");
        }
    }

    [MenuItem("Tools/Time Travel/Set Up Phase 5 Player Controls")]
    public static void SetUpPlayerControls()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != ScenePath)
        {
            Debug.LogError("Time Travel setup: Exit Play mode and open MainScene first.");
            return;
        }

        GameObject player;
        GameObject floor;
        try
        {
            player = FindRoot(scene, "TestPlayer");
            floor = FindRoot(scene, "TestFloor");
        }
        catch (InvalidOperationException error)
        {
            Debug.LogError(error.Message);
            return;
        }

        Camera camera = Camera.main;
        if (camera == null || camera.gameObject.scene != scene || camera.transform.parent != null || player != null)
        {
            Debug.LogWarning("Time Travel setup: Requires an unparented Main Camera and no existing TestPlayer. Existing setup was preserved.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Set Up Player Controls");

        player = CreateRoot(scene, "TestPlayer");
        player.transform.position = camera.transform.position - Vector3.up * 1.6f;
        player.transform.rotation = Quaternion.Euler(0f, camera.transform.eulerAngles.y, 0f);
        CharacterController controller = Undo.AddComponent<CharacterController>(player);
        Undo.RecordObject(controller, "Configure Character Controller");
        controller.height = 1.8f;
        controller.radius = 0.3f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        controller.skinWidth = 0.03f;
        controller.stepOffset = 0.3f;
        Undo.SetTransformParent(camera.transform, player.transform, "Parent Player Camera");
        PlayerTestControls controls = Undo.AddComponent<PlayerTestControls>(player);
        SetReference(controls, "viewCamera", camera);

        if (floor == null)
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "TestFloor";
            SceneManager.MoveGameObjectToScene(floor, scene);
            floor.transform.position = player.transform.position - Vector3.up * 0.1f;
            floor.transform.localScale = new Vector3(40f, 0.2f, 40f);
            Undo.RegisterCreatedObjectUndo(floor, "Create Technical Test Floor");
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        if (EditorSceneManager.SaveScene(scene))
        {
            Debug.Log("Time Travel setup: Player saved. WASD moves, mouse looks, Escape toggles cursor capture. T and E retain their existing functions.");
        }
        else
        {
            Debug.LogError("Time Travel setup: Save MainScene manually; automatic save failed.");
        }
    }

    [MenuItem("Tools/Time Travel/Set Up Phase 4 Interaction Test")]
    public static void SetUpInteractionTest()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != ScenePath)
        {
            Debug.LogError("Time Travel setup: Exit Play mode and open MainScene first.");
            return;
        }

        GameObject managerRoot;
        GameObject past;
        GameObject future;
        try
        {
            managerRoot = FindRoot(scene, "TimeTravelManager");
            past = FindRoot(scene, "PastWorld");
            future = FindRoot(scene, "FutureWorld");
        }
        catch (InvalidOperationException error)
        {
            Debug.LogError(error.Message);
            return;
        }

        TimeTravelManager manager = null;
        if (managerRoot != null)
        {
            manager = managerRoot.GetComponent<TimeTravelManager>();
        }
        Camera camera = Camera.main;
        if (manager == null || past == null || future == null || camera == null ||
            camera.gameObject.scene != scene)
        {
            Debug.LogError("Time Travel setup: Phase 2 roots and an active MainCamera-tagged camera are required.");
            return;
        }

        if (past.transform.Find("PastInteractionTest") != null ||
            future.transform.Find("FutureConsequenceTest") != null)
        {
            Debug.LogWarning("Time Travel setup: Phase 4 objects already exist; existing settings were preserved.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Create Time Travel Interaction Test");
        TimeTravelState state = managerRoot.GetComponent<TimeTravelState>();
        if (state == null)
        {
            state = Undo.AddComponent<TimeTravelState>(managerRoot);
        }

        PlayerInteractor interactor = camera.GetComponent<PlayerInteractor>();
        if (interactor == null)
        {
            interactor = Undo.AddComponent<PlayerInteractor>(camera.gameObject);
            SetReference(interactor, "viewCamera", camera);
        }

        Vector3 position = camera.transform.position + camera.transform.forward * 2f;
        GameObject source = GameObject.CreatePrimitive(PrimitiveType.Cube);
        source.name = "PastInteractionTest";
        source.transform.SetParent(past.transform, false);
        source.transform.position = position;
        source.transform.rotation = camera.transform.rotation;
        source.transform.localScale = Vector3.one * 0.5f;
        Undo.RegisterCreatedObjectUndo(source, "Create Past Interaction Cube");
        TimelineInteraction interaction = Undo.AddComponent<TimelineInteraction>(source);
        SetReference(interaction, "manager", manager);
        SetReference(interaction, "state", state);

        GameObject controller = new GameObject("FutureConsequenceTest");
        controller.transform.SetParent(future.transform, false);
        Undo.RegisterCreatedObjectUndo(controller, "Create Future Consequence Controller");
        GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
        target.name = "ConsequenceTarget";
        target.transform.SetParent(controller.transform, false);
        target.transform.position = position;
        target.transform.rotation = camera.transform.rotation;
        target.transform.localScale = Vector3.one * 0.8f;
        target.SetActive(false);
        Undo.RegisterCreatedObjectUndo(target, "Create Future Consequence Cube");
        TimelineConsequence consequence = Undo.AddComponent<TimelineConsequence>(controller);
        SetReference(consequence, "state", state);
        SetReference(consequence, "target", target);
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        if (EditorSceneManager.SaveScene(scene))
        {
            Debug.Log("Time Travel setup: Phase 4 saved. Test future before E, then return to past, press E, and check future again.");
        }
        else
        {
            Debug.LogError("Time Travel setup: Save MainScene manually; automatic save failed.");
        }
    }

    // SerializedObject edits an Inspector field by its C# field name.
    // These names must match the fields in Assets/Scripts.
    private static void SetReference(UnityEngine.Object component, string field, UnityEngine.Object value)
    {
        SerializedObject serialized = new SerializedObject(component);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedProperties();
    }

    [MenuItem("Tools/Time Travel/Set Up Phase 3 Test Objects")]
    public static void SetUpTimelineObjects()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != ScenePath)
        {
            Debug.LogError("Time Travel setup: Exit Play mode and open MainScene first.");
            return;
        }

        GameObject managerRoot;
        GameObject examples;
        try
        {
            managerRoot = FindRoot(scene, "TimeTravelManager");
            examples = FindRoot(scene, "TimelineObjectTests");
        }
        catch (InvalidOperationException error)
        {
            Debug.LogError(error.Message);
            return;
        }

        TimeTravelManager manager = null;
        if (managerRoot != null)
        {
            manager = managerRoot.GetComponent<TimeTravelManager>();
        }
        if (manager == null)
        {
            Debug.LogError("Time Travel setup: Run Phase 2 setup first.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Create Timeline Object Tests");
        if (examples == null)
        {
            examples = CreateRoot(scene, "TimelineObjectTests");
        }
        CreateTimelineExample(examples.transform, manager, "PastOnly", true, false,
            new Vector3(-2f, 1.5f, 0f), new Vector3(-2f, 1.5f, 0f));
        CreateTimelineExample(examples.transform, manager, "FutureOnly", false, true,
            new Vector3(2f, 1.5f, 0f), new Vector3(2f, 1.5f, 0f));
        CreateTimelineExample(examples.transform, manager, "BothMoving", true, true,
            new Vector3(-0.75f, 2.5f, 0f), new Vector3(0.75f, 2.5f, 0f));
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        if (EditorSceneManager.SaveScene(scene))
        {
            Debug.Log("Time Travel setup: Phase 3 examples saved. Press Play and T to test.");
        }
        else
        {
            Debug.LogError("Time Travel setup: Save MainScene manually; automatic save failed.");
        }
    }

    private static void CreateTimelineExample(Transform parent, TimeTravelManager manager,
        string name, bool pastExists, bool futureExists, Vector3 pastPosition, Vector3 futurePosition)
    {
        // Existing examples are left intact, including any Inspector edits.
        if (parent.Find(name) != null)
        {
            return;
        }

        GameObject controller = new GameObject(name);
        controller.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(controller, "Create Timeline Object Controller");
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Target";
        cube.transform.SetParent(controller.transform, false);
        if (pastExists)
        {
            cube.transform.localPosition = pastPosition;
        }
        else
        {
            cube.transform.localPosition = futurePosition;
        }
        cube.transform.localScale = Vector3.one * 0.6f;
        cube.SetActive(pastExists);
        Undo.RegisterCreatedObjectUndo(cube, "Create Timeline Object Target");

        TimelineDependentObject component = Undo.AddComponent<TimelineDependentObject>(controller);
        SerializedObject serialized = new SerializedObject(component);
        serialized.FindProperty("manager").objectReferenceValue = manager;
        serialized.FindProperty("target").objectReferenceValue = cube;
        SerializedProperty states = serialized.FindProperty("states");
        for (int i = 0; i < 2; i++)
        {
            SerializedProperty state = states.GetArrayElementAtIndex(i);
            // Entry 0 is Past and entry 1 is Future, matching the component defaults.
            if (i == 0)
            {
                state.FindPropertyRelative("exists").boolValue = pastExists;
                state.FindPropertyRelative("localPosition").vector3Value = pastPosition;
            }
            else
            {
                state.FindPropertyRelative("exists").boolValue = futureExists;
                state.FindPropertyRelative("localPosition").vector3Value = futurePosition;
            }
            state.FindPropertyRelative("applyLocalTransform").boolValue = true;
        }
        serialized.ApplyModifiedProperties();
    }

    [MenuItem(MenuPath)]
    public static void SetUp()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != ScenePath)
        {
            Debug.LogError("Time Travel setup: Exit Play mode and open Assets/Scenes/MainScene.unity first.");
            return;
        }

        // Check for ambiguous names before making any scene changes.
        GameObject managerObject;
        GameObject past;
        GameObject future;
        try
        {
            managerObject = FindRoot(scene, "TimeTravelManager");
            past = FindRoot(scene, "PastWorld");
            future = FindRoot(scene, "FutureWorld");
        }
        catch (InvalidOperationException error)
        {
            Debug.LogError(error.Message);
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Set Up Time Travel Test Scene");

        if (managerObject == null)
        {
            managerObject = CreateRoot(scene, "TimeTravelManager");
        }
        if (past == null)
        {
            past = CreateRoot(scene, "PastWorld");
        }
        if (future == null)
        {
            future = CreateRoot(scene, "FutureWorld");
        }

        TimeTravelManager manager = managerObject.GetComponent<TimeTravelManager>();
        if (manager == null)
        {
            manager = Undo.AddComponent<TimeTravelManager>(managerObject);
        }

        // Use serialized properties so the runtime component needs no Editor-only API.
        SerializedObject serializedManager = new SerializedObject(manager);
        SerializedProperty timelines = serializedManager.FindProperty("timelines");
        AssignTimeline(timelines, TimelineId.Past, past);
        AssignTimeline(timelines, TimelineId.Future, future);
        serializedManager.ApplyModifiedProperties();

        CreateTestCube(past.transform, "PastTestCube", new Vector3(-1f, 0f, 0f));
        CreateTestCube(future.transform, "FutureTestCube", new Vector3(1f, 0f, 0f));

        Undo.RecordObject(manager, "Enable Time Travel Manager");
        manager.enabled = true;
        SetActive(managerObject, true);
        SetActive(past, true);
        SetActive(future, false);
        Undo.CollapseUndoOperations(undoGroup);

        EditorSceneManager.MarkSceneDirty(scene);
        if (EditorSceneManager.SaveScene(scene))
        {
            Selection.activeGameObject = managerObject;
            Debug.Log("Time Travel setup: MainScene saved. Press Play, focus Game view, and press T. " +
                "The camera and shared lighting were left in place.", manager);
        }
        else
        {
            Debug.LogError("Time Travel setup: Scene could not be saved. Save MainScene manually.");
        }
    }

    private static GameObject FindRoot(Scene scene, string name)
    {
        GameObject match = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != name)
            {
                continue;
            }
            if (match != null)
            {
                throw new InvalidOperationException($"Time Travel setup: Multiple roots named {name}. Rename duplicates first.");
            }
            match = root;
        }
        return match;
    }

    private static GameObject CreateRoot(Scene scene, string name)
    {
        GameObject root = new GameObject(name);
        SceneManager.MoveGameObjectToScene(root, scene);
        Undo.RegisterCreatedObjectUndo(root, "Create Timeline Root");
        return root;
    }

    private static void AssignTimeline(SerializedProperty timelines, TimelineId id, GameObject root)
    {
        int index = 0;
        while (index < timelines.arraySize &&
            timelines.GetArrayElementAtIndex(index).FindPropertyRelative("timeline").intValue != (int)id)
            index++;

        if (index == timelines.arraySize)
        {
            timelines.arraySize++;
        }

        SerializedProperty entry = timelines.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("timeline").intValue = (int)id;
        entry.FindPropertyRelative("root").objectReferenceValue = root;
    }

    private static void CreateTestCube(Transform parent, string name, Vector3 localPosition)
    {
        if (parent.Find(name) != null)
        {
            return;
        }

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        Undo.RegisterCreatedObjectUndo(cube, "Create Timeline Test Cube");
    }

    private static void SetActive(GameObject target, bool active)
    {
        Undo.RecordObject(target, "Set Initial Timeline State");
        target.SetActive(active);
    }
}
