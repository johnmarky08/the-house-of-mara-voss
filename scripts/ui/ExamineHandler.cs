using Godot;

public partial class ExamineHandler : TileMapLayer
{
    [ExportCategory("Hover Cursor")]
    [Export(PropertyHint.File, "*.png,*.webp,*.jpg")] public string UnlockedCursorTexturePath = "res://assets/images/core/cursor_magnifying_glass.png";
    [Export(PropertyHint.File, "*.png,*.webp,*.jpg")] public string LockedCursorTexturePath = "res://assets/images/core/cursor_locked_magnifying_glass.png";
    [Export] public int CursorWidth = 30;
    [Export] public int CursorHeight = 30;
    [Export] public bool HoverEnabled = true;

    private bool _isHovering = false;
    private string _currentHoverCursorTexturePath = string.Empty;

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
    }

    private static string GetRoomNameFromNode(Node node)
    {
        var current = node;
        bool passedObjects = false;

        while (current != null)
        {
            current = current.GetParent();
            if (current == null)
                break;

            string currentName = current.Name.ToString();

            if (currentName == "Objects")
            {
                passedObjects = true;
                continue;
            }

            if (passedObjects && (current is Node2D || current is CanvasLayer))
                return currentName;
        }

        return "Room1";
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
        var parent = GetParent();
        bool isFinal = parent != null && parent.Name.ToString().EndsWith("Final");

        if (hasTile)
        {
            if (isFinal)
            {
                string roomName = GetRoomNameFromNode(this);
                bool roomReady = RoomExamineTracker.HasRoomBeenFullyExamined(roomName);
                string desiredCursor = roomReady ? UnlockedCursorTexturePath : LockedCursorTexturePath;

                if (!_isHovering || _currentHoverCursorTexturePath != desiredCursor)
                {
                    CursorHelper.BeginHover(this, desiredCursor, CursorWidth, CursorHeight);
                    _currentHoverCursorTexturePath = desiredCursor;
                    _isHovering = true;
                }

                return;
            }

            if (!_isHovering)
            {
                CursorHelper.BeginHover(this, UnlockedCursorTexturePath, CursorWidth, CursorHeight);
                _currentHoverCursorTexturePath = UnlockedCursorTexturePath;
                _isHovering = true;
            }
        }
        else
        {
            if (_isHovering)
            {
                CursorHelper.EndHover(this);
                _isHovering = false;
                _currentHoverCursorTexturePath = string.Empty;
            }
        }
    }
}
