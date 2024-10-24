# LNjector

**LNjector** is a lightweight Windows DLL injector designed to inject dynamic-link libraries (DLL) into running processes. This project serves educational and testing purposes and should be used responsibly.

<img src="https://github.com/DataSearchers/LNjector---Windows-DLL-injector/blob/main/LNjector.png" width="200" />

## Features

- Supports both 32-bit and 64-bit processes.
- Multiple injection methods (e.g., CreateRemoteThread, ManualMap).
- Simple and user-friendly command-line interface.
- Error handling and informative logs.

## Requirements

- Windows OS (7, 8, 10, 11)
- Visual Studio (or any C++ IDE supporting Windows API)
- Admin privileges to inject into protected processes
- Target process and DLL compatibility (32-bit DLL for 32-bit processes, etc.)

## Installation

1. Download the latest release from the [Releases](https://github.com/DataSearchers/LNjector---Windows-DLL-injector/releases/tag/LNjector) page.
2. Run the `LNjector.exe` file.

<br>

![Screenshot](https://github.com/DataSearchers/LNjector---Windows-DLL-injector/blob/main/guiscreen.png?raw=true)
   
## Usage

1. Run the executable file `LNjector.exe`.
2. Chose dll files you want to inject and they will be displayed on the screen.
3. Chose the process you want the dll to inject to.
4. Inject and your done !
