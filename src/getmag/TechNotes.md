# GET MAG – Launcher Utility
Technical Documentation for `getMag` (getMag.cs)

## 1. Overview
The `getMag.cs` utility serves as the primary entry point and lifecycle manager for the GET MAG ecosystem. It acts as a wrapper that ensures environmental prerequisites are met, prevents duplicate process execution, and handles background update checks before handing off execution to the core capture engine.

**Design goals:**
- **Single Instance Enforcement:** Prevents resource conflicts.
- **Environment Sanitization:** Ensures required folder structures exist.
- **Decoupled Updates:** Launches the update checker as a non-blocking process.
- **User-Friendly Errors:** Provides clear GUI-based feedback if components are missing.

---

## 2. Process Management

### 2.1 Singleton Instance (Mutex)
The application uses a system-wide `Mutex` to ensure only one instance of the software is running at any given time.
- **Identifier:** `{4A365DC4-2249-4C4C-939B-9140304DE5A9}`.
- **Inno Setup Integration:** This GUID matches the AppId used in the installer, allowing the installer to detect if the application is still running during an upgrade or uninstall.

### 2.2 Execution Flow

1.  **Mutex Acquisition:** Check if an instance is already running.
2.  **Environment Check:** Verify and create `%USERPROFILE%\Documents\Magazines`.
3.  **Update Logic:** Parse `config.ini` to determine if `check_for_update.exe` should be triggered.
4.  **Handoff:** Launch `launcher.exe` and terminate the launcher utility.

---

## 3. Configuration & Updates

### 3.1 Update Check Logic
The launcher reads the machine-wide `config.ini` using the `kernel32.dll` interop layer. 

| Setting | Default | Action |
|:---|:---|:---|
| `CheckForUpdate` | "True" | If "True", the utility attempts to launch `check_for_update.exe` from the base directory. |

The update check is "Fire-and-Forget"—the launcher does not wait for the updater to finish, ensuring the main application opens immediately for the user.

---

## 4. API & Interop Reference

### 4.1 Native Imports
* **`GetPrivateProfileString`**: Used to read the update preferences from the INI file.

### 4.2 Methods

#### `PrepareEnvironment()`
- Targets the `SpecialFolder.MyDocuments` directory.
- Silently creates the `Magazines` folder if it has been deleted or is missing.
- Wrapped in a `try-catch` to prevent launch failure if directory permissions are restrictive.

#### `LaunchTarget(string targetApp)`
- Resolves the absolute path for `launcher.exe`.
- Sets the `WorkingDirectory` to the application's base directory to ensure relative pathing for dependencies (like DLLs) works correctly.

---

## 5. Technical Specifications

| Property | Value |
|:---|:---|
| **Namespace** | `GetMagLauncher` |
| **Threading Model** | `STAThread` (Required for Windows Forms compatibility) |
| **Error Handling** | GUI Message Boxes for missing files/launch errors. |
| **Assembly Company** | Ottawa Moose |

---

## 6. Implementation Notes
- **Base Directory:** The utility uses `AppDomain.CurrentDomain.BaseDirectory` rather than `Directory.GetCurrentDirectory()` to ensure the correct path is resolved even if the launcher is called from a command line or a different shortcut context.
- **Mutex Lifetime:** The `using` block ensures the Mutex is released only after the `LaunchTarget` call is initiated.
