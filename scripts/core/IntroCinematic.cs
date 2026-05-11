using Godot;
using System.Threading.Tasks;

public partial class IntroCinematic : Control
{
	private const float BaseViewportWidth = 1152f;
	private const float BaseViewportHeight = 648f;
	private const int BaseIntroFontSize = 36;
	private const int BaseTitleFontSize = 56;
	private const float BaseIntroMinimumWidth = 900f;
	private const float BaseTitleMinimumWidth = 209f;

	private ColorRect _blackBg;
	private Label _introText;
	private Label _titleCard;
	private ColorRect _whiteFlash;

	public override async void _Ready()
	{
		_blackBg = GetNode<ColorRect>("BlackBG");
		_introText = GetNode<Label>("IntroText");
		_titleCard = GetNode<Label>("TitleCard");
		_whiteFlash = GetNode<ColorRect>("WhiteFlash");

		ConfigureFullscreenRect(_blackBg, new Color(0, 0, 0, 1), 0);
		ConfigureFullscreenRect(_whiteFlash, new Color(1, 1, 1, 1), 2);
		_introText.ZIndex = 1;
		_titleCard.ZIndex = 3;
		_titleCard.AddThemeColorOverride("font_color", new Color(0.05f, 0.05f, 0.05f, 1.0f));
		GetViewport().SizeChanged += ApplyTypographyScale;

		_introText.Modulate = new Color(1, 1, 1, 0);
		_titleCard.Modulate = new Color(1, 1, 1, 0);
		_whiteFlash.Modulate = new Color(1, 1, 1, 0);
		ApplyTypographyScale();

		await Wait(2.0f);

		await ShowLine("Memory is not a recording.", 1.2f, 2.0f, 1.0f);

		await ShowLine("It is a story a mind tells itself about what it cannot bear to look at directly.", 1.6f, 2.0f, 1.0f);

		await ShowLine("You are not Mara Voss.", 1.0f, 1.0f, 0.8f);

		await ShowLine("You are the part of her that still knows something is wrong.", 1.4f, 1.0f, 1.0f);

		await FadeRectAlpha(_whiteFlash, 1.0f, 0.8f);

		await FadeLabelAlpha(_titleCard, 1.0f, 0.8f);
		await Wait(1.5f);

		await FadeLabelAlpha(_titleCard, 0.0f, 0.6f);
		await FadeRectAlpha(_whiteFlash, 0.0f, 0.8f);
		await Wait(0.3f);

		// Start game scene.
		GetTree().ChangeSceneToFile("res://scenes/core/game_manager.tscn");
	}

	public override void _ExitTree()
	{
		if (GetViewport() != null)
		{
			GetViewport().SizeChanged -= ApplyTypographyScale;
		}
	}

	private static void ConfigureFullscreenRect(ColorRect rect, Color color, int zIndex)
	{
		rect.SetAnchorsPreset(LayoutPreset.FullRect);
		rect.OffsetLeft = 0;
		rect.OffsetTop = 0;
		rect.OffsetRight = 0;
		rect.OffsetBottom = 0;
		rect.Color = color;
		rect.ZIndex = zIndex;
	}

	private void ApplyTypographyScale()
	{
		var viewportSize = GetViewportRect().Size;
		if (viewportSize.X <= 0 || viewportSize.Y <= 0)
			return;

		var scaleX = viewportSize.X / BaseViewportWidth;
		var scaleY = viewportSize.Y / BaseViewportHeight;
		var scale = Mathf.Clamp(Mathf.Min(scaleX, scaleY), 0.75f, 1.75f);

		_introText.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(BaseIntroFontSize * scale));
		_titleCard.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(BaseTitleFontSize * scale));
		_introText.CustomMinimumSize = new Vector2(BaseIntroMinimumWidth * scale, _introText.CustomMinimumSize.Y);
		_titleCard.CustomMinimumSize = new Vector2(BaseTitleMinimumWidth * scale, _titleCard.CustomMinimumSize.Y);
	}

	private async Task ShowLine(string text, float fadeInDuration, float holdDuration, float fadeOutDuration)
	{
		_introText.Text = text;
		await FadeLabelAlpha(_introText, 1.0f, fadeInDuration);
		await Wait(holdDuration);
		await FadeLabelAlpha(_introText, 0.0f, fadeOutDuration);
	}

	private async Task FadeLabelAlpha(Label label, float targetAlpha, float duration)
	{
		var tween = CreateTween();
		tween.TweenProperty(label, "modulate:a", targetAlpha, duration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private async Task FadeRectAlpha(ColorRect rect, float targetAlpha, float duration)
	{
		var tween = CreateTween();
		tween.TweenProperty(rect, "modulate:a", targetAlpha, duration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private async Task Wait(float seconds)
	{
		await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
	}
}