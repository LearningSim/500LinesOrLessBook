using System.Windows;

namespace Blockcode
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoad;
            BlocksTab.OutputTab = OutputTab;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
        }
    }
}