using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Reflection;

// --- Assembly Metadata ---
[assembly: AssemblyTitle("Get Magazine Hotkey Launcher")]
[assembly: AssemblyDescription("Global Hotkey Management for Get Mag Toolset")]
[assembly: AssemblyCompany("Ottawa Moose Software Solutions")]
[assembly: AssemblyProduct("Get Mag")]
[assembly: AssemblyCopyright("Copyright © 2025")]
[assembly: AssemblyVersion("2.2.0.0")]
[assembly: AssemblyFileVersion("2.2.0.0")]

public class LauncherForm : Form
{
    private static Mutex _mutex;

    // --- Win32 APIs for Window Management ---
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    private const int SW_RESTORE = 9;

    // --- Win32 APIs for Hotkeys and INI ---
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern uint GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, uint size, string filePath);

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;

    public LauncherForm()
    {
        this.Text = "GET MAG — HOTKEYS"; 
        this.Size = new Size(400, 320);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MaximizeBox = false;

        Font titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
        Font boldFont = new Font("Segoe UI", 9, FontStyle.Bold);

        AddLabel("GET MAGAZINE", 130, 10, titleFont);
        AddLabel("HOTKEYS", 155, 30, boldFont);
        AddLabel("Function", 40, 60, titleFont);
        AddLabel("Hotkey", 240, 60, titleFont);

        AddRow("<A>bout", "<CTRL><ALT><A>", 80);
        AddRow("<H>elp", "<CTRL><ALT><H>", 100);
        AddRow("<R>egion Setup", "<CTRL><ALT><R>", 120);
        AddRow("<C>onfiguration", "<CTRL><ALT><C>", 140);
        AddRow("<S>tart Capture", "<CTRL><ALT><S>", 160);
        AddRow("<P>DF Conversion", "<CTRL><ALT><P>", 180);
        AddRow("<V>iew Output", "<CTRL><ALT><V>", 200);

        Button btnClose = new Button() { Text = "Close", Location = new Point(150, 240), Size = new Size(100, 30) };
        btnClose.Click += (s, e) => Application.Exit();
        this.Controls.Add(btnClose);

        RegisterHotKey(this.Handle, 1, MOD_CONTROL | MOD_ALT, (uint)Keys.A);
        RegisterHotKey(this.Handle, 2, MOD_CONTROL | MOD_ALT, (uint)Keys.H);
        RegisterHotKey(this.Handle, 3, MOD_CONTROL | MOD_ALT, (uint)Keys.R);
        RegisterHotKey(this.Handle, 4, MOD_CONTROL | MOD_ALT, (uint)Keys.C);
        RegisterHotKey(this.Handle, 5, MOD_CONTROL | MOD_ALT, (uint)Keys.S);
        RegisterHotKey(this.Handle, 6, MOD_CONTROL | MOD_ALT, (uint)Keys.P);
        RegisterHotKey(this.Handle, 7, MOD_CONTROL | MOD_ALT, (uint)Keys.V);
    }

    private void AddLabel(string text, int x, int y, Font f) {
        this.Controls.Add(new Label() { Text = text, Location = new Point(x, y), Font = f, AutoSize = true });
    }

    private void AddRow(string d, string k, int y) {
        AddLabel(d, 40, y, new Font("Segoe UI", 9));
        AddLabel(k, 240, y, new Font("Segoe UI", 9));
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x0312)
        {
            int id = m.WParam.ToInt32();
            switch (id)
            {
                case 1: Run("about.exe"); break;
                case 2: 
                    string helpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "help.html");
                    if(File.Exists(helpPath)) Process.Start(new ProcessStartInfo(helpPath) { UseShellExecute = true }); 
                    break;
                case 3: Run("region.exe"); break;
                case 4: Run("configure.exe"); break;
                case 5: Run("start_capture.exe"); break;
                case 6: Run("convert_to_pdf.exe"); break;
                case 7: OpenOutput(); break;
            }
        }
        base.WndProc(ref m);
    }

    private void Run(string exe) {
        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exe);
        if (File.Exists(fullPath)) {
            try { Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true }); } catch { }
        }
        else MessageBox.Show("Missing component: " + exe, "Error");
    }

    private void OpenOutput() {
        string commonData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        string iniPath = Path.Combine(commonData, "getMag", "config.ini");

        // Use the common Documents\Magazines path as the primary fallback
        string defaultFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Magazines");

        StringBuilder sb = new StringBuilder(255);
        GetPrivateProfileString("Settings", "FolderLocation", defaultFolder, sb, 255, iniPath);
        string path = sb.ToString().Trim();

        if (!Directory.Exists(path)) {
            try { Directory.CreateDirectory(path); } catch { path = defaultFolder; }
        }

        try { Process.Start("explorer.exe", path); } catch { }
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        for (int i = 1; i <= 7; i++) UnregisterHotKey(this.Handle, i);
        base.OnFormClosing(e);
    }

    [STAThread]
    static void Main()
    {
        bool createdNew;
        _mutex = new Mutex(true, "Global\\GetMag_Final_Lock_ID", out createdNew);

        if (!createdNew) {
            IntPtr hWnd = FindWindow(null, "GET MAG — HOTKEYS");
            if (hWnd != IntPtr.Zero) {
                if (IsIconic(hWnd)) ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new LauncherForm());
        GC.KeepAlive(_mutex);
    }

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);
}
