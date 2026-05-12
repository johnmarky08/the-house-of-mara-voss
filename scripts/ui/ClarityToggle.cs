using Godot;
public partial class ClarityToggle : TileMapHoverCursor
{
	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseButton)
			return;

		if (!mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
			return;

		if (!HasPointedTile(mouseButton.Position))
			return;

		if (Globals.Instance.CLARITY_TOGGLE_COUNT <= 1)
		{
			Logger.Info("No clarity toggles left!");
			return;
		}

		CursorHelper.ApplyCursor("res://assets/images/core/cursor_eye.png", 35, 35);
		Globals.Instance.IS_CLARIFYING = true;
	}

	public override void _Process(double delta)
	{
		if (Globals.Instance.IS_CLARIFYING)
			return;

		base._Process(delta);
	}

	private bool HasPointedTile(Vector2 globalMousePosition)
	{
		Vector2 localMousePosition = ToLocal(globalMousePosition);
		Vector2I cell = LocalToMap(localMousePosition);
		return GetCellSourceId(cell) != -1;
	}
}
