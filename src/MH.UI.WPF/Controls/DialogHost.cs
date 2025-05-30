﻿using MH.UI.Controls;
using MH.Utils.BaseClasses;
using MH.Utils.Extensions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MH.UI.WPF.Controls;

public class DialogHost : ObservableObject {
  public Dialog Content { get; }
  public CustomWindow Window { get; }
  public DataTemplateSelector? DialogTemplateSelector { get; }

  private DialogHost(Dialog content) {
    var owner = _getOwner();

    Content = content;
    
    Window = new() {
      Content = this,
      Owner = owner,
      WindowStartupLocation = WindowStartupLocation.CenterOwner,
      ShowInTaskbar = false,
      CanResize = false,
      SizeToContent = SizeToContent.WidthAndHeight,
      Style = Application.Current.FindResource("MH.S.CustomWindow") as Style
    };

    DialogTemplateSelector = Application.Current.FindResource("MH.S.DialogHost.DialogTemplateSelector") as DataTemplateSelector;

    content.PropertyChanged += (_, e) => {
      if (e.Is(nameof(content.Result)))
        Window.Close();
    };
  }

  public static int Show(Dialog content) {
    var dh = new DialogHost(content);
    dh.Window.ShowDialog();

    return content.Result;
  }

  public static Task<int> ShowAsync(Dialog content) {
    var dh = new DialogHost(content);
    dh.Window.ShowDialog();

    return content.TaskCompletionSource.Task;
  }

  private static Window _getOwner() =>
    Application.Current.Windows[^1]?.Content is DialogHost dh
      ? dh.Window
      : Application.Current.MainWindow!;
}