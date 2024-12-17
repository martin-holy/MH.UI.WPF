using MH.Utils.Types;
using System.Windows;

namespace MH.UI.WPF.Extensions;

public static class SizeExtensions {
  public static SizeD ToSizeD(this Size size) => new(size.Width, size.Height);
}