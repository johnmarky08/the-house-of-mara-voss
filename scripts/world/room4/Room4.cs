using Godot;

public partial class Room4 : Node2D
{
	[ExportCategory("Door Transition")]
	[Export(PropertyHint.File, "*.tscn")] public string DoorTargetScenePath = "res://scenes/world/Room4Puzzle.tscn";
	[Export] public NodePath DoorClickAreaPath = "Door/Area2D";

	[Export] public bool DebugHoverDetection = true;
	[Export] public float HoverPaddingPixels = 14f;
	[Export] public Vector2 HoverLabelNudge = new Vector2(3f, -14f);
	private const string HoverFontPath = "res://assets/fonts/CormorantGaramond-VariableFont_wght.ttf";

	private static readonly string[] HoverWords =
	[
		"She",
		"would",
		"have",
		"loved",
		"to",
		"see",
		"how",
		"far",
		"you've",
		"come."
	];

	private static readonly string[] PictureNodePaths =
	[
		"Pic 1",
		"Pic 2",
		"Pic 3",
		"Pic 4",
		"Pic 5",
		"Pic 6",
		"Pic 7",
		"Pic 8",
		"Pic 9",
		"Pic 10"
	];

	private static readonly Vector2[] HoverLabelOffsets =
	[
		new Vector2(2f, 83f),
		new Vector2(97f, 98f),
		new Vector2(10f, -17f),
		new Vector2(75f, 1f),
		new Vector2(125f, 18f),
		new Vector2(171f, 29f),
		new Vector2(-159f, 102f),
		new Vector2(-64f, 104f),
		new Vector2(11f, 109f),
		new Vector2(75f, 112f)
	];

	private static readonly float[] HoverPaddingByIndex =
	[
		44f,
		40f,
		14f,
		14f,
		14f,
		14f,
		14f,
		14f,
		14f,
		14f
	];

	private int _currentHoveredIndex = -1;
	private Label _runtimeHoverLabel;
	private Tween _hoverTween;
	private int _currentSequenceIndex = 0;
	private AnimatedSprite2D _doorAnimation;
	private Area2D _doorClickArea;
	private Area2D _mirrorCollisionArea;
	private Area2D _drawerCollisionArea;
	private bool _doorAnimationStarted = false;
	private bool _doorUnlocked = false;
	private Label _sentenceLabel;
	private CanvasLayer _sentenceLayer;
	private static readonly Vector2 SentencePosition = new Vector2(400, 80);

	public override void _Ready()
	{
		EnsureRuntimeHoverLabel();
		SetProcess(true);
		HideRuntimeLabel(true);

		_doorAnimation = GetNodeOrNull<AnimatedSprite2D>("Door");
		if (_doorAnimation != null)
			_doorAnimation.Visible = false;

		_mirrorCollisionArea = GetNodeOrNull<Area2D>("Mirror/Mirror");
		if (_mirrorCollisionArea != null)
			_mirrorCollisionArea.InputPickable = true;

		_drawerCollisionArea = GetNodeOrNull<Area2D>("Drawer/Drawer");
		if (_drawerCollisionArea != null)
			_drawerCollisionArea.InputPickable = true;

		_doorClickArea = GetNodeOrNull<Area2D>(DoorClickAreaPath);
		if (_doorClickArea == null)
			Logger.Error("[ Room4 ] Door click area not found at path: " + DoorClickAreaPath);

		_sentenceLayer = GetNodeOrNull<CanvasLayer>("SentenceLayer");
		if (_sentenceLayer == null)
		{
			_sentenceLayer = new CanvasLayer
			{
				Name = "SentenceLayer"
			};
			AddChild(_sentenceLayer);
		}

		_sentenceLabel = GetNodeOrNull<Label>("SentenceLabel");
		if (_sentenceLabel == null)
		{
			_sentenceLabel = new Label
			{
				Name = "SentenceLabel",
				Position = SentencePosition,
				Text = string.Empty,
				Visible = true,
				CustomMinimumSize = new Vector2(800, 160),
				ZIndex = 4096  // Fixed: was 10000, Godot max is 4096
			};
			_sentenceLayer.AddChild(_sentenceLabel);
		}

		if (_sentenceLabel != null)
		{
			_sentenceLabel.AddThemeColorOverride("font_color", Colors.White);
			var font = ResourceLoader.Load<FontFile>(HoverFontPath);
			if (font != null)
				_sentenceLabel.AddThemeFontOverride("font", font);
			_sentenceLabel.AddThemeFontSizeOverride("font_size", 40);
			_sentenceLabel.HorizontalAlignment = HorizontalAlignment.Left;
			_sentenceLabel.VerticalAlignment = VerticalAlignment.Top;
			_sentenceLabel.Position = SentencePosition;
			_sentenceLabel.Visible = true;
			_sentenceLabel.Modulate = Colors.White;
			_sentenceLabel.AutowrapMode = TextServer.AutowrapMode.Word;
			_sentenceLabel.Size = new Vector2(800, 160);
		}
	}

