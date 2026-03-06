using MH.Utils.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MH.UI.WPF.Controls;

public class TreeMenuHost : Control {
  public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
    nameof(Icon), typeof(string), typeof(TreeMenuHost));

  private static readonly DependencyPropertyKey MenuPropertyKey = DependencyProperty.RegisterReadOnly(
    nameof(Menu), typeof(IEnumerable<ITreeItem>), typeof(TreeMenuHost),
    new PropertyMetadata(new ITreeItem[] { new MH.Utils.BaseClasses.TreeItem() }));

  public static readonly DependencyProperty MenuProperty = MenuPropertyKey.DependencyProperty;

  public static readonly DependencyProperty MenuFactoryProperty = DependencyProperty.Register(
    nameof(MenuFactory), typeof(Func<IEnumerable<ITreeItem>>), typeof(TreeMenuHost));

  public string Icon {
    get => (string)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }

  public IEnumerable<ITreeItem>? Menu {
    get => (IEnumerable<ITreeItem>?)GetValue(MenuProperty);
    private set => SetValue(MenuPropertyKey, value);
  }

  public Func<IEnumerable<ITreeItem>>? MenuFactory {
    get => (Func<IEnumerable<ITreeItem>>?)GetValue(MenuFactoryProperty);
    set => SetValue(MenuFactoryProperty, value);
  }

  private MenuItem? _rootMenuItem;

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();

    if (_rootMenuItem != null) _rootMenuItem.SubmenuOpened -= _onMenuOpened;
    _rootMenuItem = GetTemplateChild("PART_RootMenuItem") as MenuItem;
    if (_rootMenuItem != null) _rootMenuItem.SubmenuOpened += _onMenuOpened;
  }

  private void _onMenuOpened(object sender, RoutedEventArgs e) {
    if (sender != e.OriginalSource) return;
    if (MenuFactory != null) Menu = MenuFactory();
  }
}