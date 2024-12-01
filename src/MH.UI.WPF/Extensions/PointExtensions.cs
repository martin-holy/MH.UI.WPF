using MH.Utils.Types;
using System.Windows;

namespace MH.UI.WPF.Extensions;

public static class PointExtensions {
  public static PointD ToPointD(this Point point) => new(point.X, point.Y);
}