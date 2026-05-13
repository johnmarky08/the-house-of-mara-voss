using Godot;
using System.Collections.Generic;

public partial class RoomExamineTracker : Node
{
    private static readonly string[] Room1RequiredObjects =
    [
        "Window",
        "Cup2",
        "Toast",
        "Note",
        "Clock",
        "Calendar",
        "Drawing"
    ];

    private static readonly string[] Room2RequiredObjects =
    [
        "Bookshelf",
        "Phone",
        "RightChair",
        "LeftChair",
        "Painting",
        "Globe",
        "Letter"
    ];

    private static readonly string[] Room3RequiredObjects =
    [
        "Painting",
        "Bag",
        "Bed",
        "Note"
    ];

    // Track which examine nodes have been clicked: roomName -> objectName -> set of examine layer names
    private static Dictionary<string, Dictionary<string, HashSet<string>>> _examineProgress = new();

    private static RoomExamineTracker _instance;

    public override void _Ready()
    {
        _instance = this;
    }

    private static RoomExamineTracker GetInstance()
    {
        if (_instance != null)
            return _instance;

        return null;
    }

    public static void OnExamineClicked(TileMapLayer examineNode)
    {
        if (examineNode == null)
            return;

        var parent = examineNode.GetParent();
        if (parent == null)
            return;

        string roomName = GetRoomNameFromNode(examineNode);
        string objectName = parent.Name.ToString();
        string examineName = examineNode.Name.ToString();

        if (objectName.EndsWith("Final"))
            return;

        if (!_examineProgress.ContainsKey(roomName))
            _examineProgress[roomName] = [];


        if (!_examineProgress[roomName].ContainsKey(objectName))
            _examineProgress[roomName][objectName] = [];


        _examineProgress[roomName][objectName].Add(examineName);
    }

    private static string GetRoomNameFromNode(Node node)
    {
        var current = node;
        bool passedObjects = false;

        while (current != null)
        {
            current = current.GetParent();
            if (current == null)
                break;

            string currentName = current.Name.ToString();

            // Track when we pass the Objects node
            if (currentName == "Objects")
            {
                passedObjects = true;
                continue;
            }

            // Once we've passed Objects, the room root can be a Node2D, CanvasLayer, or TileMapLayer
            if (passedObjects && (current is Node2D or CanvasLayer or TileMapLayer))
                return currentName;
        }

        return "Room1";
    }

    public static bool HasObjectBeenFullyExamined(string roomName, string objectName)
    {
        if (objectName.EndsWith("Final") || objectName.EndsWith("Exclude"))
            return true;

        if (_examineProgress.ContainsKey(roomName) && _examineProgress[roomName].ContainsKey(objectName))
        {
            int clickedExamines = _examineProgress[roomName][objectName].Count;
            return clickedExamines >= 1;
        }

        if (_examineProgress.ContainsKey(objectName) && _examineProgress[objectName].Count > 0)
            return true;

        return false;
    }

    public static bool HasRoomBeenFullyExamined(string roomName, string objectName = null)
    {
        if (!string.IsNullOrEmpty(objectName))
            return HasObjectBeenFullyExamined(roomName, objectName);

        List<string> notExamined = new();

        var requiredObjects = roomName switch
        {
            "Room1" => Room1RequiredObjects,
            "Room2" => Room2RequiredObjects,
            "Room3" => Room3RequiredObjects,
            _ => []
        };

        foreach (var requiredObject in requiredObjects)
        {
            if (!HasObjectBeenFullyExamined(roomName, requiredObject))
                notExamined.Add(requiredObject);
        }

        if (notExamined.Count > 0)
        {
            // Logger.Debug($"[Drawer Check] LOCKED - Still need to examine: {string.Join(", ", notExamined)}");
            return false;
        }

        // Logger.Debug($"[Drawer Check] ✓ ALL OBJECTS EXAMINED - Drawer is UNLOCKED!");
        return true;
    }

    public static Dictionary<string, Dictionary<string, HashSet<string>>> GetExamineProgress()
    {
        return _examineProgress;
    }

    public static void ResetProgress(string roomName = null)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            _examineProgress.Clear();
        }
        else if (_examineProgress.ContainsKey(roomName))
        {
            _examineProgress[roomName].Clear();
        }
    }
}
