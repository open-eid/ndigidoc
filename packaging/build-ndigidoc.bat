rem --------------------------------------------
rem build-ndigidoc.bat
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
SET SIGN_CMD=signtool.exe sign /a /v /ac "C:/codesigning/MSCV-VSClass3.cer" /s MY /n "RIIGI INFOSUSTEEMI AMET" /t http://timestamp.verisign.com/scripts/timstamp.dll
set MSI_VERSION=%1
set PREFIX=\install
set sourceLocation=\install\source\ndigidoc
set WIX_DIR=c:\Program Files (x86)\WiX Toolset v3.8\bin
set HEAT_CMD="%WIX_DIR%\heat.exe"
SET CANDLE_CMD="%WIX_DIR%\candle.exe"
SET LIGHT_CMD="%WIX_DIR%\light.exe"

echo "Cleanup install bin dir"
rmdir /S /Q %INSTALL_BIN_DIR%
mkdir %INSTALL_BIN_DIR%

echo "Cleanup install src dir"
rmdir /S /Q %INSTALL_SRC_DIR%
mkdir %INSTALL_SRC_DIR%

echo "Copying sources to install src dir"
xcopy /Y /E %GIT_SRC_DIR% %INSTALL_SRC_DIR%

echo "Build NDigiDoc"
call %VC_VARS_CMD%
cd %GIT_BUILD_DIR%
msbuild NDigiDoc.sln /p:Configuration=Install

echo "Sign NDigiDoc.dll"
%SIGN_CMD% %INSTALL_BIN_DIR%\NDigiDoc.dll

echo "Sign NDigiDocUtility.exe"
%SIGN_CMD% %INSTALL_BIN_DIR%\NDigiDocUtility.exe

echo "Packaging NDigiDoc.dll"
%HEAT_CMD% dir %sourceLocation% -cg Source -gg -scom -sreg -sfrag -srd -dr SourceFolder -var env.sourceLocation -out SourceFilesFragment.wxs
%CANDLE_CMD% ndigidoc-dev.wxs SourceFilesFragment.wxs -v -ext WixIIsExtension
%LIGHT_CMD% -out %INSTALL_BIN_DIR%\Eesti_ID_kaart-NET-teek-arendajale-%MSI_VERSION%.%build_number%.msi ndigidoc-dev.wixobj SourceFilesFragment.wixobj -v -ext WixIIsExtension

echo "Sign NDigiDoc package"
%SIGN_CMD% %INSTALL_BIN_DIR%\Eesti_ID_kaart-NET-teek-arendajale-%MSI_VERSION%.%build_number%.msi

