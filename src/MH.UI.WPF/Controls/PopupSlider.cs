using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MH.UI.WPF.Controls;

public class PopupSlider : Slider {
  public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
    nameof(Content), typeof(Button), typeof(PopupSlider));

  public static readonly DependencyProperty PopupClosedCommandProperty = DependencyProperty.Register(
    nameof(PopupClosedCommand), typeof(ICommand), typeof(PopupSlider));

  public Button? Content {
    get => (Button?)GetValue(ContentProperty);
    set => SetValue(ContentProperty, value);
  }

  public ICommand? PopupClosedCommand {
    get => (ICommand?)GetValue(PopupClosedCommandProperty);
    set => SetValue(PopupClosedCommandProperty, value);
  }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();

    if (Content == null || GetTemplateChild("PART_Popup") is not Popup popup) return;

    Content.Click += delegate { popup.IsOpen = true; };

    popup.PreviewMouseUp += delegate {
      popup.IsOpen = false;

      if (PopupClosedCommand?.CanExecute(null) == true)
        PopupClosedCommand.Execute(null);
    };

    popup.CustomPopupPlacementCallback += (size, targetSize, _) => {
      var x = targetSize.Width / 2 - size.Width / 2;
      return [new(new(x, targetSize.Height), PopupPrimaryAxis.Vertical)];
    };
  }
}