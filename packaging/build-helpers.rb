require 'fileutils'
require 'yaml'

def log_stdout line
  STDOUT.sync = true
  puts line
end

def is_windows?
  RUBY_PLATFORM=~/(win|w)32$/
end

def running_in_debug_mode?
  ENV['DEBUG_MODE'] == 'yes' or ENV['DEBUG_MODE'] == 'true' or ENV['DEBUG_MODE'] == 'on'
end

def slave_home
  if is_windows?
    "\\hudson\\"
  else
    slave_path = `ps -ef | grep 'hudson' | grep 'slave.jar' | grep 'bash' | grep -v 'grep' | awk '{print $11}' | sed "s/'//g"`.strip + "/"
    if slave_path == '/'
      slave_path = `ps -ef | grep 'jenkins' | grep 'slave.jar' | grep 'bash' | grep -v 'grep' | awk '{print $11}' | sed "s/'//g"`.strip + "/"
    end
    slave_path.gsub('"', '')
  end
end

def secret property
  log_stdout "Loading secret '#{property}' from #{slave_home}secret.yaml"
  data = YAML.load_file("#{slave_home}secret.yaml")
  data[property] ? data[property] : raise("no property found: #{property}")
end

def execute_shell_command command
  result = ""
  begin
    log_stdout "Executing: #{command}"
    f = IO.popen(command + " 2>&1")
    while line = f.gets
      result += line
      log_stdout line
=begin
      File.open "#{package_name}.log", "a+" do |file|
        file.puts Time.new.inspect + ": #{line}"
      end
=end
    end
    Process.waitpid2 f.pid

    exitstatus = $?.exitstatus
    if exitstatus != 0
      log_stdout "Command failed (#{exitstatus}): #{command}"
      exit 1
    end
    result
  rescue Exception => e
    log_stdout "Command failed: #{command}:"
    log_stdout e.backtrace.join("\r\n")
    exit 2
  end
end

def ubuntu_build_package source_path = "../../../../#{package_name}"
  FileUtils.cd "#{build_directory}/" do
    if source_path.length > 0
      FileUtils.mkdir "../source"
      FileUtils.cd "../source" do
        source_name = "#{package_name}-#{version}#{version_suffix}"
        FileUtils.cp_r "#{source_path}", "#{source_name}"
        if File.file? "#{source_name}/cmake/modules/VersionInfo.cmake"
          major, minor, revision, build = get_splitted_version_number_components
          replace_in_file "#{source_name}/cmake/modules/VersionInfo.cmake", /set\(\s*BUILD_VER .*?\)/, "set( BUILD_VER #{build} )"
        end
        execute_shell_command "tar cfz ../#{package_name}_#{version}#{version_suffix}#{linux_release}.orig.tar.gz --exclude-vcs #{source_name}"
      end
    elsif File.exists? "../../#{package_name}"
      FileUtils.cp_r Dir.glob("../../#{package_name}/*"), "."
    end
    execute_shell_command "DEB_SRCDIR=#{source_path} dpkg-buildpackage -rfakeroot -us -uc"
  end
end

def ubuntu_create_changelog
  File.open("#{build_directory}/debian/changelog", "w") do |file|
    file.write "#{package_name} (#{version}#{version_suffix}#{linux_release}-1) unstable; urgency=low

  * Release: #{version}#{version_suffix}#{linux_release}.

 -- RIA <info@ria.ee>  Tue, 10 Aug 2010 15:42:56 +0300
"
  end
end

def replace_in_file filename, regex, value
  spec = File.read(filename)
  File.open(filename, "w") do |file|
    file.write spec.gsub(regex, value)
  end
end

def ubuntu_put_package_to_repository
  if repository
    Dir.glob("**/#{package_name}*.deb").each do |file|
      execute_shell_command "lintian #{file}"
    end
    Dir.glob("**/#{package_name}*.{changes,deb,ddeb,dsc,gz}").each do |file|
      execute_shell_command "cp #{file} #{repository}/ubuntu/debs"
    end
    Dir.glob("**/#{package_name}*.{deb,changes,gz,ddeb,dsc}").each do |file|
      execute_shell_command "mkdir -p #{repository}/archive/#{package_name}/ubuntu"
      execute_shell_command "cp #{file} #{repository}/archive/#{package_name}/ubuntu"
    end
  end
end

