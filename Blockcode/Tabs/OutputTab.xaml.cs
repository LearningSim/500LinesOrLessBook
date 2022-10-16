using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Blockcode
{
    public partial class OutputTab : UserControl
    {
        public bool IsPenDown { get; set; }
        public Vector Position { get; set; }
        public double Angle { get; set; }
        public double Visible { get; set; }
        public OutputTab()
        {
            InitializeComponent();
            //PenDown();
            //TurnRight(45);
            Move(100);
        }

        private void Move(int distance)
        {
            if(!IsPenDown) return;
            
            var start = Position;
            Position = start + new Vector(Math.Cos(Angle) * distance, -Math.Sin(Angle) * distance);
            Canvas.Children.Add(new Line
            {
                Stroke = Brushes.Black,
                X1 = start.X,
                Y1 = start.Y,
                X2 = Position.X,
                Y2 = Position.Y
            });
        }

        public Action<Block> PenUp => block => IsPenDown = false;
        public Action<Block> PenDown => block => IsPenDown = true;
        public Action<Block> Forward => block => Move(block.Value.Value);
        public Action<Block> Back => block => Move(-block.Value.Value);
        public Action<Block> TurnLeft => block => Angle += block.Value.Value * Math.PI / 180;
        public Action<Block> TurnRight => block => Angle -= block.Value.Value * Math.PI / 180;
    }
}