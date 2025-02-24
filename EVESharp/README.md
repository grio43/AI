EVESharp
===============
<img src="https://c4s.de/EVESharp/Images/1.png" width="400"></img>
## Installation

#### Requirements

1. Windows 10
2. .NET 4.6.2+ (https://www.microsoft.com/en-us/download/details.aspx?id=53321)
3. .NET Build Tools 14 (https://www.microsoft.com/en-us/download/confirmation.aspx?id=48159)
4. Git (https://git-scm.com/downloads)

#### (Preferred) Use the updater to download the most recent source-code and compile it automatically.
```
Download most recent Updater.exe from https://github.com/duketwo/EVESharp/releases
```
1. Start Updater.exe in an empty directory.This automatically creates the directory structure which is required to actually make use of hot reloading questor with new compiled code while eve is running.

####  (Alternative) Build from source
```
$ git clone https://github.com/duketwo/EVESharp.git
```
1. Select injector as startup project.
2. Run injector

#### Modify your hosts file to block remote logging by CCP - C:\Windows\System32\drivers\etc\hosts

```
# Copyright (c) 1993-2009 Microsoft Corp.
#
# This is a sample HOSTS file used by Microsoft TCP/IP for Windows.
#
# This file contains the mappings of IP addresses to host names. Each
# entry should be kept on an individual line. The IP address should
# be placed in the first column followed by the corresponding host name.
# The IP address and the host name should be separated by at least one
# space.
#
# Additionally, comments (such as these) may be inserted on individual
# lines or following the machine name denoted by a '#' symbol.
#
# For example:
#
#      102.54.94.97     rhino.acme.com          # source server
#       38.25.63.10     x.acme.com              # x client host

# localhost name resolution is handled within DNS itself.
#	127.0.0.1       localhost
#	::1             localhost
127.0.0.1 sentry.tech.ccp.is
127.0.0.1 logs-01.loggly.com
127.0.0.1 crashes.eveonline.com
127.0.0.1 sentry.evetech.net
```

## FEATURES

- Monolithic / standalone Questor Framework, no additional software required.
- Hardware profiling API hooks to allow a unique hardware profile for each game client. Socks5 proxy support included. Multiple accounts could also be paired as group by using the same hardware profiles.
- Hardware profile form with following settings: TotalPhysicalRam, WindowUserLogin, Computername, WindowsKey, ProcessorIdent, ProcessorRevision, ProcessorCoreAmount, ProcessorLevel, ProxyIP, ProxyPort, ProxyUsername, ProxyPassword, NetworkAdapterGuid, NetworkAddress, MacAddress, GPUDescription, GPUDeviceId, GPUVendorId, GPURevision, GPUDriverVersion and GPUIdentifier.
- Hardware profiles are partly randomized, a graphics card hardware profile can be retrieved by using DXDIAG.TXT files found via google search.
- Launch a hooked Firefox instance in private mode. Socks5 proxy usage with user authentication similiar to SocksCap or Proxifier. Access the "Eve Online Accountmanagement" website with the ip address corresponding to your eve account.
- Handling of multiple eve online instances, including downtime detection and automatic restarts of not responding eve instances.

## How to use

1. Installation via the Updater. See @Installation.
2. Start .\EVESharpLauncher.exe
3. Goto "Settings". Add the ExeFile.exe Location. For example: "C:\eveonline\bin\exefile.exe".
4. If you dont own a Socks5 proxy, either check the Tor-Socks-Proxy or the Local-Socks-Proxy.
5. Add a new eve account either via the account creator (Windows -> Account creator) or manually by editing the datagrid rows.
6. Right-click the just created accont -> Edit hardware profile -> Generate random hardware profile.
7. Add your Socks5 proxy or use the Tor-Socks-Proxy or the Local-Socks-Proxy . [127.0.0.1:15001 - no User / no Password OR 127.0.0.1:15002 - no User / no Password]
8. Click "Test-Proxy" to ensure the proxy is working. You'll get the outgoing Ip-address back.
9. Click Save.
10. Right-click the account -> Click "Start eve".
11. Create a character.
11. Change the EVE in game setting: Chat->"Eve-Voice-Enabled" to FALSE.
13. Goto .\QuestorSettings directory and add your CHARNAME.xml with your config.
14. Restart eve
15. Go to the HookManager. Open it with the button on the right side and click "Start/Restart Questor" to reload your config.

## How to contribute

1. Installation via the Updater. See @Installation.
2. Goto .\Resources\EVESharpSource\EVESharp-master
3. Make sure you have a GIT client. If not download one from here: https://git-scm.com/downloads
4. Execute ConvertToGitRepo.bat
5. .\Resources\EVESharpSource\EVESharp-master is now your base GIT directory.
6. Start .\EVESharp.exe
7. Follow with steps from @How to use
18. Modify the Questor source code.
19. Go to the HookManager. Open it with the button on the right side and click "Compile/Start/Restart Questor" to reload the new changes made to the source code.
20. Verfiy the changes are working properly
21. Commit the changes

## CAUTION

The automatic generation of the hardware profile is ONLY partly. The GPU details aren't generated automatically.
The provided GPU details are most likely to get your account banned within no time. DONT USE THAT DETAILS FOR YOUR REAL ACCOUNTS. ONLY for testing.
Google for "DxDiag.txt" files and copy the content to the clipboard and import it with the import function within the hardware profile.
You can also generate a "DxDiag.txt" yourself to use your real GPU hardware profile.
Keep ALWAYS in mind which accounts are linked together.
The Tor-Socks-Proxy is ONLY for trial accounts.
If you dont have a dedicated Socks5-Proxy use the Local-Socks-Proxy. Remember that all accounts are using the same Ip-address while using the Local-Socks-Proxy. When using multiple accounts from the same Ip-address it's most likely beneficial to use the same hardware profile on all accounts.

Troubleshooting:
If the hardware spoofing controllers are crashing try disabling Windows exploit protection:
PC Settings --> Update and Security -> Windows Security -> App and Browser Control --> Exploit Protection Settings
see: https://docs.microsoft.com/en-us/windows/security/threat-protection/microsoft-defender-atp/customize-exploit-protection

## License

[MIT](LICENSE)
