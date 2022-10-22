using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Blockcode
{
    public static class Extensions
    {
        public static SolidColorBrush FromRgb(this SolidColorBrush brush, int rgb)
        {
            brush.Color = Color.FromRgb((byte)(rgb >> 16), (byte)((rgb >> 8) & 0xff), (byte)(rgb & 0xff));
            return brush;
        }
        
        public static void Add(this Storyboard storyboard, DependencyObject target, DependencyProperty prop, double from, double to, double seconds)
        {
            var anim = new DoubleAnimation(from, to, TimeSpan.FromSeconds(seconds));
            storyboard.Children.Add(anim);
            Storyboard.SetTarget(anim, target);
            Storyboard.SetTargetProperty(anim, new PropertyPath(prop));
        }
    }
}