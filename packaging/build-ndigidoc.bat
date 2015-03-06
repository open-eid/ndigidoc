rem --------------------------------------------
rem build-ndigidoc.bat
rem BEFORE USING THIS SCIPT 
rem define in the environment:
rem set BUILD_NUMBER=<some-build-number>
rem set VC_DIR=<path-to-your-visual-studio>. Check that the provided location is correct.
rem set PROJECT_DIR=<project-dir>. Where NDigiDoc.sln is
rem set INSTALL_DIR=<output-directory>. Schould be c:\install\IdCard\ndigidoc
rem because VC project outputs files there. This dir is used later for packaging.
rem download CLR security Security.Cryptography.dll 1.6 from http://clrsecurity.codeplex.com/downloads/get/128947
rem and unpack it to %INSTALL_DIR%\clrsecurity or directly to %PROJECT_DIR%\clrsecurity
rem --------------------------------------------

set build_number = %BUILD_NUMBER%
set repository = %EXTERNAL_REPOSITORY%
echo INSTALL DIR: %INSTALL_DIR%
SET INSTALL_BIN_DIR=%INSTALL_DIR%\IdCard\ndigidoc
SET INSTALL_SRC_DIR=%INSTALL_DIR%\source\ndigidoc
SET VC_DIR=C:\Program Files (x86)\Microsoft Visual Studio 12.0\VC
SET VC_VARS_CMD="%VC_DIR%\vcvarsall.bat"
echo PROJECT DIR: %PROJECT_DIR%

echo "Cleanup install bin dir"
rmdir /S /Q %INSTALL_BIN_DIR%
mkdir %INSTALL_BIN_DIR%

echo "Cleanup install src dir"
rmdir /S /Q %INSTALL_SRC_DIR%
mkdir %INSTALL_SRC_DIR%

echo "Copying sources to install src dir"
xcopy /Y /E %PROJECT_DIR% %INSTALL_SRC_DIR%
rmdir /S /Q %INSTALL_SRC_DIR%\NDigiDoc\bin
rmdir /S /Q %INSTALL_SRC_DIR%\NDigiDoc\obj
rmdir /S /Q %INSTALL_SRC_DIR%\NDigiDocUtil\bin
rmdir /S /Q %INSTALL_SRC_DIR%\NDigiDocUtil\obj
rmdir /S /Q %INSTALL_SRC_DIR%\packages

echo "Build NDigiDoc"
call %VC_VARS_CMD%
cd %PROJECT_DIR%
mkdir clrsecurity
copy %INSTALL_DIR%\clrsecurity\*.* clrsecurity

msbuild NDigiDoc.sln /p:Configuration=Install


