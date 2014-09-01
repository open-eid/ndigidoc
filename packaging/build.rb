require 'fileutils'

project_name = ARGV[0]
version = ARGV[1] || "1.0.0"
build_number = ENV["BUILD_NUMBER"] || "0"
repository = ENV["EXTERNAL_REPOSITORY"] || "repository"

FileUtils.cd File.dirname(__FILE__)
exec "vcvarsall.bat x86 && set && ipconfig && ruby build-#{project_name}.rb #{version}.#{build_number} #{repository}"
