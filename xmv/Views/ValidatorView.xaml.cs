using System.Windows;

namespace Xmv.Views
{
  using Xmv.ViewModels;

  public partial class ValidatorView : Window
  {
    internal ValidatorView(ValidatorVM validatorVm)
    {
      InitializeComponent();
      DataContext = validatorVm;
    }
  }
}
