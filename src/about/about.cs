using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;

// --- File Properties (Visible in Windows Explorer Details) ---
[assembly: AssemblyTitle("Get Mag Capture Utility")]
[assembly: AssemblyDescription("Automated online magazine capture tool.")]
[assembly: AssemblyCompany("Ottawa Moose Software Solutions")]
[assembly: AssemblyProduct("Get Mag")]
[assembly: AssemblyCopyright("Copyright © 2025 Ottawa Moose")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

public class AboutForm : Form
{
    private static Mutex mutex = null;

    public AboutForm()
    {
        // Window Settings - Classic Gray (AutoIt Style)
        this.Text = "About Get Mag";
        this.Size = new Size(500, 500);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MaximizeBox = false;
        this.BackColor = SystemColors.Control; 

        // Font Definitions
        Font titleFont = new Font("Segoe UI", 9, FontStyle.Bold);
        Font bodyFont = new Font("Segoe UI", 9, FontStyle.Regular);

        // --- Logo Area ---
        PictureBox logo = new PictureBox();
        logo.Size = new Size(100, 100);
        logo.Location = new Point(20, 20);
        logo.SizeMode = PictureBoxSizeMode.Zoom;
        logo.BorderStyle = BorderStyle.FixedSingle;

        try {
            // FIXED: Look for the logo in the actual application installation folder
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.jpg");
            if (File.Exists(logoPath)) {
                logo.Image = Image.FromFile(logoPath);
            }
        } catch { }
        this.Controls.Add(logo);

        // --- Header Column Layout ---
        AddLabel("Get Mag – Magazine Capture Utility", 140, 25, titleFont);
        
        AddLabel("Version:", 140, 50, bodyFont); 
        AddLabel("1.0.0", 240, 50, bodyFont);
        
        AddLabel("Executable:", 140, 75, bodyFont); 
        AddLabel("getMag.exe", 240, 75, bodyFont);

        // --- Content Sections ---
        int y = 140;
        AddLabel("Purpose:", 20, y, bodyFont);
        AddLabel("This software automates the capture of online magazine pages so they can be viewed offline.", 20, y + 20, bodyFont, 440);

        y += 70;
        AddLabel("Developed by:", 20, y, bodyFont); 
        AddLabel("Ottawa Moose Software Solutions", 130, y, bodyFont);
        
        AddLabel("\u00a9 2025", 20, y + 25, bodyFont); 
        AddLabel("Ottawa Moose All rights reserved.", 130, y + 25, bodyFont);
        
        AddLabel("License:", 20, y + 50, bodyFont);
        AddLabel("Proprietary – unauthorized reproduction or distribution is prohibited.", 130, y + 50, bodyFont, 330);

        y += 100;
        AddLabel("Credits:", 20, y, bodyFont); 
        AddLabel("pdfsharp, InnoSetup", 130, y, bodyFont);
        
        // --- Links Section ---
        AddLabel("Support:", 20, y + 25, bodyFont);
        AddLink("https://ottawamoosesoftwaresolutions8.wordpress.com/contact/", 130, y + 25);

        AddLabel("Website:", 20, y + 50, bodyFont);
        AddLink("https://ottawamoosesoftwaresolutions8.wordpress.com/", 130, y + 50);

        // --- OK Button ---
        Button btnOk = new Button();
        btnOk.Text = "OK";
        btnOk.Size = new Size(100, 30);
        btnOk.Location = new Point((this.ClientSize.Width - 100) / 2, 410);
        btnOk.Click += (s, e) => this.Close();
        this.Controls.Add(btnOk);
    }

    private void AddLabel(string text, int x, int y, Font font, int width = 0)
    {
        Label lbl = new Label() { 
            Text = text, 
            Location = new Point(x, y), 
            Font = font, 
            AutoSize = (width == 0) 
        };
        if (width > 0) lbl.Size = new Size(width, 45);
        this.Controls.Add(lbl);
    }

    private void AddLink(string url, int x, int y)
    {
        LinkLabel lnk = new LinkLabel() { 
            Text = url, 
            Location = new Point(x, y), 
            AutoSize = true, 
            Font = new Font("Segoe UI", 9) 
        };
        lnk.LinkClicked += (s, e) => {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
        };
        this.Controls.Add(lnk);
    }

    [STAThread]
    static void Main()
    {
        const string appGuid = "GetMag_About_SingleInstance_Lock";
        using (mutex = new Mutex(false, "Global\\" + appGuid))
        {
            if (!mutex.WaitOne(0, false)) return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AboutForm());
        }
    }
}
