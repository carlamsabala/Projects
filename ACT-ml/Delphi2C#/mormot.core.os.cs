using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Mormot.Core.OS
{

    public static class OSConstants
    {
        
        public static readonly string CRLF =
#if WINDOWS
            "\r\n";
#else
            "\n";
#endif

        
#if WINDOWS
        public const string FILES_ALL = "*.*";
        public const char InvertedPathDelim = '/';
        public const bool PathCaseInsensitive = true;
#else
        public const string FILES_ALL = "*";
        public const char InvertedPathDelim = '\\';
        public const bool PathCaseInsensitive = false;
#endif

        
        public const FileShare fmShareRead      = FileShare.Read;
        public const FileShare fmShareWrite     = FileShare.Write;
        public const FileShare fmShareReadWrite = FileShare.ReadWrite;
    }

    

    public static class HttpStatus
    {
        
        public const int HTTP_NONE = 0;
        public const int HTTP_CONTINUE = 100;
        public const int HTTP_SWITCHINGPROTOCOLS = 101;
        public const int HTTP_SUCCESS = 200;
        public const int HTTP_CREATED = 201;
        public const int HTTP_ACCEPTED = 202;
        public const int HTTP_NONAUTHORIZEDINFO = 203;
        public const int HTTP_NOCONTENT = 204;
        public const int HTTP_RESETCONTENT = 205;
        public const int HTTP_PARTIALCONTENT = 206;
        public const int HTTP_MULTIPLECHOICES = 300;
        public const int HTTP_MOVEDPERMANENTLY = 301;
        public const int HTTP_FOUND = 302;
        public const int HTTP_SEEOTHER = 303;
        public const int HTTP_NOTMODIFIED = 304;
        public const int HTTP_USEPROXY = 305;
        public const int HTTP_TEMPORARYREDIRECT = 307;
        public const int HTTP_PERMANENTREDIRECT = 308;
        public const int HTTP_BADREQUEST = 400;
        public const int HTTP_UNAUTHORIZED = 401;
        public const int HTTP_FORBIDDEN = 403;
        public const int HTTP_NOTFOUND = 404;
        public const int HTTP_NOTALLOWED = 405;
        public const int HTTP_NOTACCEPTABLE = 406;
        public const int HTTP_PROXYAUTHREQUIRED = 407;
        public const int HTTP_TIMEOUT = 408;
        public const int HTTP_CONFLICT = 409;
        public const int HTTP_PAYLOADTOOLARGE = 413;
        public const int HTTP_RANGENOTSATISFIABLE = 416;
        public const int HTTP_TEAPOT = 418;
        public const int HTTP_SERVERERROR = 500;
        public const int HTTP_NOTIMPLEMENTED = 501;
        public const int HTTP_BADGATEWAY = 502;
        public const int HTTP_UNAVAILABLE = 503;
        public const int HTTP_GATEWAYTIMEOUT = 504;
        public const int HTTP_HTTPVERSIONNONSUPPORTED = 505;

        private static readonly int[] HTTP_CODES = new int[]
        {
            HTTP_SUCCESS,
            HTTP_NOCONTENT,
            HTTP_TEMPORARYREDIRECT,
            HTTP_PERMANENTREDIRECT,
            HTTP_MOVEDPERMANENTLY,
            HTTP_BADREQUEST,
            HTTP_UNAUTHORIZED,
            HTTP_FORBIDDEN,
            HTTP_NOTFOUND,
            HTTP_NOTALLOWED,
            HTTP_NOTMODIFIED,
            HTTP_NOTACCEPTABLE,
            HTTP_PARTIALCONTENT,
            HTTP_PAYLOADTOOLARGE,
            HTTP_CREATED,
            HTTP_SEEOTHER,
            HTTP_CONTINUE,
            HTTP_SWITCHINGPROTOCOLS,
            HTTP_ACCEPTED,
            HTTP_NONAUTHORIZEDINFO,
            HTTP_RESETCONTENT,
            207, 
            HTTP_MULTIPLECHOICES,
            HTTP_FOUND,
            HTTP_USEPROXY,
            HTTP_PROXYAUTHREQUIRED,
            HTTP_TIMEOUT,
            HTTP_CONFLICT,
            410, 
            411, 
            412, 
            414, 
            415, 
            HTTP_RANGENOTSATISFIABLE,
            HTTP_TEAPOT,
            426, 
            HTTP_SERVERERROR,
            HTTP_NOTIMPLEMENTED,
            HTTP_BADGATEWAY,
            HTTP_UNAVAILABLE,
            HTTP_GATEWAYTIMEOUT,
            HTTP_HTTPVERSIONNONSUPPORTED,
            511, 
            513  
        };

        private static readonly string[] HTTP_REASON = new string[]
        {
            "OK",
            "No Content",
            "Temporary Redirect",
            "Permanent Redirect",
            "Moved Permanently",
            "Bad Request",
            "Unauthorized",
            "Forbidden",
            "Not Found",
            "Method Not Allowed",
            "Not Modified",
            "Not Acceptable",
            "Partial Content",
            "Payload Too Large",
            "Created",
            "See Other",
            "Continue",
            "Switching Protocols",
            "Accepted",
            "Non-Authoritative Information",
            "Reset Content",
            "Multi-Status",
            "Multiple Choices",
            "Found",
            "Use Proxy",
            "Proxy Authentication Required",
            "Request Timeout",
            "Conflict",
            "Gone",
            "Length Required",
            "Precondition Failed",
            "URI Too Long",
            "Unsupported Media Type",
            "Requested Range Not Satisfiable",
            "I'm a teapot",
            "Upgrade Required",
            "Internal Server Error",
            "Not Implemented",
            "Bad Gateway",
            "Service Unavailable",
            "Gateway Timeout",
            "HTTP Version Not Supported",
            "Network Authentication Required",
            "Invalid Request"
        };

        
        public static string StatusCodeToText(int code)
        {
            int index = Array.IndexOf(HTTP_CODES, code);
            if (index < 0)
            {
                
                index = HTTP_CODES.Length - 1;
            }
            return HTTP_REASON[index];
        }

        
        public static void StatusCodeToReason(int code, out string reason)
        {
            reason = StatusCodeToText(code);
        }

        
        public static string StatusCodeToShort(int code)
        {
            if (code > 599) code = 999;
            return $"{code} {StatusCodeToText(code)}";
        }

        
        public static bool StatusCodeIsSuccess(int code)
        {
            return code >= HTTP_SUCCESS && code < HTTP_BADREQUEST;
        }
    }

    
    public static class FileUtils
    {
        
        public static string NormalizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return fileName;
#if WINDOWS
            return fileName.Replace('/', Path.DirectorySeparatorChar);
#else
            return fileName.Replace('\\', Path.DirectorySeparatorChar);
#endif
        }

        
        public static string QuoteFileName(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) &&
                fileName.Contains(" ") &&
                !fileName.StartsWith("\""))
            {
                return $"\"{fileName}\"";
            }
            return fileName;
        }

        
        public static string EnsureDirectoryExists(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("Directory name is empty");

            string full = Path.GetFullPath(directory);
            if (!Directory.Exists(full))
            {
                Directory.CreateDirectory(full);
            }
            return full;
        }

        
        public static string ExtractPath(string fileName)
        {
            return Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar;
        }

        
        public static string ExtractName(string fileName)
        {
            return Path.GetFileName(fileName);
        }

        
        public static string ExtractExt(string fileName, bool withoutDot = false)
        {
            string ext = Path.GetExtension(fileName);
            return withoutDot ? ext.TrimStart('.') : ext;
        }

        
        public static string GetFileNameWithoutExt(string fileName, out string extension)
        {
            extension = Path.GetExtension(fileName);
            return Path.GetFileNameWithoutExtension(fileName);
        }

        
        public static string GetFileNameWithoutExtOrPath(string fileName)
        {
            return Path.GetFileNameWithoutExtension(Path.GetFileName(fileName));
        }

        
        public static bool DirectoryDelete(string directory, string mask = "*.*", bool deleteOnlyFilesNotDirectory = false, out int deletedCount)
        {
            deletedCount = 0;
            if (!Directory.Exists(directory))
                return true;
            bool result = true;
            foreach (var file in Directory.GetFiles(directory, mask))
            {
                try
                {
                    File.Delete(file);
                    deletedCount++;
                }
                catch
                {
                    result = false;
                }
            }
            if (!deleteOnlyFilesNotDirectory)
            {
                try
                {
                    Directory.Delete(directory);
                }
                catch
                {
                    result = false;
                }
            }
            return result;
        }

        
        public static bool DirectoryDeleteOlderFiles(string directory, TimeSpan ageThreshold, string mask = "*.*", bool recursive = false, out long totalSize)
        {
            totalSize = 0;
            if (!Directory.Exists(directory))
                return true;
            bool result = true;
            DateTime cutoff = DateTime.UtcNow - ageThreshold;
            foreach (var file in Directory.GetFiles(directory, mask))
            {
                try
                {
                    DateTime lastWrite = File.GetLastWriteTimeUtc(file);
                    if (lastWrite < cutoff)
                    {
                        totalSize += new FileInfo(file).Length;
                        File.Delete(file);
                    }
                }
                catch
                {
                    result = false;
                }
            }
            if (recursive)
            {
                foreach (var subDir in Directory.GetDirectories(directory))
                {
                    DirectoryDeleteOlderFiles(subDir, ageThreshold, mask, true, out long subSize);
                    totalSize += subSize;
                }
            }
            return result;
        }

        
        public static string TemporaryFileName()
        {
            return Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)}_{Guid.NewGuid():N}.tmp");
        }
    }


    public class MemoryMap : IDisposable
    {
        private MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;
        public byte[] Buffer { get; private set; }
        public long Size { get; private set; }
        public long FileSize { get; private set; }

        
        public MemoryMap(string filePath, long? customSize = null, long customOffset = 0)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            FileSize = new FileInfo(filePath).Length;
            Size = customSize ?? (FileSize - customOffset);
            
            if (Size < (1 << 20))
            {
                Buffer = File.ReadAllBytes(filePath);
            }
            else
            {
                _mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, FileSize, MemoryMappedFileAccess.Read);
                _accessor = _mmf.CreateViewAccessor(customOffset, Size, MemoryMappedFileAccess.Read);
                Buffer = new byte[Size];
                _accessor.ReadArray(0, Buffer, 0, (int)Size);
            }
        }

        public void Dispose()
        {
            _accessor?.Dispose();
            _mmf?.Dispose();
        }
    }

    
    public class LightLock
    {
        private int _flag;

        public void Lock()
        {
            while (Interlocked.Exchange(ref _flag, 1) != 0)
            {
                Thread.SpinWait(100);
                Thread.Yield();
            }
        }

        public bool TryLock()
        {
            return Interlocked.Exchange(ref _flag, 1) == 0;
        }

        public void Unlock()
        {
            Volatile.Write(ref _flag, 0);
        }
    }

    
    public class RWLock
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public void ReadLock() => _lock.EnterReadLock();
        public void ReadUnlock() => _lock.ExitReadLock();
        public void WriteLock() => _lock.EnterWriteLock();
        public void WriteUnlock() => _lock.ExitWriteLock();
    }

    
    public class OSLock
    {
        private readonly object _lockObj = new object();

        public void Lock() => Monitor.Enter(_lockObj);
        public bool TryLock() => Monitor.TryEnter(_lockObj);
        public void Unlock() => Monitor.Exit(_lockObj);
    }


    public class ExecutableCommandLine
    {
        public List<string> Args { get; private set; }
        public ExecutableCommandLine()
        {
            Args = new List<string>(Environment.GetCommandLineArgs());
            if (Args.Count > 0)
                Args.RemoveAt(0);
        }

        
        public string FullDescription(string exeDescription = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.IsNullOrEmpty(exeDescription)
                ? "Usage:"
                : exeDescription);
            sb.AppendLine("Arguments:");
            foreach (var arg in Args)
                sb.AppendLine("  " + arg);
            return sb.ToString();
        }

        
        public string DetectUnknown(HashSet<string> expectedParameters)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var arg in Args)
            {
                if (!expectedParameters.Contains(arg))
                    sb.AppendLine($"Unexpected parameter: {arg}");
            }
            return sb.ToString();
        }
    }

    
    public static class ExceptionInterceptor
    {
        
        public static void Setup(Action<Exception> handler)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                handler(e.ExceptionObject as Exception);
            };
        }
    }

    
    public class FileVersionInfoWrapper
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Release { get; private set; }
        public int Build { get; private set; }
        public DateTime BuildDateTime { get; private set; }
        public string Detailed => $"{Major}.{Minor}.{Release}.{Build}";
        public string FileName { get; private set; }

        public FileVersionInfoWrapper(string fileName)
        {
            FileName = fileName;
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(fileName);
            Major = fvi.FileMajorPart;
            Minor = fvi.FileMinorPart;
            Release = fvi.FileBuildPart;
            Build = fvi.FilePrivatePart;
            BuildDateTime = File.GetCreationTime(fileName);
        }

        public override string ToString()
        {
            return $"{Path.GetFileName(FileName)} {Detailed} ({BuildDateTime:yyyy-MM-dd HH:mm:ss})";
        }
    }

    
    public static class ConsoleUtils
    {
        
        public static void ConsoleWrite(string text, ConsoleColor color = ConsoleColor.Gray, bool noLineFeed = false)
        {
            Console.ForegroundColor = color;
            if (noLineFeed)
                Console.Write(text);
            else
                Console.WriteLine(text);
            Console.ResetColor();
        }

        
        public static void ConsoleWaitForEnterKey()
        {
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
        }
    }

    
    public static class ServiceHelpers
    {
        
        public static bool InstallService(string serviceName, string displayName, string exePath, string dependencies = "", string username = "", string password = "")
        {
            throw new NotImplementedException("Service installation not implemented in this port.");
        }

        public static bool StartService(string serviceName)
        {
            
            throw new NotImplementedException("Service start not implemented in this port.");
        }

        public static bool StopService(string serviceName)
        {
            
            throw new NotImplementedException("Service stop not implemented in this port.");
        }
    }

    
    public static class PrivilegeHelpers
    {
        
        public static bool DropPrivileges(string userName = "nobody")
        {
            
            throw new NotImplementedException("Privilege dropping is not implemented in this port.");
        }
    }
}
