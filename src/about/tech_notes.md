# 📄 Component Technical Detail: `About.cs`

The **About Utility** provides the user-facing identity for the getMag suite. It is designed to be a lightweight, high-compatibility Windows Forms dialog.

## 🛠 Core Responsibilities
1. **Branding:** Displays the corporate logo and software title.
2. **Version Tracking:** Clearly states the current build version.
3. **Legal/Compliance:** Displays copyright years and proprietary license warnings.
4. **Support Routing:** Provides direct links to support and contact web pages.

---

## 🏗 Architectural Features

### 1. Instance Synchronization (Mutex)
To prevent the user from opening multiple "About" windows simultaneously, the code implements a local Mutex.
* **ID:** `Global\GetMag_About_SingleInstance_Lock`
* **Implementation:** `mutex.WaitOne(0, false)` is checked inside the `Main()` method. If the Mutex is already held, the new process exits instantly.

### 2. Layout & Typography
The UI is constructed programmatically (without a `.Designer.cs` file) to keep the executable size minimal and portable.
* **Fonts:** Utilizes `Segoe UI` (the modern Windows system font).
* **Styles:** Uses a mix of `FontStyle.Bold` for headers and `FontStyle.Regular` for body text to create a clean visual hierarchy.

### 3. Smart Asset Loading
The logo is loaded dynamically to ensure compatibility with various installation paths:
```csharp
string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.jpg");