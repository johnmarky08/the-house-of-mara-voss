using Godot;
using System.Collections.Generic;
using System.Linq;

public static class ExamineHelper
{
    public static void CycleExamine(TileMapLayer currentExamine)
    {
        string fileStart = Globals.Instance.IS_CLARIFYING ? "Clarify" : "Examine";
        if (currentExamine == null)
            return;

        var parent = currentExamine.GetParent();
        Node targetParent = null;
        TileMapLayer targetLayer = null;

        var childrenExamineLayers = new List<TileMapLayer>();
        foreach (var child in currentExamine.GetChildren())
        {
            if (child is TileMapLayer tileLayer && tileLayer.Name.ToString().StartsWith(fileStart))
            {
                childrenExamineLayers.Add(tileLayer);
            }
        }

        if (childrenExamineLayers.Count > 0)
        {
            targetParent = currentExamine;

            if (Globals.Instance.IS_CLARIFYING)
            {
                foreach (var child in currentExamine.GetChildren())
                {
                    if (child is TileMapLayer tile && tile.Name.ToString().StartsWith("Examine"))
                        HideExamine(tile);
                }
            }

            ProcessExamineLayerCycle(childrenExamineLayers);
            return;
        }

        targetParent = parent;
        targetLayer = currentExamine;

        if (targetParent == null)
            return;

        var examineLayers = new List<TileMapLayer>();

        if (Globals.Instance.IS_CLARIFYING)
        {
            foreach (var child in targetParent.GetChildren())
            {
                if (child is TileMapLayer tile && tile.Name.ToString().StartsWith("Examine"))
                    HideExamine(tile);
            }
        }

        foreach (var child in targetParent.GetChildren())
        {
            if (child is TileMapLayer tileLayer && tileLayer.Name.ToString().StartsWith(fileStart))
            {
                examineLayers.Add(tileLayer);
            }
        }

        if (examineLayers.Count == 0)
            return;

        examineLayers = examineLayers.OrderBy(layer => ExtractExamineNumber(layer.Name.ToString())).ToList();

        int currentIndex = -1;
        for (int i = 0; i < examineLayers.Count; i++)
        {
            if (IsLayerVisible(examineLayers[i]))
            {
                currentIndex = i;
                break;
            }
        }

        if (Globals.Instance.IS_CLARIFYING)
        {
            if (targetLayer != null)
                HideExamine(targetLayer);

            if (currentIndex == -1)
            {
                ShowExamine(examineLayers[0]);
                if (examineLayers.Count <= 1)
                    PruneEarlierExamineLayers(examineLayers, 0);
                return;
            }
        }

        if (currentIndex == -1)
            currentIndex = examineLayers.FindIndex(layer => layer == targetLayer);

        if (currentIndex == -1)
            return;

        if (currentIndex >= examineLayers.Count - 1)
        {
            ShowExamine(examineLayers[currentIndex]);
            PruneEarlierExamineLayers(examineLayers, currentIndex);
            return;
        }

        HideExamine(examineLayers[currentIndex]);
        var nextExamine = examineLayers[currentIndex + 1];
        ShowExamine(nextExamine);

        if (currentIndex + 1 >= examineLayers.Count - 1)
            PruneEarlierExamineLayers(examineLayers, currentIndex + 1);
    }

    private static void ProcessExamineLayerCycle(List<TileMapLayer> examineLayers)
    {
        if (examineLayers.Count == 0)
            return;

        examineLayers = examineLayers.OrderBy(layer => ExtractExamineNumber(layer.Name.ToString())).ToList();

        int currentIndex = -1;
        for (int i = 0; i < examineLayers.Count; i++)
        {
            if (IsLayerVisible(examineLayers[i]))
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex == -1)
        {
            ShowExamine(examineLayers[0]);
            if (examineLayers.Count <= 1)
                PruneEarlierExamineLayers(examineLayers, 0);
            return;
        }

        if (currentIndex >= examineLayers.Count - 1)
        {
            ShowExamine(examineLayers[currentIndex]);
            PruneEarlierExamineLayers(examineLayers, currentIndex);
            return;
        }

        HideExamine(examineLayers[currentIndex]);
        var nextExamine = examineLayers[currentIndex + 1];
        ShowExamine(nextExamine);

        if (currentIndex + 1 >= examineLayers.Count - 1)
            PruneEarlierExamineLayers(examineLayers, currentIndex + 1);
    }

    public static void ShowExamine(TileMapLayer examineLayer)
    {
        if (examineLayer == null)
            return;

        if (examineLayer.Name.ToString() == "Clarify1" && examineLayer.GetParent() != null && examineLayer.GetParent().Name.ToString() == "Toast")
        {
            var currentScene = SceneManager.Instance?.CurrentScene;
            var cup2Node = currentScene?.GetNodeOrNull<TileMapLayer>("Objects/Cup2");
            cup2Node?.QueueFree();
        }

        var color = examineLayer.Modulate;
        color.A = 1.0f;
        examineLayer.Modulate = color;

        if (Globals.Instance.IS_CLARIFYING)
        {
            Globals.Instance.IS_CLARIFYING = false;
            Globals.Instance.CLARITY_TOGGLE_COUNT--;
            CursorHelper.ResetCursor();

            int newToggleNumber = Globals.Instance.CLARITY_TOGGLE_COUNT;
            int toggleNumber = newToggleNumber + 1;
            string newToggleName = newToggleNumber.ToString();
            string toggleName = toggleNumber.ToString();

            var currentScene = SceneManager.Instance?.CurrentScene;
            var togglePath = $"UI/ClarityToggle/{toggleName}";
            var newTogglePath = $"UI/ClarityToggle/{newToggleName}";

            var toggle = currentScene?.GetNodeOrNull<TileMapLayer>(togglePath);
            var newToggle = currentScene?.GetNodeOrNull<TileMapLayer>(newTogglePath);

            if (toggle != null)
                SetLayerAlpha(toggle, 0.0f);

            if (newToggle != null)
                SetLayerAlpha(newToggle, 1.0f);

            Logger.Info("New Clarity Toggle Count: " + newToggleName);
        }
    }

    public static void HideExamine(TileMapLayer examineLayer)
    {
        if (examineLayer == null)
            return;

        SetLayerAlpha(examineLayer, 0.0f);
    }

    private static void SetLayerAlpha(CanvasItem layer, float alpha)
    {
        var modulate = layer.Modulate;
        modulate.A = alpha;
        layer.Modulate = modulate;

        var selfModulate = layer.SelfModulate;
        selfModulate.A = alpha;
        layer.SelfModulate = selfModulate;
    }

    private static bool IsLayerVisible(CanvasItem layer)
    {
        return layer.Modulate.A >= 0.9f && layer.SelfModulate.A >= 0.9f;
    }

    public static int ExtractExamineNumber(string layerName)
    {
        string fileStart = Globals.Instance.IS_CLARIFYING ? "Clarify" : "Examine";

        if (layerName.StartsWith(fileStart) && int.TryParse(layerName.Substring(fileStart.Length), out int number))
            return number;
        return 0;
    }

    private static void PruneEarlierExamineLayers(IReadOnlyList<TileMapLayer> examineLayers, int keepIndex)
    {
        for (int index = 0; index < keepIndex; index++)
        {
            var layer = examineLayers[index];
            if (GodotObject.IsInstanceValid(layer))
                layer.QueueFree();
        }
    }
}
