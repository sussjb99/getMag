# GET MAG – Configuration Utility  
Technical Documentation for `ConfigForm` (config.cs)

## 1. Overview
The GET MAG Configuration Utility is a standalone WinForms application that 
manages all user‑editable settings for the GET MAG automation tool. 

It provides a graphical interface for editing values stored in a machine‑wide INI file
and includes built‑in license key validation using HMAC‑SHA256.

**Design goals:**
- **Zero external dependencies:** Uses native .NET and Win32 libraries.
- **Audit‑friendly storage:** Plain-text INI configuration.
- **Simple UI:** Fixed-layout, programmatic UI (no `.designer.cs`).
- **Compatibility:** Lightweight footprint for various Windows environments.

## 2. Configuration Storage

### 2.1 INI File Path
The configuration file is stored under:  
`%CommonApplicationData%\getMag\config.ini`  
(Typically `C:\ProgramData\getMag\config.ini`)

This ensures:
- **Machine‑wide settings:** Consistent behavior across different user accounts.
- **Accessibility:** Readable by the application without requiring administrative elevation for reading.
- **Resilience:** The directory is automatically created upon initialization if missing.

### 2.2 INI Structure
The file is organized into three primary sections:
* `[Settings]`: Global application parameters (Path, Delay, Version, License, Update flag).
* `[Click1]`: Primary coordinate pair (X, Y).
* `[Click2]`: Secondary coordinate pair (X, Y).

---

## 3. INI Access Layer
The program interfaces with the Windows kernel to handle file I/O via P/Invoke:

| API | Purpose |
|:---|:---|
| `GetPrivateProfileString` | Retrieves a string from the specified section in the INI file. |
| `WritePrivateProfileString` | Writes or deletes a string in the specified section. |

### 3.1 Wrapper Methods

#### `ReadIni(section, key, default)`
- Facilitates retrieval of settings.
- Returns a trimmed string to prevent whitespace errors.
- Automatically returns the provided `default` value if the key is not found.

#### `WritePrivateProfileString(...)`
- Directly persists UI values to the file.
- **Note:** A final call using `(null, null, null)` is executed to flush the Windows cache to physical storage.

---

## 4. UI Architecture
The UI is built entirely in code within the `SetupUI()` method. 
This "code-first" approach ensures no hidden designer dependencies and simplifies version control tracking.

### 4.1 Layout Components

| Component | Purpose |
|:---|:---|
| **Version (Read-only)** | Displays the current software version. |
| **License Key** | Field for user-entered activation strings. |
| **Check for Updates** | Boolean CheckBox for startup behavior. |
| **Output Folder** | Path where magazines/captured content are saved. |
| **Key Delay (ms)** | Timing control for automated input sequences. |
| **Max Pages** | Integer limit for the capture loop. |
| **Capture Coordinates** | GroupBox containing X1/Y1 and X2/Y2 fields. |
| **Save / Exit Buttons** | Triggers validation/persistence or terminates the app. |

All controls use **Segoe UI** for a modern Windows appearance.

---

## 5. License Key Validation

### 5.1 Expected Format
The system expects a specific string pattern:  
`GM-XXXX-XXXX-YYYYYYYY`  
*(Where `X` represents identity data and `Y` is the 8-character HMAC signature)*

### 5.2 Validation Logic (HMAC-SHA256)
The security layer performs the following steps:
1. **Pattern Match:** Uses Regex to split the key into "DataToVerify" and "SignatureProvided".
2. **Hashing:** Re-computes a hash of the data part using the internal `SECRET_KEY`.
3. **Truncation:** Converts the hash to a hex string and takes the first 8 characters.
4. **Comparison:** Performs a case-insensitive match to grant or deny access.
