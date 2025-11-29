using System.Drawing;

namespace TagCloud;
public class CircularCloudLayouter(Point center)
{
    private readonly List<Rectangle> _rectangles = [];
    private double _angle;
    private const double AngleStep = 0.1;//их лучше в дальнейшем протестировать и возможно подобрать другие значения
    private const double RadiusStep = 0.5;//их лучше в дальнейшем протестировать и возможно подобрать другие значения

    public IReadOnlyCollection<Rectangle> Rectangles => _rectangles.AsReadOnly();
    
    public Rectangle PutNextRectangle(Size rectangleSize)
    {
        if (rectangleSize.Width <= 0 || rectangleSize.Height <= 0) 
            throw new ArgumentException("Rectangle size must be greater than zero");
        
        Rectangle rectangle;

        if (_rectangles.Count == 0)
            rectangle = CreateRectangleWithCenter(center, rectangleSize);
        
        else
        {
            rectangle = FindPlaceForRectangle(rectangleSize);
            rectangle = ShiftRectangleToCenter(rectangle);
        }

        _rectangles.Add(rectangle);
        return rectangle;
    }
    
    private static Rectangle CreateRectangleWithCenter(Point center, Size size)
    {
        var left = center.X - size.Width / 2;
        var top = center.Y - size.Height / 2;
        return new Rectangle(new Point(left, top), size);
    }
    
    private Rectangle FindPlaceForRectangle(Size size)
    {
        while (true)
        {
            var pointOnSpiral = GetNextPointOnSpiral();
            var rectangle = CreateRectangleWithCenter(pointOnSpiral, size);

            if (!_rectangles.Any(r => r.IntersectsWith(rectangle)))
                return rectangle;
        }
    }
    
    private Rectangle ShiftRectangleToCenter(Rectangle rectangle)
    {
        while (true)
        {
            var direction = GetDirectionToCenter(rectangle);

            if (direction == Point.Empty)
                return rectangle;

            var shifted = new Rectangle(
                new Point(rectangle.Left + direction.X, rectangle.Top + direction.Y),
                rectangle.Size);

            if (_rectangles.Any(r => r.IntersectsWith(shifted)))
                return rectangle;

            rectangle = shifted;
        }
    }
    
    private Point GetDirectionToCenter(Rectangle rectangle)
    {
        var rectangleCenter = new Point(
            rectangle.Left + rectangle.Width / 2,
            rectangle.Top + rectangle.Height / 2);

        var dx = 0;
        if (rectangleCenter.X > center.X)
            dx = -1;
        else if (rectangleCenter.X < center.X)
            dx = 1;

        var dy = 0;
        if (rectangleCenter.Y > center.Y)
            dy = -1;
        else if (rectangleCenter.Y < center.Y)
            dy = 1;

        return new Point(dx, dy);
    }
    
    private Point GetNextPointOnSpiral()
    {
        var radius = RadiusStep * _angle;
        var x = center.X + (int)Math.Round(radius * Math.Cos(_angle));
        var y = center.Y + (int)Math.Round(radius * Math.Sin(_angle));

        _angle += AngleStep;
        return new Point(x, y);
    }
}