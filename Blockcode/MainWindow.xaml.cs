using System.Windows;

namespace Blockcode
{
    public partial class MainWindow
    {
        private DragAndDrop dragAndDrop;

        public MainWindow()
        {
            InitializeComponent();
            BlocksSection.OutputSection = OutputSection;
            Loaded += OnLoad;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            dragAndDrop = new DragAndDrop(BlocksSection, ScriptSection);
        }
    }
}