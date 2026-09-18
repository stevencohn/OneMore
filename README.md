[![version](https://img.shields.io/github/v/release/stevencohn/OneMore?display_name=tag&color=7E5C81)](https://github.com/stevencohn/OneMore/releases/latest) [![downloads](https://img.shields.io/github/downloads/stevencohn/OneMore/total?color=blue)](https://github.com/stevencohn/OneMore/releases/latest) [![platform](https://img.shields.io/badge/platform-windows%20%7C%20onenote%20desktop-649BC1)](https://onemoreaddin.com/get-started/How%20to%20Install%20OneNote.htm) [![GitHub license](https://img.shields.io/badge/license-mpl--2.0-BF6A48)](https://github.com/stevencohn/OneMore/blob/main/LICENSE) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://onemoreaddin.com/developers/Setup.htm) ![](https://tokei.rs/b1/github/project-jedi/jcl)


# <img src="OneMore/Properties/Images/Logo48.png" alt="Description" width="36" height="36"> OneMore - a OneNote Add-in

OneMore is an add-in for OneNote with simple and powerful features that make OneNote a better OneNote.

* Download the [latest release](https://github.com/stevencohn/OneMore/releases/latest)
* Read the [installation instructions](https://onemoreaddin.com/get-started/How%20to%20Install%20OneMore.htm)
* See the [OneMore Guide](https://onemoreaddin.com/) for complete feature documentation and Command descriptions

## Everything OneNote should have shipped with

A few hundred small conveniences, all built into one add-in. Free, open source, and installs in under a minute.

### Key Features

- **[Command Palette](https://onemoreaddin.com/the-basics/Command%20Palette.htm)** — Access 200+ commands or use the Quick Palette to apply styles, without leaving the keyboard
- **Styles & Formatting** — One-click [text styles](https://onemoreaddin.com/commands/My%20Styles%20Commands.htm) and [table styles](https://onemoreaddin.com/commands/Table%20Style%20Commands.htm), custom color themes, and consistent formatting across every notebook
- **[Table of Contents](https://onemoreaddin.com/commands/Snippets%20Commands.htm#table-of-contents)** — Auto-generate a table of contents for any page, section, or notebook — kept in sync as you edit
- **Code & Markdown** — Paste [Markdown](https://onemoreaddin.com/commands/Edit%20Commands.htm#markdown) straight into OneNote, or drop in [syntax-highlighted](https://onemoreaddin.com/commands/Colorize%20Command.htm) code boxes for snippets
- **Favorites & Hashtags** — Pin your most-used pages as [Favorites](https://onemoreaddin.com/the-basics/Favorites.htm), and tag notes with [inline #hashtags](https://onemoreaddin.com/commands/Hashtag%20Commands.htm) to find them instantly
- **[Image Editing & Diagrams](https://onemoreaddin.com/commands/Image%20Commands.htm)** — Crop, rotate, and adjust images without leaving OneNote — plus render Mermaid and PlantUML diagrams
- **[Snippets](https://onemoreaddin.com/commands/Snippets%20Commands.htm)** — Save any block of content as a reusable snippet, then drop it into any page whenever you need it
- **[Free & Open Source](https://github.com/stevencohn/OneMore)** — No subscription, no telemetry opt-in required, no catch. Built in the open on GitHub since 2018

### Popular with Users

Top commands from real usage across the OneMore community:
- **[Apply Style](https://onemoreaddin.com/commands/My%20Styles%20Commands.htm)** — 23,902 uses/month
- **[Paste Text](https://onemoreaddin.com/commands/Edit%20Commands.htm#paste-and-keep-text-only)** — 9,055 uses/month  
- **[Insert TOC](https://onemoreaddin.com/commands/Snippets%20Commands.htm#table-of-contents)** — 8,543 uses/month
- **[Join Paragraph](https://onemoreaddin.com/commands/Edit%20Commands.htm#join-paragraph)** — 7,670 uses/month
- **[Insert Code Box](https://onemoreaddin.com/commands/Snippets%20Commands.htm#code-box)** — 5,197 uses/month
- **[Colorize](https://onemoreaddin.com/commands/Colorize%20Command.htm)** — 5,155 uses/month

See the full [OneMore User Guide](https://onemoreaddin.com/) for complete feature documentation and command descriptions.

### Get Started

- **[Installation Guide](https://onemoreaddin.com/get-started/index.html)** — Step-by-step setup instructions
- **[Feature Documentation](https://onemoreaddin.com/)** — Full user guide and command reference
- **[Command Line Interface](https://onemoreaddin.com/the-basics/OneMore%20CLI.htm)** — Automate 40+ commands from the command line

---

# For Developers

OneMore is a comprehensive OneNote add-in built with C# and the OneNote Object Model. The project is organized into multiple components to support core functionality, testing, and deployment.

### Repository Structure

**Core Components**
- **OneMore/** — Primary add-in source code and ribbon UI
- **OneMoreCalendar/** — Specialized calendar application for tracking page creation/modification
- **OneMoreCli/** — Command-line interface runner for automating OneMore commands
- **OneMoreTray/** — System tray application component
- **OneMoreProtocolHandler/** — URI protocol handler for deep linking

**Setup & Installation**
- **OneMoreSetup/** — Installation package and setup wizard
- **OneMoreSetupActions/** — Custom setup actions and configuration
- **OneMoreBundle/** — Bundle packaging utilities

**Testing & Quality**
- **OneMoreTests/** — Unit and integration test suite

**Extensibility**
- **Plugins/** — Plugin architecture for extending OneMore functionality
- **Templates/** — Reusable content templates
- **Themes/** — UI theme definitions and customization

**Documentation & Resources**
- **docs/** — User documentation, guides, and website source
- **Reference/** — Reference materials and specifications
- **packers/** — Packaging and distribution utilities

### Build & Development

The project uses a Visual Studio solution structure (OneMore.sln) with PowerShell build scripts for compilation and deployment.

**Key Technologies**
- **Language:** C# (.NET Framework)
- **Platform:** Windows Desktop with OneNote Desktop integration
- **UI Framework:** OneNote Ribbon UI and WinForms/WPF components
- **Testing:** Unit tests in OneMoreTests

**Build Requirements**
- Visual Studio 2026 Community Edition
- .NET Framework 4.8.1
- OneNote Desktop Edition

See the [Developers Guide](https://onemoreaddin.com/developers/Setup.htm) for detailed setup instructions and contribution guidelines.

### Solution Architecture

The modular architecture allows OneMore to:
- Maintain a stable core add-in (OneMore/) while supporting specialized features
- Provide automation through the CLI runner without bloating the main add-in
- Enable plugin-based extensibility for advanced users
- Separate concerns between UI (Calendar, Tray), backend (Core), and tooling (CLI)

### License & Privacy

OneMore is released under the [Mozilla Public License 2.0](https://github.com/stevencohn/OneMore/blob/main/LICENSE). See the [Privacy Policy](https://onemoreaddin.com/get-started/Privacy%20Policy.htm) for information about how we handle user data — we don't collect personal information and remain committed to your privacy.

### Contributing

Contributions are welcome! Please see the [Developers Guide](https://onemoreaddin.com/developers/Setup.htm) for information on setting up a development environment and submitting pull requests.

---

**© 2020 Steven M Cohn. All rights reserved.**
