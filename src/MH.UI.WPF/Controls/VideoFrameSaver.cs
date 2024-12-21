using MH.UI.HelperClasses;
using MH.UI.Interfaces;
using MH.UI.WPF.Extensions;
using MH.Utils;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MH.UI.WPF.Controls;

/* Not visible parts of the VideoFrameSaver are filed with black color.
   But there is no problem with hiding VideoFrameSaver behind other controls. */

public class VideoFrameSaver : MediaElement, IVideoFrameSaver {
  private bool _capture;
  private int _idxVideo;
  private int _idxFrame;
  private long _hash;
  private VfsVideo[]? _videos;
  private VfsVideo? _video;
  private VfsFrame? _frame;
  private readonly Stopwatch _timeOut = new();
  private Action<VfsFrame>? _onSaveAction;
  private Action<VfsFrame, Exception>? _onErrorAction;
  private Action? _onFinishedAction;

  public VideoFrameSaver() {
    LoadedBehavior = MediaState.Manual;
    UnloadedBehavior = MediaState.Close;
    IsMuted = true;
    Stretch = Stretch.Uniform;
    StretchDirection = StretchDirection.Both;
    ScrubbingEnabled = true;
    MediaOpened += delegate { _onMediaOpened(); };
  }

  public void Save(VfsVideo[] videos, Action<VfsFrame>? onSaveAction, Action<VfsFrame, Exception>? onErrorAction, Action? onFinishedAction) {
    _videos = videos;
    _onSaveAction = onSaveAction;
    _onErrorAction = onErrorAction;
    _onFinishedAction = onFinishedAction;
    _idxVideo = -1;
    MaxWidth = ((FrameworkElement)Parent).ActualWidth;
    MaxHeight = ((FrameworkElement)Parent).ActualHeight;
    CompositionTarget.Rendering += _compositionTargetOnRendering;
    _nextVideo();
  }

  private void _nextVideo() {
    if (_idxVideo + 1 > _videos!.Length - 1) {
      CompositionTarget.Rendering -= _compositionTargetOnRendering;
      Source = null;
      _frame = null;
      _video = null;
      _videos = null;
      _onFinishedAction?.Invoke();
      return;
    }

    _video = _videos[++_idxVideo];
    Source = new(_video.FilePath);
    Play();
    Stop();
  }

  private void _onMediaOpened() {
    if (NaturalVideoWidth + NaturalVideoHeight == 0) {
      _nextVideo();
      return;
    }

    _hash = 0;
    _idxFrame = -1;
    _frame = null;
    Width = NaturalVideoWidth;
    Height = NaturalVideoHeight;
    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, _nextFrame);
  }

  private void _nextFrame() {
    if (_idxFrame + 1 > _video!.Frames.Count - 1) {
      _timeOut.Stop();
      _nextVideo();
      return;
    }

    var oldPos = _frame?.Position ?? 0;
    _frame = _video.Frames[++_idxFrame];
    if (_frame.Position == oldPos) {
      _saveFrame();
      _nextFrame();
      return;
    }

    if (_hash == 0) _hash = _getHash();
    Position = new(0, 0, 0, 0, _frame.Position);
    _timeOut.Reset();
    _timeOut.Start();
    _capture = true;
  }

  private void _compositionTargetOnRendering(object? sender, EventArgs e) {
    if (!_capture) return;
    var hash = _getHash();
    if (_timeOut.ElapsedMilliseconds > 2000)
      Log.Error("VideoFrameSaver TimeOut", _frame?.FilePath ?? string.Empty);
    else if (Imaging.CompareHashes(_hash, hash) == 0) return;
    _capture = false;
    _hash = hash;
    _saveFrame();
    _nextFrame();
  }

  private void _saveFrame() {
    if (_frame == null) return;
    try {
      _crop(this.ToBitmap(), _frame).Resize(_frame.Size).SaveAsJpeg(_frame.FilePath, _frame.Quality);
      _onSaveAction?.Invoke(_frame);
    }
    catch (Exception ex) {
      _onErrorAction?.Invoke(_frame, ex);
    }
  }

  private BitmapSource _crop(BitmapSource bmp, VfsFrame frame) {
    var rect = new Int32Rect(frame.X, frame.Y, frame.Width, frame.Height);
    if (!rect.HasArea) return bmp;
    if (ActualWidth >= Width && ActualHeight >= Height) return bmp.Crop(rect);
    rect = _validateRect(bmp, rect.Scale(ActualWidth / Width), frame);
    return bmp.Crop(rect);
  }

  private static Int32Rect _validateRect(BitmapSource bmp, Int32Rect rect, VfsFrame frame) {
    var xDiff = (int)bmp.Width - rect.X - rect.Width;
    var yDiff = (int)bmp.Height - rect.Y - rect.Height;

    if (rect.X >= 0 && rect.Y >= 0 && xDiff >= 0 && yDiff >= 0) return rect;

    if (rect.X < 0) rect.X = 0;
    if (rect.Y < 0) rect.Y = 0;
    if (xDiff < 0) rect.Width += xDiff;
    if (yDiff < 0) rect.Height += yDiff;
    if (rect.Width > rect.Height) rect.Width = rect.Height;
    if (rect.Height > rect.Width) rect.Height = rect.Width;

    Log.Warning(
      "VideoFrameSaver: Segment position corrected",
      $"Segment position was corrected because it was out of video bounds.\n{frame.FilePath}");

    return rect;
  }

  private long _getHash() {
    try {
      return this.ToBitmap().GetAvgHash();
    }
    catch (Exception ex) {
      Log.Error(ex);
      return 0;
    }
  }
}