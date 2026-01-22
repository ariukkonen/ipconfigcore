# ipconfigcore
## ipcofigcore - Written by Ari Ukkonen

**ipconfigcore** is a cross-platform implementation of ipconfig viewing functionality for windows and macos written in .NET 10 (Core).
Note: One powershell script was previously included for windows to query the registry called **getdhcpv6iaid.ps1**, however, the code has been 
to not require the script and instead uses inline powershell calls sent to the shell.

### Usage:
**ipconfigcore**

 - without a switch displays interfaces with less details

**/about** - displays About screen.

**/all** - display all interfaces

**/help** - display usage.

**/ips** - display list of active IP addresses.

**/license** - displays the license

### Building the windows version:

Open **ipconfigcore.sln** in Visual Studio 2026 or later
This solution contains common code with the macos version but has a different project file called **ipconfigcore.csproj**

### Building the macos version:

Open **ipconfigcore-mac-rider.sln** in Rider IDE for MacOS.
This solution contains common code with the windows version but has a different project file called **ipconfigcore-mac-rider.csproj**

### Building the linux version:

Open **ipconfigcore-linux.sln** in Visual Studio 2026 or later
This solution contains common code with the windows version but has a different project file called **ipconfigcore-linux.csproj**


Note: The windows version of the executable is called **ipconfigcore.exe** and the mac/linux version is called **ipconfigcore** with
both bundling the **LICENSE** file with the executable.

### How Platform specific compiler constants are defined
The first step is to define the following conditions in the main **PropertyGroup**.
```xml
<IsWindows Condition="'$([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform($([System.Runtime.InteropServices.OSPlatform]::Windows)))' == 'true'">true</IsWindows> 
<IsOSX Condition="'$([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform($([System.Runtime.InteropServices.OSPlatform]::OSX)))' == 'true'">true</IsOSX> 
<IsLinux Condition="'$([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform($([System.Runtime.InteropServices.OSPlatform]::Linux)))' == 'true'">true</IsLinux>
<Description>- a cross-platform implementation of ipconfig from windows.</Description>
``` 
Next, we need to define three property groups defining constants that will use in programmer directives in the code:
```xml
<PropertyGroup Condition="'$(IsWindows)'=='true'">
  <DefineConstants>Windows</DefineConstants>
</PropertyGroup>
<PropertyGroup Condition="'$(IsOSX)'=='true'">
  <DefineConstants>OSX</DefineConstants>
</PropertyGroup>
<PropertyGroup Condition="'$(IsLinux)'=='true'">
  <DefineConstants>Linux</DefineConstants>
</PropertyGroup>
 ```

Finally, we write some operating specific code blocks:
```
#if OSX
            string duid = GetDUIDforMacOS();
#elif Windows
            string DUID = GetDUIDforWindows();
#endif
```
The OSX code block will execute if the code is runing on MacOS.

Note: Running the linux verison on WLS will not respect the Linux constant properly so there are additonal calls to GetOSPlatform() to deal with issue.
