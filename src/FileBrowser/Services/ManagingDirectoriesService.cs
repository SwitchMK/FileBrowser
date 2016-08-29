using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileBrowser.Models;
using ZetaLongPaths;
using System;

namespace FileBrowser.Services
{
    public class ManagingDirectoriesService
    {
        public DirectoryModel[] GetDirectories(string path)
        {
            try
            {
                return new DirectoryInfo(path).GetDirectories()
                    .Select(directory => new DirectoryModel
                    {
                        Name = directory.Name,
                        Path = directory.FullName,
                        Type = Models.Type.Directory
                    }).ToArray();
            }
            catch
            {
                return null;
            }
        }

        public DirectoryModel[] GetLogicalDrives()
        {
            return DriveInfo.GetDrives().Where(drive => drive.DriveType == DriveType.Fixed)
                .Select(drive => new DirectoryModel
                {
                    Name = drive.Name,
                    Path = drive.Name,
                    Type = Models.Type.Drive
                }).ToArray();
        }

        public DirectoryModel[] GetFilesInDirectory(string path)
        {
            try
            {
                var files = new DirectoryInfo(path).GetFiles()
                    .Select(file => new DirectoryModel
                    {
                        Name = file.Name,
                        Path = path,
                        Type = Models.Type.File
                    }).ToArray();

                var directories = GetDirectories(path);

                var filesAndDirectories = new List<DirectoryModel>();

                filesAndDirectories.AddRange(directories);
                filesAndDirectories.AddRange(files);

                return filesAndDirectories.ToArray();
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        public DirectoryModel GoTopDirectory(string path)
        {
            var directory = new DirectoryInfo(path).Parent;
            if (directory == null)
                return new DirectoryModel
                {
                    Name = new DirectoryInfo(path).Name,
                    Path = path,
                    Type = Models.Type.Directory
                };

            return new DirectoryModel
            {
                Name = directory.Name,
                Path = directory.FullName,
                Type = Models.Type.Directory
            };
        }

        public int[] GetAmountOfFiles(string path, CancellationToken cancellationToken)
        {
            int less = 0, between = 0, more = 0;

                Parallel.Invoke(() =>
                {
                    try
                    {
                        less = GetFiles(path, 0, GetBytes(10), cancellationToken).Count();
                    }
                    catch { }

                },
                () => {
                    try
                    {
                        between = GetFiles(path, GetBytes(10), GetBytes(50), cancellationToken).Count();
                    }
                    catch { }
                },
                () => {
                    try
                    {
                        more = GetFiles(path, GetBytes(100), -1, cancellationToken).Count();
                    }
                    catch { }
                });

            return new[] {less, between, more};
        }

        private long GetBytes(int megaBytes)
        {
            return megaBytes*1024*1024;
        }

        private IEnumerable<string> GetFiles(string path, long startVolume, long endVolume, CancellationToken cancellationToken)
        {
            var queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                path = queue.Dequeue();
                try
                {
                    foreach (
                        var subDirectory in
                            new ZlpDirectoryInfo(path).GetDirectories().Select(directory => directory.FullName))
                        queue.Enqueue(subDirectory);
                }
                catch { }

                string[] files = null;
                try
                {
                    if (endVolume > 0)
                        files = new ZlpDirectoryInfo(path)
                            .GetFiles()
                            .Where(file => file.Length <= endVolume && file.Length > startVolume)
                            .Select(file => file.FullName).ToArray();
                    else
                        files = new ZlpDirectoryInfo(path)
                            .GetFiles()
                            .Where(file => file.Length > startVolume)
                            .Select(file => file.FullName).ToArray();
                }
                catch { }

                if (files == null) continue;

                foreach (var file in files)
                {
                    yield return file;
                }
            }
        }
    }
}