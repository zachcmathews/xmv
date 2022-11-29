using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;


namespace Xmv.Models
{
  using Xmv.ViewModels;

  public delegate void Runner();
  public delegate void Resolver();
  public delegate void ResultsShower();
  
  public class Test : INotifyPropertyChanged
  {
    public Guid Id { get; } = Guid.NewGuid();

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

    public Runner Run { get; set; }
    public Resolver Resolve { get; set; }
    public ResultsShower ShowResults { get; set; }

    public Timer RunTimer { get; } = new Timer
    {
      Interval = double.MaxValue,
      AutoReset = false,
      Enabled = false,
    };

    public ObservableCollection<object> Results { get; } = new ObservableCollection<object>();

    public ConsoleVM Console { get; } = new ConsoleVM();

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
