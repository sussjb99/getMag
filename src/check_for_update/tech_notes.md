# 📄 Module Technical Specification — `check_for_update.cs`

The **Update Checker Utility** is a standalone Windows Forms executable responsible for determining whether a newer version of **getMag** is available. It performs a lightweight, silent version check against the GitHub repository and notifies the user only when an update exists. The module is intentionally minimal, portable, and designed to avoid interfering with the user’s workflow.

---

## 🛠 Core Responsibilities

- **Remote Version Retrieval**  
  Contacts the GitHub API to fetch the latest published version number stored in the repository.

- **Version Comparison**  
  Compares the locally running assembly version with the remote version.

- **User Notification**  
  Displays a minimal dialog only when an update is available.

- **Download Routing**  
  Provides a direct link to the GitHub Releases page for the latest build.

- **Non‑intrusive Behavior**  
  Silently ignores all errors to avoid blocking startup or annoying the user.

---

## 🏗 Architectural Features

### 1. Secure Remote Fetch (TLS 1.2)

GitHub requires TLS 1.2 for all API requests.  
The module explicitly sets:


### 2. GitHub API Integration

The update checker retrieves the latest version number from a file stored in the repository:
https://api.github.com/repos/sussjb99/getMag/contents/src/check_for_update/current_version




