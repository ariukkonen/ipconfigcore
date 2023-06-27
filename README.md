# ipconfigcore
ipcofigcore - Written by Ari Ukkonen
ipconfigcore is a cross-platform implementation of ipconfig viewing functionality for windows and macos written in .NET 7 (Core).
One powershell script is include for windows to query the registry called getdhcpv6iaid.ps1 and this script needs to be included
with the binary in order for it to work on windows.

Usage:
ipconfigcore
/help - display usage.
/ips - display list of active IP addresses.
/all - display all interfaces
/license - displays the license
