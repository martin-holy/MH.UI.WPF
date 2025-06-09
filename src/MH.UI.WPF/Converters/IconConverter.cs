using System;
using System.Windows.Data;

namespace MH.UI.WPF.Converters;

public class IconConverter : BaseConverter {
  private static readonly object _lock = new();
  private static IconConverter? _inst;
  public static IconConverter Inst { get { lock (_lock) { return _inst ??= new(); } } }

  public override object? Convert(object? value, object? parameter) =>
    value is string iconName
    ? RelayCommandConverter.GetIcon(iconName)
    : Binding.DoNothing;
}