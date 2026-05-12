using Godot;

public partial class MainMenu : Control
{
	[Export(PropertyHint.File, "*.tscn")]
	public string StartScenePath { get; set; } = "res://scenes/managers/game_manager.tscn";

	[Export]
	public NodePath StartButtonPath { get; set; } = "Center/MarginContainer/VBox/StartButton";

	[Export]
	public NodePath QuitButtonPath { get; set; } = "Center/MarginContainer/VBox/QuitButton";

	private BaseButton _startButton;
	private BaseButton _quitButton;

	public override void _Ready()
	{
		_startButton = ResolveButton(StartButtonPath, "StartButton",
			"Center/MarginContainer/VBox/StartButton",
			"Center/VBox/StartButton");
		_quitButton = ResolveButton(QuitButtonPath, "QuitButton",
			"Center/MarginContainer/VBox/QuitButton",
			"Center/VBox/QuitButton");

		if (_startButton == null)
			Logger.Error("MainMenu could not find StartButton at Center/VBox/StartButton.");
		else
			_startButton.Pressed += OnStartPressed;

		if (_quitButton == null)
			Logger.Error("MainMenu could not find QuitButton at Center/VBox/QuitButton.");
		else
			_quitButton.Pressed += OnQuitPressed;

		_startButton?.GrabFocus();
	}

	private BaseButton ResolveButton(NodePath configuredPath, string logicalName, params string[] fallbackPaths)
	{
		BaseButton button = null;

		if (configuredPath != null && !configuredPath.IsEmpty)
			button = GetNodeOrNull<BaseButton>(configuredPath);

		if (button != null)
			return button;

		foreach (var fallback in fallbackPaths)
		{
			button = GetNodeOrNull<BaseButton>(fallback);
			if (button != null)
				return button;
		}

		Logger.Error("MainMenu could not find ", logicalName, ". Tried: ", configuredPath, " and fallbacks.");
		return null;
	}

	private void OnStartPressed()
	{
		if (string.IsNullOrWhiteSpace(StartScenePath))
		{
			Logger.Error("MainMenu StartScenePath is empty.");
			return;
		}

		Logger.Info("Starting game; changing scene to: ", StartScenePath);
		GetTree().ChangeSceneToFile(StartScenePath);
	}

	private void OnQuitPressed()
	{
		Logger.Info("Quitting game from main menu.");
		GetTree().Quit();
	}
}
