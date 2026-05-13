using Godot;

public partial class Photo1Reward : Area2D
{
	private const string NextScenePath = "res://scenes/world/room_3.tscn";

	public override void _Ready()
	{
		InputPickable = true;
	}

	public override void _Input(InputEvent @event)
	{
		if (!Visible)
			return;

		if (@event is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
			return;

		var shapeOwner = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (shapeOwner == null || shapeOwner.Shape is not RectangleShape2D rectShape)
			return;

		var globalMousePos = GetGlobalMousePosition();
		var localMousePos = shapeOwner.ToLocal(globalMousePos);

		var size = rectShape.Size;
		var rect = new Rect2(-size / 2, size);

		if (rect.HasPoint(localMousePos))
		{
			GetTree().Root.SetInputAsHandled();
			CollectAndChangeScene();
		}
	}

	private async void CollectAndChangeScene()
	{
		if (Globals.Instance != null)
			Globals.Instance.PHOTO1_COLLECTED = true;

		Globals.SHARDS_COLLECTED[1] = true;
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		SceneManager.ChangeScene(NextScenePath);
	}
}
