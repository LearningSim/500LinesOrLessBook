using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Blockcode
{
    [ContentProperty(nameof(Children))]
    public partial class Block : ExtendedUserControl
    {
        public bool IsStub { get; set; }
        public string Label { get; set; } = "Block Name";
        public Brush LabelColor { get; set; } = Brushes.Black;
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
        private bool HasStub { get; set; }
        private static readonly Thickness TabPadding = new Thickness(14, 0, 0, 0);
        private static readonly Thickness ZeroPadding = new Thickness();

        public Block()
        {
            InitializeComponent();
            DataContext = this;
            Children = ChildrenHolder.Children;
            Loaded += OnLoad;
            ChildrenHolder.ChildAdded += OnChildAdded;
            ChildrenHolder.ChildRemoved += OnChildRemoved;
        }

        private Block(string label, Brush labelColor = null, int? value = null, string units = null, List<Block> children = null, bool isStub = false, bool hasStub = false)
        {
            Label = label;
            LabelColor = labelColor;
            Value = value;
            Units = units;
            IsStub = isStub;
            HasStub = hasStub;
            InitializeComponent();
            DataContext = this;
            Children = ChildrenHolder.Children;
            children?.ForEach(child => Children.Add(child));
            if (children?.Any() == true)
            {
                IsContainer = true;
            }

            Loaded += OnLoad;
            ChildrenHolder.ChildAdded += OnChildAdded;
            ChildrenHolder.ChildRemoved += OnChildRemoved;
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
            var clone = new Block(Label, LabelColor, Value, Units, children, IsStub, HasStub);
            return clone;
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
                child.Border.Padding = TabPadding;
            }
        }

        private void OnChildAdded(BlockStackPanel _, Block child)
        {
            child.Border.Padding = TabPadding;
        }

        private void OnChildRemoved(BlockStackPanel _, Block child)
        {
            child.Border.Padding = ZeroPadding;
        }

        public IReadOnlyList<Block> GetChildren() => Children.OfType<Block>().ToList();
        private UIElementCollection GetSiblings() => (Parent as Panel)?.Children;

        public void Remove() => GetSiblings().Remove(this);
        public void AddBefore(Block block)
        {
            var siblings = block.GetSiblings();
            siblings.Insert(siblings.IndexOf(block), this);
        }

        public override string ToString()
        {
            return $"{GetType().Name}({Label}, {Value})";
        }
    }
}