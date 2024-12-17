using System.Windows;

namespace MH.UI.WPF.Converters;

public class PropertyChangedConverter : BaseConverter {
  private static readonly object _lock = new();
  private static PropertyChangedConverter? _inst;
  public static PropertyChangedConverter Inst { get { lock (_lock) { return _inst ??= new(); } } }

  public override object? Convert(object? value, object? parameter) {
    if (value is not RoutedPropertyChangedEventArgs<double> e) return null;

    return new MH.Utils.EventsArgs.PropertyChangedEventArgs<double>(e.OldValue, e.NewValue) {
      OriginalSource = e.OriginalSource,
      DataContext = (e.OriginalSource as FrameworkElement)?.DataContext
    };
  }
}