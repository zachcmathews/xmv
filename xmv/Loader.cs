using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Globalization;

namespace Xmv
{
  using Xmv.Models;
  using Xmv.ViewModels;
  using Xmv.Views;

  public class Loader
  {
    public static List<Test> Load(string[] files, object[] context = null)
    {
      if (new FileInfo(files[0]).Extension.ToUpper(CultureInfo.InvariantCulture) == ".CS")
      {
        var results = CompileCSharp(files);
        var modules = results.CompiledAssembly.Modules;
        var types =
          modules.SelectMany(
            m => m.GetTypes().Where(
              t => t.BaseType == typeof(Test))).ToList();

        var tests = new List<Test>();
        foreach (var type in types)
        {
          var test = results.CompiledAssembly.CreateInstance(
            type.FullName,
            false,
            BindingFlags.Default,
            null,
            context,
            CultureInfo.InvariantCulture,
            null
          ) as Test;

          var runMethod = type.GetMethod("Run");
          if (runMethod != null)
          {
            var runner = type.GetMethod("Run").CreateDelegate(typeof(Runner), test) as Runner;

            // Running of tests may or may not need to be scheduled.
            // We're going to schedule them all anyways.
            // Each add-in implementation can decide when to run the tests.
            test.Run = () =>
            {
              Scheduler.Queue.Enqueue(new Task
              {
                Source = test.Id,
                Action = () =>
                {
                  try
                  {
                    runner();
                  }
                  catch (Exception e)
                  {
                    test.Console.WriteLine(e.Message + "\n" + e.StackTrace);
                  }
                }
              });
            };
          }

          var resolveMethod = type.GetMethod("Resolve");
          if (resolveMethod != null)
          {
            var resolver = type.GetMethod("Resolve").CreateDelegate(typeof(Resolver), test) as Resolver;

            // Resolving of tests will likely need to be scheduled since changes will
            // be made to the model.
            test.Resolve = () =>
            {
              Scheduler.Queue.Enqueue(new Task
              {
                Source = test.Id,
                Action = () =>
                {
                  try
                  {
                    resolver();
                  }
                  catch (Exception e)
                  {
                    test.Console.WriteLine(e.Message + "\n" + e.StackTrace);
                  }
                }
              });
            };
          }

          var showResultsMethod = type.GetMethod("ShowResults");
          ResultsShower resultsShower;
          if (showResultsMethod == null)
          {
            resultsShower = () =>
            {
              var vm = new ResultsVM(test);
              var view = new ResultsView(vm);
              view.Show();
            };
          }
          else
          {
            resultsShower = showResultsMethod.CreateDelegate(typeof(ResultsShower), test) as ResultsShower;
          }

          test.ShowResults = () =>
          {
            Scheduler.Queue.Enqueue(new Task {
              Source = test.Id,
              Action = () => 
              {
                try
                {
                  resultsShower();
                }
                catch (Exception e)
                {
                  test.Console.WriteLine(e.Message + "\n" + e.StackTrace);
                }
              }
            });
          };

          // NOTE: Test.Run actually enqueues the Run method here.
          // Refer to above comments about scheduling runs.
          test.RunTimer.Elapsed += (s, e) =>
          {
            test.Run();
          };

          tests.Add(test);
        }

        return tests;
      }
      else
      {
        return new List<Test>();
      }
    }

    public static CompilerResults CompileCSharp(string[] files)
    {
      var provider = CodeDomProvider.CreateProvider("CSharp");
      if (provider == null)
        throw new Exception("Could not create CodeDomProvider");

      var parameters = new CompilerParameters
      {
        GenerateInMemory = true,
        IncludeDebugInformation = true
      };

      // TODO: These shouldn't be hardcoded
      {
        // System references
        parameters.ReferencedAssemblies.Add("System.dll");
        parameters.ReferencedAssemblies.Add("System.Core.dll");
        parameters.ReferencedAssemblies.Add("System.ComponentModel.Primitives.dll");
        parameters.ReferencedAssemblies.Add("System.ComponentModel.TypeConverter.dll");
        parameters.ReferencedAssemblies.Add("System.ObjectModel.dll");
        parameters.ReferencedAssemblies.Add("System.Linq.dll");
        parameters.ReferencedAssemblies.Add("System.Linq.Expressions.dll");
        parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
      }

      var references = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => a.Location);
      // WPF references
      try
      {
        var presCore = references.Where(r => r.EndsWith("PresentationCore.dll")).First();
        parameters.ReferencedAssemblies.Add(presCore);
        var presFramework = references.Where(r => r.EndsWith("PresentationFramework.dll")).First();
        parameters.ReferencedAssemblies.Add(presFramework);
        var xaml = references.Where(r => r.EndsWith("System.Xaml.dll")).First();
        parameters.ReferencedAssemblies.Add(xaml);
        var windowsBase = references.Where(r => r.EndsWith("WindowsBase.dll")).First();
        parameters.ReferencedAssemblies.Add(windowsBase);
      }
      catch { }

      // Revit references
      try
      {
        var revitApi = references.Where(r => r.EndsWith("RevitAPI.dll")).First();
        parameters.ReferencedAssemblies.Add(revitApi);
        var revitApiUI = references.Where(r => r.EndsWith("RevitAPIUI.dll")).First();
        parameters.ReferencedAssemblies.Add(revitApiUI);
      }
      catch { }

      // XMV reference
      var assembly = Assembly.GetExecutingAssembly();
      parameters.ReferencedAssemblies.Add(assembly.Location);

      var results = provider.CompileAssemblyFromFile(parameters, files);
      if (results.Errors.Count > 0)
      {
        var output = "";
        foreach (var result in results.Output)
        {
          output += result + System.Environment.NewLine;
        }
        throw new Exception(output);
      }
      return results;
    }
  }
}
