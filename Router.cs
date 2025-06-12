using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebServerCSharp.Classes;
using WebServerCSharp.Extensions;

namespace WebServerCSharp
{
  internal class Router
  {
    public string WebsitePath { get; set; }

    private Dictionary<string, ExtensionInfo> extFolderMap;
    private List<Route> routes;
    public const string POST = "post";
    public const string GET = "get";
    public const string PUT = "put";
    public const string DELETE = "delete";

    public Router()
    {
      routes = new List<Route>();
      extFolderMap = new Dictionary<string, ExtensionInfo>()
            {
              {"ico", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/ico"}},
              {"png", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/png"}},
              {"jpg", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/jpg"}},
              {"gif", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/gif"}},
              {"bmp", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/bmp"}},
              {"html", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
              {"css", new ExtensionInfo() {Loader=FileLoader, ContentType="text/css"}},
              {"js", new ExtensionInfo() {Loader=FileLoader, ContentType="text/javascript"}},
              {"", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
            };
    }
    /// <summary>
    /// Read in an image file and returns a ResponsePacket with the raw data.
    /// </summary>
    private ResponsePacket ImageLoader(string fullPath, string ext, ExtensionInfo extInfo)
    {
      ResponsePacket ret;
      if (File.Exists(fullPath))
      {
      FileStream fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
      BinaryReader br = new BinaryReader(fStream);
      ret = new ResponsePacket() { Data = br.ReadBytes((int)fStream.Length), ContentType = extInfo.ContentType };
      br.Close();
      fStream.Close();
      } 
      else
      {
        ret = new ResponsePacket() { Error = WebServer.ServerError.FileNotFound};
      }

      return ret;
    }
    /// <summary>
    /// Read in what is basically a text file and return a ResponsePacket with the text UTF8 encoded.
    /// </summary>
    private ResponsePacket FileLoader(string fullPath, string ext, ExtensionInfo extInfo)
    {
      ResponsePacket ret;

      if (File.Exists(fullPath))
      {
        string text = File.ReadAllText(fullPath);
        ret = new() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };
      }
      else
      {
        ret = new ResponsePacket() { Error = WebServer.ServerError.FileNotFound };
        Console.WriteLine("File Not Found " + fullPath);
      }

      return ret;
    }
    /// <summary>
    /// Load an HTML file, taking into account missing extensions and a file-less IP/domain, 
    /// which should default to index.html.
    /// </summary>
    private ResponsePacket PageLoader(string fullPath, string ext, ExtensionInfo extInfo)
    {
      ResponsePacket ret = new();

      if (fullPath == WebsitePath) // If nothing follows the domain name or IP, then default to loading index.html.
      {
        ret = Route("GET", "/index.html", null);
      }
      else
      {
        if (string.IsNullOrEmpty(ext))
        {
          // No extension, so we make it ".html"
          fullPath = fullPath + ".html";
        }

        // Inject the "Pages" folder into the path
        fullPath = WebsitePath + "\\Pages" + fullPath.RightOf(WebsitePath);
        if (File.Exists(fullPath))
        {
          ret = FileLoader(fullPath, ext, extInfo);
        } 
        else
        {
          ret = new ResponsePacket() { Error=WebServer.ServerError.PageNotFound};
        }
      }

      return ret;
    }
    public void AddRoute(Route route)
    {
      routes.Add(route);
    }
    public ResponsePacket Route(string verb, string path, Dictionary<string, string> kvParams)
    {
      string ext = path.RightOfRightmostOf('.');
      ExtensionInfo extInfo;
      ResponsePacket ret = null;
      verb = verb.ToLower();
      path = path.ToLower();


      if (extFolderMap.TryGetValue(ext, out extInfo))
      {
        // Strip off leading '/' and reformat as with windows path separator.
        string fullPath = Path.Combine(WebsitePath, path.RightOf('/'));
        fullPath = fullPath.Replace("/", "\\");

        Route routeHandler = routes.SingleOrDefault(r => r.Path == path && r.Verb.ToLower() == verb);  

        if (routeHandler != null)
        {
          // Application has a handler for this route.
          string redirect = routeHandler.Action(kvParams);

          if (String.IsNullOrEmpty(redirect))
          {
            // Respond with default content loader.
            ret = extInfo.Loader(fullPath, ext, extInfo);
          }
          else
          {
            // Respond with redirect.
            ret = new ResponsePacket() { Redirect = redirect };
          }
        }
        else
        {
          ret = extInfo.Loader(fullPath, ext, extInfo);
        }

      }
      else
      {
        ret = new ResponsePacket() { Error = WebServer.ServerError.UnknownType };
      }

      return ret;
    }

  }
}
