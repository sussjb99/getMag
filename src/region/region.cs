using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

public class RegionSelector : Form
{
    [DllImport("user32.dll")]
    static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")]
    static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
    [DllImport("user32.dll")]
    static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern uint WritePrivateProfileString(string s, string k, string v, string f);

    private string iniPath;
    private Point startPos;
    private Point currentPos;
    private bool isDragging = false;
    private Rectangle selectionRect;

    public RegionSelector()
    {
        // Pathing: ProgramData\getMag\config.ini
        string commonData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        string folderPath = Path.Combine(commonData, "getMag");
        this.iniPath = Path.Combine(folderPath, "config.ini");

        if (!Directory.Exists(folderPath))
        {
            try { Directory.CreateDirectory(folderPath); } catch { }
        }

        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.Black;
        this.Opacity = 0.0; 
        this.ShowInTaskbar = false;
        this.TopMost = true;
        this.DoubleBuffered = true;
        this.Cursor = Cursors.Cross;
        
        // Multi-monitor support: Span the entire virtual screen
        this.StartPosition = FormStartPosition.Manual;
        this.Location = SystemInformation.VirtualScreen.Location;
        this.Size = SystemInformation.VirtualScreen.Size;

        // Hotkeys: CTRL+ALT+F12 to show, ESC to exit
        RegisterHotKey(this.Handle, 1, 0x0002 | 0x0001, (int)Keys.F12);
        RegisterHotKey(this.Handle, 2, 0, (int)Keys.Escape);

        ShowStartupInstructions();
    }

    private void ShowStartupInstructions()
    {
        MessageBox.Show("REGION SELECTOR ACTIVE\n\n1. Press <CTRL><ALT><F12> to reveal the overlay.\n2. Click and Drag to select your magazine area.\n3. Release to save.\n\nPress <ESC> to cancel.", "Get Mag Setup");
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x0312) // Hotkey pressed
        {
            int id = m.WParam.ToInt32();
            if (id == 1) { this.Opacity = 0.35; this.BringToFront(); this.Activate(); }
            else if (id == 2) { Application.Exit(); }
        }
        base.WndProc(ref m);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (this.Opacity > 0) { isDragging = true; startPos = Control.MousePosition; }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (isDragging)
        {
            currentPos = Control.MousePosition;
            // Calculate rectangle for visual feedback
            Point localStart = this.PointToClient(startPos);
            Point localCurrent = this.PointToClient(currentPos);
            
            int x = Math.Min(localStart.X, localCurrent.X);
            int y = Math.Min(localStart.Y, localCurrent.Y);
            int w = Math.Abs(localStart.X - localCurrent.X);
            int h = Math.Abs(localStart.Y - localCurrent.Y);
            
            selectionRect = new Rectangle(x, y, w, h);
            this.Invalidate(); 
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (isDragging) { isDragging = false; currentPos = Control.MousePosition; SaveAndExit(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (selectionRect.Width > 0)
        {
            using (Pen pen = new Pen(Color.Cyan, 2)) e.Graphics.DrawRectangle(pen, selectionRect);
            using (Brush brush = new SolidBrush(Color.FromArgb(50, Color.Cyan))) e.Graphics.FillRectangle(brush, selectionRect);
        }
    }

    private void SaveAndExit()
    {
        // Lock Check: Ensure no other Get Mag component has the file open
        if (File.Exists(iniPath))
        {
            try
            {
                using (FileStream fs = new FileStream(iniPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
            }
            catch (IOException)
            {
                MessageBox.Show("ACCESS DENIED: The config file is locked.\n\nPlease stop the Capture Engine or Launcher before saving coordinates.", 
                                "File Lock Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        // Calculate absolute screen coordinates for the INI
        int x1 = Math.Min(startPos.X, currentPos.X);
        int y1 = Math.Min(startPos.Y, currentPos.Y);
        int x2 = Math.Max(startPos.X, currentPos.X);
        int y2 = Math.Max(startPos.Y, currentPos.Y);

        WritePrivateProfileString("Click1", "X", x1.ToString(), iniPath);
        WritePrivateProfileString("Click1", "Y", y1.ToString(), iniPath);
        WritePrivateProfileString("Click2", "X", x2.ToString(), iniPath);
        WritePrivateProfileString("Click2", "Y", y2.ToString(), iniPath);
        
        // FLUSH CACHE
        WritePrivateProfileString(null, null, null, iniPath);

        UnregisterHotKey(this.Handle, 1);
        UnregisterHotKey(this.Handle, 2);
        this.Hide();

        MessageBox.Show($"Region Saved!\n\nX1: {x1}, Y1: {y1}\nX2: {x2}, Y2: {y2}", "Success");
        Application.Exit();
    }

    [STAThread]
    static void Main()
    {
        if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
        Application.EnableVisualStyles();
        Application.Run(new RegionSelector());
    }
}
