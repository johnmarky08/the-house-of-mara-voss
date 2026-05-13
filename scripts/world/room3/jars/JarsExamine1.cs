using Godot;

public partial class JarsExamine1 : ExamineHandler
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

        if (GetCellSourceId(cell) == -1)
            return;

        if (!RoomExamineTracker.HasRoomBeenFullyExamined("Room3"))
        {
            // Locked. Do nothing.
            return;
        }

        // Unlocked. Use base logic.
        base._Input(@event);
    }
}
