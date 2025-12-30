# GET MAG – Hotkey Launcher
Technical Documentation for `LauncherForm` (launcher.cs)

## 1. Overview
The Hotkey Launcher is the central control hub for the GET MAG toolset. It operates as a persistent background utility that listens for global system-wide hotkeys, allowing users to trigger various application components (Capture, Configuration, PDF Conversion) without needing to navigate through file directories.

**Design goals:**
- **Global Accessibility:** Responds to hotkeys even when the application is not in focus.
- **Component Orchestration:** Acts as a dispatcher for separate executable modules.
- **Instance Persistence:** Prevents multiple launcher instances using Mutex and Window focus restoration.
- **Resource Cleanup:** Ensures all registered system hooks are released on exit.

---

## 2. Hotkey Management
The application utilizes the Win32 `RegisterHotKey` API to hook into the Windows message queue. 

### 2.1 Modifier Keys
All system commands are mapped to the dual-modifier combination:
* **MOD_CONTROL (0x0002)**
* **MOD_ALT (0x0001)**

### 2.2 Command Mapping

| Hotkey | Target Action | Executable / Method |
|:---|:---|:---|
| **Ctrl + Alt + A** | About Dialog | `about.exe` |
| **Ctrl + Alt + H** | User Manual | `help.html` (Browser) |
| **Ctrl + Alt + R** | Region Selection | `region.exe` |
| **Ctrl + Alt + C** | System Configuration | `configure.exe` |
| **Ctrl + Alt + S** | Start Capture Engine | `start_capture.exe` |
| **Ctrl + Alt + P** | Compile to PDF | `convert_to_pdf.exe` |
| **Ctrl + Alt + V** | Open Output Folder | `OpenOutput()` |

---

## 3. Architecture & Windows Messaging

### 3.1 The Window Procedure (`WndProc`)
Because hotkeys are delivered as Windows messages, the launcher overrides `WndProc` to intercept `WM_HOTKEY` (0x0312). 



When a `WM_HOTKEY` message is received, the `WParam` is parsed to identify the unique ID (1-7) assigned during registration, triggering the corresponding `Run()` or `OpenOutput()` method.

### 3.2 Singleton Enforcement & Window Restoration
The `Main` method uses a global Mutex (`Global\GetMag_Final_Lock_ID`). If a second instance is launched:
1. It uses `FindWindow` to locate the existing "GET MAG — HOTKEYS" window.
2. If minimized, it uses `ShowWindow` with `SW_RESTORE`.
3. It uses `SetForegroundWindow` to bring the existing controls to the user's attention.

---

## 4. API & Interop Reference

### 4.1 Imported Win32 Functions
| API | Purpose |
|:---|:---|
| `RegisterHotKey` | Reserves a key combination system-wide for this window. |
| `UnregisterHotKey` | Frees the reserved keys back to the OS. |
| `SetForegroundWindow` | Forces the existing launcher window to the front. |
| `GetPrivateProfileString` | Reads the `FolderLocation` from `config.ini` to open the viewer. |

### 4.2 Lifecycle Methods

#### `OnFormClosing`
Crucial for system stability. It iterates through all 7 registered IDs and calls `UnregisterHotKey`. Failure to do this can occasionally leave "ghost" hotkeys reserved until the user logs out of Windows.

#### `OpenOutput()`
- Queries `config.ini` for the user's custom output path.
- **Self-Healing:** If the path in the INI no longer exists, it attempts to recreate it or falls back to the default `MyDocuments\Magazines` path before launching `explorer.exe`.

---

## 5. UI Design
The interface is a fixed 400x320 dialog using a clean, tabular layout.
- **Typography:** Uses **Segoe UI** with bold weight for headers.
- **Logic:** The UI serves primarily as a "Cheat Sheet" so users can remember the assigned hotkeys while the app runs in the background.

---

## 6. Implementation Notes
- **Composability:** Each `Run()` call uses `UseShellExecute = true`, ensuring that the child processes inherit the correct environment variables.
- **Memory Management:** `GC.KeepAlive(_mutex)` is used at the end of `Main` to ensure the garbage collector does not dispose of the Mutex while the application is still running.
