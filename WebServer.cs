using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebServerCSharp.Classes;

namespace WebServerCSharp
{
  /// <summary>
  /// Create webserver, must run VS with administration privilege to use 
  /// </summary>
  public class WebServer
  {
    private static HttpListener _listener;
    public static int maxSimultaneousConnections = 20;
    private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);
    private static Router router = new Router();
    public static Func<ServerError, string>? onError;
    public enum ServerError
    {
      OK,
      ExpiredSession,
      NotAuthorized,
      FileNotFound,
      PageNotFound,
      ServerError,
      UnknownType,
    }
    /// <summary>
    /// Starts the web server.
    /// </summary>
    public static void Start(string websitePath)
    {
      router.WebsitePath = websitePath;
      List<IPAddress> localHostIPs = GetLocalHostIPs();
      HttpListener listener = InitializeListener(localHostIPs);
      listener.Start();

      Task.Run(() => RunServer(listener));
    }
    public static void AddRoute(Route route)
    {
      router.AddRoute(route);
    }
    /// <summary>
    /// Returns list of IP addresses assigned to localhost network devices, such as hardwired ethernet, wireless, etc.
    /// </summary>
    private static List<IPAddress> GetLocalHostIPs()
    {
      IPHostEntry host;
      host = Dns.GetHostEntry(Dns.GetHostName());
      List<IPAddress> ips = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

      return ips;
    }
    private static HttpListener InitializeListener(List<IPAddress> ips)
    {
      HttpListener listener = new HttpListener();
      listener.Prefixes.Add("http://localhost/");

      // listen to IP address as well.
      foreach (var ip in ips)
      {
        string ipAddress = $"http://{ip.ToString()}/";
        Console.WriteLine($"Listening on IP {ipAddress}");
        listener.Prefixes.Add(ipAddress);
      }
      return listener;
    }

    /// <summary>
    /// Start awaiting for connections, up to the "maxSimultaneousConnections" value.
    /// This code runs in a(one) separate thread.
    /// </summary>
    private static void RunServer(HttpListener listener)
    {
      while (true)
      {
        sem.WaitOne();
        StartConnectionListener(listener);
      }
    }

    /// <summary>
    /// Await connections.
    /// </summary>
    private static async void StartConnectionListener(HttpListener listener)
    {
      ResponsePacket responsePacket = new ResponsePacket();

      // Wait for a connection. Return to caller while we wait.
      HttpListenerContext context = await listener.GetContextAsync();

      // Release the semaphore so that another listener can be immediately started up.
      sem.Release();
      Log(context.Request);

      // We have a connection, do something...

      HttpListenerRequest request = context.Request;
      string url = request.RawUrl;
      string path = string.Empty;
      string paramsString = string.Empty;
      string verb = string.Empty;
      Dictionary<string, string> kvParams = new Dictionary<string, string>();

      try
      {
        if (url.IndexOf('?') != -1)
        {
          path = url.Substring(0, url.IndexOf('?')); // Only the path, not any of the parameters
          paramsString = request.RawUrl.Substring(url.IndexOf('?'), url.Length - url.IndexOf('?')); // Params on the URL itself follow the URL and are separated by a ?
          kvParams = ConvertStringParamsToDictionary(paramsString); // Extract into key-value entries
          Log(kvParams);
        }
        else
        {
          path = url;
        }

        verb = request.HttpMethod; // get, post, delete, etc.

        responsePacket = router.Route(verb, path, kvParams);

        if (responsePacket.Error != ServerError.OK)
        {
          //responsePacket = router.Route("GET", onError(responsePacket.Error), null);
          responsePacket.Redirect = onError(responsePacket.Error);
        }

        Respond(context.Request, context.Response, responsePacket);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        Console.WriteLine(e.StackTrace);
        responsePacket = new ResponsePacket() { Redirect = onError(ServerError.ServerError) };
      }


    }
    private static void Respond(HttpListenerRequest request, HttpListenerResponse response, ResponsePacket responsePacket)
    {
      if (String.IsNullOrEmpty(responsePacket.Redirect))
      {
        response.ContentType = responsePacket.ContentType;
        response.ContentLength64 = responsePacket.Data.Length;
        response.OutputStream.Write(responsePacket.Data, 0, responsePacket.Data.Length);
        response.ContentEncoding = responsePacket.Encoding;
        response.StatusCode = (int)HttpStatusCode.OK;
      }
      else
      {
        response.StatusCode = (int)HttpStatusCode.Redirect;
        response.Redirect("http://" + request.UserHostAddress + responsePacket.Redirect);
      }

      response.OutputStream.Close();
    }
    private static Dictionary<string, string> ConvertStringParamsToDictionary(string paramStr)
    {
      string tempString = paramStr;
      if (tempString[0] == '?')
      {
        tempString = paramStr.Substring(1);
      }
      Dictionary<string, string> dict = new Dictionary<string, string>();
      foreach (string keyVal in tempString.Split('&'))
      {
        if (keyVal.Length == 2)
        {
          dict.Add(keyVal.Split('=')[0], keyVal.Split("=")[1]);
        }
      }
      return dict;
    }

    /// <summary>
    /// Log requests.
    /// </summary>
    public static void Log(HttpListenerRequest request)
    {
      Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /" + request.Url.AbsoluteUri);
    }
    private static void Log(Dictionary<string, string> kv)
    {
      foreach (var item in kv)
      {
        Console.WriteLine(item.Key + " : " + item.Value);
      }
    }
  }
}
