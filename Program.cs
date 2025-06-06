using System.Net;
using System.Net.Sockets;
using System.Reflection;
using WebServerCSharp.Extensions;

namespace WebServerCSharp
{
    /// <summary>
    /// A lean and mean server
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            string websitePath = GetWebsitePath();
            WebServer.Start(websitePath);
            Console.ReadLine();
        }
        public static string GetWebsitePath()
        {
            // Path of our exe.
            string websitePath = Assembly.GetExecutingAssembly().Location;
            websitePath = websitePath.LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\") + "\\Website";

            return websitePath;
        }
    }
}

