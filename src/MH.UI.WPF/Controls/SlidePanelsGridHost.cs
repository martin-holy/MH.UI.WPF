using MH.UI.Controls;
using MH.UI.WPF.Extensions;
using MH.Utils.Types;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MH.UI.WPF.Controls;

public class SlidePanelsGridHost : Control, ISlidePanelsGridHost {
  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
    nameof(ViewModel), typeof(SlidePanelsGrid), typeof(SlidePanelsGridHost), new(_viewModelChanged));

  public SlidePanelsGrid? ViewModel {
    get => (SlidePanelsGrid?)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  private static void _viewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (d is not SlidePanelsGridHost host || host.ViewModel == null) return;
    host.ViewModel.Host = host;
  }

  public event EventHandler<(PointD Position, double Width, double Height)>? HostMouseMoveEvent;

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();
    MouseMove += _onMouseMove;
    _initPanel(GetTemplateChild("PART_LeftPanel") as SlidePanelHost);
    _initPanel(GetTemplateChild("PART_TopPanel") as SlidePanelHost);
    _initPanel(GetTemplateChild("PART_RightPanel") as SlidePanelHost);
    _initPanel(GetTemplateChild("PART_BottomPanel") as SlidePanelHost);
  }

  private void _initPanel(SlidePanelHost? host) {
    if (host == null) return;
    host.SizeChanged += (_, e) => {
      ViewModel?.SetPin(host.SlidePanel);
      host.UpdateAnimation(e);
    };
  }

  private void _onMouseMove(object sender, MouseEventArgs e) =>
    HostMouseMoveEvent?.Invoke(this, new(e.GetPosition(this).ToPointD(), ActualWidth, ActualHeight));
}