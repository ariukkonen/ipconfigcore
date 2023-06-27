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

Bulding the windows version:
Open ipconfigcore.sln in Visual Studio 2022 or later
This solution contains common code with the macos version but a different project file called ipconfigcore.csproj
Building the macos version:
Open ipconfigcore-mac.sln in Visual STudio 2022 for Mac
This solution contains common code with the macos version but a different project file called ipconfigcore-mac.csproj

Note: The vindows verison of the executable is called ipconfigcore.exe and the mac version is called ipconfigcore-mac with
both bundling the LICENSE file with the executable and the windows verison also builds the getdhcpv6iaid.ps1 powershell script.