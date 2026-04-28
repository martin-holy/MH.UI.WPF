using MH.UI.Controls;
using MH.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Dock = MH.UI.Controls.Dock;

namespace MH.UI.WPF.Controls;

public class SlidePanelHost : ContentControl {
  private readonly TranslateTransform _translate = new();
  private IDisposable? _panelIsOpenBinding;

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
    nameof(ViewModel), typeof(SlidePanel), typeof(SlidePanelHost), new(_onViewModelChanged));

  public SlidePanel? ViewModel {
    get => (SlidePanel?)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  public SlidePanelHost() {
    RenderTransform = _translate;
    SizeChanged += _onSizeChanged;
  }

  private static void _onViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
    (d as SlidePanelHost)?._bindToIsOpen();

  private void _bindToIsOpen() {
    _panelIsOpenBinding?.Dispose();
    if (ViewModel == null) return;
    _panelIsOpenBinding = ViewModel?.Bind(nameof(SlidePanel.IsOpen), x => x.IsOpen, _ => _refreshPosition(true), false);
  }

  private void _onSizeChanged(object sender, SizeChangedEventArgs e) =>
    _refreshPosition(false);

  private void _refreshPosition(bool animated) {
    if (ViewModel == null) return;

    double targetX = 0;
    double targetY = 0;

    if (!ViewModel.IsOpen) {
      switch (ViewModel.Dock) {
        case Dock.Left: targetX = -ActualWidth; break;
        case Dock.Right: targetX = ActualWidth; break;
        case Dock.Top: targetY = -ActualHeight; break;
        case Dock.Bottom: targetY = ActualHeight; break;
      }
    }

    if (animated)
      _animateTo(targetX, targetY);
    else
      _setTransform(targetX, targetY);
  }

  private void _animateTo(double x, double y) {
    var distance = Math.Max(Math.Abs(_translate.X - x), Math.Abs(_translate.Y - y));
    var ms = Math.Max(120, (int)(distance * 0.7));
    var animX = new DoubleAnimation(x, TimeSpan.FromMilliseconds(ms), FillBehavior.HoldEnd);
    var animY = new DoubleAnimation(y, TimeSpan.FromMilliseconds(ms), FillBehavior.HoldEnd);

    _translate.BeginAnimation(TranslateTransform.XProperty, animX);
    _translate.BeginAnimation(TranslateTransform.YProperty, animY);
  }

  private void _setTransform(double x, double y) {
    _translate.BeginAnimation(TranslateTransform.XProperty, null);
    _translate.BeginAnimation(TranslateTransform.YProperty, null);
    _translate.X = x;
    _translate.Y = y;
  }
}