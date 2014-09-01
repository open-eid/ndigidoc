def wix_dir
  "#{ENV['WIX']}bin"
end

def candle
  "\"#{wix_dir}\\candle.exe\""
end

def light
  "\"#{wix_dir}\\light.exe\""
end

def lit
  "\"#{wix_dir}\\lit.exe\""
end

def heat
  "\"#{wix_dir}\\heat.exe\""
end

def doxygen
  "\"C:\\Program Files (x86)\\doxygen\\bin\\doxygen.exe\""
end

def run_cmake_win platform, source_path, cmake_parameters="", type="RelWithDebInfo"
    execute_shell_command "#{visual_studio_cmd(platform)} && " +
    	'cmake -G "NMake Makefiles" -DCMAKE_VERBOSE_MAKEFILE=ON -DCMAKE_BUILD_TYPE=' + 
    	"#{type} #{cmake_parameters} #{source_path}"
end
