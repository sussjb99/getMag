using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Web.Script.Serialization; // for JSON parsing

[assembly: AssemblyTitle("Get Mag Capture Utility")]
[assembly: AssemblyDescription("Automated online magazine capture tool.")]
[assembly: AssemblyCompany("Ottawa Moose Software Solutions")]
[assembly: AssemblyProduct("Get Mag")]
[assembly: AssemblyCopyright("Copyright © 2025 Ottawa Moose")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string apiUrl =
                "https://api.github.com/repos/sussjb99/getMag/contents/src/check_for_update/current_version";

            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "GetMagUpdater"); // GitHub API requires this

            string json = client.DownloadString(apiUrl);

            // Parse JSON
            JavaScriptSerializer js = new JavaScriptSerializer();
            dynamic data = js.Deserialize<dynamic>(json);

            // Extract base64 content
            string base64 = data["content"];
            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64)).Trim();

            Version current = Assembly.GetExecutingAssembly().GetName().Version;
            Version latest = new Version(decoded);

            if (latest > current)
            {
                ShowUpdateDialog(current, latest);
            }
        }
        catch (Exception)
        {
            // silently ignore errors
        }
    }

    static void ShowUpdateDialog(Version current, Version latest)
    {
        Form form = new Form()
        {
            Text = "Update Available",
            Width = 400,
            Height = 150,
            StartPosition = FormStartPosition.CenterScreen
        };

        Label info = new Label()
        {
            Text = $"A new version ({latest}) is available.\nYou are running {current}.",
            AutoSize = true,
            Top = 20,
            Left = 20
        };

        LinkLabel link = new LinkLabel()
        {
            Text = "Download from GitHub Releases",
            AutoSize = true,
            Top = 70,
            Left = 20
        };
        link.Links.Add(0, link.Text.Length, "https://github.com/sussjb99/getMag/releases/latest");
        link.LinkClicked += (sender, e) =>
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        };

        form.Controls.Add(info);
        form.Controls.Add(link);
        Application.Run(form);
    }
}
