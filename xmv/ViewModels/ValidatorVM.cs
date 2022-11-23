using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Xmv.ViewModels
{
  using Xmv.Models;

  internal class AddTestFileCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      var dialog = new OpenFileDialog
      {
        Filter = "CSharp Files (*.cs)|*.cs",
        Multiselect = true
      };

      if (dialog.ShowDialog() != true) return;

      var validatorVM = parameter as ValidatorVM;
      try
      {
        validatorVM.Validator.AddTestFile(dialog.FileName);
      }
      catch
      {
        // TODO: Alert user here.
      }
    }
  }

  internal class AddTestDirectoryCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      var dialog = new System.Windows.Forms.FolderBrowserDialog();
      if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

      var validatorVM = parameter as ValidatorVM;
      try
      {
        validatorVM.Validator.AddTestDirectory(dialog.SelectedPath);
      }
      catch
      {
        // TODO: Alert user here.
      }
    }
  }

  internal class ReloadTestsCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      var validatorVM = parameter as ValidatorVM;
      validatorVM.Validator.ReloadTests();
    }
  }

  internal class ValidatorVM : INotifyPropertyChanged
  {
    public ICommand AddTestFile { get; set; } = new AddTestFileCommand();
    public ICommand AddTestDirectory { get; set; } = new AddTestDirectoryCommand();
    public ICommand ReloadTests { get; set; } = new ReloadTestsCommand();

    public Validator Validator { get; set; }
    public ObservableCollection<TestVM> Tests { get; set; } = new ObservableCollection<TestVM>();
    public string Title { get; set; }

    public uint NumberPassed {
      get
      {
        uint count = 0;
        foreach(var testVM in Tests)
        {
          if (testVM.Test.Passed == true) count++;
        }
        return count;
      }
    }

    public uint NumberFailed
    {
      get
      {
        uint count = 0;
        foreach (var testVM in Tests)
        {
          if (testVM.Test.Passed == false) count++;
        }
        return count;
      }
    }

    public uint NumberPending
    {
      get
      {
        uint count = 0;
        foreach (var testVM in Tests)
        {
          if (testVM.Test.Passed == null) count++;
        }
        return count;
      }
    }

    public ValidatorVM(Validator validator)
    {
      Title = "eXtensible Model Validator: " + validator.Configuration.Name;
      Validator = validator;
      CreateTestVMs();

      // Recreate test view models when validator changes
      validator.Tests.CollectionChanged += (sender, args) =>
      {
        Tests.Clear();
        CreateTestVMs();
      };
    }

    private void CreateTestVMs()
    {
      foreach (var test in Validator.Tests)
      {
        var testVm = new TestVM(test);

        // Listen to Test VM changes
        testVm.PropertyChanged += (sender, args) =>
        {
          if (args.PropertyName == nameof(TestVM.Passed))
          {
            OnPropertyChanged("NumberPassed");
            OnPropertyChanged("NumberFailed");
            OnPropertyChanged("NumberPending");
          }
        };

        Tests.Add(testVm);
        OnPropertyChanged("NumberPassed");
        OnPropertyChanged("NumberFailed");
        OnPropertyChanged("NumberPending");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
