using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Blockcode
{
    public class DragAndDrop
    {
        public event Action ScriptUpdated = delegate { };

        private readonly BlocksSection blocksSection;
        private readonly ScriptSection scriptSection;
        private UserControl startSection;

        public DragAndDrop(BlocksSection blocksSection, ScriptSection scriptSection)
        {
            this.blocksSection = blocksSection;
            this.scriptSection = scriptSection;

            blocksSection.PreviewDrop += BlocksSectionOnDrop;
            foreach (Block block in blocksSection.BlocksHolder.Children)
            {
                AddHandlerRecursively(block, b => b.MouseDown += BlocksTabOnMouseDown);
            }

            scriptSection.BlankArea.PreviewDrop += ScriptSectionOnDrop;
            foreach (Block block in scriptSection.BlocksHolder.Children)
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
            AddHandlerRecursively(block, b => b.ValueUpdated += () => ScriptUpdated());
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
            OnMouseDown(blocksSection, (Block)sender, e);

        private void ScriptTabOnMouseDown(object sender, MouseButtonEventArgs e) =>
            OnMouseDown(scriptSection, (Block)sender, e);

        private void OnMouseDown(UserControl tab, Block block, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (block.IsStub) return;

            startSection = tab;
            DragDrop.DoDragDrop(block, block, tab == blocksSection ? DragDropEffects.Copy : DragDropEffects.Move);
        }

        private void BlocksSectionOnDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (!(e.Data.GetData(e.Data.GetFormats()[0]) is Block dropped)) return;

            if (startSection == scriptSection)
            {
                dropped.Remove();
                ScriptUpdated();
            }
        }

        private void ScriptSectionOnDrop(object sender, DragEventArgs e)
        {
            OnDragLeave(sender, e);
            if (!(e.Data.GetData(e.Data.GetFormats()[0]) is Block dropped)) return;

            var targetBlock = sender as Block;
            if (dropped == sender) return;

            if (startSection == blocksSection)
            {
                var droppedClone = dropped.Clone();
                AddScriptSectionHandlers(droppedClone);
                if (targetBlock != null)
                {
                    droppedClone.AddBefore(targetBlock);
                }
                else
                {
                    scriptSection.BlocksHolder.Children.Add(droppedClone);
                }
            }
            else if (targetBlock != null)
            {
                dropped.Remove();
                dropped.AddBefore(targetBlock);
            }
            else
            {
                dropped.Remove();
                scriptSection.BlocksHolder.Children.Add(dropped);
            }

            ScriptUpdated();
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