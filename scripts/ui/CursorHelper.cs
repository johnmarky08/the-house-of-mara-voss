using Godot;
using System.Collections.Generic;

public static class CursorHelper
{
    private static readonly HashSet<long> ActiveHoverOwners = new();
    private static string activeTexturePath = string.Empty;
    private static int activeWidth;
    private static int activeHeight;

    public static void ApplyCursor(string texturePath, int width = 0, int height = 0)
    {
        if (string.IsNullOrWhiteSpace(texturePath))
            return;

        var texture = ResourceLoader.Load<Texture2D>(texturePath);
        if (texture == null)
            return;

        Texture2D texToUse = texture;

        if (width > 0 && height > 0)
        {
            var imgRes = texture.GetImage();
            var img = imgRes as Image;
            if (img != null)
            {
                var dupRes = img.Duplicate();
                var dup = dupRes as Image;
                if (dup != null)
                {
                    dup.Resize(width, height);
                    var resized = ImageTexture.CreateFromImage(dup);
                    if (resized is Texture2D t)
                        texToUse = t;
                }
            }
        }

        Input.SetCustomMouseCursor(texToUse);
    }

    public static void BeginHover(GodotObject owner, string texturePath, int width = 0, int height = 0)
    {
        if (owner == null)
            return;

        long ownerId = (long)owner.GetInstanceId();
        bool firstOwner = ActiveHoverOwners.Count == 0;

        ActiveHoverOwners.Add(ownerId);

        if (firstOwner || activeTexturePath != texturePath || activeWidth != width || activeHeight != height)
        {
            activeTexturePath = texturePath;
            activeWidth = width;
            activeHeight = height;
            ApplyCursor(texturePath, width, height);
        }
    }

    public static void EndHover(GodotObject owner)
    {
        if (owner == null)
            return;

        long ownerId = (long)owner.GetInstanceId();
        ActiveHoverOwners.Remove(ownerId);

        if (ActiveHoverOwners.Count == 0)
            ResetCursor();
    }

    public static void ResetCursor()
    {
        Input.SetCustomMouseCursor(null);

        ActiveHoverOwners.Clear();
        activeTexturePath = string.Empty;
        activeWidth = 0;
        activeHeight = 0;
    }
}
