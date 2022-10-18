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
            BlocksTab.OutputTab = OutputTab;
            Loaded += OnLoad;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            BlocksTab.PreviewDrop += BlocksTabOnDrop;
            foreach (Block block in BlocksTab.BlocksHolder.Children)
            {
                AddHandlerRecursively(block, b => b.MouseDown += BlocksTabOnMouseDown);
            }

            ScriptTab.BlankArea.PreviewDrop += ScriptTabOnDrop;
            foreach (Block block in ScriptTab.BlocksHolder.Children)
            {
                AddScriptTabHandlers(block);
            }
        }

        private void AddScriptTabHandlers(Block block)
        {
            AddHandlerRecursively(block, b => b.MouseDown += ScriptTabOnMouseDown);
            AddHandlerRecursively(block, b => b.Drop += ScriptTabOnDrop);
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
            ((Block)sender).HideDropIndicator();
        }

        private void BlocksTabOnMouseDown(object sender, MouseButtonEventArgs e) =>
            OnMouseDown(BlocksTab, (Block)sender, e);

        private void ScriptTabOnMouseDown(object sender, MouseButtonEventArgs e) =>
            OnMouseDown(ScriptTab, (Block)sender, e);

        private void OnMouseDown(UserControl tab, Block block, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (block.IsStub) return;

            startTab = tab;
            DragDrop.DoDragDrop(block, block, tab == BlocksTab ? DragDropEffects.Copy : DragDropEffects.Move);
        }

        private void BlocksTabOnDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (!(e.Data.GetData(e.Data.GetFormats()[0]) is Block dropped)) return;

            if (startTab == ScriptTab)
            {
                dropped.Remove();
            }
        }

        private void ScriptTabOnDrop(object sender, DragEventArgs e)
        {
            OnDragLeave(sender, e);
            if (!(e.Data.GetData(e.Data.GetFormats()[0]) is Block dropped)) return;

            var targetBlock = sender as Block;
            if (dropped == sender) return;

            if (startTab == BlocksTab)
            {
                var droppedClone = dropped.Clone();
                AddScriptTabHandlers(droppedClone);
                if (targetBlock != null)
                {
                    droppedClone.AddBefore(targetBlock);
                }
                else
                {
                    ScriptTab.BlocksHolder.Children.Add(droppedClone);
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
                    ScriptTab.BlocksHolder.Children.Add(dropped);
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