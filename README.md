# C# Cache
A ccache like tool for mono.

CSCache is a tool based on ccache that is used for compilation cache for c/c++. The idea of the tool is the same however the implementation is totally different.
The tool was made in C# and is fully compatible with Mono C# compiler (mcs). It is possible to configure the tool for other compilers, but this feature was not tested yet.

# Usage
#### Running in terminal 
In order to run the tool in terminal you either need to install using one of the installation files or compile the library and add it to the path. The command for running the tool in the terminal:
`CSCache [compiler command with arguments] [arguments to the tool]`

It is important that the compiler command is passed as one argument, separate arguments will result in error or unwanted behiavor.
Example of passing a mcs compile command:
`CSCache "mcs input1.cs 'input with space.cs' -out:out.exe"`

#### Running as a library
You can use this tool also as a .net library. After adding the library to your project the class is located in:
- Namespace CSCacheLib.
- Class CSCache

The CSCache class has the following public methods/constructors:
- **public CSCache(string[] args)** - The constructor of initialisation of the class. The args are the arguments you normally pass in terminal.
- **public void Cache()** - The method that starts to process the passed argument and generate the cache if the passed compiler with arguments is valid.

# Installation
#### Installing using one of the install scripts.
You can install the tool by using one of the installation scripts:
- install.bat (windows) - Must be run as administrator or else it will throw error.
- install (unix)

#### Installing manually
First you must compile the libraries and entry point executable. Execute buildLib.bat (windows) or buildLib (unix), or you can use the following command in terminal:
````
mcs dependencies/Options.cs -t:library -out:Options.dll
mcs -r:System.Configuration.dll -r:Options.dll -t:library Config.cs ConsoleTools.cs CSCacheLib.cs FilesTools.cs MD5Tools.cs LibArgs.cs ParseTools.cs -out:CSCacheLib.dll
mcs CSCache.cs -r:CSCacheLib.dll
````
After compilation you can copy the following files to a custom folder:
- CScache, Options.dll, CSCacheLib.dll, CSCache.exe (unix)
- CSCache.bat, Options.dll, CSCacheLib.dll, CSCache.exe (windws) - In case if you use the bash in windows add the unix files list

The last step is to add the folder to the path:
- `set PATH=%PATH%;pathToCSCache.bat` (cmd)
- `export PATH=$PATH:pathToCSCache` (bash)


# Features

This tool is not as powerful as ccache however it is more flexible. The list of features is the following:
- configuration file.
- clearing of cache.
- changing the directory where cache can be saved.
- changing specific arguments that are passed to compiler.
- ignoring of specific arguments passed to compiler.

# Dependencies
The list of dependencies is the following:
- System.Configuration.dll.
- Mono.Options. (in this project is located in dependencies/Options.cs)
- Mono C# compiler. (mcs)

#### Configuration file

