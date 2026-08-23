using BenchmarkDotNet.Attributes;
using MH.Utils.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;

namespace MH.UI.WPF.Benchmarks;

[ShortRunJob]
[MemoryDiagnoser]
public class ReadMetadataBenchmarks {
  private string _path = null!;

  [GlobalSetup]
  public void Setup() {
    _path = @"c:\Programs\-=Graphics\ExifTool\a_comment.jpg";
  }

  /*| Method | Mean     | Error     | StdDev    | Gen0     | Gen1     | Gen2     | Allocated |
    |------- |---------:|----------:|----------:|---------:|---------:|---------:|----------:|
    | Read   | 3.413 ms | 0.4478 ms | 0.0245 ms | 968.7500 | 968.7500 | 968.7500 |   5.94 KB |*/
  [Benchmark]
  public void Read() {
    var mim = new MediaItemMetadata(_path);
    _readMetadata(mim);
  }

  private const string _msRegionInfo = "/xmp/MP:RegionInfo";
  private const string _msRegions = "/xmp/MP:RegionInfo/MPRI:Regions";
  private const string _msPersonName = "/MPReg:PersonDisplayName";
  private const string _msPersonRectangle = "/MPReg:Rectangle";
  private const string _msPersonRectangleKeywords = "/MPReg:RectangleKeywords";

  private static void _readMetadata(MediaItemMetadata mim, bool gpsOnly = false) {
    mim.Success = false;
    try {
      if (mim.FilePath != null) {
        using Stream srcFileStream = File.Open(mim.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var decoder = BitmapDecoder.Create(srcFileStream, BitmapCreateOptions.None, BitmapCacheOption.None);
        var frame = decoder.Frames[0];

        mim.Width = frame.PixelWidth;
        mim.Height = frame.PixelHeight;

        // true because only media item dimensions are required
        if (frame.Metadata is not BitmapMetadata bm) {
          mim.Success = true;
          return;
        }

        _readImageMetadata(mim, bm, gpsOnly);
        mim.Success = true;
      }
    }
    catch (Exception ex) {
      
    }
  }

  private static void _readImageMetadata(MediaItemMetadata mim, BitmapMetadata bm, bool gpsOnly) {
    try {
      mim.Lat = _getGps(bm, "System.GPS.Latitude.Proxy");
      mim.Lng = _getGps(bm, "System.GPS.Longitude.Proxy");
      var geoNameId = bm.GetQuery<string>("/xmp/GeoNames:GeoNameId");
      mim.GeoNameId = string.IsNullOrEmpty(geoNameId) ? null : int.Parse(geoNameId);
      if (gpsOnly) return;

      mim.PeopleSegmentsKeywords = _readPeopleSegmentsKeywords(bm);
      mim.Rating = bm.Rating;
      mim.Comment = StringUtils.NormalizeComment(bm.Comment);
      // Orientation 1: 0, 3: 180, 6: 270, 8: 90
      mim.Orientation = (Orientation)bm.GetQuery<ushort>("System.Photo.Orientation", 1);
      mim.Keywords = bm.Keywords?.ToArray();
    }
    catch (Exception) {
      // ignored
    }
  }

  private static double? _getGps(BitmapMetadata bm, string query) {
    var val = bm.GetQuery<string>(query);
    if (val == null) return null;
    var vals = val[..^1].Split(',');

    return (int.Parse(vals[0]) + double.Parse(vals[1], CultureInfo.InvariantCulture) / 60)
           * (val.EndsWith("S") || val.EndsWith("W") ? -1 : 1);
  }

  private static List<Tuple<string, List<Tuple<string, string[]?>>>>? _readPeopleSegmentsKeywords(BitmapMetadata bm) {
    if (bm.GetQuery<BitmapMetadata>(_msRegions) is not { } regions) return null;
    var output = new List<Tuple<string, List<Tuple<string, string[]?>>>>();

    foreach (var r in regions.Select(x => _msRegions + x)) {
      var name = bm.GetQuery<string>(r + _msPersonName)!;

      if (output.SingleOrDefault(x => string.Equals(x.Item1, name, StringComparison.OrdinalIgnoreCase)) is not { } person) {
        person = new(name, []);
        output.Add(person);
      }

      if (bm.GetQuery<string>(r + _msPersonRectangle) is not { } rect) continue;

      var keywords = bm.GetQuery<BitmapMetadata>(r + _msPersonRectangleKeywords);
      person.Item2.Add(new(rect, keywords?
        .Select(x => keywords.GetQuery<string>(x))
        .Where(x => x != null)
        .Select(x => x!)
        .ToArray()));
    }

    return output;
  }

  private class MediaItemMetadata(string filePaht) {
    public string FilePath { get; set; } = filePaht;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Orientation Orientation { get; set; }
    public bool Success { get; set; }
    public string[]? Keywords { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public int? GeoNameId { get; set; }
    public List<Tuple<string, List<Tuple<string, string[]?>>>>? PeopleSegmentsKeywords { get; set; }
  }
}

public static class StringUtils {
  private static readonly HashSet<char> _commentAllowedCharacters = new("@#$€_&+-()*'.:;!?=<>% ");

  public static string? NormalizeComment(string? comment) =>
    string.IsNullOrEmpty(comment)
      ? null
      : new string(comment.Where(x => char.IsLetterOrDigit(x) || _commentAllowedCharacters.Contains(x)).ToArray());
}

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