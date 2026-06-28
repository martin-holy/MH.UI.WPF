using MH.Utils.BaseClasses;
using System.Windows;

namespace MH.UI.WPF.Utils;

public class FlatTreeItemTemplateSelector : TypeDataTemplateSelector {
  public override DataTemplate? SelectTemplate(object? item, DependencyObject container) {
    if (item is FlatTreeItem flatTreeItem)
      return base.SelectTemplate(flatTreeItem.TreeItem, container);

    return base.SelectTemplate(item, container);
  }
}