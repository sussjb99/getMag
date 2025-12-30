# GET MAG – Region Selector
Technical Documentation for `RegionSelector` (region.cs)

## 1. Overview
The `RegionSelector` is a specialized utility that provides a visual, transparent overlay for defining screen capture boundaries. It allows users to "draw" a rectangle over any portion of their screen (including multi-monitor setups) and saves those absolute coordinates directly to the system configuration.

**Design goals:**
- **Invisible Startup:** Launches with 0% opacity to remain non-intrusive until summoned.
- **Multi-Monitor Support:** Dynamically spans the entire `VirtualScreen` area.
- **Visual Feedback:** Real-time rendering of the selection area using GDI+.
- **DPI Awareness:** Ensures pixel-perfect coordinate capture on high-resolution displays.

---

## 2. Display & Overlay Logic

### 2.1 Multi-Monitor Geometry
Unlike standard forms, the `RegionSelector` does not target a single monitor. It queries `SystemInformation.VirtualScreen` to calculate the total bounding box of all connected displays.
- **Location:** Set to the top-leftmost coordinate of the primary or extended display.
- **Size:** Set to the total width and height of the combined desktop area.

### 2.2 Transparency States
The form utilizes a state-based visibility model:
1.  **Initialized:** `Opacity = 0.0`. The form is active but invisible to the user.
2.  **Revealed:** Triggered by `<CTRL><ALT><F12>`. `Opacity` shifts to `0.35` (35% black tint), signaling that the user can now begin the selection.

---

## 3. Interaction Workflow

### 3.1 Selection Mechanism
The utility tracks global mouse positions to handle selections that may start on one monitor and end on another.



| Event | Logic |
|:---|:---|
| **OnMouseDown** | Sets `startPos` using `Control.MousePosition` (Absolute Screen Coordinates). |
| **OnMouseMove** | Calculates the delta between `startPos` and current position to update the `selectionRect` for the `OnPaint` event. |
| **OnMouseUp** | Captures `currentPos`, stops dragging, and initiates the `SaveAndExit` sequence. |

### 3.2 Drawing Logic (`OnPaint`)
The selection is rendered using a Cyan (Light Blue) theme for high visibility against dark or busy backgrounds:
- **Border:** A 2-pixel wide Cyan pen.
- **Fill:** A semi-transparent Cyan brush (`Alpha 50`) to highlight the selected area.

---

## 4. Technical Reference & Interop

### 4.1 Win32 API Integration
| API | Purpose |
|:---|:---|
| `SetProcessDPIAware` | Prevents Windows from scaling the app, ensuring captured coordinates match physical screen pixels. |
| `RegisterHotKey` | Maps `<CTRL><ALT><F12>` to reveal the form and `<ESC>` to abort. |
| `WritePrivateProfileString` | Persists the calculated `X1, Y1, X2, Y2` to `config.ini`. |

### 4.2 File Lock Detection
Before saving, the utility performs a "Sanity Check" on `config.ini`. It attempts to open the file with `FileShare.None`. If this fails, it alerts the user that another component (like the Capture Engine) is currently using the file, preventing data corruption.

---

## 5. Output Data Structure
The captured coordinates are normalized so that `Click1` is always the top-left and `Click2` is always the bottom-right, regardless of the direction the user dragged the mouse.

```ini
[Click1]
X=100  ; Minimum X detected
Y=150  ; Minimum Y detected

[Click2]
X=800  ; Maximum X detected
Y=950  ; Maximum Y detected
