using Godot;

public partial class BrassKey1 : Area2D
{
	public override void _Ready()
	{
		InputPickable = true;
	}

	public override async void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
	{
		if (!Visible)
			return;

		if (@event is not InputEventMouseButton mouseEvent)
			return;

		if (!mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
			return;

		Globals.Instance.SHARDS_COLLECTED[0] = true;
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		SceneManager.ChangeScene("res://scenes/world/room_1.tscn");
	}
}
