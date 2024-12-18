using MH.UI.Controls;
using MH.UI.WPF.Extensions;
using MH.Utils.Types;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

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

  public event EventHandler<MH.Utils.EventsArgs.SizeChangedEventArgs>? HostSizeChangedEvent;

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();
    Storyboard.SetTargetProperty(_openAnimation, new(MarginProperty));
    Storyboard.SetTargetProperty(_closeAnimation, new(MarginProperty));
    _sbOpen.Children.Add(_openAnimation);
    _sbClose.Children.Add(_closeAnimation);
    SizeChanged += _onSizeChanged;
  }

  private void _onSizeChanged(object sender, SizeChangedEventArgs e) =>
    HostSizeChangedEvent?.Invoke(this, new(e.PreviousSize.ToSizeD(), e.NewSize.ToSizeD(), e.WidthChanged, e.HeightChanged));

  public void OpenAnimation() => _sbOpen.Begin(this);

  public void CloseAnimation() => _sbClose.Begin(this);

  public void UpdateOpenAnimation(ThicknessD from, ThicknessD to, TimeSpan duration) {
    _openAnimation.From = from.FromThicknessD();
    _openAnimation.To = to.FromThicknessD();
    _openAnimation.Duration = new(duration);
  }

  public void UpdateCloseAnimation(ThicknessD from, ThicknessD to, TimeSpan duration) {
    _closeAnimation.From = from.FromThicknessD();
    _closeAnimation.To = to.FromThicknessD();
    _closeAnimation.Duration = new(duration);
  }
}