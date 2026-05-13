using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

public partial class ExamineHandler : TileMapLayer
{
    [Export(PropertyHint.MultilineText)] public string ExamineText = "";
    [Export] public float DialogueDuration = 3f;
    [Export(PropertyHint.MultilineText)] public string DialogueDurationText = "";
    [Export] public int DialogueFontSize = 32;
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
        // Always clean up cursor on removal, regardless of any state flags.
        if (_isHovering)
        {
            CursorHelper.EndHover(this);
            _isHovering = false;
            _currentHoverCursorTexturePath = string.Empty;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (Dialogue.IsInputBlocked)
            return;

        if (!(@event is InputEventMouseButton mouseEvent) || !mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
            return;

        Vector2 globalMouse = GetGlobalMousePosition();
        Vector2 localMouse = ToLocal(globalMouse);
        Vector2I cell = LocalToMap(localMouse);

        if (GetCellSourceId(cell) == -1)
            return;

        // If clarifying, delegate entirely to clarify logic and return.
        if (Globals.Instance.IS_CLARIFYING)
        {
            ExamineHelper.TriggerClarify(this);
            GetTree().Root.SetInputAsHandled();
            return;
        }

        ExamineHelper.CycleExamine(this);
        RoomExamineTracker.OnExamineClicked(this);
        OnExamineClicked();
        OnAnyExamineClicked();

        var dialogueSegments = GetDialogueSegments();
        var dialogueDurations = GetDialogueDurations();
        var globalPos = GetGlobalPosition();
        _ = ShowDialogueSequenceAsync(dialogueSegments, dialogueDurations, globalPos);

        GetTree().Root.SetInputAsHandled();
    }

    private List<string> GetDialogueSegments()
    {
        var segments = new List<string>();

        if (string.IsNullOrWhiteSpace(ExamineText))
            return segments;

        var pipeSplit = ExamineText.Split('|');
        foreach (var part in pipeSplit)
        {
            var lineSplit = part.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lineSplit)
            {
                var segment = line.Trim();
                if (!string.IsNullOrWhiteSpace(segment))
                    segments.Add(segment);
            }
        }

        return segments;
    }

    private List<float> GetDialogueDurations()
    {
        var durations = new List<float>();

        if (!string.IsNullOrWhiteSpace(DialogueDurationText))
        {
            var pipeSplit = DialogueDurationText.Split('|');
            foreach (var part in pipeSplit)
            {
                var lineSplit = part.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (var line in lineSplit)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed))
                        continue;

                    if (float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var val) ||
                        float.TryParse(trimmed, out val))
                    {
                        durations.Add(val);
                    }
                }
            }
        }

        if (durations.Count == 0)
            durations.Add(DialogueDuration);

        return durations;
    }

    private async Task ShowDialogueSequenceAsync(List<string> segments, List<float> durations, Vector2 globalPos)
    {
        Dialogue.BeginInputBlock();

        // When input is blocked the cursor must be cleared immediately —
        // _Process will not run its EndHover path while blocked.
        if (_isHovering)
        {
            CursorHelper.EndHover(this);
            _isHovering = false;
            _currentHoverCursorTexturePath = string.Empty;
        }

        try
        {
            for (int i = 0; i < segments.Count; i++)
            {
                var text = segments[i];
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                float duration = durations[Math.Min(i, durations.Count - 1)];
                await Dialogue.ShowText(this, text, duration, globalPos.X, globalPos.Y, fontSize: DialogueFontSize);
            }
        }
        finally
        {
            Dialogue.EndInputBlock();
        }
    }

    private void OnAnyExamineClicked()
    {
        // Only increment corruption when clicking something other than Examine1
        // (the first/default examine layer).
        if (ExamineHelper.ExtractExamineNumber(Name.ToString()) != 1)
            Globals.Instance.CORRUPTION_COUNT++;
    }

    protected virtual void OnExamineClicked()
    {
    }

    public override void _Process(double delta)
    {
        // While input is blocked (dialogue playing), forcibly end any hover so
        // the cursor doesn't stay stuck as a magnifying glass during dialogue.
        if (Dialogue.IsInputBlocked)
        {
            if (_isHovering)
            {
                CursorHelper.EndHover(this);
                _isHovering = false;
                _currentHoverCursorTexturePath = string.Empty;
            }
            return;
        }

        if (!HoverEnabled)
        {
            if (_isHovering)
            {
                CursorHelper.EndHover(this);
                _isHovering = false;
                _currentHoverCursorTexturePath = string.Empty;
            }
            return;
        }

        Vector2 globalMouse = GetGlobalMousePosition();
        Vector2 localMouse = ToLocal(globalMouse);
        Vector2I cell = LocalToMap(localMouse);
        bool hasTile = GetCellSourceId(cell) != -1;

        if (hasTile)
        {
            // Determine which cursor to show.
            string desiredCursor = UnlockedCursorTexturePath;

            var parent = GetParent();
            bool isFinal = parent != null && parent.Name.ToString().EndsWith("Final");
            if (isFinal)
            {
                string roomName = GetRoomNameFromNode(this);
                bool roomReady = RoomExamineTracker.HasRoomBeenFullyExamined(roomName);
                desiredCursor = roomReady ? UnlockedCursorTexturePath : LockedCursorTexturePath;
            }

            // Only call BeginHover when we first enter or when the cursor type changes.
            if (!_isHovering || _currentHoverCursorTexturePath != desiredCursor)
            {
                CursorHelper.BeginHover(this, desiredCursor, CursorWidth, CursorHeight);
                _currentHoverCursorTexturePath = desiredCursor;
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

            if (passedObjects && (current is Node2D || current is CanvasLayer || current is TileMapLayer))
                return currentName;
        }

        return "Room1";
    }
}