using System.Windows;

namespace Xmv.Views
{
  using System.Windows.Controls;
  using Xmv.ViewModels;

  public partial class ResultsView : Window
  {
    public ResultsView(ResultsVM resultsVM)
    {
      InitializeComponent();
      Title = resultsVM.Name;

      var grid = FindName("grid") as Grid;
      grid.Children.Add(resultsVM.ResultsDataGrid);
    }
  }
}
