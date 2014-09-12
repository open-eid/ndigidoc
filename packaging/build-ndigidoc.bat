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
set PREFIX=\install
set sourceLocation=\install\source\ndigidoc

echo "Cleanup install bin dir"
rmdir /S /Q %INSTALL_BIN_DIR%
mkdir %INSTALL_BIN_DIR%

echo "Cleanup install src dir"
rmdir /S /Q %INSTALL_SRC_DIR%
mkdir %INSTALL_SRC_DIR%

echo "Copying sources to install src dir"
xcopy /Y /E %GIT_SRC_DIR% %INSTALL_SRC_DIR%
rmdir /S /Q %INSTALL_SRC_DIR%\clrsecurity
rmdir /S /Q %INSTALL_SRC_DIR%\log4net
rmdir /S /Q %INSTALL_SRC_DIR%\NDigiDoc\bin
rmdir /S /Q %INSTALL_SRC_DIR%\NDigiDoc\obj
rmdir /S /Q %INSTALL_SRC_DIR%\NDigiDocUtil\bin
rmdir /S /Q %INSTALL_SRC_DIR%\NDigiDocUtil\obj
rmdir /S /Q %INSTALL_SRC_DIR%\packages

echo "Build NDigiDoc"
call %VC_VARS_CMD%
cd %GIT_BUILD_DIR%
msbuild NDigiDoc.sln /p:Configuration=Install