def mac_put_package_to_repository package = "#{package_name}", subdirectory = '', ext = ".dmg"
  if repository

    if (subdirectory != '' and subdirectory[0] != '/')
      subdirectory = "/#{subdirectory}"
    end

    puts "++++++ #{package}"
    execute_shell_command "ssh administrator@10.0.25.57 'rm -rf #{repository}/mac#{subdirectory}/#{package}_*#{ext}'"

    Dir.glob("**/#{package}_*#{ext}").each do |file|
      execute_shell_command "ssh administrator@10.0.25.57 'mkdir -p #{repository}/archive/#{package_name}/mac'" if subdirectory == ''
      execute_shell_command "ssh administrator@10.0.25.57 'mkdir -p #{repository}/mac#{subdirectory}'" if subdirectory != ''
      execute_shell_command "scp #{file} administrator@10.0.25.57:#{repository}/archive/#{package_name}/mac" if subdirectory == ''
      execute_shell_command "scp #{file} administrator@10.0.25.57:#{repository}/mac#{subdirectory}/"
    end
  end
end

def win_put_unsigned_package_to_repository file_prefix, platform
  if repository == "repository"
    log_stdout "Storing unsigned MSI in repository..."
    execute_shell_command "plink -i \\hudson\\hudson-slave.ppk administrator@10.0.25.57 \"rm -rf #{repository}/windows/unsigned_msi/Eesti_ID_kaart-*_#{platform}*\""
    execute_shell_command "plink -i \\hudson\\hudson-slave.ppk administrator@10.0.25.57 \"mkdir -p #{repository}/windows/unsigned_msi\""
    execute_shell_command "pscp -i \\hudson\\hudson-slave.ppk #{file_prefix}*.msi administrator@10.0.25.57:#{repository}/windows/unsigned_msi/"
    execute_shell_command "pscp -i \\hudson\\hudson-slave.ppk products.xml administrator@10.0.25.57:#{repository}/windows/unsigned_msi/products_#{platform}.xml"
  end
end

def win_put_package_to_repository file_prefix, platform = ''
  if repository
    log_stdout "Storing MSI in repository..."
    execute_shell_command "plink -i \\hudson\\hudson-slave.ppk administrator@10.0.25.57 \"rm -rf #{repository}/windows/Eesti_ID_kaart-*#{platform}*\""
    execute_shell_command "plink -i \\hudson\\hudson-slave.ppk administrator@10.0.25.57 \"mkdir -p #{repository}/archive/#{package_name}/windows\""
    execute_shell_command "pscp -i \\hudson\\hudson-slave.ppk #{file_prefix}*.msi administrator@10.0.25.57:#{repository}/archive/#{package_name}/windows"
    execute_shell_command "pscp -i \\hudson\\hudson-slave.ppk #{file_prefix}*.msi administrator@10.0.25.57:#{repository}/windows"
    execute_shell_command "pscp -i \\hudson\\hudson-slave.ppk products.xml administrator@10.0.25.57:#{repository}/windows/products_#{platform}.xml"
  end
end

def win_put_other_package_to_repository file_prefix, platform, mask_to_delete="Eesti_ID_kaart_debug-*"
 if File.exists? "\\hudson\\hudson-slave.ppk"
  if repository
    execute_shell_command "plink -i \\hudson\\hudson-slave.ppk administrator@10.0.25.57 \"rm -rf #{repository}/windows/#{mask_to_delete}#{platform}*\""
    execute_shell_command "plink -i \\hudson\\hudson-slave.ppk administrator@10.0.25.57 \"mkdir -p #{repository}/archive/#{package_name}/windows\""
    execute_shell_command "pscp -i \\hudson\\hudson-slave.ppk #{file_prefix}*.msi administrator@10.0.25.57:#{repository}/archive/#{package_name}/windows"
    execute_shell_command "pscp -i \\hudson\\hudson-slave.ppk #{file_prefix}*.msi administrator@10.0.25.57:#{repository}/windows"
  end
 else
   log_stdout "WARN: \\hudson\\hudson-slave.ppk does not exist - skipping upload..."
 end
end

def ubuntu_copy_package_templates
  #debian directory initial content is created with next command: execute "dh_make -s -f ../#{package_name}.tar.gz"
  FileUtils.cp_r "debian_#{package_name}", "#{build_directory}/debian"
end

def run_cmake source_path, cmake_parameters=""
  FileUtils.cd build_directory do
    execute_shell_command "cmake -DCMAKE_VERBOSE_MAKEFILE=ON -DCMAKE_BUILD_TYPE=RelWithDebInfo #{cmake_parameters} #{source_path}"
  end
end

def visual_studio_cmd platform="x86"
  "\"#{ENV['VCINSTALLDIR']}\\vcvarsall.bat\" #{platform}"
end

def visual_studio_cmd_x86 platform="x86"
  "\"\\Program Files\\Microsoft Visual Studio 9.0\\VC\\vcvarsall.bat\" #{platform}"
end

