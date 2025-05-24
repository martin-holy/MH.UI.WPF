namespace MH.UI.WPF.Sample;

public class CoreUI {
  public CoreUI() {
    MH.UI.WPF.Utils.Init.SetDelegates();
    MH.UI.WPF.Resources.Dictionaries.IconToBrush = MH.UI.Sample.Resources.Res.IconToBrushDic;
  }
}