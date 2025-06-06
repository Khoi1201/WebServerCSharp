using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebServerCSharp.Extensions;

namespace WebServerCSharp.Classes
{
    internal class Router
    {
        public string WebsitePath { get; set; }

        private Dictionary<string, ExtensionInfo> extFolderMap;

        public Router()
        {
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
            FileStream fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fStream);
            ResponsePacket ret = new ResponsePacket() { Data = br.ReadBytes((int)fStream.Length), ContentType = extInfo.ContentType };
            br.Close();
            fStream.Close();

            return ret;
        }
        /// <summary>
        /// Read in what is basically a text file and return a ResponsePacket with the text UTF8 encoded.
        /// </summary>
        private ResponsePacket FileLoader(string fullPath, string ext, ExtensionInfo extInfo)
        {
            string text = File.ReadAllText(fullPath);
            ResponsePacket ret = new () { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };

            return ret;
        }
        /// <summary>
        /// Load an HTML file, taking into account missing extensions and a file-less IP/domain, 
        /// which should default to index.html.
        /// </summary>
        private ResponsePacket PageLoader(string fullPath, string ext, ExtensionInfo extInfo)
        {
            ResponsePacket ret = new ();

            if (fullPath == WebsitePath) // If nothing follows the domain name or IP, then default to loading index.html.
            {
                ret = Route("GET", "/index.html", null);
            }
            else
            {
                if (String.IsNullOrEmpty(ext))
                {
                    // No extension, so we make it ".html"
                    fullPath = fullPath + ".html";
                }

                // Inject the "Pages" folder into the path
                fullPath = WebsitePath + "\\Pages" + fullPath.RightOf(WebsitePath);
                ret = FileLoader(fullPath, ext, extInfo);
            }

            return ret;
        }
        public ResponsePacket Route(string verb, string path, Dictionary<string, string> kvParams)
        {
            string ext = path.RightOfRightmostOf('.');
            ExtensionInfo extInfo;
            ResponsePacket ret = null;

            if (extFolderMap.TryGetValue(ext, out extInfo))
            {
                // Strip off leading '/' and reformat as with windows path separator.
                string fullPath = Path.Combine(WebsitePath, path.RightOf('/'));
                fullPath = fullPath.Replace("/", "\\");
                ret = extInfo.Loader(fullPath, ext, extInfo);
            }

            return ret;
        }

    }
}
