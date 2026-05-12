using Godot;

public partial class TileMapHoverCursor : TileMapLayer
{
    [ExportCategory("Hover Cursor")]
    [Export(PropertyHint.File, "*.png,*.webp,*.jpg")] public string CursorTexturePath = "res://assets/images/core/cursor_magnifying_glass.png";
    [Export] public int CursorWidth = 32;
    [Export] public int CursorHeight = 32;
    [Export] public bool HoverEnabled = true;

    private bool _isHovering = false;

    public override void _Ready()
    {
        SetProcess(true);
    }

    public override void _ExitTree()
    {
        if (_isHovering)
        {
            CursorHelper.EndHover(this);
            _isHovering = false;
        }
    }

    public override void _Process(double delta)
    {
        if (Dialogue.IsInputBlocked)
            return;

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
                ApplyCursor();
                _isHovering = true;
            }
        }
        else
        {
            if (_isHovering)
            {
                ResetCursor();
                _isHovering = false;
            }
        }
    }

    private void ApplyCursor()
    {
        CursorHelper.BeginHover(this, CursorTexturePath, CursorWidth, CursorHeight);
    }

    private void ResetCursor()
    {
        CursorHelper.EndHover(this);
    }
}
