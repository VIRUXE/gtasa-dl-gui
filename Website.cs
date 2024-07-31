using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtasa_dl_gui {
    static class Website {
        static private readonly string websiteURL = "https://gta.flaviopereira.dev";
        static public readonly string gameArchiveURL = websiteURL + "/sa/gtasa.7z";

        // Check website's availability
        static public bool CheckAvailability() {
            try {
                System.Net.WebRequest req = System.Net.WebRequest.Create(websiteURL);
                System.Net.WebResponse resp = req.GetResponse();
                resp.Close();
                return true;
            } catch {
                return false;
            }
        }

        static public long GetGameArchiveSize() {
            System.Net.WebRequest req = System.Net.WebRequest.Create(gameArchiveURL);
            req.Method = "HEAD";
            System.Net.WebResponse resp = req.GetResponse();
            long contentLength = long.Parse(resp.Headers.Get("Content-Length"));
            resp.Close();
            return contentLength;
        }
    }
}
