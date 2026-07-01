using System;
using System.IO;

namespace Lanternfall.Gameplay.Save
{
    public interface ISaveStorage
    {
        bool Exists { get; }
        string Read();
        string ReadBackup();
        void WriteAtomic(string contents);
    }

    public sealed class FileSaveStorage : ISaveStorage
    {
        private readonly string _path;
        private readonly string _backupPath;
        private readonly string _temporaryPath;

        public FileSaveStorage(string directory, string filename = "lanternfall.save")
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException(nameof(directory));
            Directory.CreateDirectory(directory);
            _path = Path.Combine(directory, filename);
            _backupPath = _path + ".bak";
            _temporaryPath = _path + ".tmp";
        }

        public bool Exists => File.Exists(_path);
        public string Read() => File.ReadAllText(_path);
        public string ReadBackup() =>
            File.Exists(_backupPath) ? File.ReadAllText(_backupPath) : null;

        public void WriteAtomic(string contents)
        {
            if (contents == null) throw new ArgumentNullException(nameof(contents));
            File.WriteAllText(_temporaryPath, contents);
            if (File.Exists(_path))
            {
                try
                {
                    File.Replace(_temporaryPath, _path, _backupPath, true);
                }
                catch (PlatformNotSupportedException)
                {
                    File.Copy(_path, _backupPath, true);
                    File.Copy(_temporaryPath, _path, true);
                    File.Delete(_temporaryPath);
                }
            }
            else
            {
                File.Move(_temporaryPath, _path);
            }
        }
    }
}

