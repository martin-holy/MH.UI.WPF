using MH.UI.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MH.UI.WPF.Controls;

public class SlidePanelsGridHost : Control {
  public static readonly DependencyProperty SlidePanelsGridProperty = DependencyProperty.Register(
    nameof(SlidePanelsGrid), typeof(SlidePanelsGrid), typeof(SlidePanelsGridHost));

  public SlidePanelsGrid SlidePanelsGrid {
    get => (SlidePanelsGrid)GetValue(SlidePanelsGridProperty);
    set => SetValue(SlidePanelsGridProperty, value);
  }

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
      SlidePanelsGrid.SetPin(host.SlidePanel);
      host.UpdateAnimation(e);
    };
  }

  private void _onMouseMove(object sender, MouseEventArgs e) {
    var pos = e.GetPosition(this);
    SlidePanelsGrid.OnMouseMove(pos.X, pos.Y, ActualWidth, ActualHeight);
  }
}