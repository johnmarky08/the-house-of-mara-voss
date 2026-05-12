using Godot;

public partial class ExamineHandler : TileMapLayer
{
    [ExportCategory("Hover Cursor")]
    [Export(PropertyHint.File, "*.png,*.webp,*.jpg")] public string CursorTexturePath = "res://assets/images/core/cursor_magnifying_glass.png";
    [Export] public int CursorWidth = 30;
    [Export] public int CursorHeight = 30;
    [Export] public bool HoverEnabled = true;

    private bool _isHovering = false;

    public override void _Ready()
    {
        SetProcess(true);
        SetProcessInput(true);
    }

    public override void _ExitTree()
    {
        if (_isHovering)
        {
            CursorHelper.EndHover(this);
            _isHovering = false;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!(@event is InputEventMouseButton mouseEvent) || !mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
            return;

        Vector2 globalMouse = GetGlobalMousePosition();
        Vector2 localMouse = ToLocal(globalMouse);
        Vector2I cell = LocalToMap(localMouse);

        bool hasTile = GetCellSourceId(cell) != -1;

        if (hasTile)
        {
            ExamineHelper.CycleExamine(this);
            RoomExamineTracker.OnExamineClicked(this);
            OnExamineClicked();
            OnAnyExamineClicked();
            GetTree().Root.SetInputAsHandled();
        }
    }

    private void OnAnyExamineClicked()
    {
        if (ExamineHelper.ExtractExamineNumber(Name.ToString()) != 1)
            Globals.Instance.CORRUPTION_COUNT++;

        // Determine current room name by walking up the scene tree (mirrors RoomExamineTracker logic)
        string roomName = "Room1";
        Node current = this;
        while (current != null)
        {
            current = current.GetParent();
            if (current != null && current.Name.ToString() != "Objects")
            {
                if (current is Node2D || current is CanvasLayer)
                {
                    roomName = current.Name.ToString();
                    break;
                }
            }
        }

        var parent = GetParent();
        if (RoomExamineTracker.HasRoomBeenFullyExamined(roomName) && parent != null && parent.Name.ToString().EndsWith("Final"))
        {
            CursorTexturePath = "res://assets/images/core/cursor_magnifying_glass.png";
            HoverEnabled = true;
        }
    }

    protected virtual void OnExamineClicked()
    {
    }

    public override void _Process(double delta)
    {
        if (!HoverEnabled)
            return;

        Vector2 globalMouse = GetGlobalMousePosition();
        Vector2 localMouse = ToLocal(globalMouse);
        Vector2I cell = LocalToMap(localMouse);

        bool hasTile = GetCellSourceId(cell) != -1;

        if (hasTile)
        {
            if (!_isHovering)
            {
                CursorHelper.BeginHover(this, CursorTexturePath, CursorWidth, CursorHeight);
                _isHovering = true;
            }
        }
        else
        {
            if (_isHovering)
            {
                CursorHelper.EndHover(this);
                _isHovering = false;
            }
        }
    }
}
