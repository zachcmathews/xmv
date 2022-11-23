using System.Windows;

namespace Xmv.Views
{
  using Xmv.ViewModels;

  /// <summary>
  /// Interaction logic for Console.xaml
  /// </summary>
  public partial class ConsoleView : Window
  {
    public ConsoleView(ConsoleVM consoleVM)
    {
      InitializeComponent();
      DataContext = consoleVM;
    }
  }
}
