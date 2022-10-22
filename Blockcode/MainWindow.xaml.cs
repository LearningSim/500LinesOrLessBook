using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Blockcode
{
    public partial class MainWindow
    {
        private DragAndDrop dragAndDrop;
        private Script script = new Script();

        public MainWindow()
        {
            InitializeComponent();
            BlocksSection.OutputSection = OutputSection;
            Loaded += OnLoad;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            dragAndDrop = new DragAndDrop(BlocksSection, ScriptSection);
            dragAndDrop.ScriptUpdated += OnScriptUpdated;
            
            Script.Commands = new Dictionary<string, Func<Block, CancellationToken, Task>>
            {
                {"Pen up", OutputSection.PenUp},
                {"Pen down", OutputSection.PenDown},
                {"Forward", OutputSection.Forward},
                {"Back", OutputSection.Back},
                {"Glide", OutputSection.Glide},
                {"Turn left", OutputSection.TurnLeft},
                {"Turn right", OutputSection.TurnRight},
                {"Back to center", OutputSection.BackToCenter},
                {"Hide turtle", OutputSection.HideTurtle},
                {"Show turtle", OutputSection.ShowTurtle},
                {"Repeat", script.Repeat},
            };
            OnScriptUpdated();
        }

        private void OnScriptUpdated()
        {
            script.Stop();
            script = new Script(ScriptSection.BlocksHolder.Children.OfType<Block>().ToList());
            OutputSection.Reset();
            script.Run();
        }
    }
}