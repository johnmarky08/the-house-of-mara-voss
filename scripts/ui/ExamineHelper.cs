using Godot;
using System.Collections.Generic;
using System.Linq;

public static class ExamineHelper
{
    // -------------------------------------------------------------------------
    // EXAMINE CYCLING
    // Examines are named Examine1, Examine2, Examine3, ...
    // Each click advances to the next. At the last one, stay there on repeat clicks.
    // Earlier layers are freed once we advance past them.
    // -------------------------------------------------------------------------
    public static void CycleExamine(TileMapLayer clicked)
    {
        if (clicked == null)
            return;

        // Collect all sibling Examine layers (same parent), sorted by number.
        var parent = clicked.GetParent();
        if (parent == null)
            return;

        var examineLayers = GetSortedExamineLayers(parent);
        if (examineLayers.Count == 0)
            return;

        // Find which layer is currently visible.
        int currentIndex = FindVisibleIndex(examineLayers);

        if (currentIndex == -1)
        {
            // Nothing visible yet — show the first one.
            ShowLayer(examineLayers[0]);
            return;
        }

        int lastIndex = examineLayers.Count - 1;

        if (currentIndex >= lastIndex)
        {
            // Already at the last examine — stay here, do nothing more.
            return;
        }

        // Advance to next.
        HideLayer(examineLayers[currentIndex]);
        ShowLayer(examineLayers[currentIndex + 1]);

        // Free all layers before the one we just showed so memory is cleaned up.
        FreeLayersBefore(examineLayers, currentIndex + 1);
    }

    // -------------------------------------------------------------------------
    // CLARIFY
    // There is exactly one Clarify1 layer per object.
    // When triggered: hide/free all sibling Examine layers, then show Clarify1.
    // -------------------------------------------------------------------------
    public static void TriggerClarify(TileMapLayer clicked)
    {
        if (clicked == null)
            return;

        var parent = clicked.GetParent();
        if (parent == null)
            return;

        // Find Clarify1 among siblings or children of clicked.
        TileMapLayer clarifyLayer = FindClarifyLayer(parent) ?? FindClarifyLayer(clicked);

        if (clarifyLayer == null)
        {
            // No Clarify layer found — cancel clarify mode and bail.
            Globals.Instance.IS_CLARIFYING = false;
            Logger.Info("[ExamineHelper] No Clarify1 found; cancelling clarify mode.");
            return;
        }

        // Hide and free all sibling Examine layers.
        var examineLayers = GetSortedExamineLayers(parent);
        foreach (var layer in examineLayers)
            FreeExamineLayer(layer);

        // Show the Clarify layer and update global state.
        ShowLayer(clarifyLayer);
        ApplyClarifyGlobalState();
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns all TileMapLayer children of <paramref name="parent"/> whose names
    /// start with "Examine", sorted by their trailing number.
    /// </summary>
    private static List<TileMapLayer> GetSortedExamineLayers(Node parent)
    {
        return parent.GetChildren()
            .OfType<TileMapLayer>()
            .Where(t => t.Name.ToString().StartsWith("Examine"))
            .OrderBy(t => ExtractExamineNumber(t.Name.ToString()))
            .ToList();
    }

    /// <summary>
    /// Finds the first TileMapLayer child of <paramref name="parent"/> named "Clarify1".
    /// </summary>
    private static TileMapLayer FindClarifyLayer(Node parent)
    {
        return parent.GetChildren()
            .OfType<TileMapLayer>()
            .FirstOrDefault(t => t.Name.ToString() == "Clarify1");
    }

    private static int FindVisibleIndex(List<TileMapLayer> layers)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (IsVisible(layers[i]))
                return i;
        }
        return -1;
    }

    public static void ShowLayer(TileMapLayer layer)
    {
        if (layer == null)
            return;

        SetAlpha(layer, 1.0f);

        // Special case: showing Clarify1 on Toast removes Cup2 from the scene.
        if (layer.Name.ToString() == "Clarify1" &&
            layer.GetParent()?.Name.ToString() == "Toast")
        {
            var cup2 = SceneManager.Instance?.CurrentScene?.GetNodeOrNull<TileMapLayer>("Objects/Cup2");
            cup2?.QueueFree();
        }
    }

    public static void HideLayer(TileMapLayer layer)
    {
        if (layer == null)
            return;

        SetAlpha(layer, 0.0f);
    }

    private static void ApplyClarifyGlobalState()
    {
        Globals.Instance.IS_CLARIFYING = false;
        Globals.Instance.CLARITY_TOGGLE_COUNT--;
        CursorHelper.ResetCursor();

        int newCount = Globals.Instance.CLARITY_TOGGLE_COUNT;
        int oldCount = newCount + 1;

        var scene = SceneManager.Instance?.CurrentScene;
        var oldToggle = scene?.GetNodeOrNull<TileMapLayer>($"UI/ClarityToggle/{oldCount}");
        var newToggle = scene?.GetNodeOrNull<TileMapLayer>($"UI/ClarityToggle/{newCount}");

        if (oldToggle != null) SetAlpha(oldToggle, 0.0f);
        if (newToggle != null) SetAlpha(newToggle, 1.0f);

        Logger.Info($"[ExamineHelper] Clarity used. Toggle count now: {newCount}");
    }

    private static void SetAlpha(CanvasItem item, float alpha)
    {
        var mod = item.Modulate;
        mod.A = alpha;
        item.Modulate = mod;

        var selfMod = item.SelfModulate;
        selfMod.A = alpha;
        item.SelfModulate = selfMod;
    }

    private static bool IsVisible(CanvasItem item)
    {
        // Consider visible when both Modulate and SelfModulate alpha are fully opaque.
        return item.Modulate.A >= 0.9f && item.SelfModulate.A >= 0.9f;
    }

    /// <summary>
    /// Extracts the trailing integer from an "ExamineN" layer name.
    /// Always parses from the literal prefix "Examine" regardless of IS_CLARIFYING,
    /// so sorting is never corrupted by global clarify state.
    /// </summary>
    public static int ExtractExamineNumber(string layerName)
    {
        const string prefix = "Examine";
        if (layerName.StartsWith(prefix) &&
            int.TryParse(layerName.Substring(prefix.Length), out int number))
            return number;
        return 0;
    }

    /// <summary>
    /// Frees layers at indices 0..(keepFromIndex - 1).
    /// </summary>
    private static void FreeLayersBefore(List<TileMapLayer> layers, int keepFromIndex)
    {
        for (int i = 0; i < keepFromIndex; i++)
            FreeExamineLayer(layers[i]);
    }

    private static void FreeExamineLayer(TileMapLayer layer)
    {
        if (layer == null || !GodotObject.IsInstanceValid(layer))
            return;

        // Disable all processing on ExamineHandler subclasses before freeing
        // so their _ExitTree cursor cleanup runs correctly.
        if (layer is ExamineHandler handler)
        {
            handler.HoverEnabled = false;
            handler.SetProcessInput(false);
            handler.SetProcess(false);
        }

        layer.QueueFree();
    }
}