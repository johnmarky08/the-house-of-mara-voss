using Godot;

public partial class ShardSlot : Area2D
{
	[Export] public int SlotIndex = 0;

	private Sprite2D _sprite;

	public override void _Ready()
	{
		InputPickable = true;
		_sprite = GetNode<Sprite2D>("Sprite2D");
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		if (Globals.Instance != null && Globals.Instance.SHARDS_PLACED[SlotIndex])
		{
			_sprite.Visible = true;
		}
		else
		{
			_sprite.Visible = false;
		}
	}

	public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			GetTree().Root.SetInputAsHandled();
			TryPlaceShard();
		}
	}

	private void TryPlaceShard()
	{
		if (Globals.Instance == null) return;

		// If already placed, do nothing
		if (Globals.Instance.SHARDS_PLACED[SlotIndex]) return;

		// Find a collected shard in inventory
		int shardToPlace = -1;
		for (int i = 0; i < Globals.Instance.SHARDS_COLLECTED.Count; i++)
		{
			if (Globals.Instance.SHARDS_COLLECTED[i])
			{
				shardToPlace = i;
				break;
			}
		}

		if (shardToPlace != -1)
		{
			// Place it
			Globals.Instance.SHARDS_COLLECTED[shardToPlace] = false; // Take out of inventory
			Globals.Instance.SHARDS_PLACED[SlotIndex] = true;
			UpdateVisuals();
		}
	}
}
