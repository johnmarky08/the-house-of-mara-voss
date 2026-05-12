using Godot;
using System.Collections.Generic;

public partial class RoomExamineTracker : Node
{
    // Track which examine nodes have been clicked: roomName -> objectName -> set of examine layer names
    private readonly Dictionary<string, Dictionary<string, HashSet<string>>> _examineProgress = new();

    private static RoomExamineTracker _instance;

    public override void _Ready()
    {
        _instance = this;
    }

    public static void OnExamineClicked(TileMapLayer examineNode)
    {
        if (_instance == null)
            return;

        var parent = examineNode.GetParent();
        if (parent == null)
            return;

        string roomName = GetRoomNameFromNode(examineNode);
        string objectName = parent.Name.ToString();
        string examineName = examineNode.Name.ToString();

        if (!_instance._examineProgress.ContainsKey(roomName))
        {
            _instance._examineProgress[roomName] = [];
        }

        if (!_instance._examineProgress[roomName].ContainsKey(objectName))
        {
            _instance._examineProgress[roomName][objectName] = [];
        }

        _instance._examineProgress[roomName][objectName].Add(examineName);
    }

    private static string GetRoomNameFromNode(Node node)
    {
        var current = node;
        while (current != null)
        {
            current = current.GetParent();
            if (current != null && current.Name.ToString() != "Objects")
            {
                if (current is Node2D or CanvasLayer)
                {
                    return current.Name.ToString();
                }
            }
        }
        return "Room1";
    }

    public static bool HasObjectBeenFullyExamined(string roomName, string objectName)
    {
        if (_instance == null)
            return false;

        if (!_instance._examineProgress.ContainsKey(roomName))
            return false;

        if (!_instance._examineProgress[roomName].ContainsKey(objectName))
            return false;

        var root = _instance.GetTree().Root.GetChild(0);
        var roomNode = root.FindChild(roomName, true, false);

        if (roomNode == null)
            return false;

        var objectNode = roomNode.FindChild(objectName, true, false);

        if (objectNode == null)
            return false;

        int totalExamines = 0;
        foreach (var child in objectNode.GetChildren())
        {
            if (child is TileMapLayer tileLayer && tileLayer.Name.ToString().StartsWith("Examine"))
            {
                totalExamines++;
            }
        }

        int clickedExamines = _instance._examineProgress[roomName][objectName].Count;
        return clickedExamines >= totalExamines;
    }

    public static bool HasRoomBeenFullyExamined(string roomName, string objectName = null)
    {
        if (_instance == null)
            return false;

        if (!string.IsNullOrEmpty(objectName))
        {
            return HasObjectBeenFullyExamined(roomName, objectName);
        }

        if (!_instance._examineProgress.ContainsKey(roomName))
            return false;

        var root = _instance.GetTree().Root.GetChild(0);
        var roomNode = root.FindChild(roomName, true, false);

        if (roomNode == null)
            return false;

        var objectsNode = roomNode.FindChild("Objects", true, false);

        if (objectsNode == null)
            return false;

        foreach (var child in objectsNode.GetChildren())
        {
            bool hasExamineChildren = false;
            foreach (var subChild in child.GetChildren())
            {
                if (subChild is TileMapLayer tileLayer && tileLayer.Name.ToString().StartsWith("Examine"))
                {
                    hasExamineChildren = true;
                    break;
                }
            }

            if (hasExamineChildren)
            {
                if (!HasObjectBeenFullyExamined(roomName, child.Name.ToString()))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static Dictionary<string, Dictionary<string, HashSet<string>>> GetExamineProgress()
    {
        return _instance?._examineProgress ?? new Dictionary<string, Dictionary<string, HashSet<string>>>();
    }

    public static void ResetProgress(string roomName = null)
    {
        if (_instance == null)
            return;

        if (string.IsNullOrEmpty(roomName))
        {
            _instance._examineProgress.Clear();
        }
        else if (_instance._examineProgress.ContainsKey(roomName))
        {
            _instance._examineProgress[roomName].Clear();
        }
    }
}
