using System;
using System.Windows;
using System.Windows.Controls;

namespace Blockcode
{
    public class ExtendedUserControl : UserControl
    {
        protected static DependencyProperty RegProp(string name, Type ownerType) =>
            DependencyProperty.Register(name, ownerType.GetProperty(name).PropertyType, ownerType);
    }
}