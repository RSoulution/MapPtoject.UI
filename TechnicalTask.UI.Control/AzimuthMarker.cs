using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TechnicalTask.UI.Control
{
    public class AzimuthMarker : Canvas //Кастомний маркер з стрілкою напрямку за азимутом
    {
        private double radius;
        private double azimuthAngle;

        public AzimuthMarker(double size, double azimuth, Brush color, string tooltip)
        {
            radius = size; 
            azimuthAngle = azimuth;

            DrawMarker(color, tooltip);
        }

        private void DrawMarker(Brush brush, string tooltip)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = radius,
                Height = radius,
                Fill = brush,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                ToolTip = tooltip
            };

            // Центрування еліпсу
            SetLeft(ellipse, -radius/2);
            SetTop(ellipse, -radius/2);
            Children.Add(ellipse);


            Path arrow = CreateArrow(new Point(0, 0), azimuthAngle);
            Children.Add(arrow);
        }

        private Path CreateArrow(Point center, double angle) // Додавання стрілки
        {
            double arrowLength = radius;
            double arrowHeadSize = radius * 0.3;

            double radians = (90 - angle) * Math.PI / 180.0;

            Point end = new Point(
                center.X + arrowLength * Math.Cos(radians),
                center.Y - arrowLength * Math.Sin(radians) 
            );

            // Головка стрілки
            Point arrowLeft = new Point(
                end.X - arrowHeadSize * Math.Cos(radians - Math.PI / 6),
                end.Y + arrowHeadSize * Math.Sin(radians - Math.PI / 6)
            );

            Point arrowRight = new Point(
                end.X - arrowHeadSize * Math.Cos(radians + Math.PI / 6),
                end.Y + arrowHeadSize * Math.Sin(radians + Math.PI / 6)
            );

            PathGeometry arrowHead = new PathGeometry();
            PathFigure arrowFigure = new PathFigure { StartPoint = end };
            arrowFigure.Segments.Add(new LineSegment(arrowLeft, true));
            arrowFigure.Segments.Add(new LineSegment(arrowRight, true));
            arrowFigure.IsClosed = true;
            arrowHead.Figures.Add(arrowFigure);

            GeometryGroup arrowGeometry = new GeometryGroup();
            arrowGeometry.Children.Add(new LineGeometry(center, end));
            arrowGeometry.Children.Add(arrowHead);

            return new Path
            {
                Data = arrowGeometry,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.Black
            };
        }

        public UIElement GetElement()
        {
            return this;
        }
    }
}
