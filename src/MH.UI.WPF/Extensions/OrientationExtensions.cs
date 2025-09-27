using System.Windows.Media.Imaging;
using Orientation = MH.Utils.Imaging.Orientation;

namespace MH.UI.WPF.Extensions;

public static class OrientationExtensions {
  public static Rotation ToRotation(this Orientation orientation) =>
    orientation switch {
      Orientation.Rotate90 => Rotation.Rotate270,
      Orientation.Rotate180 => Rotation.Rotate180,
      Orientation.Rotate270 => Rotation.Rotate90,
      _ => Rotation.Rotate0
    };
}