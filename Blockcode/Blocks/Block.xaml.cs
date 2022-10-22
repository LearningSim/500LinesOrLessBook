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
    public partial class Block : ExtendedUserControl, IDropIndicatorHolder
    {
        public event Action ValueUpdated = delegate { };
        public bool IsStub { get; set; }
        public string Label { get; set; } = "Block Name";
        public Brush LabelColor { get; set; } = Brushes.Black;

        private int? value;
        public int? Value
        {
            get => value;
            set
            {
                this.value = value;
                ValueUpdated();
            }
        }

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
        private static readonly Thickness Indent = new Thickness(26, 0, 0, 0);
        private static readonly Thickness ZeroIndent = new Thickness();

        public Block()
        {
            Init();
        }

        private Block(string label, Brush labelColor = null, int? value = null, string units = null,
            List<Block> children = null, bool isStub = false, bool hasStub = false)
        {
            Label = label;
            LabelColor = labelColor;
            Value = value;
            Units = units;
            IsStub = isStub;
            HasStub = hasStub;

            Init();
            children?.ForEach(child => Children.Add(child));
            if (children?.Any() == true)
            {
                IsContainer = true;
            }
        }

        private void Init()
        {
            InitializeComponent();
            DataContext = this;
            Children = ChildrenHolder.Children;

            Loaded += OnLoad;
            ChildrenHolder.ChildAdded += OnChildAdded;
            ChildrenHolder.ChildRemoved += OnChildRemoved;
        }

        private void OnLoad(object sender, RoutedEventArgs args)
        {
            if (IsContainer && !HasStub)
            {
                Children.Add(new Block("", isStub: true));
                HasStub = true;
            }
        }

        private void OnChildAdded(BlockStackPanel _, Block child)
        {
            child.Border.Padding = Indent;
            child.BorderThickness = new Thickness(.5, 0, 0, 0);
            var lineSpacing = new Thickness(0, 4, 0, 0);
            Border.Margin = lineSpacing;
            ChildrenHolder.Margin = lineSpacing;
        }

        private void OnChildRemoved(BlockStackPanel _, Block child) => child.Border.Padding = ZeroIndent;

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

        public void ShowDropIndicator() => DropIndicator.Visibility = Visibility.Visible;
        public void HideDropIndicator() => DropIndicator.Visibility = Visibility.Collapsed;

        public Block Clone()
        {
            var children = GetChildren().Select(c => c.Clone()).ToList();
            return new Block(Label, LabelColor, Value, Units, children, IsStub, HasStub);
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