	private async void OnMirrorCollisionPressed()
	{
		var globalPosition = _mirrorCollisionArea.GlobalPosition;
		await Dialogue.ShowText(this, "The hallway stretches back in the mirror. All those photographs, reversed. Ida at fourteen, mirrored, looking in the other direction.", 6.0f, globalPosition.X, globalPosition.Y, HoverFontPath, 20);
		await Dialogue.ShowText(this, "The mirror is showing something different. A different version of this hallway. One where more has been taken down.", 6.0f, globalPosition.X, globalPosition.Y, HoverFontPath, 20);
		await Dialogue.ShowText(this, "That's not this hallway. That's somewhere else.", 4.0f, globalPosition.X, globalPosition.Y, HoverFontPath, 32);
		await Dialogue.ShowText(this, "There is a woman in the mirror. She is sitting on a floor. She is not here.", 5.0f, globalPosition.X, globalPosition.Y, HoverFontPath, 28);
	}

	private async void OnDrawerCollisionPressed()
	{
		var globalPosition = _drawerCollisionArea.GlobalPosition;
		await Dialogue.ShowText(this, "A hall table drawer. Old wood, slightly sticky.", 4.0f, globalPosition.X, globalPosition.Y, HoverFontPath, 32);
		await Dialogue.ShowText(this, "These were here. In the truth of the house, these tools were always here. Someone who lived here was a locksmith, or careful, or both.", 4.0f, globalPosition.X, globalPosition.Y, HoverFontPath, 32);
	}

	private bool TryHandleMirrorCollision(InputEventMouseButton mouseButton)
	{
		if (Dialogue.IsInputBlocked || _mirrorCollisionArea == null)
			return false;

		Vector2 globalMouse = GetGlobalMousePosition();
		if (!IsInsideArea2D(_mirrorCollisionArea, globalMouse, 0f))
			return false;

		OnMirrorCollisionPressed();
		GetTree().Root.SetInputAsHandled();
		return true;
	}

	private bool TryHandleDrawerCollision(InputEventMouseButton mouseButton)
	{
		if (Dialogue.IsInputBlocked || _drawerCollisionArea == null)
			return false;

		Vector2 globalMouse = GetGlobalMousePosition();
		if (!IsInsideArea2D(_drawerCollisionArea, globalMouse, 0f))
			return false;

		OnDrawerCollisionPressed();
		GetTree().Root.SetInputAsHandled();
		return true;
	}

	private void EnsureRuntimeHoverLabel()
	{
		var layer = GetNodeOrNull<CanvasLayer>("HoverLabelLayer");
		if (layer == null)
		{
			layer = new CanvasLayer
			{
				Name = "HoverLabelLayer"
			};
			AddChild(layer);
		}

		var font = ResourceLoader.Load<FontFile>(HoverFontPath);

		_runtimeHoverLabel = layer.GetNodeOrNull<Label>("RuntimeHoverLabel");
		if (_runtimeHoverLabel == null)
		{
			_runtimeHoverLabel = new Label
			{
				Name = "RuntimeHoverLabel",
				Visible = false,
				Text = string.Empty,
				ZIndex = 1000
			};
			layer.AddChild(_runtimeHoverLabel);
		}

		if (font != null)
			_runtimeHoverLabel.AddThemeFontOverride("font", font);

		_runtimeHoverLabel.AddThemeFontSizeOverride("font_size", 14);
		_runtimeHoverLabel.AddThemeColorOverride("font_color", Colors.White);
		_runtimeHoverLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_runtimeHoverLabel.AddThemeFontSizeOverride("font_size", 32);

	}

	public override void _Process(double delta)
	{
		if (Dialogue.IsInputBlocked)
		{
			if (_currentHoveredIndex != -1)
			{
				HideRuntimeLabel();
				_currentHoveredIndex = -1;
			}
			return;
		}

		Vector2 mousePosition = GetGlobalMousePosition();
		int hoveredIndex = GetHoveredIndex(mousePosition);

		if (hoveredIndex == _currentHoveredIndex)
			return;

		if (_currentHoveredIndex != -1)
			HideRuntimeLabel();

		_currentHoveredIndex = hoveredIndex;

		if (_currentHoveredIndex != -1)
			ShowRuntimeLabel(_currentHoveredIndex);
	}

