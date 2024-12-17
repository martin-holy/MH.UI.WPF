using System.Windows;
using System.Windows.Input;

namespace MH.UI.WPF.Converters;

public class MouseWheelConverter : BaseConverter {
  private static readonly object _lock = new();
  private static MouseWheelConverter? _inst;
  public static MouseWheelConverter Inst { get { lock (_lock) { return _inst ??= new(); } } }

  public override object? Convert(object? value, object? parameter) {
    if (value is not MouseWheelEventArgs e) return null;

    return new MH.Utils.EventsArgs.MouseWheelEventArgs {
      OriginalSource = e.OriginalSource,
      DataContext = (e.OriginalSource as FrameworkElement)?.DataContext,
      Delta = e.Delta
    };
  }
}