using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

public class ConfigForm : Form
{
    private static readonly byte[] SECRET_KEY = Encoding.UTF8.GetBytes("YOUR-SUPER-SECRET-KEY-HERE"); 

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern uint GetPrivateProfileString(string s, string k, string d, StringBuilder r, uint z, string f);
    
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern bool WritePrivateProfileString(string s, string k, string v, string f);

    private string iniPath;
    private TextBox txtFolder, txtDelay, txtMaxPages;
    private TextBox txtX1, txtY1, txtX2, txtY2;
    private TextBox txtVersion, txtLicense;
    private CheckBox chkUpdate; // Added CheckBox reference

    public ConfigForm()
    {
        string commonData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        string dir = Path.Combine(commonData, "getMag");
        
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        this.iniPath = Path.Combine(dir, "config.ini");

        this.Text = "GET MAG - CONFIGURE";
        this.Size = new Size(480, 620); // Increased height to accommodate new field
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.StartPosition = FormStartPosition.CenterScreen;

        SetupUI();
        LoadSettings();
    }

    private void SetupUI()
    {
        int labelX = 35, currentY = 20;
        int alignedFieldOffset = 110;

        Label lblHeader = new Label { Text = "System Parameters", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(labelX, currentY) };
        this.Controls.Add(lblHeader);
        currentY += 45;

        AddInputRow("Version:", ref txtVersion, labelX, currentY, 80, alignedFieldOffset);
        txtVersion.ReadOnly = true;
        currentY += 35;

        AddInputRow("License Key:", ref txtLicense, labelX, currentY, 250, alignedFieldOffset);
        currentY += 40;

        // Added CheckBox for Updates
        chkUpdate = new CheckBox { 
            Text = "Check for Updates on Startup", 
            Font = new Font("Segoe UI", 9), 
            Location = new Point(labelX + alignedFieldOffset, currentY), 
            AutoSize = true 
        };
        this.Controls.Add(chkUpdate);
        currentY += 40;

        Label lblFolder = new Label { Text = "Output Folder:", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true, Location = new Point(labelX, currentY) };
        txtFolder = new TextBox { Location = new Point(labelX, currentY + 22), Width = 400 };
        this.Controls.Add(lblFolder);
        this.Controls.Add(txtFolder);
        currentY += 65;

        AddInputRow("Key Delay (ms):", ref txtDelay, labelX, currentY, 65, alignedFieldOffset);
        AddInputRow("Max Pages:", ref txtMaxPages, 250, currentY, 60, 85);
        currentY += 55;

        GroupBox grpCoords = new GroupBox {
            Text = "Capture Coordinates",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(labelX, currentY),
            Size = new Size(400, 105)
        };
        this.Controls.Add(grpCoords);

        txtX1 = AddCoordInputToGroup("X1:", 20, 35, grpCoords);
        txtY1 = AddCoordInputToGroup("Y1:", 210, 35, grpCoords);
        txtX2 = AddCoordInputToGroup("X2:", 20, 70, grpCoords);
        txtY2 = AddCoordInputToGroup("Y2:", 210, 70, grpCoords);

        // Positioned buttons relative to the group box
        Button btnSave = new Button { Text = "Save Settings", Location = new Point(100, currentY + 120), Size = new Size(120, 40), Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.LightSkyBlue };
        btnSave.Click += (s, e) => SaveSettings();

        Button btnExit = new Button { Text = "Exit", Location = new Point(250, currentY + 120), Size = new Size(120, 40) };
        btnExit.Click += (s, e) => Application.Exit();

        this.Controls.Add(btnSave);
        this.Controls.Add(btnExit);
    }

    private void LoadSettings()
    {
        string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string defaultPath = Path.Combine(myDocuments, "Magazines");

        string savedPath = ReadIni("Settings", "FolderLocation", "");
        txtFolder.Text = string.IsNullOrWhiteSpace(savedPath) ? defaultPath : savedPath;

        txtDelay.Text = ReadIni("Settings", "KeyDelay", "3000");
        txtMaxPages.Text = ReadIni("Settings", "MaxPages", "11");
        txtVersion.Text = ReadIni("Settings", "SoftwareVersion", "1.0.0");
        txtLicense.Text = ReadIni("Settings", "LicenseKey", "");
        
        // Load CheckBox state
        string updateVal = ReadIni("Settings", "CheckForUpdate", "True");
        chkUpdate.Checked = updateVal.Equals("True", StringComparison.OrdinalIgnoreCase);

        txtX1.Text = ReadIni("Click1", "X", "0");
        txtY1.Text = ReadIni("Click1", "Y", "0");
        txtX2.Text = ReadIni("Click2", "X", "0");
        txtY2.Text = ReadIni("Click2", "Y", "0");
    }

    private void SaveSettings()
    {
        string keyInput = txtLicense.Text.Trim();

        if (!string.IsNullOrWhiteSpace(keyInput))
        {
            if (!ValidateLicenseWithKey(keyInput))
            {
                MessageBox.Show("Invalid License Key!", "Security Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        try {
            if (!Directory.Exists(txtFolder.Text)) Directory.CreateDirectory(txtFolder.Text);
        } catch { }

        WritePrivateProfileString("Settings", "FolderLocation", txtFolder.Text, iniPath);
        WritePrivateProfileString("Settings", "KeyDelay", txtDelay.Text, iniPath);
        WritePrivateProfileString("Settings", "MaxPages", txtMaxPages.Text, iniPath);
        WritePrivateProfileString("Settings", "SoftwareVersion", txtVersion.Text, iniPath);
        WritePrivateProfileString("Settings", "LicenseKey", keyInput, iniPath);
        // Save CheckBox state
        WritePrivateProfileString("Settings", "CheckForUpdate", chkUpdate.Checked.ToString(), iniPath);
        
        WritePrivateProfileString("Click1", "X", txtX1.Text, iniPath);
        WritePrivateProfileString("Click1", "Y", txtY1.Text, iniPath);
        WritePrivateProfileString("Click2", "X", txtX2.Text, iniPath);
        WritePrivateProfileString("Click2", "Y", txtY2.Text, iniPath);

        WritePrivateProfileString(null, null, null, iniPath);
        MessageBox.Show("Configuration Saved Successfully!", "Success");
    }

    private bool ValidateLicenseWithKey(string license)
    {
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

    private string ReadIni(string s, string k, string d)
    {
        StringBuilder sb = new StringBuilder(255);
        GetPrivateProfileString(s, k, d, sb, 255, iniPath);
        return sb.ToString().Trim();
    }

    private void AddInputRow(string labelText, ref TextBox tb, int x, int y, int width, int offset)
    {
        this.Controls.Add(new Label { Text = labelText, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(x, y), AutoSize = true });
        tb = new TextBox { Location = new Point(x + offset, y - 3), Width = width };
        this.Controls.Add(tb);
    }

    private TextBox AddCoordInputToGroup(string labelText, int x, int y, GroupBox grp)
    {
        grp.Controls.Add(new Label { Text = labelText, Font = new Font("Segoe UI", 9), Location = new Point(x, y), AutoSize = true });
        TextBox tb = new TextBox { Location = new Point(x + 40, y - 3), Width = 60 };
        grp.Controls.Add(tb);
        return tb;
    }

    [STAThread]
    static void Main() 
    { 
        Application.EnableVisualStyles(); 
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ConfigForm()); 
    }
}
