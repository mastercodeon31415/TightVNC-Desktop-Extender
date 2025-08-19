# TightVNC Desktop Extender

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/mastercodeon31415/TightVNC-Desktop-Extender/blob/main/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues/mastercodeon31415/TightVNC-Desktop-Extender)](https://github.com/mastercodeon31415/TightVNC-Desktop-Extender/issues)
[![GitHub stars](https://img.shields.io/github/stars/mastercodeon31415/TightVNC-Desktop-Extender)](https://github.com/mastercodeon31415/TightVNC-Desktop-Extender/stargazers)

A Windows tray application that extends your desktop by displaying a TightVNC connection on a secondary monitor, designed to work seamlessly with a virtual display driver.

![VNC Extender Options](https://github.com/user-attachments/assets/8d03a0b7-e35e-4d49-be98-e231d17b8b97)

## Description

TightVNC Desktop Extender is a utility that allows you to connect to a TightVNC server and display the remote session on a secondary monitor. It runs as a tray application and continuously monitors the VNC host. When the host is online, the application automatically initiates a connection and positions the VNC window on your second display. If the host goes offline, the connection window is automatically closed.

This tool is ideal for users who want to dedicate a monitor to a remote machine, effectively extending their desktop space.

## Features

*   **Automatic Connection Management:** Automatically connects to the VNC host when it's available and disconnects when it's not.
*   **Secondary Display Targeting:** Designed to place the VNC window on a secondary monitor.
*   **System Tray Operation:** Runs unobtrusively in the system tray.
*   **Configurable Options:** Easily set the VNC host, port, and polling interval.
*   **Taskbar Visibility Control:** Option to hide the VNC window from the taskbar for a cleaner workspace.

## Prerequisites

Before using this application, you must install and configure a virtual display driver. This is necessary to create the secondary display that the VNC window will be moved to.

*   **Virtual Display Driver:** We recommend using the open-source [Virtual Display Driver](https://github.com/VirtualDrivers/Virtual-Display-Driver).
    *   Install the driver.
    *   Configure the desired resolution and refresh rate for your virtual display.

## Getting Started

1.  **Ensure Prerequisites are Met:** Make sure you have a virtual display driver installed and configured.
2.  **Download:** Grab the latest release from the [Releases](https://github.com/mastercodeon31415/TightVNC-Desktop-Extender/releases) page.
3.  **Run the Application:** Launch the `VNC Extender.exe`. The application will start in the system tray.
4.  **Configure Settings:**
    *   Right-click the tray icon and select "Options".
    *   **Host:** Enter the hostname or IP address of the TightVNC server.
    *   **Port:** Specify the port for the VNC connection (default is 5900).
    *   **Polling Interval (ms):** Set how frequently the application checks the status of the VNC host.
    *   **Hide VNC Window from Taskbar:** Check this to prevent the VNC viewer window from appearing in your taskbar.
    *   Click **Save**.
5.  **Resume Polling:** The application will now start monitoring the VNC host and will automatically open the connection on your secondary display when the host is available.

## Usage

Once configured, the application will run in the background. You can interact with it via its system tray icon:

*   **Right-click > Options:** Opens the configuration window.
*   **Right-click > Exit:** Closes the application.

The application continuously polls the specified VNC host.
*   **Host Online:** A TightVNC viewer window is spawned on the secondary monitor.
*   **Host Offline:** The TightVNC viewer window is automatically closed.

## Contributing

Contributions are welcome! If you have suggestions for improvements or encounter any issues, please feel free to open an issue or submit a pull request.

## Donation links

Anything is super helpful! Anything donated helps me keep developing this program and others!
- https://www.paypal.com/paypalme/lifeline42
- https://cash.app/$codoen314

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/mastercodeon31415/TightVNC-Desktop-Extender/blob/main/LICENSE) file for details. 