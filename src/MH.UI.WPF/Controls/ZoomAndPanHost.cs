using MH.UI.Controls;
using MH.UI.WPF.Extensions;
using MH.Utils.Types;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MH.UI.WPF.Controls;

public class ZoomAndPanHost : ContentControl, IZoomAndPanHost {
  private Canvas _canvas = null!;
  private UIElement _content = null!;
  private TranslateTransform _contentTransform = null!;

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
    nameof(ViewModel), typeof(ZoomAndPan), typeof(ZoomAndPanHost), new(_onViewModelChanged));

  public ZoomAndPan? ViewModel {
    get => (ZoomAndPan?)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  double IZoomAndPanHost.Width => ActualWidth;
  double IZoomAndPanHost.Height => ActualHeight;

  public event EventHandler? HostSizeChangedEvent;
  public event EventHandler<PointD>? HostMouseMoveEvent;
  public event EventHandler<(PointD, PointD)>? HostMouseDownEvent;
  public event EventHandler? HostMouseUpEvent;
  public event EventHandler<(int, PointD)>? HostMouseWheelEvent;

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();

    _canvas = (Canvas)GetTemplateChild("PART_Canvas")!;
    _canvas.SizeChanged += _onCanvasSizeChanged;
    _canvas.MouseMove += _onCanvasMouseMove;

    _content = (UIElement)GetTemplateChild("PART_Content")!;
    _content.MouseLeftButtonDown += _onContentMouseLeftButtonDown;
    _content.MouseLeftButtonUp += _onContentMouseLeftButtonUp;
    _content.MouseWheel += _onContentMouseWheel;

    _contentTransform = (TranslateTransform)GetTemplateChild("PART_ContentTransform")!;
  }

  public void StartAnimation(double toValue, double duration, bool horizontal, Action onCompleted) {
    var animation = new DoubleAnimation(0, toValue, TimeSpan.FromMilliseconds(duration), FillBehavior.Stop);
    animation.Completed += (_, _) => onCompleted();
    _contentTransform.BeginAnimation(horizontal
      ? TranslateTransform.XProperty
      : TranslateTransform.YProperty, animation);
  }

  public void StopAnimation() =>
    _contentTransform.BeginAnimation(TranslateTransform.XProperty, null);

  private void _onCanvasSizeChanged(object sender, SizeChangedEventArgs e) =>
    HostSizeChangedEvent?.Invoke(this, EventArgs.Empty);

  private void _onCanvasMouseMove(object sender, MouseEventArgs e) {
    if (!_content.IsMouseCaptured) return;
    HostMouseMoveEvent?.Invoke(this, e.GetPosition(_canvas).ToPointD());
  }

  private void _onContentMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
    HostMouseDownEvent?.Invoke(this, new(e.GetPosition(_canvas).ToPointD(), e.GetPosition(_content).ToPointD()));
    _canvas.Cursor = Cursors.Hand;
    _content.CaptureMouse();
  }

  private void _onContentMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
    _canvas.Cursor = Cursors.Arrow;
    _content.ReleaseMouseCapture();
    HostMouseUpEvent?.Invoke(this, EventArgs.Empty);
  }

  private void _onContentMouseWheel(object sender, MouseWheelEventArgs e) =>
    HostMouseWheelEvent?.Invoke(this, (e.Delta, e.GetPosition(_content).ToPointD()));

  private static void _onViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (d is not ZoomAndPanHost host || host.ViewModel == null) return;
    host.ViewModel.Host = host;
  }
}