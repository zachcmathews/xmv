using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Xmv.Models
{
  using Xmv.ViewModels;

  internal class Validator
  {
    public ConsoleVM Console { get; set; } = new ConsoleVM();
    public Configuration Configuration { get; set; }
    public ObservableCollection<Test> Tests { get; set; } = new ObservableCollection<Test>();

    public Validator(Configuration configuration)
    {
      Configuration = configuration;
      ReloadTests();
    }

    public event EventHandler ConfigurationChanged;
    protected virtual void OnConfigurationChanged()
    {
      ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ReloadTests()
    {
      Tests.Clear();
      foreach (var dir in Configuration.TestDirectories)
      {
        try
        {
          LoadTests(Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories));
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message + "\n" + e.StackTrace);
        }
      }

      foreach(var file in Configuration.TestFiles)
      {
        try
        {
          LoadTests(new string[] { file });
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message + "\n" + e.StackTrace);
        }
      }
    }

    public void AddTestDirectory(string dir)
    {
      if (!Directory.Exists(dir)) throw new Exception("Directory does not exist");
      if (Configuration.TestDirectories.Contains(dir)) return;
      Configuration.TestDirectories.Add(dir);
      LoadTests(Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories));
    }

    public void AddTestFile(string file)
    {
      if (!File.Exists(file)) throw new Exception("File does not exist");
      if (Configuration.TestFiles.Contains(file)) return;
      Configuration.TestFiles.Add(file);
      LoadTests(new string[]{ file });
    }

    private void LoadTests(string[] files)
    {
      var new_tests = Loader.Load(files, Configuration.Context);

      foreach (var test in new_tests)
      {
        Tests.Add(test);
      }

      OnConfigurationChanged();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
