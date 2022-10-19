using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Blockcode
{
    public partial class MainWindow
    {
        private UserControl startTab;

        public MainWindow()
        {
            InitializeComponent();
            BlocksSection.OutputSection = OutputSection;
            Loaded += OnLoad;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            BlocksSection.PreviewDrop += BlocksSectionOnDrop;
            foreach (Block block in BlocksSection.BlocksHolder.Children)
            {
                AddHandlerRecursively(block, b => b.MouseDown += BlocksTabOnMouseDown);
            }

            ScriptSection.BlankArea.PreviewDrop += ScriptSectionOnDrop;
            foreach (Block block in ScriptSection.BlocksHolder.Children)
            {
                AddScriptSectionHandlers(block);
            }
        }

        private void AddScriptSectionHandlers(Block block)
        {
            AddHandlerRecursively(block, b => b.MouseDown += ScriptTabOnMouseDown);
            AddHandlerRecursively(block, b => b.Drop += ScriptSectionOnDrop);
            AddHandlerRecursively(block, b => b.DragEnter += OnDragEnter);
            AddHandlerRecursively(block, b => b.DragLeave += OnDragLeave);
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;
            ((Block)sender).ShowDropIndicator();
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
            (sender as Block)?.HideDropIndicator();
        }

        private void BlocksTabOnMouseDown(object sender, MouseButtonEventArgs e) =>
            OnMouseDown(BlocksSection, (Block)sender, e);

        private void ScriptTabOnMouseDown(object sender, MouseButtonEventArgs e) =>
            OnMouseDown(ScriptSection, (Block)sender, e);

        private void OnMouseDown(UserControl tab, Block block, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (block.IsStub) return;

            startTab = tab;
            DragDrop.DoDragDrop(block, block, tab == BlocksSection ? DragDropEffects.Copy : DragDropEffects.Move);
        }

        private void BlocksSectionOnDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (!(e.Data.GetData(e.Data.GetFormats()[0]) is Block dropped)) return;

            if (startTab == ScriptSection)
            {
                dropped.Remove();
            }
        }

        private void ScriptSectionOnDrop(object sender, DragEventArgs e)
        {
            OnDragLeave(sender, e);
            if (!(e.Data.GetData(e.Data.GetFormats()[0]) is Block dropped)) return;

            var targetBlock = sender as Block;
            if (dropped == sender) return;

            if (startTab == BlocksSection)
            {
                var droppedClone = dropped.Clone();
                AddScriptSectionHandlers(droppedClone);
                if (targetBlock != null)
                {
                    droppedClone.AddBefore(targetBlock);
                }
                else
                {
                    ScriptSection.BlocksHolder.Children.Add(droppedClone);
                }
            }
            else
            {
                if (targetBlock != null)
                {
                    dropped.Remove();
                    dropped.AddBefore(targetBlock);
                }
                else
                {
                    dropped.Remove();
                    ScriptSection.BlocksHolder.Children.Add(dropped);
                }
            }
        }

        private void AddHandlerRecursively(Block block, Action<Block> addHandler)
        {
            addHandler(block);
            block.ChildrenHolder.ChildAdded += (_, added) =>
            {
                if (!added.IsStub) return;
                addHandler(added);
            };

            foreach (var child in block.GetChildren())
            {
                AddHandlerRecursively(child, addHandler);
            }
        }
    }
}