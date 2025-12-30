# Technical Notes: Magazine Capture Engine (start_capture.cs)

## Core Architecture
* **Logic Framework**: A Windows Forms-based automation utility versioned as **1.0.0.0**.
* **Threading**: Utilizes `Task.Run` to handle the capture loop asynchronously, preventing the UI from freezing during operation.
* **System Integration**: Employs **Win32 API** (User32.dll and Kernel32.dll) for hardware-level input simulation and INI file management.

## Input & Navigation Logic
* **Hardware Simulation**: Uses the `SendInput` API with `KEYEVENTF_SCANCODE` to send the `SCAN_RIGHT` (0x4D) signal. This mimics physical keyboard hardware to bypass software-level input restrictions.
* **Window Management**: Calls `SetForegroundWindow` before each input to ensure the browser remains focused.
* **DPI Scaling**: Executes `SetProcessDPIAware()` at startup to ensure screen coordinates map correctly to physical pixels regardless of Windows display scaling settings.

## Unified Licensing System
The engine shares a synchronized security layer with the PDF Compiler:
* **Validation Method**: Uses **HMAC-SHA256** signatures.
* **Regex Pattern**: Validates keys against the format `^(GM-\d{4}-[A-Z0-9]{4})-([A-Z0-9]{8})$`.
* **Enforcement**: 
    * **Pro Mode**: Full access to `maxPages` defined in the INI.
    * **Trial Mode**: Hard-coded limit of **25 pages**.

## Functional Workflow
1. **INI Initialization**: Reads `config.ini` from `%ProgramData%\getMag`.
2. **Setup Phase**: Prompts for magazine name and registers global hotkeys: `CTRL+ALT+G` (Start) and `ESC` (Emergency Stop).
3. **Capture Loop**:
    * Minimizes the utility window to clear the capture area.
    * Parks the mouse cursor at the screen edge.
    * Captures the defined region and generates an **MD5 hash** of the image.
    * Compares the current hash against a `HashSet` of previous pages to detect the end of the magazine.
4. **Data Persistence**: Uses a "Flush" command (`WritePrivateProfileString` with null parameters) to force-write INI changes to the disk immediately, protecting against data loss during crashes.

## File Specifications
* **Config Path**: `C:\ProgramData\getMag\config.ini`.
* **Output Format**: 24-bit PNG images.
* **Log Location**: `[MagazineFolder]\logs\capture_log.txt`.
