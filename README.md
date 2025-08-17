## TightVNC Desktop Extender
<img width="286" height="228" alt="{CD34FD3B-1933-4C0A-B45E-34F4101D2676}" src="https://github.com/user-attachments/assets/8d03a0b7-e35e-4d49-be98-e231d17b8b97" />

# What this is
TightVNC Desktop Extender is a try app that allows you to spawn a connection to a tightvnc server host as you specify, and put the window that displays the sever you're connected to on a secondary display. 
It is intended that the user sets up a virtual display driver (https://github.com/VirtualDrivers/Virtual-Display-Driver) and configures it before using this program (install driver, set virtual display resolution and refresh rate)
The program will run and act as a service, monitoring the host for when it goes offline or online. When it comes online, it automatically spawns a connection on the secondary display. When the server goes dark, it kills the connection window. 