def ubuntu_install_dependencies * libraries
  # /etc/sudoers
  # hudson ALL=NOPASSWD: /usr/bin/apt-get

  # /etc/apt/sources.list
  # deb http://10.0.13.144/ubuntu lucid main
  execute_shell_command "sudo apt-get update"

  libraries.each do |library|
    execute_shell_command "sudo apt-get install --yes --force-yes #{library}"
  end
end

def mac_install_dependencies * libraries
  FileUtils.cd build_directory do
    libraries.each do |library|
      execute_shell_command "scp administrator@10.0.25.57:#{repository}/mac/#{library}_*.pkg #{library}.pkg"
      execute_shell_command "sudo installer -verboseR -pkg #{library}.pkg -target /"
    end
  end
end

def sign_files_win pattern
  if File.exists? "C:/codesigning/MSCV-VSClass3.cer"
    execute_shell_command "#{visual_studio_cmd} && signtool.exe sign /a /v /ac \"C:/codesigning/MSCV-VSClass3.cer\" /s MY /n \"RIIGI INFOSUSTEEMI AMET\" /t http://timestamp.verisign.com/scripts/timstamp.dll #{pattern}"
    #execute_shell_command "#{visual_studio_cmd} && signtool.exe sign /ph /t http://timestamp.verisign.com/scripts/timstamp.dll /f c:\\wincert\\wincert.pfx /p #{secret "win_sign_password"} #{pattern}"
  else
    log_stdout "WARN: C:\\codesigning\\MSCV-VSClass3.cer does not exist - skipping signing..."
  end
end

def copy_windows_debug_symbols platform=''
  path = "/install/debug-symbols/#{package_name}/#{platform}"
  FileUtils.rm_rf path
  FileUtils.mkdir_p path

  Dir.glob("**/*.pdb").each do |file|
    FileUtils.cp "#{file}", "#{path}"
  end
end

def zip_all_windows_debug_symbols file
  execute_shell_command "zip -r #{file}-debug-symbols.zip /install/debug-symbols/*"
end

def create_build_directory
  FileUtils.rm_rf "build"
  FileUtils.mkdir_p build_directory
  build_directory
end

def is_release?
  (ENV["RELEASE"] == 'yes') or (ENV["RELEASE"] == 'true')
end

def version_suffix
  is_release? ? "" : ".BETA"
end

def version
  ARGV[0] || "1.VERSION.NUMBER.NOT.SET"
end

def repository
  return ARGV[1] if @repository == nil
  @repository
end

def package_name
  $PROGRAM_NAME.scan(/build-(.*?).rb/)[0][0]
end

def build_directory
  File.expand_path "build/#{package_name}-#{version}"
end

def linux_release
  linux_release = ""
  isLinux = /linux/ =~ RUBY_PLATFORM
  if (isLinux)
    linux_release = case (`lsb_release -s -i`).strip
                      when "Ubuntu" then
                        "-ubuntu-" + (`lsb_release -s -r`).gsub(".", "-").strip
                    end
  end
  linux_release
end

def ubuntu_make_package changelog='true',  source_path="../../../../#{package_name}"
  ubuntu_copy_package_templates

  if changelog == 'true'
    ubuntu_create_changelog
  end

  ubuntu_build_package source_path

  ubuntu_put_package_to_repository
end

def osx_version
  (execute_shell_command "sw_vers -productVersion").scan(/([0-9]+\.[0-9]+)/).flatten.first
end

def sign_all_exe_and_dll_files_in location
  a = Dir.glob("#{location}/**/*.dll") + Dir.glob("#{location}/**/*.exe")
  a.each do |pattern|
    file_to_sign = pattern.gsub("/", "\\")
    sign_files_win file_to_sign
  end
end

def get_splitted_version_number_components
  version.split(".")
end

def directory_exists?(directory)
  File.directory?(directory)
end

def get_from_git(project_dir, location)
  if directory_exists? project_dir
    FileUtils.cd project_dir do
      execute_shell_command "git clean -f -d"
      execute_shell_command "git pull origin master"
    end
  else
    execute_shell_command "git clone #{location} #{project_dir}"
  end
end

def download_and_unzip_minidriver version
  execute_shell_command "pscp -i \\hudson\\hudson-slave.ppk administrator@10.0.25.57:#{repository}/minidriver/3.9/esteidcm.[0-9]*_win#{version}.zip ."
  execute_shell_command "if not exist #{ENV['PREFIX']}\\minidriver\\#{version} md #{ENV['PREFIX']}\\minidriver\\#{version}"
  execute_shell_command "unzip -o esteidcm*.zip -d #{ENV['PREFIX']}\\minidriver\\#{version}"
  execute_shell_command "del esteidcm*.zip"
end
