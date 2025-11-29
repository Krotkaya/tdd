using System.Drawing;
using FluentAssertions;

namespace TagCloud.Tests;

[TestFixture]
public class CircularCloudLayouterTests
{
    private CircularCloudLayouter _layouter;
    private Point _center;

    [SetUp]
    public void SetUp()
    {
        _center = new Point(100, 100);
        _layouter = new CircularCloudLayouter(_center);
    }

    [TestCase(-1, -10, 
        TestName = "Должен бросать исключение если ширина и высота отрицательные")]
    [TestCase(0, 10,   
        TestName = "Должен бросать исключение если ширина равна нулю")]
    [TestCase(10, 0,  
        TestName = "Должен бросать исключение если высота равна нулю")]
    [TestCase(0, 0,    
        TestName = "Должен бросать исключение если ширина и высота равны нулю")]
    public void PutNextRectangle_ShouldThrowException_SizeIsNonPositive(int width, int height)
    {
        var size = new Size(width, height);
        
        var action = () => _layouter.PutNextRectangle(size);
        
        action
            .Should()
            .Throw<ArgumentException>();
    }

    [TestCase(10, 20, 
        TestName = "Первый прямоугольник 10x20 должен быть размещен строго в центре")]
    [TestCase(1, 1,   
        TestName = "Первый прямоугольник 1x1 должен быть размещен строго в центре")]
    [TestCase(50, 30, 
        TestName = "Первый прямоугольник 50x30 должен быть размещен строго в центре")]
    public void PutNextRectangle_ShouldPlaceInCenter_FirstRectangle(int width, int height)
    {
        var size = new Size(width, height);
        
        var rectangle = _layouter.PutNextRectangle(size);
        var rectangleCenter = new Point(
            rectangle.Left + rectangle.Width / 2,
            rectangle.Top + rectangle.Height / 2);
        
        rectangleCenter
            .Should()
            .Be(_center);
    }

    [TestCase(30, 20, 100, 
        TestName = "Сто прямоугольников 30x20 не должны пересекаться")]
    [TestCase(10, 10, 200, 
        TestName = "Двести прямоугольников 10x10 не должны пересекаться")]
    [TestCase(50, 30, 50,  
        TestName = "Пятьдесят прямоугольников 50x30 не должны пересекаться")]
    public void Rectangles_ShouldNotIntersect_PlacingManySimilarRectangles(
        int width, 
        int height, 
        int rectanglesCount)
    {
        var size = new Size(width, height);
        
        for (var i = 0; i < rectanglesCount; i++)
            _layouter.PutNextRectangle(size);

        var rectangles = _layouter.Rectangles.ToArray();
        
        for (var i = 0; i < rectangles.Length; i++)
        {
            for (var j = i + 1; j < rectangles.Length; j++)
            { 
                rectangles[i]
                    .IntersectsWith(rectangles[j])
                    .Should()
                    .BeFalse( $"Прямоугольники {i} и {j} не пересекаются");
            }
        }
    }

    [TestCase(40, 20, 150, 1.5, 
        TestName = "Облако из 150 прямоугольников 40x20 должно иметь почти круглую форму")]
    [TestCase(60, 10, 200, 1.7,
        TestName = "Облако из 200 прямоугольников 60x10 должно иметь почти круглую форму")]
    public void Cloud_ShouldBeCircular_ByBoundingBoxRatio(int width,
        int height,
        int rectanglesCount,
        double maxRatio)
    {
        var size = new Size(width, height);
        
        for (var i = 0; i < rectanglesCount; i++)
            _layouter.PutNextRectangle(size);

        var rectangles = _layouter.Rectangles.ToArray();

        var minX = rectangles.Min(r => r.Left);
        var maxX = rectangles.Max(r => r.Right);
        var minY = rectangles.Min(r => r.Top);
        var maxY = rectangles.Max(r => r.Bottom);

        var boundingWidth = maxX - minX;
        var boundingHeight = maxY - minY;
        var ratio = 
            (double)Math.Max(boundingWidth, boundingHeight) / Math.Min(boundingWidth, boundingHeight);

        ratio
            .Should()
            .BeLessThanOrEqualTo(1.5, "Прямоугольник, ограничивающий облако не должен быть сильно вытянутым");
    }

    [TestCase(30, 20, 50,
        TestName = "Последний из пятидесяти прямоугольников 30x20 нельзя сдвинуть ближе к центру на один пиксель")]
    [TestCase(50, 30, 30,
        TestName = "Последний из тридцати прямоугольников 50x30 нельзя сдвинуть ближе к центру на один пиксель")]
    public void Rectangle_ShouldNotBeShiftableToCenterByOnePixel_PlacingManySimilarRectangles(     
        int width,
        int height,
        int alreadyPlacedCount)
    {
        var size = new Size(width, height);
        for (var i = 0; i < alreadyPlacedCount; i++)
            _layouter.PutNextRectangle(size);
        
        var lastRectangle = _layouter.PutNextRectangle(size);
        var rectangles = _layouter.Rectangles.ToArray();

        var lastCenter = new Point(
            lastRectangle.Left + lastRectangle.Width / 2,
            lastRectangle.Top + lastRectangle.Height / 2);

        var dx = 0;
        if (lastCenter.X > _center.X) dx = -1;
        else if (lastCenter.X < _center.X) dx = 1;

        var dy = 0;
        if (lastCenter.Y > _center.Y) dy = -1;
        else if (lastCenter.Y < _center.Y) dy = 1;

        var canBeShifted = false;

        if (dx != 0 || dy != 0)
        {
            var shifted = new Rectangle(
                new Point(lastRectangle.Left + dx, lastRectangle.Top + dy),
                lastRectangle.Size);

            canBeShifted = !rectangles
                .Where(r => r != lastRectangle)
                .Any(r => r.IntersectsWith(shifted));
        }
        canBeShifted
            .Should()
            .BeFalse("Алгоритм должен прижать прямоугольник максимально близко к центру");
    }
}
