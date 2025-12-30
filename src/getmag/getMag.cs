using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

[assembly: AssemblyTitle("getMag")]
[assembly: AssemblyDescription("Launcher utility for Magazine Capture Engine")]
[assembly: AssemblyCompany("Ottawa Moose")]
[assembly: AssemblyProduct("Get Mag")]
[assembly: AssemblyCopyright("Copyright © 2025")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace GetMagLauncher
{
    class Program
    {
        // Import for reading the config.ini file
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(string s, string k, string d, StringBuilder r, uint z, string f);

        [STAThread]
        static void Main()
        {
            // 1. CREATE MUTEX FOR INSTALLER DETECTION
            // This ID matches your Inno Setup AppId exactly.
            using (Mutex mutex = new Mutex(true, "{4A365DC4-2249-4C4C-939B-9140304DE5A9}", out bool createdNew))
            {
                if (!createdNew)
                {
                    // If the app is already open, don't launch another instance
                    MessageBox.Show("getMag is already running.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 2. PREPARE DIRECTORIES
                PrepareEnvironment();

                // 3. RUN UPDATE CHECK (IF ENABLED)
                RunUpdateCheck();

                // 4. LAUNCH MAIN ENGINE
                LaunchTarget("launcher.exe");
                
                // Keep the mutex alive until launcher.exe is started
            }
        }

        private static void PrepareEnvironment()
        {
            try
            {
                string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string magPath = Path.Combine(docsPath, "Magazines");

                if (!Directory.Exists(magPath))
                {
                    Directory.CreateDirectory(magPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not create default directory: " + ex.Message);
            }
        }

        private static void RunUpdateCheck()
        {
            try
            {
                string commonData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string iniPath = Path.Combine(commonData, "getMag", "config.ini");

                if (File.Exists(iniPath))
                {
                    StringBuilder sb = new StringBuilder(255);
                    // Default to "True" if the key doesn't exist yet
                    GetPrivateProfileString("Settings", "CheckForUpdate", "True", sb, 255, iniPath);
                    
                    if (sb.ToString().Trim().Equals("True", StringComparison.OrdinalIgnoreCase))
                    {
                        string updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "check_for_update.exe");
                        if (File.Exists(updaterPath))
                        {
                            // Launch without waiting (Post-and-forget)
                            Process.Start(updaterPath);
                        }
                    }
                }
            }
            catch { /* Silently fail to ensure main launch continues */ }
        }

        private static void LaunchTarget(string targetApp)
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, targetApp);

            try
            {
                if (File.Exists(fullPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fullPath,
                        UseShellExecute = true,
                        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                    });
                }
                else
                {
                    MessageBox.Show($"Error: '{targetApp}' was not found in the application folder.", 
                                    "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while starting the application: {ex.Message}", 
                                "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
