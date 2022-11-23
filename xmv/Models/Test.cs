using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;


namespace Xmv.Models
{
  using Xmv.ViewModels;

  public delegate void Runner();
  public delegate void Resolver();
  public delegate void ResultsShower();

  public class Test : INotifyPropertyChanged
  {
    private string name;
    public string Name
    {
      get { return name; }
      set { name = value; OnPropertyChanged(); }
    }

    private string category;
    public string Category
    {
      get { return category; }
      set { category = value; OnPropertyChanged(); }
    }

    private string description;
    public string Description
    {
      get { return description; }
      set { description = value; OnPropertyChanged(); }
    }

    private bool isResolvable = false;
    public bool IsResolvable { 
      get { return isResolvable; }
      set { isResolvable = value; OnPropertyChanged(); } } 

    private bool? passed;
    public bool? Passed {
      get { return passed; }
      set { passed = value; OnPropertyChanged(); }
    }

    private bool canShowResults = false;
    public bool CanShowResults
    {
      get { return canShowResults; }
      set { canShowResults = value; OnPropertyChanged(); }
    }

    public ObservableCollection<object> Results { get; set; } = new ObservableCollection<object>();

    public Runner Run { get; set; }
    public Resolver Resolve { get; set; }
    public ResultsShower ShowResults { get; set; }

    public ConsoleVM Console { get; set; } = new ConsoleVM();

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
