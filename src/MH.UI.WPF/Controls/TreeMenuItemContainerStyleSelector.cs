using MH.Utils.BaseClasses;
using System.Windows;
using System.Windows.Controls;

namespace MH.UI.WPF.Controls;

public class TreeMenuItemContainerStyleSelector : StyleSelector {
  public Style? MenuItemStyle { get; set; }
  public Style? SeparatorStyle { get; set; }

  public override Style SelectStyle(object item, DependencyObject container) =>
    item is MenuItemSeparator ? SeparatorStyle! : MenuItemStyle!;
}