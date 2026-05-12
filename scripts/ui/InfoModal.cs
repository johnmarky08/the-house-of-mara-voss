using Godot;

public partial class InfoModal : Control
{
	[Export]
	public NodePath BackdropPath { get; set; } = "Backdrop";

	[Export]
	public NodePath PanelPath { get; set; } = "Panel";

	[Export]
	public NodePath TitleLabelPath { get; set; } = "Panel/Content/Title";

	[Export]
	public NodePath ArtRectPath { get; set; } = "Panel/Content/Art";

	[Export]
	public NodePath BodyLabelPath { get; set; } = "Panel/Content/Scroll/Body";

	[Export]
	public NodePath CloseButtonPath { get; set; } = "Panel/Content/CloseButton";

	private Control _backdrop;
	private Control _panel;
	private Label _title;
	private TextureRect _art;
	private RichTextLabel _body;
	private BaseButton _closeButton;

	public override void _Ready()
	{
		Visible = false;
		ProcessMode = ProcessModeEnum.Always;

		_backdrop = GetNodeOrNull<Control>(BackdropPath);
		_panel = GetNodeOrNull<Control>(PanelPath);
		_title = GetNodeOrNull<Label>(TitleLabelPath);
		_art = GetNodeOrNull<TextureRect>(ArtRectPath);
		_body = GetNodeOrNull<RichTextLabel>(BodyLabelPath);
		_closeButton = GetNodeOrNull<BaseButton>(CloseButtonPath);

		if (_closeButton != null)
			_closeButton.Pressed += Close;

		if (_backdrop != null)
			_backdrop.GuiInput += OnBackdropGuiInput;

		// Make sure the modal can capture escape even when nothing is focused.
		SetProcessUnhandledKeyInput(true);
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (!Visible)
			return;

		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if (keyEvent.Keycode == Key.Escape)
				Close();
		}
	}

	private void OnBackdropGuiInput(InputEvent @event)
	{
		if (!Visible)
			return;

		if (@event is InputEventMouseButton mouse && mouse.Pressed)
			Close();
	}

	public void Open(string title, string body, Texture2D art)
	{
		if (_title != null)
			_title.Text = title ?? string.Empty;

		if (_body != null)
			_body.Text = body ?? string.Empty;

		if (_art != null)
		{
			_art.Texture = art;
			_art.Visible = art != null;
		}

		Visible = true;
		GrabFocus();
		_closeButton?.GrabFocus();
	}

	public void Close()
	{
		Visible = false;
	}
}
