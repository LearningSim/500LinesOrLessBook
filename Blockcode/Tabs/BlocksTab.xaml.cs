using System.Windows.Controls;

namespace Blockcode
{
    public partial class BlocksTab : UserControl
    {
        public OutputTab OutputTab { get; set; }
        public BlocksTab()
        {
            InitializeComponent();
            DataContext = OutputTab;
        }
    }
}