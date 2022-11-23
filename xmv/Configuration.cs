using System.Collections.Generic;

namespace Xmv
{
  internal class Configuration
  {
    public string Name { get; set; }
    public object[] Context { get; set; }
    public List<string> TestDirectories = new List<string>();
    public List<string> TestFiles = new List<string>();
  }
}
