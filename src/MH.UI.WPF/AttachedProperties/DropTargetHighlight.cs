using System.Windows;

namespace MH.UI.WPF.AttachedProperties;

public static class DropTargetHighlight {
  public static readonly DependencyProperty TrackProperty = DependencyProperty.RegisterAttached(
    "Track", typeof(bool), typeof(DropTargetHighlight), new PropertyMetadata(false, _onTrackChanged));

  public static readonly DependencyProperty IsDragOverProperty = DependencyProperty.RegisterAttached(
    "IsDragOver", typeof(bool), typeof(DropTargetHighlight), new PropertyMetadata(false));

  public static void SetTrack(DependencyObject element, bool value) => element.SetValue(TrackProperty, value);
  public static bool GetTrack(DependencyObject element) => (bool)element.GetValue(TrackProperty);
  public static bool GetIsDragOver(DependencyObject element) => (bool)element.GetValue(IsDragOverProperty);

  private static void _onTrackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (d is not UIElement element) return;

    if ((bool)e.NewValue) {
      element.DragEnter += _onDragEnter;
      element.DragLeave += _onDragLeave;
      element.Drop += _onDrop;
    }
    else {
      element.DragEnter -= _onDragEnter;
      element.DragLeave -= _onDragLeave;
      element.Drop -= _onDrop;
      element.SetValue(IsDragOverProperty, false);
    }
  }

  private static void _onDragEnter(object sender, DragEventArgs e) =>
    ((UIElement)sender).SetValue(IsDragOverProperty, true);

  private static void _onDragLeave(object sender, DragEventArgs e) =>
    ((UIElement)sender).SetValue(IsDragOverProperty, false);

  private static void _onDrop(object sender, DragEventArgs e) =>
    ((UIElement)sender).SetValue(IsDragOverProperty, false);
}