Configuration file is a feature that makes this tool more flexible and allows to use multiple compilers to cache the output.The configuration filename is **config.xml**.  The configuration file is located in:
- **%appdata%/.cscache/** (Windows)
- **$HOME/.cscache/** (Linux)

The configuration file contains the following fields:
- **CacheLocation** - Location where the cache will be stored.
- **IgnoredArguments** - Arguments that will be ignored when passed as compiler arguments.
- **VersionArgument** - This argument is important since it is used in cache process. It is used to determine if the compiler version is the same as the cached one.
- **ReferenceArgument** - Argument used for referencing to a library or assembly.
- **OutputArgument** - Argument for specifying location and filename of output assembly.
- **TagetArgument** - Argument for targeting a specific type of output assembly. (currently supported only exe, winexe, library, module)
- **RecurseArgument** - Argument for adding input files by using a pattern. (example : *.cs)
- **AddModuleArgument** - Argument for adding a module to the assembly.
- **DefaultExtension** - The default extension of the output file if it is not specified.

The configuration file can be modified or removed. In case when it is removed the default configuration will be generated. The default configuration is for mcs.

#### Clearing of cache

`--clear=all` The argument is passed to the tool. The argument clears all files that have extension .cache from the location of the cache specified in configuration file. Once the argument is passed nothing else is executed.

# Nuget package
The nuget package is located at: https://www.nuget.org/packages/CSCache/ . The nuget package just adds the library to the reference.

# Additional useful methods and fields

Almost all clases used for generating cache contains static methods and fields that are useful by themselves. There are public static methods in the following files:
- ConsoleTools.cs
- FilesTools.cs
- MD5Tools.cs
- ParseTools.cs

#### ConsoleTools.cs
Contains the class ConsoleTools with following methods and fields:
- `static bool IsUnix` - This field can determine if your OS is either unix or windows.
- `static string Execute(string cmdLine, out int errCode)` - This method allows execution of a command either in bash or cmd. The execution is based on the enviromnent from where the application was executed.
    - **string cmdLine** - The command line passed to the bash/cmd.
    - **out int errCode** - out parameter that returns the exit code of the executed command line.
    - **return** - string that contains the concatenation of standard error and standard output.
- `static void Error(string msg, int errCode)` - Exits the program and returns specified code.
    - **string msg** - Error message that will be displayed to user.
    - **int errCode** - Exit code of the application.

#### FilesTools.cs
Contains the class FilesTools and methods to work with files:
- `static string[] GetRecurseFiles(string pattern)` - Gets all files that are fitting the current pattern.
    - **string pattern** - pattern that will be evaluated.
    - **return** - an array of all the files found that correspond to the pattern.

#### MD5Tools.cs
Contains the class MD5Tools and methods for hashing:
- `static byte[] GenerateFilesCache(List<string> inputF)` - Generates a single MD5 hash from all the input files. Uses the following methods from the same class : **static byte[] MakeMD5File(string filename)**, **static byte[] CombineHashes(List<byte[]> input)**.
    - **List<string> inputF** - a list of input files location. The method throws unhandled error if the file does not exist.
    - **return** - an byte array that contains the hash that is result of combining the hashes of each seaparte file.
- `static byte[] CombineHashes(List<byte[]> input)` - Generates a single MD5 hash from a list of hashes.
    - **List<byte[]> input** - a list of hashes of the same size.
    - **return** - an byte array that contains the hash of the input hashes.
- `static byte[] MakeMD5String(string input)` - Generates MD5 hash from the input string.
    - **string input** - Input string.
    - **return** - an byte array that is the result of hashing the current string.
- `static byte[] MakeMD5File(string filename)` - Generates MD5 hash from the content of the file.
    - **string filename** - The name of the input file.
    - **return** - an byte array that is the result of hashing the contents of the file.

#### ParseTools.cs
Contains the class ParseTools with methods for parsing:
- `static string[] ParseArguments(string commandLine)` - parses the input string as a command line argument.
    - **string commandLine** - the input string that will be parsed.
    - **return** - and array that contains the parsed arguments, all empty elements are removed before return.
- `static string generateOption(string[] argArr)` - parses an array of string and generates a compatible option for OptionSet from Mono.Options.
    - **string[] argArr** - The input array that will be parsed.
    - **return** - a string that will be of form accepted as option in OptionSet. (Example: `o|out|output`)
- `string[] ParseResponseFile(string file)` - parses a response file into an array of strings. It uses the method **static string[] ParseArguments(string commandLine)**.
    - **string file** - The name of the file that will be parsed.
    - **return** - An array of string that contains all arguments parsed, if the file does not exit the returned array will be with 0 elements.
- `static string[] ParseArgArrayWithResponse(string[] input)` - Parses a command line that contains response files and generates a final command line that is composed from original arguments and the ones from response files in correct order. This method is using **static string[] ParseResponseFile(string file)**.
    - **string[] input** - parsed command line arguments that contain response files. (Response files must be of form: @response.txt)
    - **return** - an array that contains the original arguments with parsed arguments from response file in original order.
- `static string[] ParseComaSemicolon(string input)` - Parses arguments that are delimited by coma or/and semicolon.
    - **string input** - string that is delimited with comas and/or semicolons.
    - **return** - array of strings with parsed elements.