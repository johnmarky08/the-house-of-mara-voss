using Godot;

public partial class RightArrow : Sprite2D
{
    /* public override void _Input(InputEvent @event)
    {
        if (!Visible)
            return;

        if (@event is not InputEventMouseButton mouseEvent)
            return;

        if (!mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
            return;

        Globals.Instance.SHARDS_COLLECTED[0] = true;
        GetTree().ProcessFrame += async () =>
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            SceneManager.ChangeScene("res://scenes/world/room_2.tscn");
        };
    } */

}
