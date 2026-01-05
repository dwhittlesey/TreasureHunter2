using Microsoft.Maui.Graphics;
using TreasureHunter.MauiAndroid.Models;

namespace TreasureHunter.MauiAndroid.Graphics;

public class TreasureOverlayDrawable : IDrawable
{
    private readonly List<TreasureOverlay> _treasures = new();
    private readonly Dictionary<string, Microsoft.Maui.Graphics.IImage?> _cachedIcons = new();
    private DateTime _lastUpdateTime = DateTime.Now;

    public void UpdateTreasures(IEnumerable<TreasureOverlay> treasures)
    {
        _treasures.Clear();
        _treasures.AddRange(treasures.Where(t => t.IsVisible));
        _lastUpdateTime = DateTime.Now;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();

        // Add semi-transparent crosshair in center for aiming
        DrawCrosshair(canvas, dirtyRect.Width, dirtyRect.Height);

        foreach (var treasure in _treasures)
        {
            DrawTreasure(canvas, treasure, dirtyRect.Width, dirtyRect.Height);
        }

        canvas.RestoreState();
    }

    private void DrawCrosshair(ICanvas canvas, float width, float height)
    {
        var centerX = width / 2;
        var centerY = height / 2;
        const float crosshairSize = 20;

        canvas.StrokeColor = Colors.White.WithAlpha(0.5f);
        canvas.StrokeSize = 2;

        // Horizontal line
        canvas.DrawLine(centerX - crosshairSize, centerY, centerX + crosshairSize, centerY);
        
        // Vertical line
        canvas.DrawLine(centerX, centerY - crosshairSize, centerX, centerY + crosshairSize);

        // Center dot
        canvas.FillColor = Colors.White.WithAlpha(0.7f);
        canvas.FillCircle(centerX, centerY, 3);
    }

    private void DrawTreasure(ICanvas canvas, TreasureOverlay treasure, float width, float height)
    {
        var x = treasure.ScreenX;
        var y = treasure.ScreenY;

        // Don't draw if off screen
        if (x < 0 || x > width || y < 0 || y > height)
            return;

        const float iconSize = 60;
        const float glowSize = 80;

        // Draw glow effect based on proximity
        var glowColor = GetProximityColor(treasure.ProximityLevel);

        // Animated pulsing glow - smooth continuous animation
        var timeMs = (DateTime.Now - _lastUpdateTime).TotalMilliseconds;
        var pulseScale = 1.0f + (float)(Math.Sin(timeMs * 0.003) * 0.2);
        
        // Draw outer glow
        canvas.FillColor = glowColor.WithAlpha(0.2f);
        canvas.FillEllipse(
            x - glowSize * pulseScale / 2,
            y - glowSize * pulseScale / 2,
            glowSize * pulseScale,
            glowSize * pulseScale
        );

        // Draw inner glow
        var innerGlowSize = glowSize * 0.6f;
        canvas.FillColor = glowColor.WithAlpha(0.4f);
        canvas.FillEllipse(
            x - innerGlowSize * pulseScale / 2,
            y - innerGlowSize * pulseScale / 2,
            innerGlowSize * pulseScale,
            innerGlowSize * pulseScale
        );

        // Draw icon background
        canvas.FillColor = Colors.White;
        canvas.FillEllipse(x - iconSize / 2, y - iconSize / 2, iconSize, iconSize);

        // Draw icon border
        canvas.StrokeColor = glowColor;
        canvas.StrokeSize = 3;
        canvas.DrawEllipse(x - iconSize / 2, y - iconSize / 2, iconSize, iconSize);

        // Draw treasure icon (simplified - you can load actual images)
        canvas.FontColor = Colors.Gold;
        canvas.FontSize = 30;
        canvas.DrawString("💎", x - 15, y - 15, 30, 30, HorizontalAlignment.Center, VerticalAlignment.Center);

        // Draw distance label with background
        var distanceText = FormatDistance(treasure.Distance);
        canvas.FillColor = Colors.Black.WithAlpha(0.7f);
        canvas.FillRoundedRectangle(x - 30, y + iconSize / 2 + 5, 60, 20, 4);
        
        canvas.FontColor = Colors.White;
        canvas.FontSize = 12;
        canvas.DrawString(
            distanceText,
            x - 30,
            y + iconSize / 2 + 5,
            60,
            20,
            HorizontalAlignment.Center,
            VerticalAlignment.Center
        );

        // Draw name label with background
        var nameWidth = Math.Max(80, treasure.Name.Length * 6);
        canvas.FillColor = Colors.Black.WithAlpha(0.7f);
        canvas.FillRoundedRectangle(x - nameWidth / 2, y - iconSize / 2 - 20, nameWidth, 18, 4);
        
        canvas.FontColor = Colors.White;
        canvas.FontSize = 10;
        canvas.DrawString(
            treasure.Name,
            x - nameWidth / 2,
            y - iconSize / 2 - 20,
            nameWidth,
            18,
            HorizontalAlignment.Center,
            VerticalAlignment.Center
        );

        // Draw point value badge
        if (treasure.PointValue > 0)
        {
            canvas.FillColor = Colors.Gold.WithAlpha(0.9f);
            canvas.FillRoundedRectangle(x + iconSize / 2 - 5, y - iconSize / 2 - 5, 28, 20, 6);
            
            canvas.StrokeColor = Colors.DarkOrange;
            canvas.StrokeSize = 1;
            canvas.DrawRoundedRectangle(x + iconSize / 2 - 5, y - iconSize / 2 - 5, 28, 20, 6);
            
            canvas.FontColor = Colors.Black;
            canvas.FontSize = 10;
            // FontWeight is not supported by ICanvas; to indicate bold, you must set a bold font if available.
            // If you have a bold font, set it here, e.g.:
            // canvas.Font = MyBoldFont; // Replace MyBoldFont with your actual bold font instance if available.
            canvas.DrawString(
                $"+{treasure.PointValue}",
                x + iconSize / 2 - 5,
                y - iconSize / 2 - 5,
                28,
                20,
                HorizontalAlignment.Center,
                VerticalAlignment.Center
            );
        }

        // Draw directional arrow if treasure is close
        if (treasure.Distance < 10)
        {
            DrawDirectionalArrow(canvas, x, y + iconSize / 2 + 30, glowColor);
        }
    }

    private void DrawDirectionalArrow(ICanvas canvas, float x, float y, Color color)
    {
        const float arrowSize = 15;
        
        canvas.FillColor = color.WithAlpha(0.8f);
        
        var path = new PathF();
        path.MoveTo(x, y);
        path.LineTo(x - arrowSize / 2, y + arrowSize);
        path.LineTo(x + arrowSize / 2, y + arrowSize);
        path.Close();
        
        canvas.FillPath(path);
    }

    private static Color GetProximityColor(string proximityLevel) => proximityLevel switch
    {
        "VERY_HOT" => Colors.Red,
        "HOT" => Colors.OrangeRed,
        "WARM" => Colors.Orange,
        "COOL" => Colors.Yellow,
        "COLD" => Colors.LightBlue,
        "VERY_COLD" => Colors.Blue,
        _ => Colors.Gray
    };

    private static string FormatDistance(double meters)
    {
        if (meters < 1)
            return $"{(int)(meters * 100)}cm";
        if (meters < 100)
            return $"{meters:F1}m";
        return $"{(meters / 1000):F2}km";
    }
}