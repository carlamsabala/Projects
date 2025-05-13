# Automated-Code-Translation-JDB-4310

This tool, developed for Robins AFB-402 Software Engineering Group, translates Delphi Pascal code into C# using Retrieval-Augmented Generation (RAG) powered by OpenAI. The web-based app is tailored for military and government developers seeking to modernize legacy Delphi systems. It streamlines translation, preserves context, and reduces errors—saving time and effort.

---

## 🚀 Version 5.0.0 — Final Release
---
### 🎉 Features & Enhancements

- **🧠 RAG-Enhanced Translation**  
  Leverages OpenAI's `gpt-4o-mini` model with Retrieval-Augmented Generation using FAISS to produce more accurate C# code from Delphi input.

- **🖊️ CodeMirror-Based Editors**  
  Rich text editors with syntax highlighting for both Delphi and C#. Includes:  
  - Placeholders for better UX  
  - Read-only C# output box  
  - Fully responsive design

- **📂 File Upload & Drag-and-Drop**  
  - Users can drag `.dpr` files anywhere across the Translator interface  
  - Also supports file browsing via "Browse Files" button  
  - Extracted code is inserted directly into the Delphi editor

- **📁 Translation History (File Drive)**  
  Saved translations are stored in CouchDB with:  
  - Title (generated from Delphi input or user-provided)  
  - Timestamp  
  - Full Delphi and C# code  
  Users can:  
  - Search by title or Delphi content  
  - View results in either list or grid view  
  - Preview input code in grid view cards  
  - Edit translation titles  
  - Delete translations permanently

- **⏱️ Translation Performance Logs**  
  Time taken for each translation is logged to the console using `performance.now()` — useful for diagnostics and future monitoring integration.

- **🔐 Authentik OIDC Integration**  
  Handles all authentication securely using Authentik:  
  - OIDC flow using access tokens and authorization endpoints  
  - Works for all users running Docker on different machines  
  - Authentik is configured via Docker with PostgreSQL and Redis

- **📤 Feedback Submission**  
  A floating feedback button (`?`) appears on every screen, allowing users to send emails to the dev team at `2msgactdev@gmail.com`.

- **🚪 Logout Support**  
  Navbar logout button uses Authentik's `end-session` endpoint to terminate user sessions securely.

- **🧹 UI & UX Enhancements**
  - Favicon now appears in browser tabs  
  - Tooltips (`title` attributes) for Translate, Copy, Download, and Grid/List toggle buttons  
  - Consistent navbar icon sizes and hover behavior  
  - Layout improvements for responsiveness  
  - Three-dot dropdown menus for editing and deleting translations

---

### ⚠️ Known Issues

- No persistent logging/analytics platform—recommend future teams implement Sentry or ELK stack  
- Translation quality not formally benchmarked (estimated ~85–90% accuracy)

---

### 📄 Documentation

- [Install Guide](./docs/INSTALL.md)
- [Architecture & Design](./docs/Architecture%20&%20Design.pdf)
- [Authentication & Security](./docs/Authentication%20&%20Security.pdf)
- [Database Schema](./docs/Database.pdf)
- [Deployment & Infrastructure](./docs/Deployment%20&%20Infrastructure.pdf)
- [High-Level Overview & User Guide](./docs/High-Level%20Overview%20&%20User%20Guide.pdf)
- [Monitoring & Logging](./docs/Monitoring%20&%20Logging.pdf)
- [Testing Strategy](./docs/Testing.pdf)
- [OpenAPI Swagger Spec](./docs/openapi.yaml)
---

### 🧱 Tech Stack

- **Frontend**: HTML, CSS, JavaScript, CodeMirror  
- **Backend**: Node.js, Express  
- **LLM Integration**: Python (Flask), OpenAI API, FAISS, Retrieval-Augmented Generation  
- **Database**: CouchDB (for translations), PostgreSQL (via Authentik)  
- **Authentication**: Authentik OIDC  
- **Deployment**: Docker + Docker Compose

---