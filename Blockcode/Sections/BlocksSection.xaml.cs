using System.Windows.Controls;

namespace Blockcode
{
    public partial class BlocksSection : UserControl
    {
        public OutputSection OutputSection { get; set; }
        public BlocksSection()
        {
            InitializeComponent();
            DataContext = OutputSection;
        }
    }
}