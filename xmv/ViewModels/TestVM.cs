using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Xmv.ViewModels
{
  using Xmv.Models;

  internal class RunTestCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    private Test Test { get; set; }

    public RunTestCommand(Test test)
    {
      Test = test;
    }

    public bool CanExecute(object parameter)
    {
      return Test.Run != null;
    }

    public void Execute(object parameter)
    {
      Test.Run();
    }
  }

  internal class ResolveTestCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    private Test Test { get; set; }

    public ResolveTestCommand(Test test)
    {
      Test = test;
    }

    public bool CanExecute(object parameter)
    {
      return Test.IsResolvable && Test.Resolve != null;
    }

    public void Execute(object parameter)
    {
      Test.Resolve();
    }
  }

  internal class ShowResultsCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    private Test Test { get; set; }

    public ShowResultsCommand(Test test)
    {
      Test = test;
    }

    public bool CanExecute(object parameter)
    {
      return Test.CanShowResults && Test.ShowResults != null;
    }

    public void Execute(object parameter)
    {
      Test.ShowResults();
    }
  }

  internal class TestVM : INotifyPropertyChanged
  {
    public ICommand Run { get; set; }
    public ICommand Resolve { get; set; }
    public ICommand ShowResults { get; set; }

    public Test Test { get; set; }
    public string Name
    {
      get
      {
        return Test.Name;
      }
    }

    public string Category
    {
      get
      {
        return Test.Category;
      }
    }

    public string Description
    {
      get {
        return Test.Description;
      }
    }

    public bool? Passed
    {
      get
      {
        return Test.Passed;
      }
    }

    public bool IsResolvable
    {
      get
      {
        return Test.IsResolvable;
      }
    }

    public bool CanShowResults
    {
      get
      {
        return Test.CanShowResults;
      }
    }

    public TestVM(Test test)
    {
      Test = test;
      Run = new RunTestCommand(Test);
      Resolve = new ResolveTestCommand(Test);
      ShowResults = new ShowResultsCommand(Test);

      // Listen to corresponding properties on Test
      Test.PropertyChanged += (sender, args) => { OnPropertyChanged(args.PropertyName); };
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
