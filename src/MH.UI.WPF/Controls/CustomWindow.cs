﻿using MH.Utils.BaseClasses;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace MH.UI.WPF.Controls;

public class CustomWindow : Window {
  public static readonly DependencyProperty CanResizeProperty = DependencyProperty.Register(
    nameof(CanResize), typeof(bool), typeof(CustomWindow));

  public static readonly DependencyProperty CanFullScreenProperty = DependencyProperty.Register(
    nameof(CanFullScreen), typeof(bool), typeof(CustomWindow));

  public static readonly DependencyProperty IsFullScreenProperty = DependencyProperty.Register(
    nameof(IsFullScreen), typeof(bool), typeof(CustomWindow),
    new((o, _) => (o as CustomWindow)?._onIsFullScreenChanged()));

  public static readonly DependencyProperty IsDragAreaForProperty = DependencyProperty.RegisterAttached(
    "IsDragAreaFor", typeof(Window), typeof(CustomWindow), new(_onIsDragAreaChanged));

  public bool CanResize { get => (bool)GetValue(CanResizeProperty); set => SetValue(CanResizeProperty, value); }
  public bool CanFullScreen { get => (bool)GetValue(CanFullScreenProperty); set => SetValue(CanFullScreenProperty, value); }
  public bool IsFullScreen { get => (bool)GetValue(IsFullScreenProperty); set => SetValue(IsFullScreenProperty, value); }

  public static Window GetIsDragAreaFor(DependencyObject d) => (Window)d.GetValue(IsDragAreaForProperty);
  public static void SetIsDragAreaFor(DependencyObject d, Window value) => d.SetValue(IsDragAreaForProperty, value);

  private const int _resizeCornerSize = 10;
  private const int _resizeBorderSize = 4;
  private const int _wmSysCommand = 0x112;

  private enum ResizeDirection {
    None = 0,
    Left = 61441,
    Right = 61442,
    Top = 61443,
    TopLeft = 61444,
    TopRight = 61445,
    Bottom = 61446,
    BottomLeft = 61447,
    BottomRight = 61448,
  }

  [DllImport("user32.dll", CharSet = CharSet.Auto)]
  private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

  public static RelayCommand<Window> MinimizeWindowCommand { get; } = new(
    x => x!.WindowState = WindowState.Minimized, x => x != null);

  public static RelayCommand<Window> MaximizeWindowCommand { get; } = new(
    x => {
      x!.MaxHeight = double.PositiveInfinity;
      x.WindowState = WindowState.Maximized;
    }, x => x != null);

  public static RelayCommand<Window> RestoreWindowCommand { get; } = new(
    x => x!.WindowState = WindowState.Normal, x => x != null);

  public static RelayCommand<Window> CloseWindowCommand { get; } = new(
    x => x!.Close(), x => x != null);

  public static RelayCommand<CustomWindow> ToggleFullScreenCommand { get; } = new(
    x => x!.IsFullScreen = !x.IsFullScreen, x => x != null);

  public CustomWindow() {
    StateChanged += delegate { _onStateChanged(); };
    Loaded += delegate {
      if (WindowState != WindowState.Maximized) return;
      var isFullScreen = IsFullScreen;
      WindowState = WindowState.Normal;
      if (isFullScreen) IsFullScreen = true;
      WindowState = WindowState.Maximized;
    };
  }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();
    if (GetTemplateChild("PART_ResizeBorder") is not Border border) return;
    border.MouseEnter += delegate { _setCursor(); };
    border.MouseLeave += delegate { _resetCursor(); };
    border.PreviewMouseLeftButtonDown += delegate { _resize(); };
  }

  private void _onStateChanged() {
    if (WindowState == WindowState.Normal && IsFullScreen)
      IsFullScreen = false;

    if (WindowState == WindowState.Maximized && !IsFullScreen)
      MaxHeight = SystemParameters.WorkArea.Height;
  }

  private void _onIsFullScreenChanged() {
    MaxHeight = IsFullScreen
      ? double.PositiveInfinity
      : SystemParameters.WorkArea.Height;

    if (IsFullScreen && WindowState != WindowState.Maximized)
      WindowState = WindowState.Maximized;
  }

  private static void _onIsDragAreaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (d is FrameworkElement fe && e.NewValue is Window window)
      fe.PreviewMouseLeftButtonDown += delegate { window.DragMove(); };
  }

  private void _setCursor() =>
    Cursor = _resizeDirectionToCursor(_getResizeDirection());

  private void _resetCursor() {
    if (Mouse.LeftButton != MouseButtonState.Pressed)
      Cursor = Cursors.Arrow;
  }

  private void _resize() {
    if (PresentationSource.FromVisual(this) is not HwndSource hwndSource) return;
    var direction = _getResizeDirection();
    Cursor = _resizeDirectionToCursor(direction);
    SendMessage(hwndSource.Handle, _wmSysCommand, (IntPtr)direction, IntPtr.Zero);
  }

  private ResizeDirection _getResizeDirection() {
    var pos = Mouse.GetPosition(this);

    if (pos.X > _resizeCornerSize && pos.X < ActualWidth - _resizeCornerSize) {
      if (pos.Y < _resizeBorderSize) return ResizeDirection.Top;
      if (pos.Y > ActualHeight - _resizeBorderSize) return ResizeDirection.Bottom;
    }
    else if (pos.Y > _resizeCornerSize && pos.Y < ActualHeight - _resizeCornerSize) {
      if (pos.X < _resizeBorderSize) return ResizeDirection.Left;
      if (pos.X > ActualWidth - _resizeBorderSize) return ResizeDirection.Right;
    }
    else if (pos.X < _resizeCornerSize) {
      if (pos.Y < _resizeCornerSize) return ResizeDirection.TopLeft;
      if (pos.Y > ActualHeight - _resizeCornerSize) return ResizeDirection.BottomLeft;
    }
    else if (pos.X > ActualWidth - _resizeCornerSize) {
      if (pos.Y < _resizeCornerSize) return ResizeDirection.TopRight;
      if (pos.Y > ActualHeight - _resizeCornerSize) return ResizeDirection.BottomRight;
    }

    return ResizeDirection.None;
  }

  private static Cursor _resizeDirectionToCursor(ResizeDirection direction) =>
    direction switch {
      ResizeDirection.Left => Cursors.SizeWE,
      ResizeDirection.Right => Cursors.SizeWE,
      ResizeDirection.Top => Cursors.SizeNS,
      ResizeDirection.TopLeft => Cursors.SizeNWSE,
      ResizeDirection.TopRight => Cursors.SizeNESW,
      ResizeDirection.Bottom => Cursors.SizeNS,
      ResizeDirection.BottomLeft => Cursors.SizeNESW,
      ResizeDirection.BottomRight => Cursors.SizeNWSE,
      ResizeDirection.None => Cursors.Arrow,
      _ => Cursors.Arrow,
    };
}