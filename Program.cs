using System.Net;
using System.Net.Sockets;

namespace WebServerCSharp
{
    /// <summary>
    /// A lean and mean server
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            WebServer.Start();
            Console.ReadLine();
        }
    }
}

