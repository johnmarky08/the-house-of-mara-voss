using Godot;

public partial class BackArrowTransition : Sprite2D
{
    [ExportCategory("Scene Transition")]
    [Export(PropertyHint.File, "*.tscn")] public string TargetScenePath = "";
    [Export] public bool TriggerEnabled = true;
    [Export] public bool RequireVisible = true;
    [Export] public NodePath ClickAreaPath = "Area2D";

    private Area2D _clickArea;

    public override void _Ready()
    {
        SetProcessInput(true);
        _clickArea = GetNodeOrNull<Area2D>(ClickAreaPath);
        if (_clickArea == null)
            Logger.Error($"{Name} could not resolve ClickAreaPath: {ClickAreaPath}");
        else
        {
            _clickArea.InputPickable = true;
            _clickArea.InputEvent += OnClickAreaInputEvent;
        }
    }

    public override void _ExitTree()
    {
        if (_clickArea != null)
            _clickArea.InputEvent -= OnClickAreaInputEvent;
    }

    public override void _Input(InputEvent @event)
    {
        TryHandleClick(@event);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        TryHandleClick(@event);
    }

    private void TryHandleClick(InputEvent @event)
    {
        if (!TriggerEnabled || SceneManager.IsChanging)
            return;

        if (@event is not InputEventMouseButton mouseButton)
            return;

        if (!mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
            return;

        if (RequireVisible && !Visible)
            return;

        Vector2 globalMousePosition = GetGlobalMousePosition();

        if (!HasPointInArea(globalMousePosition))
            return;

        if (string.IsNullOrWhiteSpace(TargetScenePath))
        {
            Logger.Error($"{Name} has no TargetScenePath configured.");
            return;
        }

        SceneManager.ChangeScene(TargetScenePath);
        GetTree().Root.SetInputAsHandled();
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

    private bool HasPointInArea(Vector2 globalMousePosition)
    {
        if (_clickArea == null)
            return false;

        var collision = _clickArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (collision == null || collision.Shape == null)
            return false;

        Vector2 localMouse = collision.ToLocal(globalMousePosition);

        if (collision.Shape is RectangleShape2D rect)
        {
            Vector2 half = rect.Size / 2f;
            return localMouse.X >= -half.X &&
                localMouse.X <= half.X &&
                localMouse.Y >= -half.Y &&
                localMouse.Y <= half.Y;
        }

        if (collision.Shape is CircleShape2D circle)
            return localMouse.LengthSquared() <= circle.Radius * circle.Radius;

        return false;
    }
}
