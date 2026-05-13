using Godot;

public partial class Door : AnimatedSprite2D
{
	[ExportCategory("Scene Transition")]
	[Export(PropertyHint.File, "*.tscn")] public string TargetScenePath = "";
	[Export] public bool TriggerEnabled = true;
	[Export] public bool RequireVisible = true;
	[Export] public NodePath ClickAreaPath = "Area2D";

	private Area2D _clickArea;

	public override void _Ready()
	{
		_clickArea = GetNodeOrNull<Area2D>(ClickAreaPath);
		if (_clickArea == null)
		{
			Logger.Error($"{Name} could not resolve ClickAreaPath: {ClickAreaPath}");
			return;
		}

		_clickArea.InputPickable = true;
		_clickArea.InputEvent += OnClickAreaInputEvent;
	}

	public override void _ExitTree()
	{
		if (_clickArea != null)
			_clickArea.InputEvent -= OnClickAreaInputEvent;
	}

	private void OnClickAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (!TriggerEnabled || SceneManager.IsChanging)
			return;

		if (@event is not InputEventMouseButton mouseButton)
			return;

		if (!mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
			return;

		if (RequireVisible && !Visible)
			return;

		if (string.IsNullOrWhiteSpace(TargetScenePath))
		{
			Logger.Error($"{Name} has no TargetScenePath configured.");
			return;
		}

		SceneManager.ChangeScene(TargetScenePath);
		GetTree().Root.SetInputAsHandled();
	}
}