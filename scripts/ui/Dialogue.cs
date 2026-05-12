using Godot;
using System;
using System.Threading.Tasks;

public partial class Dialogue : Control
{
    private static int _inputBlockCount = 0;

    public TextureRect _backgroundRect;
    public Label _dialogueLabel;
    private ColorRect _inputBlocker;
    private string _fullText;
    private float _typewriterSpeed = 0.01f;
    private Color _textColor = Colors.Black;
    private string _fontPath = "res://assets/fonts/CormorantGaramond-VariableFont_wght.ttf";
    private int _fontSize = 32;

    public static bool IsInputBlocked => _inputBlockCount > 0;

    public static void BeginInputBlock()
    {
        _inputBlockCount++;
    }

    public static void EndInputBlock()
    {
        if (_inputBlockCount > 0)
            _inputBlockCount--;
    }

    public void Initialize()
    {
        Name = "DialogueUI";
        ProcessMode = ProcessModeEnum.Always;
        ZIndex = 100;
        Visible = true;
        AnchorLeft = 0;
        AnchorTop = 0;
        AnchorRight = 1;
        AnchorBottom = 1;
        OffsetLeft = 0;
        OffsetTop = 0;
        OffsetRight = 0;
        OffsetBottom = 0;
        GrowHorizontal = GrowDirection.Both;
        GrowVertical = GrowDirection.Both;

        _inputBlocker = new ColorRect
        {
            Name = "InputBlocker",
            Color = new Color(0, 0, 0, 0),
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 0,
            OffsetTop = 0,
            OffsetRight = 0,
            OffsetBottom = 0,
            MouseFilter = MouseFilterEnum.Stop,
            ProcessMode = ProcessModeEnum.Always,
        };
        AddChild(_inputBlocker);

        _backgroundRect = new TextureRect
        {
            Name = "Background",
            Texture = GD.Load<Texture2D>("res://assets/images/core/bubble_chat.png"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            AnchorLeft = 0,
            AnchorTop = 1,
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 50,
            OffsetTop = -223,
            OffsetRight = -45,
            OffsetBottom = -123,
            Modulate = new Color(1, 1, 1, 0),
            ProcessMode = ProcessModeEnum.Always,
        };
        AddChild(_backgroundRect);

        _dialogueLabel = new Label
        {
            Name = "DialogueText",
            AnchorLeft = 0,
            AnchorTop = 1,
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 50,
            OffsetTop = -233,
            OffsetRight = -27,
            OffsetBottom = -133,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.Word,
            Modulate = new Color(1, 1, 1, 0),
            ProcessMode = ProcessModeEnum.Always,
        };
        AddChild(_dialogueLabel);

        var font = GD.Load<FontFile>(_fontPath);
        var theme = new Theme();
        if (font != null)
            theme.SetFont("font", "Label", font);
        theme.SetFontSize("font_size", "Label", _fontSize);
        theme.SetColor("font_color", "Label", _textColor);
        _dialogueLabel.Theme = theme;
    }

    public override void _Ready() { }

    public static async Task ShowText(
        Node caller,
        string text,
        float durationInSeconds,
        float x,
        float y,
        string fontPath = null,
        int? fontSize = null,
        Color? textColor = null,
        float? typewriterSpeed = null)
    {
        var tree = caller.GetTree();
        var root = tree.Root;

        var container = new SubViewportContainer
        {
            Name = "DialogueContainer",
            Stretch = true,
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 0,
            OffsetTop = 0,
            OffsetRight = 0,
            OffsetBottom = 0,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
        };

        var subViewport = new SubViewport
        {
            Name = "DialogueViewport",
            Size = new Vector2I(1152, 648),
            TransparentBg = true,
        };
        subViewport.SetSize2DOverride(new Vector2I(1152, 648));
        subViewport.SetSize2DOverrideStretch(true);
        container.AddChild(subViewport);

        var dialogue = new Dialogue
        {
            _fullText = text
        };

        if (fontPath != null) dialogue._fontPath = fontPath;
        if (fontSize.HasValue) dialogue._fontSize = fontSize.Value;
        if (textColor.HasValue) dialogue._textColor = textColor.Value;
        if (typewriterSpeed.HasValue) dialogue._typewriterSpeed = typewriterSpeed.Value;

        dialogue.Initialize();
        subViewport.AddChild(dialogue);
        root.AddChild(container);

        await dialogue.ToSignal(tree, SceneTree.SignalName.ProcessFrame);

        try
        {
            await dialogue.PlayFadeInAnimation();

            await dialogue.PlayTypewriterAnimation();

            await dialogue.ToSignal(
                dialogue.GetTree().CreateTimer(durationInSeconds),
                SceneTreeTimer.SignalName.Timeout);

            await dialogue.PlayFadeOutAnimation();
        }
        catch (Exception e)
        {
            Logger.Error("[ Dialogue.ShowText ] Exception: " + e.Message);
            throw;
        }
        finally
        {
            container.QueueFree();
        }
    }

    private async Task PlayFadeInAnimation()
    {
        var tween = CreateTween();
        tween.TweenProperty(_backgroundRect, "modulate:a", 1.0f, 0.4f);
        tween.TweenProperty(_dialogueLabel, "modulate:a", 1.0f, 0.4f);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    private async Task PlayTypewriterAnimation()
    {
        _dialogueLabel.Text = "";
        _dialogueLabel.Visible = true;
        _dialogueLabel.SelfModulate = new Color(1, 1, 1, 1);
        int i = 0;
        foreach (char c in _fullText)
        {
            _dialogueLabel.Text += c;
            i++;
            await ToSignal(
                GetTree().CreateTimer(_typewriterSpeed),
                SceneTreeTimer.SignalName.Timeout);
        }
    }

    private async Task PlayFadeOutAnimation()
    {
        var tween = CreateTween();
        tween.TweenProperty(_backgroundRect, "modulate:a", 0.0f, 0.3f);
        tween.TweenProperty(_dialogueLabel, "modulate:a", 0.0f, 0.3f);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}