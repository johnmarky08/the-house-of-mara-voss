using Godot;
using System.Collections.Generic;
using System.Linq;

public static class ExamineHelper
{
    public static void CycleExamine(TileMapLayer currentExamine)
    {
        if (currentExamine == null)
            return;

        // Determine if this is a parent node or an examine child node
        var parent = currentExamine.GetParent();
        Node targetParent = null;
        TileMapLayer targetLayer = null;

        // Check if this layer has Examine children (it's a parent like Toast)
        var childrenExamineLayers = new List<TileMapLayer>();
        foreach (var child in currentExamine.GetChildren())
        {
            if (child is TileMapLayer tileLayer && tileLayer.Name.ToString().StartsWith("Examine"))
            {
                childrenExamineLayers.Add(tileLayer);
            }
        }

        // If this node has Examine children, use it as the parent
        if (childrenExamineLayers.Count > 0)
        {
            targetParent = currentExamine;
            ProcessExamineLayerCycle(childrenExamineLayers);
            return;
        }

        // Otherwise, this IS an Examine child - collect only siblings from this specific parent
        targetParent = parent;
        targetLayer = currentExamine;

        if (targetParent == null)
            return;

        // Collect ONLY Examine layers that are direct children of this specific parent
        var examineLayers = new List<TileMapLayer>();
        foreach (var child in targetParent.GetChildren())
        {
            if (child is TileMapLayer tileLayer && tileLayer.Name.ToString().StartsWith("Examine"))
            {
                examineLayers.Add(tileLayer);
            }
        }

        if (examineLayers.Count == 0)
            return;

        // Sort by examine number and ensure independence
        examineLayers = examineLayers.OrderBy(layer => ExtractExamineNumber(layer.Name.ToString())).ToList();

        // IMPORTANT: Check which layer is actually visible, not which was clicked
        // This ensures each parent's examine cycle is truly independent
        int currentIndex = -1;
        for (int i = 0; i < examineLayers.Count; i++)
        {
            var color = examineLayers[i].Modulate;
            if (color.A >= 0.9f)  // Visible
            {
                currentIndex = i;
                break;
            }
        }

        // If none visible, assume clicked layer
        if (currentIndex == -1)
            currentIndex = examineLayers.FindIndex(layer => layer == targetLayer);

        if (currentIndex == -1)
            return;

        // If at the last examine, stay there
        if (currentIndex >= examineLayers.Count - 1)
        {
            ShowExamine(examineLayers[currentIndex]);
            PruneEarlierExamineLayers(examineLayers, currentIndex);
            return;
        }

        // Hide current, show next
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

        // Sort by examine number
        examineLayers = examineLayers.OrderBy(layer => ExtractExamineNumber(layer.Name.ToString())).ToList();

        // Find which examine is currently visible
        int currentIndex = -1;
        for (int i = 0; i < examineLayers.Count; i++)
        {
            var color = examineLayers[i].Modulate;
            if (color.A >= 0.9f)  // Visible
            {
                currentIndex = i;
                break;
            }
        }

        // If none visible, start at index 0
        if (currentIndex == -1)
            currentIndex = 0;

        // If at the last examine, stay there
        if (currentIndex >= examineLayers.Count - 1)
        {
            ShowExamine(examineLayers[currentIndex]);
            PruneEarlierExamineLayers(examineLayers, currentIndex);
            return;
        }

        // Hide current, show next
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

        var color = examineLayer.Modulate;
        color.A = 1.0f;
        examineLayer.Modulate = color;
    }

    public static void HideExamine(TileMapLayer examineLayer)
    {
        if (examineLayer == null)
            return;

        var color = examineLayer.Modulate;
        color.A = 0.0f;
        examineLayer.Modulate = color;
    }

    public static int ExtractExamineNumber(string layerName)
    {
        if (layerName.StartsWith("Examine") && int.TryParse(layerName.Substring(7), out int number))
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
