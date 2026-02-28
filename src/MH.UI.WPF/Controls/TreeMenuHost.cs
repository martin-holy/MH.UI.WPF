using MH.Utils.Interfaces;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MH.UI.WPF.Controls;

public class TreeMenuHost : Control {
  public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
    nameof(Icon), typeof(string), typeof(TreeMenuHost));

  public static readonly DependencyProperty MenuProperty = DependencyProperty.Register(
    nameof(Menu), typeof(IEnumerable<ITreeItem>), typeof(TreeMenuHost));

  public string Icon {
    get => (string)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }

  public IEnumerable<ITreeItem> Menu {
    get => (IEnumerable<ITreeItem>)GetValue(MenuProperty);
    set => SetValue(MenuProperty, value);
  }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();

    if (GetTemplateChild("PART_RootMenuItem") is MenuItem rootMenuItem)
      rootMenuItem.SubmenuOpened += _onMenuOpened;
  }

  private void _onMenuOpened(object sender, RoutedEventArgs e) {
    (DataContext as MH.Utils.BaseClasses.ObservableObject)?.OnPropertyChanged("Menu");
  }
}