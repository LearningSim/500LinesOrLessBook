using System.Windows.Controls;
using System.Windows.Media;

namespace Blockcode
{
    public partial class ScriptSection : UserControl, IDropIndicatorHolder
    {
        public ScriptSection()
        {
            InitializeComponent();
        }
        public void ShowDropIndicator() => DropIndicator.BorderBrush = Brushes.SlateBlue;
        public void HideDropIndicator() => DropIndicator.BorderBrush = Brushes.Transparent;
    }
}