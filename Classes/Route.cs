using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCSharp.Classes
{
  public class Route
  {
    public string Verb { get; set; }
    public string Path { get; set; }
    public Func<Dictionary<string, string>, string> Action { get; set; }
  }
}
