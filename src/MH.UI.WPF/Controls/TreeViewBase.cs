using MH.UI.Interfaces;
using MH.UI.WPF.Extensions;
using MH.Utils;
using MH.Utils.BaseClasses;
using MH.Utils.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MH.UI.WPF.Controls;

public class TreeViewBase : TreeView {
  private bool _isScrollingTo;
  private bool _resetHScroll;
  private double _resetHOffset;
  private ScrollViewer _sv = null!;

  public static readonly DependencyProperty TreeViewProperty = DependencyProperty.Register(
    nameof(TreeView), typeof(ITreeView), typeof(TreeViewBase));

  public ITreeView? TreeView {
    get => (ITreeView?)GetValue(TreeViewProperty);
    set => SetValue(TreeViewProperty, value);
  }

  public RelayCommand<RequestBringIntoViewEventArgs> TreeItemIntoViewCommand { get; }

  public TreeViewBase() {
    TreeItemIntoViewCommand = new(_onTreeItemIntoView);
  }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();

    _sv = (ScrollViewer)Template.FindName("PART_ScrollViewer", this);
    _sv.IsVisibleChanged += delegate { _setIsVisible(); };
    _sv.ScrollChanged += _onScrollChanged;

    _setIsVisible();
    _setItemsSource();
  }

  private void _setIsVisible() {
    if (TreeView != null) TreeView.IsVisible = _sv.IsVisible;
  }

  private void _onTreeItemIntoView(RequestBringIntoViewEventArgs? e) {
    _resetHScroll = true;
    _resetHOffset = _sv.HorizontalOffset;
  }

  private void _onScrollChanged(object sender, ScrollChangedEventArgs e) {
    if (!_isScrollingTo && TreeView != null && _sv.IsVisible)
      TreeView.TopTreeItem = _getHitTestItem(10, 10);

    if (_resetHScroll) {
      _resetHScroll = false;
      _sv.ScrollToHorizontalOffset(_resetHOffset);
    }
  }

  private void _setItemsSource() {
    if (TreeView == null) return;
    var expand = false;
    var root = TreeView.RootHolder.FirstOrDefault() as ITreeItem;
    if (root is { IsExpanded: true }) {
      expand = true;
      root.IsExpanded = false;
    }
    ItemsSource = TreeView.RootHolder;
    if (expand) _expandRootWhenReady(root!);

    TreeView.ScrollToTopAction = _scrollToTop;
    TreeView.ScrollToItemsAction = _scrollToItemsWhenReady;
    TreeView.ExpandRootWhenReadyAction = _expandRootWhenReady;
  }

  private void _scrollToTop() {
    if (_sv.VerticalOffset == 0) return;
    _sv.ScrollToTop();
    _sv.UpdateLayout();

    if (!_sv.IsVisible && TreeView != null)
      TreeView.TopTreeItem = _getHitTestItem(10, 10);
  }

  private void _scrollToItemsWhenReady(object[] items, bool exactly) =>
    _sv.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => _scrollToItems(items, exactly));

  private void _scrollToItems(object[] items, bool exactly) {
    var root = (ITreeItem)items[0];
    var idxItem = ((ITreeItem)items[^1]).GetIndex(root);

    if (idxItem < 0 || !_getDiff(idxItem, root, out var diff) || diff == 0) return;

    _isScrollingTo = true;

    if (_isDiffInView(idxItem, root)) {
      if (!exactly) {
        _isScrollingTo = false;
        return;
      }
      _sv.ScrollToVerticalOffset(_sv.VerticalOffset + diff);
    }

    _sv.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => {
      if (!_getDiff(idxItem, root, out diff) || diff == 0) return;

      // if diff was not in the view
      try {
        var parent = _scrollIntoView(this, items);
        parent.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => {
          if (!_getDiff(idxItem, root, out diff) || diff == 0) return;
          _sv.ScrollToVerticalOffset(_sv.VerticalOffset + diff);
          _sv.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => {
            _isScrollingTo = false;
          });
        });
      }
      catch (Exception e) {
        Log.Error("TreeViewBase.ScrollIntoView wasn't successful!", e.Message);
      }
    });
  }

  private bool _getDiff(int idxItem, ITreeItem root, out int diff) {
    diff = 0;
    var flag = false;
    var idxTopItem = _getHitTestItem(10, 10)?.GetIndex(root);

    if (idxTopItem is not (null or < 0)) {
      diff = idxItem - (int)idxTopItem;
      flag = true;
    }

    if (diff == 0) _isScrollingTo = false;
    return flag;
  }

  private bool _isDiffInView(int idxItem, ITreeItem root) {
    var bottomY = _sv.ActualHeight - 10;
    if (_sv.ComputedHorizontalScrollBarVisibility == Visibility.Visible) bottomY -= 14;
    var idxTopItem = _getHitTestItem(10, 10)?.GetIndex(root);
    var idxBottomItem = _getHitTestItem(10, bottomY)?.GetIndex(root);
    return idxTopItem is not (null or < 0) && idxBottomItem is not (null or < 0) &&
           idxTopItem <= idxItem && idxItem <= idxBottomItem;
  }

  private static ItemsControl _scrollIntoView(ItemsControl parent, IEnumerable<object> items) {
    foreach (var item in items) {
      var index = parent.Items.IndexOf(item);
      if (index < 0) break;
      var panel = parent.GetChildOfType<VirtualizingStackPanel>();
      if (panel == null) break;
      panel.BringIndexIntoViewPublic(index);
      panel.UpdateLayout();
      if (parent.ItemContainerGenerator.ContainerFromIndex(index) is not TreeViewItem tvi) break;
      parent = tvi;
    }

    return parent;
  }

  private ITreeItem? _getHitTestItem(double x, double y) {
    ITreeItem? outItem = null;
    VisualTreeHelper.HitTest(_sv, null, e => {
      if (e.VisualHit is not FrameworkElement { DataContext: ITreeItem item } || !TreeView!.IsHitTestItem(item))
        return HitTestResultBehavior.Continue;

      outItem = item;
      return HitTestResultBehavior.Stop;

    }, new PointHitTestParameters(new(x, y)));

    return outItem;
  }

  /// <summary>
  /// TreeView loads all items when everything is expanded.
  /// So I collapsed the root on reload and expanded it after to load just what is in the view.
  /// </summary>
  private void _expandRootWhenReady(ITreeItem root) =>
    _sv.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, void () => root.IsExpanded = true);

  /// <summary>
  /// Scroll TreeView when the mouse is near the top or bottom
  /// </summary>
  private protected void _dragDropAutoScroll(DragEventArgs e) {
    const int px = 25;
    var unit = GetValue(VirtualizingPanel.ScrollUnitProperty) is ScrollUnit.Item ? 1 : px;
    var pos = e.GetPosition(this);
    if (pos.Y < px)
      _sv.ScrollToVerticalOffset(_sv.VerticalOffset - unit);
    else if (_sv.ActualHeight - pos.Y < px)
      _sv.ScrollToVerticalOffset(_sv.VerticalOffset + unit);
  }
}