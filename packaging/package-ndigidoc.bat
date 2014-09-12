rem --------------------------------------------
rem package-ndigidoc.bat
rem --------------------------------------------

set build_number = %BUILD_NUMBER%
set repository = %EXTERNAL_REPOSITORY%
set INSTALL_DIR=c:\install
SET INSTALL_BIN_DIR=%INSTALL_DIR%\IdCard\ndigidoc
SET INSTALL_SRC_DIR=%INSTALL_DIR%\source\ndigidoc
SET VC_DIR=C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC
SET VC_VARS_CMD="%VC_DIR%\vcvarsall.bat"
SET GIT_SRC_DIR=c:\jenkins\workspace\nDigiDoc\idkaart\current\ndigidoc
SET GIT_BUILD_DIR=c:\jenkins\workspace\nDigiDoc\label\Windows_trunk_VS2012\idkaart\current\ndigidoc
set MSI_VERSION=%1
set PREFIX=\install
set sourceLocation=\install\source\ndigidoc
set WIX_DIR=c:\Program Files (x86)\WiX Toolset v3.8\bin
set HEAT_CMD="%WIX_DIR%\heat.exe"
SET CANDLE_CMD="%WIX_DIR%\candle.exe"
SET LIGHT_CMD="%WIX_DIR%\light.exe"

echo "Packaging NDigiDoc.dll"
%HEAT_CMD% dir %sourceLocation% -cg Source -gg -scom -sreg -sfrag -srd -dr SourceFolder -var env.sourceLocation -out SourceFilesFragment.wxs
%CANDLE_CMD% ndigidoc-dev.wxs SourceFilesFragment.wxs -v -ext WixIIsExtension
%LIGHT_CMD% -out %INSTALL_BIN_DIR%\Eesti_ID_kaart-NET-teek-arendajale-%MSI_VERSION%.%build_number%.msi ndigidoc-dev.wixobj SourceFilesFragment.wixobj -v -ext WixIIsExtension

