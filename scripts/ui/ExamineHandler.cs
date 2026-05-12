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
        if (_isHovering)
        {
            CursorHelper.EndHover(this);
            _isHovering = false;
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

        bool hasTile = GetCellSourceId(cell) != -1;

        if (hasTile)
        {
            ExamineHelper.CycleExamine(this);
            RoomExamineTracker.OnExamineClicked(this);
            OnExamineClicked();

            var dialogueSegments = GetDialogueSegments();
            var dialogueDurations = GetDialogueDurations();
            var globalPos = GetGlobalPosition();

            _ = ShowDialogueSequenceAsync(dialogueSegments, dialogueDurations, globalPos);
            OnAnyExamineClicked();
            GetTree().Root.SetInputAsHandled();
        }
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
            // Split on pipe first, then on newlines to handle both separators
            var pipeSplit = DialogueDurationText.Split('|');
            foreach (var part in pipeSplit)
            {
                // Further split by newlines
                var lineSplit = part.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (var line in lineSplit)
                {
                    var trimmedDuration = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedDuration))
                        continue;

                    if (float.TryParse(trimmedDuration, NumberStyles.Float, CultureInfo.InvariantCulture, out var durationValue) ||
                        float.TryParse(trimmedDuration, out durationValue))
                    {
                        durations.Add(durationValue);
                    }
                }
            }
        }

        if (durations.Count == 0)
            durations.Add(DialogueDuration);

        return durations;
    }

    private async Task ShowDialogueSequenceAsync(List<string> dialogueSegments, List<float> dialogueDurations, Vector2 globalPos)
    {
        Dialogue.BeginInputBlock();

        try
        {
            for (int index = 0; index < dialogueSegments.Count; index++)
            {
                var dialogueText = dialogueSegments[index];
                if (string.IsNullOrWhiteSpace(dialogueText))
                    continue;

                float duration = dialogueDurations[Math.Min(index, dialogueDurations.Count - 1)];
                await Dialogue.ShowText(this, dialogueText, duration, globalPos.X, globalPos.Y, fontSize: DialogueFontSize);
            }
        }
        finally
        {
            Dialogue.EndInputBlock();
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
        if (Dialogue.IsInputBlocked)
            return;

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
