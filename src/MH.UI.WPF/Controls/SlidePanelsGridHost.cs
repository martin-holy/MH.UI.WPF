using MH.UI.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MH.UI.WPF.Controls;

public class SlidePanelsGridHost : Control {
  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
    nameof(ViewModel), typeof(SlidePanelsGrid), typeof(SlidePanelsGridHost));

  public SlidePanelsGrid? ViewModel {
    get => (SlidePanelsGrid?)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  public SlidePanelsGridHost() {
    MouseMove += _onMouseMove;
  }

  private void _onMouseMove(object sender, MouseEventArgs e) {
    if (ViewModel == null) return;
    var p = e.GetPosition(this);
    ViewModel.OnMouseMove(p.X, p.Y, ActualWidth, ActualHeight);
  }
}