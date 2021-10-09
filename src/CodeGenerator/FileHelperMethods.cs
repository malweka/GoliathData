using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Data.SqlClient;

namespace Goliath.Data.CodeGenerator
{
    public static class FileHelperMethods
    {
        /// <summary>
        /// Gets the database name from connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public static string GetDatabaseNameFromConnectionString(string connectionString)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            return connectionStringBuilder.InitialCatalog;
        }

        /// <summary>
        /// Unzips the specified zipped stream to destination directory name.
        /// </summary>
        /// <param name="zippedStream">The zipped stream.</param>
        /// <param name="destinationDirectoryName">Name of the destination directory.</param>
        /// <exception cref="System.ArgumentNullException">zippedStream</exception>
        /// <exception cref="System.ArgumentException">The directory does not exist. - destinationDirectoryName</exception>
        public static void Unzip(this Stream zippedStream, string destinationDirectoryName)
        {
            if (zippedStream == null)
                throw new ArgumentNullException(nameof(zippedStream));

            if (!Directory.Exists(destinationDirectoryName))
                throw new ArgumentException("The directory does not exist.", nameof(destinationDirectoryName));

            using (var archive = new ZipArchive(zippedStream, ZipArchiveMode.Read))
            {
                ExtractToDirectory(archive, destinationDirectoryName);
            }
        }

        /// <summary>
        /// Zips the specified files.
        /// </summary>
        /// <param name="zipStream">The zip stream.</param>
        /// <param name="filePaths">The file paths.</param>
        public static void Zip(this Stream zipStream, string[] filePaths)
        {
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                foreach (var filePath in filePaths)
                {
                    var name = Path.GetFileName(filePath);
                    CreateEntryFromFile(archive, filePath, name, CompressionLevel.Optimal);
                }
            }
        }
        
        public static void ExtractToDirectory(ZipArchive source, string destinationDirectoryName)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (destinationDirectoryName == null)
            {
                throw new ArgumentNullException(nameof(destinationDirectoryName));
            }
            DirectoryInfo directoryInfo = Directory.CreateDirectory(destinationDirectoryName);
            string fullName = directoryInfo.FullName;
            foreach (ZipArchiveEntry current in source.Entries)
            {
                string fullPath = Path.GetFullPath(Path.Combine(fullName, current.FullName));
                if (!fullPath.StartsWith(fullName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("Extracting Zip entry would have resulted in a file outside the specified destination directory.");
                }
                if (Path.GetFileName(fullPath).Length == 0)
                {
                    if (current.Length != 0L)
                    {
                        throw new IOException("Zip entry name ends in directory separator character but contains data.");
                    }
                    Directory.CreateDirectory(fullPath);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    ExtractToFile(current, fullPath, false);
                }
            }
        }

        static ZipArchiveEntry CreateEntryFromFile(ZipArchive destination, string sourceFileName, string entryName, CompressionLevel compressionLevel)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (sourceFileName == null)
            {
                throw new ArgumentNullException(nameof(sourceFileName));
            }
            if (entryName == null)
            {
                throw new ArgumentNullException(nameof(entryName));
            }
            ZipArchiveEntry result;
            using (Stream stream = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ZipArchiveEntry zipArchiveEntry = destination.CreateEntry(entryName, compressionLevel);
                DateTime lastWriteTime = File.GetLastWriteTime(sourceFileName);
                if (lastWriteTime.Year < 1980 || lastWriteTime.Year > 2107)
                {
                    lastWriteTime = new DateTime(1980, 1, 1, 0, 0, 0);
                }
                zipArchiveEntry.LastWriteTime = lastWriteTime;
                using (Stream stream2 = zipArchiveEntry.Open())
                {
                    stream.CopyTo(stream2);
                }
                result = zipArchiveEntry;
            }
            return result;
        }

        static void ExtractToFile(ZipArchiveEntry source, string destinationFileName, bool overwrite)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (destinationFileName == null)
            {
                throw new ArgumentNullException(nameof(destinationFileName));
            }
            FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
            using (Stream stream = File.Open(destinationFileName, mode, FileAccess.Write, FileShare.None))
            {
                using (Stream stream2 = source.Open())
                {
                    stream2.CopyTo(stream);
                }
            }
            File.SetLastWriteTime(destinationFileName, source.LastWriteTime.DateTime);
        }
    }
}