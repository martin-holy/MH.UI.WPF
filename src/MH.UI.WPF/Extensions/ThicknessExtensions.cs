using MH.Utils.Types;
using System.Windows;

namespace MH.UI.WPF.Extensions;

public static class ThicknessExtensions {
  public static Thickness FromThicknessD(this ThicknessD thickness) =>
    new(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
}