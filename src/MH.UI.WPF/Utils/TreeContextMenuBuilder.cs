using MH.UI.WPF.Converters;
using MH.Utils.Tree;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MenuModel = MH.Utils.BaseClasses.MenuItem;
using MenuSeparatorModel = MH.Utils.BaseClasses.MenuItemSeparator;
using WpfMenuItem = System.Windows.Controls.MenuItem;

namespace MH.UI.WPF.Utils;

public static class TreeContextMenuBuilder {
  public static ContextMenu? Create(IEnumerable<ITreeItem> entries) {
    var contextMenu = new ContextMenu();
    var hasItems = false;

    foreach (var entry in entries) {
      contextMenu.Items.Add(_createEntry(entry));
      hasItems = true;
    }

    return hasItems ? contextMenu : null;
  }

  private static UIElement _createEntry(ITreeItem item) =>
    item switch {
      MenuSeparatorModel => new Separator(),
      MenuModel menuItem => _createMenuItem(menuItem),
      _ => throw new NotSupportedException(
        $"Unsupported context-menu item type: {item.GetType().FullName}")
    };

  private static WpfMenuItem _createMenuItem(MenuModel model) {
    var result = new WpfMenuItem {
      Header = model.Text,
      Command = model.Command,
      CommandParameter = model.CommandParameter,
      InputGestureText = model.InputGestureText,
      Visibility = model.IsHidden ? Visibility.Collapsed : Visibility.Visible,
      Icon = RelayCommandConverter.GetIcon(model.Icon)
    };

    foreach (var child in model.Items)
      result.Items.Add(_createEntry(child));

    return result;
  }
}