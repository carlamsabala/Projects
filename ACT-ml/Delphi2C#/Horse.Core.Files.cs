using System;
using System.IO;

namespace Horse.Core.Files
{
    public class HorseCoreFile : IDisposable
    {
        private string _fileName;
        private string _name;
        private FileStream _fileStream;
        private bool _freeContentStream;
        private string _contentType;

        public HorseCoreFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Invalid FileName");

            if (!File.Exists(fileName))
                throw new FileNotFoundException("File not exist", fileName);

            _fileName = fileName;
            _name = Path.GetFileName(_fileName);
            _freeContentStream = true;
            _contentType = HorseMimeTypes.GetFileType(_fileName);
        }

        public string ContentType()
        {
            return _contentType;
        }

        public Stream ContentStream()
        {
            if (_fileStream == null)
            {
                _fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            return _fileStream;
        }

        public long Size()
        {
            return ContentStream().Length;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool FreeContentStream
        {
            get { return _freeContentStream; }
            set { _freeContentStream = value; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _freeContentStream && _fileStream != null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }
        }

        ~HorseCoreFile()
        {
            Dispose(false);
        }
    }

    public static class HorseMimeTypes
    {
        public static string GetFileType(string fileName)
        {
            string ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            switch (ext)
            {
                case ".html":
                case ".htm":
                    return "text/html";
                case ".txt":
                    return "text/plain";
                case ".json":
                    return "application/json";
                case ".xml":
                    return "application/xml";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
