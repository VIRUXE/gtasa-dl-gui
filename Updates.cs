using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gtasa_dl_gui {
    class Version {
        // Major, Minor, Patch
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
    }

    internal static class Updates {
        public static void CheckForUpdates() {
            // Query the GitHub API for the latest release
            // Compare the latest release with the current version
            // If the latest release is newer, prompt the user to download the latest release

            // If the user chooses to download the latest release, download the latest release

            try {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "gtasa-dl-gui");

                HttpResponseMessage response = client.GetAsync("https://api.github.com/repos/VIRUXE/gtasa-dl-gui/releases/latest").Result;
                response.EnsureSuccessStatusCode();

                string responseBody = response.Content.ReadAsStringAsync().Result;
                
                MessageBox.Show(responseBody);
            } catch (Exception e) {
                MessageBox.Show($"Unable to check for updates:\n\n{e.Message}", "Checking for Updates", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
