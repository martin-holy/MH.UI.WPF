using MH.UI.WPF.Extensions;
using MH.Utils.BaseClasses;
using MH.Utils.Tree;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using UIC = MH.UI.Controls;

namespace MH.UI.WPF.Controls;

public class TreeViewHost : ListBox, UIC.ITreeViewHost {
  private bool _isScrollingTo;
  private bool _resetHScroll;
  private double _resetHOffset;
  private ScrollViewer _sv = null!;
  private readonly ObservableCollection<FlatTreeItem> _items = [];

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
    nameof(ViewModel), typeof(UIC.TreeView), typeof(TreeViewHost), new(_viewModelChanged));

  public UIC.TreeView? ViewModel {
    get => (UIC.TreeView?)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  private static void _viewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if (d is not TreeViewHost host || host.ViewModel == null) return;
    host.ViewModel.Host = host;
  }

  public RelayCommand<RequestBringIntoViewEventArgs> TreeItemIntoViewCommand { get; }

  public TreeViewHost() {
    TreeItemIntoViewCommand = new(_onTreeItemIntoView);
  }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();

    _sv = (ScrollViewer)Template.FindName("PART_ScrollViewer", this);
    _sv.IsVisibleChanged += delegate { ViewModel?.SetVisible(_sv.IsVisible); };
    _sv.ScrollChanged += _onScrollChanged;

    ViewModel?.SetVisible(_sv.IsVisible);
    _setItemsSource();
  }

  // BUG this is not called
  private void _onTreeItemIntoView(RequestBringIntoViewEventArgs? e) {
    _resetHScroll = true;
    _resetHOffset = _sv.HorizontalOffset;
  }

  private void _onScrollChanged(object sender, ScrollChangedEventArgs e) {
    if (!_isScrollingTo && ViewModel != null && _sv.IsVisible)
      ViewModel.TopTreeItem = _getHitTestItem(10, 10);

    if (_resetHScroll) {
      _resetHScroll = false;
      _sv.ScrollToHorizontalOffset(_resetHOffset);
    }
  }

  private void _setItemsSource() {
    if (ViewModel == null) return;

    ViewModel.FlatTree.ResetEvent += _resetItems;
    ViewModel.FlatTree.RangeInsertedEvent += _insertItems;
    ViewModel.FlatTree.RangeRemovedEvent += _removeItems;

    ItemsSource = _items;
    ViewModel.FlatTree.Reset();
  }

  private void _resetItems() {
    _items.Clear();

    foreach (var item in ViewModel!.FlatTree.Items)
      _items.Add(item);
  }

  private void _insertItems(int index, int count) {
    for (int i = 0; i < count; i++)
      _items.Insert(index + i, ViewModel!.FlatTree.Items[index + i]);
  }

  private void _removeItems(int index, int count) {
    for (int i = 0; i < count; i++)
      _items.RemoveAt(index);
  }

  public void ScrollToTop() {
    if (_sv.VerticalOffset == 0) return;
    _sv.ScrollToTop();
    _sv.UpdateLayout();

    if (!_sv.IsVisible && ViewModel != null)
      ViewModel.TopTreeItem = _getHitTestItem(10, 10);
  }

  public void ScrollTo(ITreeItem item, bool exactly) =>
    _sv.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => _scrollTo(item, exactly));

  private void _scrollTo(ITreeItem item, bool exactly) {
    if (ViewModel == null) return;

    int index = ViewModel.FlatTree.IndexOf(item);
    if (index < 0) return;

    _isScrollingTo = true;

    if (_isDiffInView(index)) {
      if (!exactly) {
        _isScrollingTo = false;
        return;
      }

      if (_getDiff(index, out var diff) && diff != 0)
        _sv.ScrollToVerticalOffset(_sv.VerticalOffset + diff);

      _isScrollingTo = false;
      return;
    }

    if (this.GetChildOfType<VirtualizingStackPanel>() is not { } panel) {
      _isScrollingTo = false;
      return;
    }

    panel.BringIndexIntoViewPublic(index);

    Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
      () => {
        if (_getDiff(index, out var diff) && diff != 0)
          _sv.ScrollToVerticalOffset(_sv.VerticalOffset + diff);

        _isScrollingTo = false;
      });
  }

  private bool _getDiff(int targetIndex, out int diff) {
    diff = 0;

    var topItem = _getHitTestItem(10, 10);

    if (topItem == null) return false;

    int topIndex = ViewModel!.FlatTree.IndexOf(topItem);

    if (topIndex < 0)
      return false;

    diff = targetIndex - topIndex;

    return true;
  }

  private bool _isDiffInView(int targetIndex) {
    var topItem = _getHitTestItem(10, 10);
    var bottomItem = _getHitTestItem(10, _sv.ActualHeight - 10);

    if (topItem == null || bottomItem == null || ViewModel == null)
      return false;

    int topIndex = ViewModel.FlatTree.IndexOf(topItem);
    int bottomIndex = ViewModel.FlatTree.IndexOf(bottomItem);

    return topIndex >= 0 &&
           bottomIndex >= 0 &&
           topIndex <= targetIndex &&
           targetIndex <= bottomIndex;
  }

  private ITreeItem? _getHitTestItem(double x, double y) {
    ITreeItem? outItem = null;
    VisualTreeHelper.HitTest(_sv, null, e => {
      if (e.VisualHit is not FrameworkElement { DataContext: FlatTreeItem item }
        || !ViewModel!.IsHitTestItem(item.TreeItem))
        return HitTestResultBehavior.Continue;

      outItem = item.TreeItem;
      return HitTestResultBehavior.Stop;

    }, new PointHitTestParameters(new(x, y)));

    return outItem;
  }

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