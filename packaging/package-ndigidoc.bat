rem --------------------------------------------
rem package-ndigidoc.bat
rem BEFORE USING THIS SCIPT 
rem define in the environment:
rem set BUILD_NUMBER=<some-build-number>
rem set VC_DIR=<path-to-your-visual-studio>. Check that the provided location is correct.
rem set PROJECT_DIR=<project-dir>. Where NDigiDoc.sln is
rem set INSTALL_DIR=<output-directory>. Schould be c:\install\IdCard\ndigidoc
rem because VC project outputs files there. This dir is used later for packaging.
rem --------------------------------------------

set build_number = %BUILD_NUMBER%
set repository = %EXTERNAL_REPOSITORY%
echo INSTALL DIR: %INSTALL_DIR%
SET INSTALL_BIN_DIR=%INSTALL_DIR%\IdCard\ndigidoc
SET INSTALL_SRC_DIR=%INSTALL_DIR%\source\ndigidoc
SET VC_DIR=C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC
SET VC_VARS_CMD="%VC_DIR%\vcvarsall.bat"
echo PROJECT DIR: %PROJECT_DIR%
set MSI_VERSION=%1
set PREFIX=\install
set sourceLocation=\install\source\ndigidoc
set WIX_DIR=c:\Program Files (x86)\WiX Toolset v3.8\bin
set HEAT_CMD="%WIX_DIR%\heat.exe"
SET CANDLE_CMD="%WIX_DIR%\candle.exe"
SET LIGHT_CMD="%WIX_DIR%\light.exe"

echo "Packaging NDigiDoc.dll"
cd %PROJECT_DIR%
%HEAT_CMD% dir %sourceLocation% -cg Source -gg -scom -sreg -sfrag -srd -dr SourceFolder -var env.sourceLocation -out SourceFilesFragment.wxs
%CANDLE_CMD% ndigidoc-dev.wxs SourceFilesFragment.wxs -v -ext WixIIsExtension
%LIGHT_CMD% -out %INSTALL_BIN_DIR%\Eesti_ID_kaart-NET-teek-arendajale-%MSI_VERSION%.%build_number%.msi ndigidoc-dev.wixobj SourceFilesFragment.wixobj -v -ext WixIIsExtension

