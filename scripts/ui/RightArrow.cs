using Godot;

public partial class RightArrow : Area2D
{
    public override void _Ready()
    {
        InputPickable = true;
    }

    public override async void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is not InputEventMouseButton mouseEvent)
            return;

        if (!mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
            return;

        if (Globals.Instance != null)
            Globals.Instance.SHARDS_COLLECTED[0] = true;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        SceneManager.ChangeScene("res://scenes/world/room_2.tscn");
    }
}