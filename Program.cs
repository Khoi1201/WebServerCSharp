using System.Net;
using System.Net.Sockets;
using System.Reflection;
using WebServerCSharp.Classes;
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
      WebServer.onError = ErrorHandler;
      WebServer.AddRoute(new Route() { Verb = Router.POST, Path = "/demo/redirect", Action = RedirectMe });

      WebServer.Start(websitePath);
      Console.ReadLine();
    }
    public static string RedirectMe(Dictionary<string, string> parms)
    {
      return "/demo/clicked";
    }
    public static string GetWebsitePath()
    {
      // Path of our exe.
      string websitePath = Assembly.GetExecutingAssembly().Location;
      websitePath = websitePath.LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\").LeftOfRightmostOf("\\") + "\\Website";

      return websitePath;
    }


    private static string ErrorHandler(WebServer.ServerError error)
    {
      string ret = null;

      switch (error)
      {
        case WebServer.ServerError.ExpiredSession:
        ret = "/ErrorPages/expiredSession.html";
        break;
        case WebServer.ServerError.FileNotFound:
        ret = "/ErrorPages/fileNotFound.html";
        break;
        case WebServer.ServerError.NotAuthorized:
        ret = "/ErrorPages/notAuthorized.html";
        break;
        case WebServer.ServerError.PageNotFound:
        ret = "/ErrorPages/pageNotFound.html";
        break;
        case WebServer.ServerError.ServerError:
        ret = "/ErrorPages/serverError.html";
        break;
        case WebServer.ServerError.UnknownType:
        ret = "/ErrorPages/unknownType.html";
        break;
      }
      return ret;

    }
  }
}