	public override void _Input(InputEvent @event)
	{
		if (SceneManager.IsChanging)
			return;

		if (@event is not InputEventMouseButton mouseButton)
			return;

		if (!mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
			return;

		if (TryHandleMirrorCollision(mouseButton))
			return;

		if (TryHandleDrawerCollision(mouseButton))
			return;

		if (!_doorUnlocked)
			return;

		if (_doorAnimation == null || !_doorAnimation.Visible)
			return;

		if (_doorClickArea == null)
			return;

		Vector2 globalMouse = GetGlobalMousePosition();
		if (!IsInsideArea2D(_doorClickArea, globalMouse, 0f))
			return;

		if (string.IsNullOrWhiteSpace(DoorTargetScenePath))
			return;


		SceneManager.ChangeScene(DoorTargetScenePath);
		GetTree().Root.SetInputAsHandled();
	}

	private int GetHoveredIndex(Vector2 mousePosition)
	{
		for (int index = 0; index < PictureNodePaths.Length; index++)
		{
			var pictureArea = GetNodeOrNull<Area2D>(PictureNodePaths[index]);
			if (pictureArea == null)
				continue;


			float padding = HoverPaddingByIndex[index] + HoverPaddingPixels;
			if (IsInsideArea2D(pictureArea, mousePosition, padding))
				return index;
		}

		return -1;
	}

	private void ShowRuntimeLabel(int index)
	{
		if (Dialogue.IsInputBlocked)
			return;

		if (_runtimeHoverLabel == null)
			return;

		var pictureArea = GetNodeOrNull<Area2D>(PictureNodePaths[index]);
		if (pictureArea == null)
			return;

		Vector2 labelPosition = pictureArea.GlobalPosition + HoverLabelOffsets[index] + HoverLabelNudge;

		_runtimeHoverLabel.Text = HoverWords[index];
		_runtimeHoverLabel.Position = labelPosition;
		_runtimeHoverLabel.Visible = true;
		_runtimeHoverLabel.Modulate = new Color(1, 1, 1, 0);

		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_runtimeHoverLabel, "modulate:a", 1f, 0.22f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);

		if (HoverWords[index] == HoverWords[_currentSequenceIndex])
		{
			if (_sentenceLabel != null)
			{
				if (_currentSequenceIndex == 0)
					_sentenceLabel.Text = HoverWords[index];
				else
					_sentenceLabel.Text = $"{_sentenceLabel.Text} {HoverWords[index]}";

				_sentenceLabel.Visible = true;
				_sentenceLabel.Modulate = Colors.White;
			}

			_currentSequenceIndex++;

			if (_currentSequenceIndex >= HoverWords.Length)
			{
				TriggerDoorAnimation();
				_currentSequenceIndex = 0;
				if (_sentenceLabel != null)
					_sentenceLabel.Text = string.Empty;
			}
		}
	}

	private void HideRuntimeLabel(bool instant = false)
	{
		if (_runtimeHoverLabel == null)
			return;

		_hoverTween?.Kill();

		if (instant)
		{
			_runtimeHoverLabel.Modulate = new Color(1, 1, 1, 0);
			_runtimeHoverLabel.Visible = false;
			return;
		}

		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_runtimeHoverLabel, "modulate:a", 0f, 0.18f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.In);
		_hoverTween.TweenCallback(Callable.From(() => _runtimeHoverLabel.Visible = false));
	}

	private static bool IsInsideArea2D(Area2D area, Vector2 globalMousePosition, float paddingPixels)
	{
		var collisionShape = area.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (collisionShape == null || collisionShape.Shape is not RectangleShape2D rectangleShape)
			return false;

		Vector2 localMousePosition = collisionShape.ToLocal(globalMousePosition);
		Vector2 halfSize = rectangleShape.Size / 2f;
		Vector2 expandedHalfSize = halfSize + new Vector2(paddingPixels, paddingPixels);
		bool inside = localMousePosition.X >= -expandedHalfSize.X &&
			localMousePosition.X <= expandedHalfSize.X &&
			localMousePosition.Y >= -expandedHalfSize.Y &&
			localMousePosition.Y <= expandedHalfSize.Y;

		return inside;
	}

	private void ShuffleArray(string[] array)
	{
		var random = new RandomNumberGenerator();
		for (int i = array.Length - 1; i > 0; i--)
		{
			int randomIndex = (int)(random.Randi() % (i + 1));
			(array[i], array[randomIndex]) = (array[randomIndex], array[i]);
		}
	}

	private void TriggerDoorAnimation()
	{
		if (_doorAnimation == null)
			return;

		_doorAnimation.Visible = true;
		_doorAnimationStarted = true;

		var animations = _doorAnimation.SpriteFrames.GetAnimationNames();
		if (animations.Length > 0)
			_doorAnimation.Play(animations[0]);

		_doorAnimation.Frame = 0;

		var speedVar = _doorAnimation.SpriteFrames.GetAnimationSpeed(animations.Length > 0 ? animations[0] : "default");
		double animSpeed = speedVar is double speed ? speed : 5.0;
		var tween = CreateTween();
		tween.TweenCallback(Callable.From(() =>
		{
			_doorAnimation.Stop();
			int frameCount = 0;
			if (animations.Length > 0)
				frameCount = _doorAnimation.SpriteFrames.GetFrameCount(animations[0]);

			_doorAnimation.Frame = Mathf.Max(0, frameCount - 1);
			_doorUnlocked = true;
		})).SetDelay(5.0 / animSpeed);
	}
}