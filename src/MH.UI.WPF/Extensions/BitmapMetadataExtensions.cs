using System.Windows.Media.Imaging;
using System;

namespace MH.UI.WPF.Extensions;

public static class BitmapMetadataExtensions {
  public static T? GetQuery<T>(this BitmapMetadata bm, string query, T? value = default) {
    try {
      if (bm.GetQuery(query) is T t) return t;
      return value;
    }
    catch (Exception) {
      return value;
    }
  }

  public static void SetIfContainsQuery(this BitmapMetadata bm, string query, object value) {
    if (bm.ContainsQuery(query))
      bm.SetQuery(query, value);
  }
}