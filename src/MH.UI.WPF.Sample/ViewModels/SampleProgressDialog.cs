using System.Threading;
using System.Threading.Tasks;
using MH.UI.Dialogs;

namespace MH.UI.WPF.Sample.ViewModels;

public sealed class SampleProgressDialog: ProgressDialog<string> {
  public SampleProgressDialog(string[] items) : base("Sample Progress Dialog", Res.IconImage, items) {
    _autoRun();
  }

  protected override Task _do(string item, CancellationToken token) {
    _reportProgress(item);
    return Task.Delay(1000, token);
  }
}