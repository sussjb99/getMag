# GET MAG – PDF Compiler
Technical Documentation for `PdfCompiler` (convert_to_pdf.cs)

## 1. Overview
The GET MAG PDF Compiler is a specialized utility designed to aggregate individual magazine page captures (PNG) into a single, high-quality PDF document. It leverages the **PdfSharp** library for document generation and features an automated watermarking system for unlicensed users.

**Design goals:**
- **Automated Workflow:** Minimal user interaction required beyond naming the magazine.
- **Dynamic Licensing:** Real-time feature adjustment (Watermarking vs. Clean output).
- **Embedded Dependencies:** Uses `AssemblyResolve` to load PdfSharp from resources.
- **Traceability:** Detailed logging of success and error states.

## 2. Document Generation Logic

### 2.1 File Discovery & Ordering
The utility scans the target directory for `.png` files and applies a specific sorting algorithm to ensure pages are compiled in the correct sequence:
1.  **Primary Sort:** String length (to ensure "Page9.png" comes before "Page10.png").
2.  **Secondary Sort:** Alphabetical (standard filename order).

### 2.2 Image Processing
- Each image is loaded as an `XImage` object.
- The PDF page dimensions are dynamically set to match the native resolution of the source image (`img.PointWidth` x `img.PointHeight`).
- This preserves the aspect ratio and quality of the original screen capture.

---

## 3. License-Driven Watermarking

The compiler checks the system's `config.ini` file for a valid license key using the same HMAC‑SHA256 validation logic as the configuration utility.

### 3.1 Demo Mode vs. Pro Mode
| Feature | Unlicensed (Demo) | Licensed (Pro) |
| :--- | :--- | :--- |
| **Status Display** | "Compiling PDF (DEMO MODE)..." | "Compiling PDF..." |
| **Watermark** | Diagonal text: "DEMO GETMAG.EXE" | No Watermark |
| **Log Entry** | Flagged as `[DEMO]` | Standard entry |

### 3.2 Watermark Implementation

The `ApplyWatermark` method calculates a diagonal angle based on the page's aspect ratio using `Math.Atan2`. It renders semi-transparent (Alpha 120) gray text in the center of the page.

---

## 4. Technical Architecture

### 4.1 Dependency Management
To maintain a "Zero External Dependency" user experience, the required `PdfSharp-gdi.dll` is embedded as a manifest resource. The `Main()` method includes an `AssemblyResolve` handler that extracts and loads the DLL into memory at runtime if it is missing from the local folder.

### 4.2 Logging System
Events are written to `%MagazineFolder%\logs\pdf_conversion_log.txt`.
- **Timestamped:** All entries include the time of the event.
- **Exception Handling:** Catch-all blocks ensure the compiler fails gracefully if an image is corrupt or the disk is full.

---

## 5. API Reference (Internal Methods)

### `RunConversion()`
The primary execution loop. It handles the UI update, file sorting, PDF object initialization, and final disk persistence.

### `ApplyWatermark(page, gfx, text)`
- **Parameters:** `PdfPage` (current), `XGraphics` (current context), `string` (text to render).
- **Visuals:** Uses a 60pt Bold Arial font with a `TranslateTransform` to the page center.

### `LogPdfEvent(string message)`
Handles the creation of the `\logs` subdirectory and appends diagnostic data to the text log.

---

## 6. Execution Flow
1. **Init:** Load `iniPath` from `CommonApplicationData`.
2. **License Check:** Validate stored key against `SECRET_KEY`.
3. **User Input:** Prompt for the Magazine Name (folder name).
4. **Validation:** Ensure the folder exists and contains PNGs.
5. **Compile:** Iterate through files, draw to PDF, apply watermark if necessary.
6. **Finalize:** Save PDF and open Windows Explorer to the output file.
