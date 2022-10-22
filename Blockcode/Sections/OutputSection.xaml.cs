using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Blockcode
{
    public partial class OutputSection : UserControl
    {
        public bool IsPenDown { get; set; }
        public Vector Position { get; set; }
        public double Angle { get; set; }
        private readonly Image turtle = new Image();
        private readonly RotateTransform turtleRotation = new RotateTransform(90);
        private Storyboard animation;

        public OutputSection()
        {
            InitializeComponent();
            turtle.Source = new BitmapImage(new Uri("pack://application:,,,/Images/turtle.png"));
            RenderOptions.SetBitmapScalingMode(turtle, BitmapScalingMode.Fant);
            turtle.Width = 40;
            turtle.RenderTransformOrigin = new Point(.5, .5);
            var transform = new TransformGroup();
            transform.Children.Add(turtleRotation);
            transform.Children.Add(new TranslateTransform(-20, -20));
            turtle.RenderTransform = transform;
        }

        public void Reset()
        {
            Position = new Vector();
            Canvas.Children.Clear();
            Canvas.Children.Add(turtle);
            Canvas.SetLeft(turtle, 0);
            Canvas.SetTop(turtle, 0);
            turtleRotation.Angle = 90;
            Angle = 0;
            turtle.Visibility = Visibility.Visible;
            IsPenDown = true;
            animation?.Stop(this);
        }

        private async Task Move(int distance, CancellationToken token)
        {
            animation?.Stop(this);
            var start = Position;
            Position = start + new Vector(Math.Cos(Angle) * distance, -Math.Sin(Angle) * distance);
            Canvas.SetLeft(turtle, Position.X);
            Canvas.SetTop(turtle, Position.Y);
            if (!IsPenDown) return;

            Canvas.Children.Add(new Line
            {
                Stroke = Brushes.Black,
                X1 = start.X,
                Y1 = start.Y,
                X2 = Position.X,
                Y2 = Position.Y
            });
            
        }

        private async Task Glide(int distance, CancellationToken token)
        {
            var start = Position;
            Position = start + new Vector(Math.Cos(Angle) * distance, -Math.Sin(Angle) * distance);

            var line = new Line
            {
                Stroke = Brushes.Black,
                X1 = start.X,
                Y1 = start.Y,
                X2 = Position.X,
                Y2 = Position.Y
            };
            if (IsPenDown)
            {
                Canvas.Children.Add(line);
            }

            animation = new Storyboard();
            animation.Add(turtle, Canvas.LeftProperty, start.X, Position.X, 2);
            animation.Add(turtle, Canvas.TopProperty, start.Y, Position.Y, 2);
            animation.Add(line, Line.X2Property, start.X, Position.X, 2);
            animation.Add(line, Line.Y2Property, start.Y, Position.Y, 2);
            animation.Begin(this, true);
            
            await Task.Delay(2000, token);
        }

        private void Rotate(int angle)
        {
            Angle += angle * Math.PI / 180;
            turtleRotation.Angle = -Angle * 180 / Math.PI + 90;
        }

        public async Task PenUp(Block block, CancellationToken token) => IsPenDown = false;
        public async Task PenDown(Block block, CancellationToken token) => IsPenDown = true;
        public async Task Forward(Block block, CancellationToken token) => await Move(block.Value.Value, token);
        public async Task Back(Block block, CancellationToken token) => await Move(-block.Value.Value, token);
        public async Task Glide(Block block, CancellationToken token) => await Glide(block.Value.Value, token);
        public async Task TurnLeft(Block block, CancellationToken token) => Rotate(block.Value.Value);
        public async Task TurnRight(Block block, CancellationToken token) => Rotate(-block.Value.Value);
        public async Task BackToCenter(Block block, CancellationToken token) => Position = new Vector();
        public async Task HideTurtle(Block block, CancellationToken token) => turtle.Visibility = Visibility.Hidden;
        public async Task ShowTurtle(Block block, CancellationToken token) => turtle.Visibility = Visibility.Visible;
    }
}