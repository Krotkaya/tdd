using System.Drawing;
using SkiaSharp;

namespace TagCloud.Visualization;

internal static class Program
{
    private static void Main()
    {
        const string imagesDirectory = "images";
        if (!Directory.Exists(imagesDirectory))
            Directory.CreateDirectory(imagesDirectory);

        GenerateCloud(
            Path.Combine(imagesDirectory, "cloud_wide_and_tall.png"),
            2000,
            2000,
            200,
            random =>
            {
                var isWide = random.NextDouble() < 0.5;
                return isWide
                    ? new Size(random.Next(60, 160), random.Next(15, 40))
                    : new Size(random.Next(15, 40), random.Next(60, 160));
            });

        GenerateCloud(
            Path.Combine(imagesDirectory, "cloud_mixed_random.png"),
            2000,
            2000,
            250,
            sizeFactory: random =>
                new Size(random.Next(20, 120), random.Next(20, 80)));

        GenerateCloud(
            Path.Combine(imagesDirectory, "cloud_extreme_shapes.png"),
            2000,
            2000,
            220,
            random =>
            {
                var mode = random.Next(0, 3);
                return mode switch
                {
                    0 => new Size(random.Next(100, 200), random.Next(10, 25)), 
                    1 => new Size(random.Next(10, 25), random.Next(100, 200)),
                    _ => new Size(random.Next(20, 60), random.Next(20, 60)) 
                };
            });
    }

    private static void GenerateCloud(
        string filePath,
        int imageWidth,
        int imageHeight,
        int rectanglesCount,
        Func<Random, Size> sizeFactory)
    {
        var center = new Point(imageWidth / 2, imageHeight / 2);
        var layouter = new CircularCloudLayouter(center);
        var random = new Random(1);

        for (var i = 0; i < rectanglesCount; i++)
        {
            var size = sizeFactory(random);
            layouter.PutNextRectangle(size);
        }
        SaveCloudImage(layouter, imageWidth, imageHeight, filePath);
    }

    private static void SaveCloudImage(
        CircularCloudLayouter layouter,
        int width,
        int height,
        string filePath)
    {
        var info = new SKImageInfo(width, height);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.Azure);

        using (var centerPaint = new SKPaint())
        {
            centerPaint.Color = SKColors.Black;
            centerPaint.StrokeWidth = 1;
            centerPaint.IsAntialias = true;
            canvas.DrawLine(width / 2f, 0, width / 2f, height, centerPaint);
            canvas.DrawLine(0, height / 2f, width, height / 2f, centerPaint);
        }

        using var rectangleFill = new SKPaint();
        rectangleFill.Color = new SKColor(0, 255, 0, 40);
        rectangleFill.Style = SKPaintStyle.Fill;
        rectangleFill.IsAntialias = true;

        using var rectanglStroke = new SKPaint();
        rectanglStroke.Color = SKColors.DarkGreen;
        rectanglStroke.Style = SKPaintStyle.Stroke;
        rectanglStroke.StrokeWidth = 1;
        rectanglStroke.IsAntialias = true;

        foreach (var rectangle in layouter.Rectangles)
        {
            var r = new SKRect(
                rectangle.Left,
                rectangle.Top,
                rectangle.Right,
                rectangle.Bottom);

            canvas.DrawRect(r, rectangleFill);
            canvas.DrawRect(r, rectanglStroke);
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
        data.SaveTo(stream);
    }
}
