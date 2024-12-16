using MH.UI.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Dock = MH.UI.Controls.Dock;

namespace MH.UI.WPF.Controls;

public class SlidePanelHost : Control, ISlidePanelHost {
  private readonly ThicknessAnimation _openAnimation = new();
  private readonly ThicknessAnimation _closeAnimation = new();
  private readonly Storyboard _sbOpen = new();
  private readonly Storyboard _sbClose = new();

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
    nameof(ViewModel), typeof(SlidePanel), typeof(SlidePanelHost), new(_onViewModelChanged));

  public SlidePanel? ViewModel {
    get => (SlidePanel?)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  private static void _onViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (d is not SlidePanelHost host || host.ViewModel == null) return;
    host.ViewModel.Host = host;
  }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();
    Storyboard.SetTargetProperty(_openAnimation, new(MarginProperty));
    Storyboard.SetTargetProperty(_closeAnimation, new(MarginProperty));
    _sbOpen.Children.Add(_openAnimation);
    _sbClose.Children.Add(_closeAnimation);
  }

  public void OpenAnimation() => _sbOpen.Begin(this);

  public void CloseAnimation() => _sbClose.Begin(this);

  public void UpdateAnimation(SizeChangedEventArgs e) {
    if (ViewModel == null ||
        (ViewModel.Dock is Dock.Top or Dock.Bottom && !e.HeightChanged) ||
        (ViewModel.Dock is Dock.Left or Dock.Right && !e.WidthChanged))
      return;

    var size = ViewModel.Size * -1;
    var duration = new Duration(TimeSpan.FromMilliseconds(size * -1 * 0.7));
    var openFrom = new Thickness(0);
    var openTo = new Thickness(0);
    var closeFrom = new Thickness(0);
    var closeTo = new Thickness(0);

    switch (ViewModel.Dock) {
      case Dock.Left: openFrom.Left = size; closeTo.Left = size; break;
      case Dock.Top: openFrom.Top = size; closeTo.Top = size; break;
      case Dock.Right: openFrom.Right = size; closeTo.Right = size; break;
      case Dock.Bottom: openFrom.Bottom = size; closeTo.Bottom = size; break;
      default: throw new ArgumentOutOfRangeException();
    }

    _openAnimation.Duration = duration;
    _openAnimation.From = openFrom;
    _openAnimation.To = openTo;
    _closeAnimation.Duration = duration;
    _closeAnimation.From = closeFrom;
    _closeAnimation.To = closeTo;

    if (!ViewModel.IsOpen) _sbClose.Begin(this);
  }
}