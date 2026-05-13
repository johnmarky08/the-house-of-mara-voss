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



		// Try to find door_animation
		_doorAnimation = GetNodeOrNull<AnimatedSprite2D>("Door");
		if (_doorAnimation == null)
			GD.PrintErr("[Room4] Door animation not found in scene!");
		else
			_doorAnimation.Visible = false;  // Hide initially

		_doorClickArea = GetNodeOrNull<Area2D>(DoorClickAreaPath);
		if (_doorClickArea == null)
			GD.PrintErr($"[Room4] Door click area not found at path: {DoorClickAreaPath}");

		// Create label for displaying the sentence as pictures are hovered
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
				ZIndex = 10000
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

	private void EnsureRuntimeHoverLabel()
	{
		GD.Print("[Room4] EnsureRuntimeHoverLabel called");

		var layer = GetNodeOrNull<CanvasLayer>("HoverLabelLayer");
		if (layer == null)
		{
			GD.Print("[Room4] Creating new CanvasLayer");
			layer = new CanvasLayer
			{
				Name = "HoverLabelLayer"
			};
			AddChild(layer);
			GD.Print($"[Room4] CanvasLayer added, children count: {GetChildCount()}");
		}
		else
		{
			GD.Print("[Room4] Found existing HoverLabelLayer");
		}

		var font = ResourceLoader.Load<FontFile>(HoverFontPath);
		GD.Print($"[Room4] Font loaded: {(font != null ? "YES" : "NO")}");

		_runtimeHoverLabel = layer.GetNodeOrNull<Label>("RuntimeHoverLabel");
		if (_runtimeHoverLabel == null)
		{
			GD.Print("[Room4] Creating new Label node");
			_runtimeHoverLabel = new Label
			{
				Name = "RuntimeHoverLabel",
				Visible = false,
				Text = string.Empty,
				ZIndex = 1000
			};
			layer.AddChild(_runtimeHoverLabel);
			GD.Print($"[Room4] Label added to layer. Layer children: {layer.GetChildCount()}");
		}
		else
		{
			GD.Print("[Room4] Found existing RuntimeHoverLabel");
		}

		if (font != null)
			_runtimeHoverLabel.AddThemeFontOverride("font", font);

		_runtimeHoverLabel.AddThemeFontSizeOverride("font_size", 14);
		_runtimeHoverLabel.AddThemeColorOverride("font_color", Colors.White);
		_runtimeHoverLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_runtimeHoverLabel.AddThemeFontSizeOverride("font_size", 32);

		GD.Print($"[Room4] Label ready. Visible={_runtimeHoverLabel.Visible}, Text='{_runtimeHoverLabel.Text}'");
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
		if (!_doorUnlocked || SceneManager.IsChanging)
			return;

		if (@event is not InputEventMouseButton mouseButton)
			return;

		if (!mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
			return;

		if (_doorAnimation == null || !_doorAnimation.Visible)
			return;

		if (_doorClickArea == null)
			return;

		Vector2 globalMouse = GetGlobalMousePosition();
		if (!IsInsideArea2D(_doorClickArea, globalMouse, 0f))
			return;

		if (string.IsNullOrWhiteSpace(DoorTargetScenePath))
		{
			GD.PrintErr("[Room4] DoorTargetScenePath is empty.");
			return;
		}

		SceneManager.ChangeScene(DoorTargetScenePath);
		GetTree().Root.SetInputAsHandled();
	}

	private int GetHoveredIndex(Vector2 mousePosition)
	{
		for (int index = 0; index < PictureNodePaths.Length; index++)
		{
			var pictureArea = GetNodeOrNull<Area2D>(PictureNodePaths[index]);
			if (pictureArea == null)
			{
				if (DebugHoverDetection)
					GD.Print($"[Room4 Hover] missing Area2D: {PictureNodePaths[index]}");
				continue;
			}

			float padding = HoverPaddingByIndex[index] + HoverPaddingPixels;
			if (IsInsideArea2D(pictureArea, mousePosition, padding))
			{
				if (DebugHoverDetection)
					GD.Print($"[Room4 Hover] hit {PictureNodePaths[index]} at mouse={mousePosition} padding={padding}");
				return index;
			}
		}

		if (DebugHoverDetection)
			GD.Print($"[Room4 Hover] no hit at mouse={mousePosition}");

		return -1;
	}

	private void ShowRuntimeLabel(int index)
	{
		GD.Print($"[Room4] ShowRuntimeLabel called for index {index} ({HoverWords[index]})");

		if (Dialogue.IsInputBlocked)
		{
			GD.Print("[Room4] ShowRuntimeLabel early exit: Dialogue blocked");
			return;
		}

		if (_runtimeHoverLabel == null)
		{
			GD.Print("[Room4] ShowRuntimeLabel early exit: _runtimeHoverLabel is NULL!");
			return;
		}

		var pictureArea = GetNodeOrNull<Area2D>(PictureNodePaths[index]);
		if (pictureArea == null)
		{
			GD.Print($"[Room4] ShowRuntimeLabel early exit: picture area '{PictureNodePaths[index]}' not found");
			return;
		}

		Vector2 labelPosition = pictureArea.GlobalPosition + HoverLabelOffsets[index] + HoverLabelNudge;
		GD.Print($"[Room4] Setting label: text='{HoverWords[index]}' pos={labelPosition}");

		_runtimeHoverLabel.Text = HoverWords[index];
		_runtimeHoverLabel.Position = labelPosition;
		_runtimeHoverLabel.Visible = true;
		_runtimeHoverLabel.Modulate = new Color(1, 1, 1, 0);

		GD.Print($"[Room4] Label after set: Visible={_runtimeHoverLabel.Visible}, Modulate.A={_runtimeHoverLabel.Modulate.A}, Parent={_runtimeHoverLabel.GetParent()?.Name}");

		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_runtimeHoverLabel, "modulate:a", 1f, 0.22f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);

		GD.Print("[Room4] Fade tween started");

		// Check if this is the next word in the sequence
		if (HoverWords[index] == HoverWords[_currentSequenceIndex])
		{
			GD.Print($"HOVERED: {HoverWords[index]}");

			// Add this word to the sentence display
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

			// Check if all words have been hovered
			if (_currentSequenceIndex >= HoverWords.Length)
			{
				GD.Print("✓✓✓ ALL WORDS HOVERED! Triggering door animation!");
				TriggerDoorAnimation();
				_currentSequenceIndex = 0; // Reset for potential replay
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
		{
			GD.Print($"[Room4 DEBUG IsInsideArea2D] Invalid collision on {area.Name}");
			return false;
		}

		Vector2 localMousePosition = collisionShape.ToLocal(globalMousePosition);
		Vector2 halfSize = rectangleShape.Size / 2f;
		Vector2 expandedHalfSize = halfSize + new Vector2(paddingPixels, paddingPixels);
		bool inside = localMousePosition.X >= -expandedHalfSize.X &&
			localMousePosition.X <= expandedHalfSize.X &&
			localMousePosition.Y >= -expandedHalfSize.Y &&
			localMousePosition.Y <= expandedHalfSize.Y;

		GD.Print($"[Room4 DEBUG IsInsideArea2D] {area.Name} - Local: {localMousePosition}, HalfSize: {halfSize}, Expanded: {expandedHalfSize}, Inside: {inside}");

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
		GD.Print("[Room4] Words shuffled. New order:");
		for (int i = 0; i < array.Length; i++)
		{
			GD.Print($"  Pic {i + 1}: {array[i]}");
		}
	}

	private void TriggerDoorAnimation()
	{
		if (_doorAnimation == null)
		{
			GD.PrintErr("[Room4] Cannot trigger door animation: node not found");
			return;
		}

		GD.Print("[Room4] Triggering door animation...");
		_doorAnimation.Visible = true;  // Show door
		_doorAnimationStarted = true;

		// Try to play the animation
		var animations = _doorAnimation.SpriteFrames.GetAnimationNames();
		if (animations.Length > 0)
		{
			_doorAnimation.Play(animations[0]);
			GD.Print($"[Room4] Playing animation: {animations[0]}");
		}
		else
		{
			GD.Print("[Room4] No animations found in door sprite frames");
		}

		_doorAnimation.Frame = 0;

		// Stop at frame 5 after animation finishes one full cycle
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
			GD.Print($"[Room4] door_animation stopped at frame {_doorAnimation.Frame + 1}");
		})).SetDelay(5.0 / animSpeed);
	}
}
