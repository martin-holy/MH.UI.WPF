using MH.UI.Controls;
using MH.UI.WPF.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MH.UI.WPF.Controls;

public class ZoomAndPanHost : ContentControl, IZoomAndPanHost {
  private Canvas _canvas = null!;
  private TranslateTransform _contentTransform = null!;
  private DependencyProperty? _animatedProperty;

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
    nameof(ViewModel), typeof(ZoomAndPan), typeof(ZoomAndPanHost), new(_onViewModelChanged));

  public ZoomAndPan? ViewModel {
    get => (ZoomAndPan?)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();

    _canvas = (Canvas)GetTemplateChild("PART_Canvas")!;
    _canvas.SizeChanged += _onSizeChanged;
    _canvas.MouseMove += _onMouseMove;
    _canvas.MouseWheel += _onMouseWheel;
    _canvas.MouseLeftButtonDown += _onMouseLeftButtonDown;
    _canvas.MouseLeftButtonUp += _onMouseLeftButtonUp;
    _canvas.LostMouseCapture += (_, _) => _canvas.Cursor = Cursors.Arrow;

    _contentTransform = (TranslateTransform)GetTemplateChild("PART_ContentTransform")!;
  }

  public void StartAnimation(double toValue, double duration, bool horizontal, Action onCompleted) {
    _animatedProperty = horizontal ? TranslateTransform.XProperty : TranslateTransform.YProperty;
    var animation = new DoubleAnimation(0, toValue, TimeSpan.FromMilliseconds(duration), FillBehavior.Stop);
    animation.Completed += (_, _) => onCompleted();
    _contentTransform.BeginAnimation(_animatedProperty, animation);
  }

  public void StopAnimation() {
    if (_animatedProperty != null)
      _contentTransform.BeginAnimation(_animatedProperty, null);
  }

  private void _onSizeChanged(object sender, SizeChangedEventArgs e) =>
    ViewModel?.SetHostSize(ActualWidth, ActualHeight);

  private void _onMouseMove(object sender, MouseEventArgs e) {
    if (!_canvas.IsMouseCaptured) return;
    ViewModel?.PointerMove(e.GetPosition(_canvas).ToPointD());
  }

  private void _onMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
    var hostPos = e.GetPosition(_canvas).ToPointD();

    ViewModel?.ZoomTo100(hostPos);
    ViewModel?.PointerDown(hostPos);
    _canvas.Cursor = Cursors.Hand;
    _canvas.CaptureMouse();
  }

  private void _onMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
    _canvas.ReleaseMouseCapture();
    ViewModel?.PointerUp();
  }

  private void _onMouseWheel(object sender, MouseWheelEventArgs e) {
    if (!MH.Utils.Keyboard.IsCtrlOn()) return;
    ViewModel?.Zoom(e.Delta > 0 ? 1.2 : 1 / 1.2, e.GetPosition(_canvas).ToPointD());
  }

  private static void _onViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (d is not ZoomAndPanHost host) return;
    if (e.OldValue is ZoomAndPan oldVm) oldVm.Host = null;
    if (e.NewValue is ZoomAndPan newVm) newVm.Host = host;
  }
}