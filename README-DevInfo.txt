NDigiDoc developers info

You need Microsoft VisualStodio 2012 to build. This library uses .NET framework 3.5.
Also uses log4net library 1.2.11. This dependency can be removed with a small code change
in Logger.cs

Preparation
Download the following libraries
1) log4net - http://logging.apache.org/log4net/ and place it under
<source-dir>\log4net\log4net.dll
<source-dir>\log4net\log4net.xml
2) CLR Security - http://clrsecurity.codeplex.com/ and place it under
<source-dir>\clrsecurity\Security.Cryptography.dll
<source-dir>\clrsecurity\Security.Cryptography.xml

To build use configuration Release or Debug in VisualStudio.
On commandline building can be done by:
msbuild NDigiDoc.sln /p:Configuration=Release
Configuration Install is meant for official packages on special build host.

Packaging an .msi file can be done with WiX Toolset v3.8 and the provided 
config files but signing keys must be changed in build-ndigidoc.bat file.

