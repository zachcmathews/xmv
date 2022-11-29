using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

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

    ~Validator()
    {
      UnloadTests();
    }

    public event EventHandler ConfigurationChanged;
    protected virtual void OnConfigurationChanged()
    {
      ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }


    public void ReloadTests()
    {
      UnloadTests();
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

    public void UnloadTests()
    {
      // Stop any timers from scheduling new tasks
      var testIds = Tests.Select(t => t.Id);
      foreach (var test in Tests)
      {
        test.Console.Close();
        test.RunTimer.Stop();
      }
      Tests.Clear();

      // Delete all the tasks in scheduler queue associated with this validator
      var tasks = new List<Task>(Scheduler.Queue);
      tasks.RemoveAll(task => testIds.Contains(task.Source));
      Scheduler.Queue.Clear();
      Scheduler.Queue = new Queue<Task>(tasks);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
