using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Blockcode
{
    [ContentProperty(nameof(Children))]
    public partial class Block : ExtendedUserControl
    {
        public bool IsStub { get; set; }
        public string Label { get; set; } = "Block Name";
        public int? Value { get; set; }
        public Visibility ValueVisibility => Value == null ? Visibility.Collapsed : Visibility.Visible;
        public string Units { get; set; }

        private static readonly DependencyProperty ActionProp = RegProp(nameof(Action), typeof(Block));

        public Action<Block> Action
        {
            get => (Action<Block>)GetValue(ActionProp);
            set => SetValue(ActionProp, value);
        }

        public UIElementCollection Children { get; private set; }
        public bool IsContainer { get; set; }
        public bool HasStub { get; set; }
        private static readonly Thickness tabMargin = new Thickness(14, 0, 0, 0);

        public Block()
        {
            InitializeComponent();
            DataContext = this;
            Children = ChildrenContainer.Children;
            Loaded += OnLoad;
        }

        public Block(string label, int? value = null, string units = null, List<Block> children = null, bool isStub = false)
        {
            Label = label;
            Value = value;
            Units = units;
            IsStub = isStub;
            InitializeComponent();
            DataContext = this;
            Children = ChildrenContainer.Children;
            children?.ForEach(child => Children.Add(child));
            if (children?.Any() == true)
            {
                IsContainer = true;
            }

            Loaded += OnLoad;
        }

        public List<object> GetToken()
        {
            var script = new List<object> { Label };
            if (Value.HasValue)
            {
                script.Add(Value);
            }

            var children = GetChildren();
            if (children.Any())
            {
                script.AddRange(children.Select(child => child.GetToken()));
            }

            return script;
        }
        
        public Block Clone()
        {
            var children = GetChildren().Select(c => c.Clone()).ToList();
            var clone = new Block(Label, Value, Units, children, IsStub){Margin = tabMargin, HasStub = HasStub};
            return clone;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            DragDrop.DoDragDrop(this, this, DragDropEffects.Copy);
            e.Handled = true;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if(!(e.Data.GetData(GetType()) is Block block)) return;
            
            Children.Add(block.Clone());
            e.Handled = true;
        }

        private void OnLoad(object sender, RoutedEventArgs args)
        {
            if (IsContainer && !HasStub)
            {
                Children.Add(new Block("", isStub: true));
                HasStub = true;
            }

            foreach (Block child in Children)
            {
                child.Margin = tabMargin;
            }
        }

        private IReadOnlyList<Block> GetChildren() => Children.OfType<Block>().ToList();

        public override string ToString()
        {
            return $"{GetType().Name}({Label}, {Value})";
        }
    }
}