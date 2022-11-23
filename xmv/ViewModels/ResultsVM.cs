using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Xmv.ViewModels
{
  using Xmv.Models;

  public class ResultsVM : INotifyPropertyChanged
  {
    private readonly Test test;

    public DataGrid ResultsDataGrid { get; set; } = new DataGrid();

    public string Name
    {
      get
      {
        return "eXtensible Model Validator Results: " + test.Name;
      }
    }

    public ResultsVM(Test test)
    {
      this.test = test;
      ResultsDataGrid.ItemsSource = test.Results;

      test.PropertyChanged += (source, e) => OnPropertyChanged(e.PropertyName);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
