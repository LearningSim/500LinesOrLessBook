using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Blockcode
{
    [ContentProperty(nameof(Children))]
    public partial class Block : UserControl
    {
        public string Label { get; set; } = "Block Name";
        public int? Value { get; set; }
        public Visibility ValueVisibility => Value == null ? Visibility.Collapsed : Visibility.Visible;
        public string Units { get; set; }

        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register(
            nameof(Children), typeof(UIElementCollection), typeof(Block)
        );

        public UIElementCollection Children
        {
            get => (UIElementCollection)GetValue(ChildrenProperty);
            private set => SetValue(ChildrenProperty, value);
        }

        public Block()
        {
            InitializeComponent();
            DataContext = this;
            Children = ChildrenContainer.Children;
            Loaded += OnLoad;
        }

        public Block(string label, int? value = null, string units = null, List<UIElement> children = null)
        {
            Label = label;
            Value = value;
            Units = units;
            InitializeComponent();
            DataContext = this;
            Children = ChildrenContainer.Children;
            children?.ForEach(child => Children.Add(child));
            Loaded += OnLoad;
        }

        public List<object> GetScript()
        {
            var script = new List<object>();
            script.Add(Label);
            if (Value.HasValue)
            {
                script.Add(Value);
            }

            var children = GetChildren();
            if (children.Any())
            {
                script.AddRange(children.Select(child => child.GetScript()));
            }

            if (Units != null)
            {
                script.Add(Units);
            }

            return script;
        }

        private void OnLoad(object sender, RoutedEventArgs args)
        {
            //Logger.Log($"{Label} {string.Join(",", Children.OfType<UIElement>())}");
            foreach (Block child in Children)
            {
                var margin = child.Margin;
                margin.Left += 10;
                child.Margin = margin;
            }
        }

        private IReadOnlyList<Block> GetChildren() => Children.OfType<Block>().ToList();
    }
}