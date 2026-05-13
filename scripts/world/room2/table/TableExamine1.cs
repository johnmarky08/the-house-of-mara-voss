using Godot;

public partial class TableExamine1 : ExamineHandler
{
    public override void _Input(InputEvent @event)
    {
        if (Dialogue.IsInputBlocked)
            return;

        if (!(@event is InputEventMouseButton mouseEvent) || !mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
            return;

        Vector2 globalMouse = GetGlobalMousePosition();
        Vector2 localMouse = ToLocal(globalMouse);
        Vector2I cell = LocalToMap(localMouse);

        bool hasTile = GetCellSourceId(cell) != -1;

        if (hasTile)
        {
            RoomExamineTracker.OnExamineClicked(this);
            OnExamineClicked();
            GetTree().Root.SetInputAsHandled();
        }
    }


    protected override void OnExamineClicked()
    {
        if (Globals.Instance != null && Globals.SHARDS_COLLECTED[0])
            return;

        string roomName = "Room2";
        var parent = GetParent();
        bool isFullyExamined = RoomExamineTracker.HasRoomBeenFullyExamined(roomName);
        bool hasRightParent = parent != null && parent.Name.ToString().EndsWith("Final");

        if (isFullyExamined && hasRightParent)
        {
            Logger.Debug("[Table Unlocked] Changing to puzzle scene!");
            Logger.Info("Current Corruption Count: " + Globals.Instance.CORRUPTION_COUNT);
            SceneManager.ChangeScene("res://scenes/world/room_2_puzzle.tscn");
        }
    }
}