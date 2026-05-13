using Godot;

public partial class BrassKey1 : Area2D
{
	private string FontPath = "res://assets/fonts/CormorantGaramond-VariableFont_wght.ttf";

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

		Globals.SHARDS_COLLECTED[0] = true;
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		await Dialogue.ShowText(this, "Every morning started here. Every morning she was already waiting, already loud, already herself.", 6.0f, GlobalPosition.X, GlobalPosition.Y, FontPath, 26);
		await Dialogue.ShowText(this, "You could set your clock by her. You always knew, in that kitchen, that someone else was in the world with you.", 6.0f, GlobalPosition.X, GlobalPosition.Y, FontPath, 24);
		await Dialogue.ShowText(this, "That's not something you notice until it's gone.", 4.0f, GlobalPosition.X, GlobalPosition.Y, FontPath, 30);

		SceneManager.ChangeScene("res://scenes/world/room_2.tscn");
	}
}
