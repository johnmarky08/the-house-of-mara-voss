using Godot;
using System.Threading.Tasks;

public partial class MainMenu : Control
{
	[Export(PropertyHint.File, "*.tscn")]
	public string StartScenePath { get; set; } = "res://scenes/managers/game_manager.tscn";

	[Export]
	public NodePath StartButtonPath { get; set; } = "Center/MarginContainer/VBox/StartButton";

	[Export]
	public NodePath FullStoryButtonPath { get; set; } = "Center/MarginContainer/VBox/FullStoryButton";

	[Export]
	public NodePath AboutButtonPath { get; set; } = "Center/MarginContainer/VBox/AboutButton";

	[Export]
	public NodePath QuitButtonPath { get; set; } = "Center/MarginContainer/VBox/QuitButton";

	[Export]
	public NodePath FullStoryModalPath { get; set; } = "SubViewportContainer/SubViewport/FullStoryModal";

	[Export]
	public NodePath AboutModalPath { get; set; } = "SubViewportContainer/SubViewport/AboutModal";

	[ExportGroup("Full Story")]
	[Export]
	public string FullStoryTitle { get; set; } = "Full Story";

	[Export(PropertyHint.MultilineText)]
	public string FullStoryBody { get; set; } = "";

	[Export]
	public Texture2D FullStoryArt { get; set; } = null;

	[ExportGroup("About")]
	[Export]
	public string AboutTitle { get; set; } = "About";

	[Export(PropertyHint.MultilineText)]
	public string AboutBody { get; set; } = "";

	[Export]
	public Texture2D AboutArt { get; set; } = null;

	private BaseButton _startButton;
	private BaseButton _fullStoryButton;
	private BaseButton _aboutButton;
	private BaseButton _quitButton;
	private InfoModal _fullStoryModal;
	private InfoModal _aboutModal;
	private AudioStreamPlayer _musicPlayer;

	public override void _Ready()
	{
		_startButton = ResolveButton(StartButtonPath, "StartButton",
			"Center/MarginContainer/VBox/StartButton",
			"Center/VBox/StartButton");
		_fullStoryButton = ResolveButton(FullStoryButtonPath, "FullStoryButton",
			"Center/MarginContainer/VBox/FullStoryButton",
			"Center/VBox/FullStoryButton");
		_aboutButton = ResolveButton(AboutButtonPath, "AboutButton",
			"Center/MarginContainer/VBox/AboutButton",
			"Center/VBox/AboutButton");
		_quitButton = ResolveButton(QuitButtonPath, "QuitButton",
			"Center/MarginContainer/VBox/QuitButton",
			"Center/VBox/QuitButton");

		_fullStoryModal = GetNodeOrNull<InfoModal>(FullStoryModalPath);
		if (_fullStoryModal == null)
			Logger.Error("MainMenu could not find FullStoryModal at: ", FullStoryModalPath);

		_aboutModal = GetNodeOrNull<InfoModal>(AboutModalPath);
		if (_aboutModal == null)
			Logger.Error("MainMenu could not find AboutModal at: ", AboutModalPath);

		if (_startButton == null)
			Logger.Error("MainMenu could not find StartButton at Center/VBox/StartButton.");
		else
			_startButton.Pressed += OnStartPressed;

		if (_fullStoryButton == null)
			Logger.Error("MainMenu could not find FullStoryButton at Center/VBox/FullStoryButton.");
		else
			_fullStoryButton.Pressed += OnFullStoryPressed;

		if (_aboutButton == null)
			Logger.Error("MainMenu could not find AboutButton at Center/VBox/AboutButton.");
		else
			_aboutButton.Pressed += OnAboutPressed;

		if (_quitButton == null)
			Logger.Error("MainMenu could not find QuitButton at Center/VBox/QuitButton.");
		else
			_quitButton.Pressed += OnQuitPressed;

		_startButton?.GrabFocus();

		// Initialize music player
		_musicPlayer = GetNodeOrNull<AudioStreamPlayer>("MusicPlayer");
		if (_musicPlayer == null)
		{
			_musicPlayer = new AudioStreamPlayer();
			AddChild(_musicPlayer);
		}

		// Load and play the menu loop music
		var menuMusic = GD.Load<AudioStream>("res://assets/Music/bg/Glance Out A Casement Window.mp3");
		if (menuMusic != null)
		{
			_musicPlayer.Stream = menuMusic;
			_musicPlayer.Play();
		}
	}

	private void OnFullStoryPressed()
	{
		if (_fullStoryModal == null)
			return;

		_fullStoryModal.Open(FullStoryTitle, FullStoryBody, FullStoryArt);
	}

	private void OnAboutPressed()
	{
		if (_aboutModal == null)
			return;

		_aboutModal.Open(AboutTitle, AboutBody, AboutArt);
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

	private async void OnStartPressed()
	{
		if (string.IsNullOrWhiteSpace(StartScenePath))
		{
			Logger.Error("MainMenu StartScenePath is empty.");
			return;
		}

		Logger.Info("Starting game; changing scene to: ", StartScenePath);

		// Fade out music before transitioning
		await FadeMusicAlpha(0.0f, 0.5f);

		GetTree().ChangeSceneToFile(StartScenePath);
	}

	private void OnQuitPressed()
	{
		Logger.Info("Quitting game from main menu.");
		GetTree().Quit();
	}

	private async Task FadeMusicAlpha(float targetAlpha, float duration)
	{
		if (_musicPlayer == null)
			return;

		var tween = CreateTween();
		tween.TweenProperty(_musicPlayer, "volume_db", Mathf.LinearToDb(targetAlpha), duration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}
}
