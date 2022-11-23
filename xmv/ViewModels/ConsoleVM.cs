using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xmv.ViewModels
{
  using Xmv.Views;

  public class ConsoleVM : INotifyPropertyChanged
  {
    private ConsoleView console;

    private string text;
    public string Text {
      get
      {
        return text;
      }
      set
      {
        text = value;
        OnPropertyChanged();
      }
    }

    public void Clear()
    {
      Text = "";
    }

    public void Write(string text)
    {
      Text += text;
      Show();
    }

    public void WriteLine(string line)
    {
      Text += line + Environment.NewLine;
      Show();
    }

    public void Show()
    {
      if (console == null)
      {
        console = new ConsoleView(this);
        console.Show();
        console.Closed += (s, e) => console = null;
      }
    }

    public ConsoleVM()
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
