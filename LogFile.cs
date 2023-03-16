using System.IO;
using System.Threading;

namespace FileSynchronizer
{
    public class LogFile
    {
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private readonly string _fileName;

        public LogFile(string fileName)
        {
            _fileName = fileName;
        }

        public void WriteToFile(string info)
        {
            _locker.EnterWriteLock();
            try
            {
                if (!File.Exists(_fileName))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_fileName));
                    File.Create(_fileName).Dispose();
                }
                using (TextWriter tw = new StreamWriter(_fileName, true))
                {
                    tw.WriteLine(info);
                }
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }
    }
}
