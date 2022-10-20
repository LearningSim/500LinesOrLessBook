using System.Windows.Media;

namespace Blockcode
{
    public static class Extensions
    {
        public static SolidColorBrush FromRgb(this SolidColorBrush brush, int rgb)
        {
            brush.Color = Color.FromRgb((byte)(rgb >> 16), (byte)((rgb >> 8) & 0xff), (byte)(rgb & 0xff));
            return brush;
        }
    }
}