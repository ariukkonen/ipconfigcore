# ipconfigcore
## ipcofigcore - Written by Ari Ukkonen

**ipconfigcore** is a cross-platform implementation of ipconfig viewing functionality for windows and macos written in .NET 7 (Core).
One powershell script is include for windows to query the registry called **getdhcpv6iaid.ps1** and this script needs to be included
with the windows binary in order for it to work on windows.

### Usage:
**ipconfigcore**

 - without a switch displays interfaces with less details

**/about** - displays About screen.

**/all** - display all interfaces

**/help** - display usage.

**/ips** - display list of active IP addresses.

**/license** - displays the license

### Building the windows version:

Open **ipconfigcore.sln** in Visual Studio 2022 or later
This solution contains common code with the macos version but has a different project file called **ipconfigcore.csproj**

### Building the macos version:

Open **ipconfigcore-mac.sln** in Visual Studio 2022 for Mac
This solution contains common code with the windows version but has a different project file called **ipconfigcore-mac.csproj**

Note: The windows version of the executable is called **ipconfigcore.exe** and the mac version is called **ipconfigcore** with
both bundling the **LICENSE** file with the executable and the windows version also bundles with the **getdhcpv6iaid.ps1** powershell script.\

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
