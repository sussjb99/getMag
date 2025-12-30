using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography; // Added for validation
using System.Text.RegularExpressions; // Added for Regex
using PdfSharp.Pdf;
using PdfSharp.Drawing;

// --- FILE DETAIL PROPERTIES ---
[assembly: AssemblyTitle("Magazine PDF Compiler")]
[assembly: AssemblyDescription("Converts captured magazine images into a single PDF document.")]
[assembly: AssemblyCompany("Custom Solutions")]
[assembly: AssemblyProduct("Get Mag PDF")]
[assembly: AssemblyCopyright("Copyright © 2025")]
[assembly: AssemblyVersion("1.50.0.0")]
[assembly: AssemblyFileVersion("1.50.0.0")]

public class PdfCompiler : Form
{
    // --- WATERMARK SETTINGS ---
    private const string WatermarkText = "DEMO GETMAG.EXE";
    private static readonly byte[] SECRET_KEY = Encoding.UTF8.GetBytes("YOUR-SUPER-SECRET-KEY-HERE"); 

    private string iniPath;
    private string sMagazine, sFolder, docFolder, sOutputPDF;
    private bool isLicensed = false;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern uint GetPrivateProfileString(string s, string k, string d, StringBuilder r, uint z, string f);

    public PdfCompiler()
    {
        string commonData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        string folderPath = Path.Combine(commonData, "getMag");
        this.iniPath = Path.Combine(folderPath, "config.ini");

        if (!Directory.Exists(folderPath))
        {
            try { Directory.CreateDirectory(folderPath); } catch { }
        }

        this.Text = "PDF Compiler";
        this.Size = new Size(400, 180);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.TopMost = true;

        string defaultOutput = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Magazines");
        docFolder = ReadIni("Settings", "FolderLocation", defaultOutput);
        string lastMag = ReadIni("Settings", "CurrentMagazine", "");

        // --- LICENSE CHECK ---
        string savedKey = ReadIni("Settings", "LicenseKey", "");
        isLicensed = ValidateLicenseWithKey(savedKey);

        sMagazine = Microsoft.VisualBasic.Interaction.InputBox("Enter the name of the magazine to convert:", "Get Mag PDF", lastMag);
        
        if (string.IsNullOrWhiteSpace(sMagazine)) Environment.Exit(0);

        sFolder = Path.Combine(docFolder, sMagazine);
        sOutputPDF = Path.Combine(sFolder, sMagazine + "_compiled.pdf");

        if (!Directory.Exists(sFolder))
        {
            MessageBox.Show("Folder not found: " + sFolder, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }

        RunConversion();
    }

    private bool ValidateLicenseWithKey(string license)
    {
        if (string.IsNullOrWhiteSpace(license)) return false;
        string pattern = @"^(GM-\d{4}-[A-Z0-9]{4})-([A-Z0-9]{8})$";
        Match match = Regex.Match(license, pattern);
        if (!match.Success) return false;

        string dataToVerify = match.Groups[1].Value; 
        string signatureProvided = match.Groups[2].Value; 

        using (HMACSHA256 hmac = new HMACSHA256(SECRET_KEY))
        {
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToVerify));
            string expectedSignature = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 8);
            return string.Equals(signatureProvided, expectedSignature, StringComparison.OrdinalIgnoreCase);
        }
    }

    private void RunConversion()
    {
        string statusText = isLicensed ? "Compiling PDF..." : "Compiling PDF (DEMO MODE)...";
        Label lbl = new Label() { 
            Text = statusText + "\nPlease wait.", 
            Dock = DockStyle.Fill, 
            TextAlign = ContentAlignment.MiddleCenter, 
            Font = new Font("Segoe UI", 10, FontStyle.Bold) 
        };
        this.Controls.Add(lbl);
        this.Show();
        this.BringToFront();
        Application.DoEvents();

        try
        {
            string[] files = Directory.GetFiles(sFolder, "*.png")
                            .OrderBy(f => f.Length)
                            .ThenBy(f => f)
                            .ToArray();

            if (files.Length == 0)
            {
                MessageBox.Show(this, "No PNG images found in folder.", "Empty Folder");
                Application.Exit();
                return;
            }

            using (PdfDocument document = new PdfDocument())
            {
                document.Info.Title = sMagazine;
                document.Info.Author = "Get Mag Utility";

                foreach (string file in files)
                {
                    PdfPage page = document.AddPage();
                    using (XImage img = XImage.FromFile(file))
                    {
                        page.Width = img.PointWidth;
                        page.Height = img.PointHeight;
                        
                        using (XGraphics gfx = XGraphics.FromPdfPage(page))
                        {
                            gfx.DrawImage(img, 0, 0, page.Width, page.Height);

                            // --- APPLY WATERMARK IF NOT LICENSED ---
                            if (!isLicensed)
                            {
                                ApplyWatermark(page, gfx, WatermarkText);
                            }
                        }
                    }
                }
                document.Save(sOutputPDF);
            }

            LogPdfEvent("Success: PDF created at " + sOutputPDF + (isLicensed ? "" : " [DEMO]"));
            
            this.Activate();
            MessageBox.Show(this, "PDF Successfully Created:\n" + sOutputPDF, "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            try {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{sOutputPDF}\"") { UseShellExecute = true });
            } catch { }
        }
        catch (Exception ex)
        {
            LogPdfEvent("Error: " + ex.Message);
            MessageBox.Show(this, "Conversion failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        Application.Exit();
    }

    private void ApplyWatermark(PdfPage page, XGraphics gfx, string text)
    {
        XFont font = new XFont("Arial", 60, XFontStyle.Bold);
        XBrush brush = new XSolidBrush(XColor.FromArgb(120, 128, 128, 128)); 
        XSize size = gfx.MeasureString(text, font);

        XGraphicsState state = gfx.Save();
        gfx.TranslateTransform(page.Width / 2, page.Height / 2);
        double angle = Math.Atan2(page.Height, page.Width) * 180 / Math.PI;
        gfx.RotateTransform(-angle);
        gfx.DrawString(text, font, brush, new XPoint(-size.Width / 2, size.Height / 2));
        gfx.Restore(state);
    }

    private void LogPdfEvent(string message)
    {
        try {
            string logDir = Path.Combine(sFolder, "logs");
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            File.AppendAllText(Path.Combine(logDir, "pdf_conversion_log.txt"), 
                DateTime.Now.ToString("HH:mm:ss") + " | " + message + "\r\n");
        } catch { }
    }

    private string ReadIni(string s, string k, string d)
    {
        StringBuilder sb = new StringBuilder(255);
        GetPrivateProfileString(s, k, d, sb, 255, iniPath);
        return sb.ToString().Trim();
    }

    [STAThread]
    static void Main() 
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
            string resName = "PdfSharp-gdi.dll"; 
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName)) {
                if (stream == null) return null;
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                return Assembly.Load(data);
            }
        };

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new PdfCompiler());
    }
}
