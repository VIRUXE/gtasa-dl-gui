using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;

namespace gtasa_dl_gui {
    public partial class Main : Form {
        private readonly WebClient webClient = new WebClient();
        private readonly long gameArchiveSize;

        public Main() {
            InitializeComponent();

            Updates.CheckForUpdates();

            webClient.Headers.Add("user-agent", "gtasa-dl-gui");

            webClient.DownloadFileCompleted += OnGameArchiveDownloaded;
            webClient.DownloadProgressChanged += OnGameArchiveDownloadProgress;

            string gamePath = GetGamePath();
            if (gamePath != null) {
                lstDebug.Items.Add("GTASA is installed at: " + gamePath);
                //btnOpenGameFolder.Enabled = true;
                //btnOpenGameFolder.Click += (sender, e) => Process.Start(gamePath);
            } else {
                lstDebug.Items.Add("Unable to find a location for GTASA.");
            }

            if (!NetworkInterface.GetIsNetworkAvailable()) {
                lstDebug.Items.Add("Internet connection is not available.");
            } else {
                if (Website.CheckAvailability()) {
                    lstDebug.Items.Add("Website is available.");

                    gameArchiveSize = Website.GetGameArchiveSize();

                    lstDebug.Items.Add(gameArchiveSize > 0
                        ? $"Website Game Archive Size: {gameArchiveSize / 1024 / 1024} MB"
                        : "Game Archive Size could not be determined. Something happened to the website. Contact VIRUXE.");

                    // Find out if the game archive is already downloaded and is of the same size
                    if (File.Exists("gtasa.7z")) {
                        long localGameArchiveSize = new FileInfo("gtasa.7z").Length;
                        if (localGameArchiveSize == gameArchiveSize) {
                            lstDebug.Items.Add("Found Game Archive. You can create Installations.");
                            downloadGameArchiveToolStripMenuItem.Enabled = false;
                        } else {
                            lstDebug.Items.Add($"Found Game Archive, but with the wrong size ({localGameArchiveSize / 1024 / 1024} MB). Download again.");
                            downloadGameArchiveToolStripMenuItem.Enabled = true;
                        }
                    } else {
                        lstDebug.Items.Add("Game Archive not found.");
                        downloadGameArchiveToolStripMenuItem.Enabled = true;
                    }

                    // Just set it either way
                    toolStripProgressBar.Maximum = (int)gameArchiveSize;
                } else {
                    lstDebug.Items.Add("Website is not available.");
                    downloadGameArchiveToolStripMenuItem.Enabled = false;
                }
            }

            toolStripProgressBar.MarqueeAnimationSpeed = 0;
        }

        private string GetGamePath() => Path.GetDirectoryName(
            (string)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\WOW6432Node\Rockstar Games\GTA San Andreas", "Installed Path", null) ??
            (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\SAMP", "gta_sa_exe", null)
        );

        private void OnGameArchiveDownloadProgress(object sender, DownloadProgressChangedEventArgs e) {
            toolStripProgressBar.Value = (int)e.BytesReceived;

            // Calculate the download speed
            double speed = (double)e.BytesReceived / e.ProgressPercentage;
            string speedText = speed > 1024 * 1024
                ? $"{speed / 1024 / 1024:0.00} MB/s"
                : $"{speed / 1024:0.00} KB/s";

            // Calculate the time remaining
            double timeRemaining = (e.TotalBytesToReceive - e.BytesReceived) / speed;
            string timeRemainingText = timeRemaining <= TimeSpan.MaxValue.TotalSeconds
                ? TimeSpan.FromSeconds(timeRemaining).ToString(@"hh\:mm\:ss")
                : "Infinite";

            toolStripStatusLabel.Text = $"Downloading at {speedText}. Time remaining: {timeRemainingText}";
        }

        private void OnGameArchiveDownloaded(object sender, AsyncCompletedEventArgs e) {
            downloadGameArchiveToolStripMenuItem.Enabled = true;
            toolStripProgressBar.Value = 0;
            toolStripStatusLabel.Text = "Ready.";

            if (e.Cancelled) {
                lstDebug.Items.Add("Download was cancelled.");
            } else if (e.Error != null) {
                lstDebug.Items.Add("Error downloading game archive: " + e.Error.Message);
            } else {
                lstDebug.Items.Add("Game archive downloaded successfully.");
                downloadGameArchiveToolStripMenuItem.Enabled = false;
            }
        }

        private void timer_Tick(object sender, EventArgs e) {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            MessageBox.Show($"GTASA Downloader GUI - Version {Application.ProductVersion}\n\n" +
                            "Created by VIRUXE\n\n" +
                            "This app was created to make the best Grand Theft Auto game ever released more accessible to everyone.\n" +
                            "Almost 20 years have passed since its release and a lot of resources have died. Especially with SA-MP.com dying...\n" +
                            "So hopefully Rockstar understands this and doesn't take my hosting down, in order to keep this alive forever.\n" +
                            "You can still buy the game on Steam and downgrade it via this app, in order to play SA-MP.\n\n" +
                            "Feel free to contribute at https://github.com/VIRUXE/gtasa-dl-gui\n\n" +
                            "This app is not affiliated with Rockstar Games or any of its partners.\n" +
                            "Grand Theft Auto: San Andreas is a trademark of Take-Two Interactive Software, Inc.\n"
                            , "About VIRUXE's GTASA Downloader", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void downloadGameArchiveToolStripMenuItem_Click(object sender, EventArgs e) {
            if (webClient.IsBusy) {
                webClient.CancelAsync();
                downloadGameArchiveToolStripMenuItem.Text = "Download Game Archive";
                return;
            }

            if (!NetworkInterface.GetIsNetworkAvailable()) {
                lstDebug.Items.Add("Internet connection is not available.");
                return;
            }

            if (!Website.CheckAvailability()) {
                lstDebug.Items.Add("Website is not available.");
                return;
            }

            lstDebug.Items.Add("Downloading game archive...");
            downloadGameArchiveToolStripMenuItem.Text = "Cancel Download";
            toolStripProgressBar.Value = 0;
            webClient.DownloadFileAsync(new Uri(Website.gameArchiveURL), "gtasa.7z");
        }
    }
}
