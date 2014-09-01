require './build-helpers'
require './helper'

install_dir = "\\install"
source_location = "#{install_dir}\\source"
ENV['sourceLocation'] = "#{source_location}\\NDigiDoc"
ENV['MSI_VERSION'] = version
ENV['PREFIX'] = install_dir
ENV['PACKAGING_DIR'] = File.expand_path(File.dirname(__FILE__))
ENV['VERSION_SUFFIX'] = version_suffix
msi_name = "Eesti_ID_kaart-NET-teek-arendajale-#{version}#{version_suffix}.msi"

#prepares source for packaging
FileUtils.rm_rf "#{source_location}\\NDigiDoc"
FileUtils.mkdir_p "#{source_location}"
FileUtils.cp_r "..", source_location
FileUtils.rm_rf "#{source_location}\\NDigiDoc\\NDigiDoc\\obj"
FileUtils.rm_rf "#{source_location}\\NDigiDoc\\NDigiDocUtility\\obj"

FileUtils.cd ".."
execute_shell_command "#{visual_studio_cmd("x64")} && #{heat} dir #{source_location}\\NDigiDoc -cg Source -gg -scom -sreg -sfrag -srd -dr SourceFolder -var env.sourceLocation -out SourceFilesFragment.wxs"
#FileUtils.cp "SourceFilesFragment.wxs", ".."

create_build_directory

FileUtils.cd build_directory do
#  FileUtils.cp_r "../../../../NDigiDoc/.", "."
  FileUtils.cp "./packaging/ndigidoc-dev.wxs", "."

  execute_shell_command "#{visual_studio_cmd("x64")} && msbuild NDigiDoc.sln /p:Configuration=Release /p:Platform=\"Any CPU\""

  FileUtils.rm_rf "/install/IdCard/ndigidoc"
  FileUtils.mkdir_p "/install/IdCard/ndigidoc"

  FileUtils.cp "NDigiDoc/bin/Release/NDigiDoc.dll", "/install/IdCard/ndigidoc"
  sign_files_win "\\install\\IdCard\\ndigidoc\\NDigiDoc.dll"
  FileUtils.cp "NDigiDoc/bin/Release/Security.Cryptography.dll", "/install/IdCard/ndigidoc"
  FileUtils.cp "NDigiDoc/bin/Release/log4net.dll", "/install/IdCard/ndigidoc"
  FileUtils.cp "NDigiDocUtility/bin/Release/NDigiDocUtility.exe", "/install/IdCard/ndigidoc"
  sign_files_win "\\install\\IdCard\\ndigidoc\\NDigiDocUtility.exe"
  FileUtils.cp "NDigiDocUtility/bin/Release/log4n.xml", "/install/IdCard/ndigidoc"

  execute_shell_command "#{visual_studio_cmd("x64")} && #{candle} ndigidoc-dev.wxs SourceFilesFragment.wxs -v -ext WixIIsExtension"
  execute_shell_command "#{visual_studio_cmd("x64")} && #{light} -out #{msi_name} ndigidoc-dev.wixobj SourceFilesFragment.wixobj -v -ext WixIIsExtension"

  sign_files_win msi_name

  win_put_other_package_to_repository File.basename(msi_name, File.extname(msi_name)),"", "Eesti_ID_kaart-NET-teek-arendajale-*"

end
