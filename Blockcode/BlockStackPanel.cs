using System;
using System.Windows;
using System.Windows.Controls;

namespace Blockcode
{
    public class BlockStackPanel : StackPanel
    {
        public event Action<BlockStackPanel, Block> ChildAdded = delegate { };
        public event Action<BlockStackPanel, Block> ChildRemoved = delegate { };

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded != null)
            {
                ChildAdded(this, (Block)visualAdded);
            }
            if (visualRemoved != null)
            {
                ChildRemoved(this, (Block)visualRemoved);
            }
        }

        public override string ToString()
        {
            var parent = Parent as FrameworkElement;
            while (parent != null)
            {
                if (parent is Block)
                {
                    return parent.ToString();
                }

                parent = parent.Parent as FrameworkElement;
            }

            return "null";
        }
    }
}