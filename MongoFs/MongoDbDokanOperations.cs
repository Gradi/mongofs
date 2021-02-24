using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using DokanNet;
using MongoFs.Handlers;
using MongoFs.Paths;
using Serilog;
using FileAccess = DokanNet.FileAccess;

namespace MongoFs
{
    public class MongoDbDokanOperations : IDokanOperations
    {
        private readonly ILogger _logger;
        private readonly CommandLineArguments _commandLineArgs;
        private readonly IPathParser _pathParser;
        private readonly IMongoDb _mongoDb;

        private readonly FindFilesWithPatternHandler _findFilesWithPatternHander;
        private readonly GetFileInformationHandler _getFileInformationHandler;
        private readonly ReadFileHandler _readFileHandler;

        public MongoDbDokanOperations
            (
                ILogger logger,
                CommandLineArguments commandLineArguments,
                IPathParser pathParser,
                IMongoDb mongoDb,

                FindFilesWithPatternHandler findFilesWithPatternHander,
                GetFileInformationHandler getFileInformationHandler,
                ReadFileHandler readFileHandler
            )
        {
            _logger = logger.ForContext<MongoDbDokanOperations>();
            _commandLineArgs = commandLineArguments;
            _pathParser = pathParser;
            _mongoDb = mongoDb;

            _findFilesWithPatternHander = findFilesWithPatternHander;
            _getFileInformationHandler = getFileInformationHandler;
            _readFileHandler = readFileHandler;
        }

        public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options,
                                   FileAttributes attributes, IDokanFileInfo info)
        {
            _logger.Verbose("CreateFile({fileName}, {access}, {share}, {mode}, {options}, {attributes} {@info}",
                fileName, access, share, mode, options, attributes, info);

            if ( access == FileAccess.None ||
                (access & FileAccess.ReadData) == FileAccess.ReadData ||
                (access & FileAccess.Execute) == FileAccess.Execute ||
                (access & FileAccess.ReadAttributes) == FileAccess.ReadAttributes ||
                (access & FileAccess.GenericExecute) == FileAccess.GenericExecute ||
                (access & FileAccess.GenericRead) == FileAccess.GenericRead ||
                (access & FileAccess.Synchronize) == FileAccess.Synchronize)
            {
                return NtStatus.Success;
            }

            return NtStatus.NotImplemented;
        }

        public void Cleanup(string fileName, IDokanFileInfo info) {}

        public void CloseFile(string fileName, IDokanFileInfo info) {}

        public NtStatus ReadFile(
            string fileName,
            byte[] buffer,
            out int bytesRead,
            long offset,
            IDokanFileInfo info)
        {
            _logger.Verbose("ReadFile({fileName}, {@info})", fileName, info);
            var path = _pathParser.Parse(fileName);
            if (path == null)
            {
                bytesRead = 0;
                return NtStatus.Error;
            }

            return _readFileHandler.ReadFile(path, buffer, out bytesRead, offset, info);
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            _logger.Verbose("WriteFile({fileName}, {@info}", fileName, info);
            bytesWritten = 0;
            return NtStatus.NotImplemented;
        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info) => NtStatus.NotImplemented;

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            _logger.Verbose("GetFileInformation({fileName}, {@info})", fileName, info);

            var path = _pathParser.Parse(fileName);
            if (path == null)
            {
                fileInfo = default;
                return NtStatus.Error;
            }

            return _getFileInformationHandler.GetFileInformation(path, out fileInfo, info);
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {
            _logger.Verbose("FindFiles({fileName}, {@info}", fileName, info);

            var path = _pathParser.Parse(fileName);
            if (path == null)
            {
                files = new List<FileInformation>();
                return NtStatus.Error;
            }

            return _findFilesWithPatternHander.FindFilesWithPattern(path, searchPattern: null, out files, info);
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files,
                                             IDokanFileInfo info)
        {
            _logger.Verbose("FindFilesWithPattern({fileName}, {searchPattern}, {@info})", fileName, searchPattern, info);

            var path = _pathParser.Parse(fileName);
            if (path == null)
            {
                files = new List<FileInformation>();
                return NtStatus.Error;
            }

            return _findFilesWithPatternHander.FindFilesWithPattern(path, searchPattern, out files, info);
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            _logger.Verbose("SetFileAttributes({fileName}, {attributes}, {@info})", fileName, attributes, info);
            return NtStatus.NotImplemented;
        }

        public NtStatus SetFileTime(
            string fileName,
            DateTime? creationTime,
            DateTime? lastAccessTime,
            DateTime? lastWriteTime,
            IDokanFileInfo info)
        {
            _logger.Verbose("SetFileTime({fileName}, {creationTime}, {lastAccessTime}, {lastWriteTime}, {@info}",
                fileName, creationTime, lastAccessTime, lastWriteTime, info);
            return NtStatus.NotImplemented;
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            _logger.Verbose("DeleteFile({fileName}, {@info})", fileName, info);
            return NtStatus.NotImplemented;
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            _logger.Verbose("DeleteDirectory({fileName}, {@info})", fileName, info);
            return NtStatus.NotImplemented;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            _logger.Verbose("MoveFile({oldName}, {newName}, {replace}, {@info}", oldName, newName, replace, info);
            return NtStatus.NotImplemented;
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            _logger.Verbose("SetEndOfFile({fileName}, {length}, {@info}", fileName, length, info);
            return NtStatus.NotImplemented;
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            _logger.Verbose("SetAllocationSize({fileName}, {length}, {@info}", fileName, length, info);
            return NtStatus.NotImplemented;
        }

        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            _logger.Verbose("LockFile({fileName}, {offset}, {length}, {@info}", fileName, offset, length, info);
            return NtStatus.NotImplemented;
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            _logger.Verbose("UnlockFile({fileName}, {offset}, {length}, {@info}", fileName, offset, length, info);
            return NtStatus.NotImplemented;
        }

        public NtStatus GetDiskFreeSpace(
            out long freeBytesAvailable,
            out long totalNumberOfBytes,
            out long totalNumberOfFreeBytes,
            IDokanFileInfo info)
        {
            _logger.Verbose("GetDiskFreeSpace({@info})", info);
            freeBytesAvailable = 0;
            totalNumberOfBytes = _mongoDb.GetTotalDbSize();
            totalNumberOfFreeBytes = 0;
            return NtStatus.Success;
        }

        public NtStatus GetVolumeInformation(
            out string volumeLabel,
            out FileSystemFeatures features,
            out string fileSystemName,
            out uint maximumComponentLength,
            IDokanFileInfo info)
        {
            _logger.Verbose("GetVolumeInformation({@info})", info);

            volumeLabel = _commandLineArgs.Name;
            features = FileSystemFeatures.ReadOnlyVolume | FileSystemFeatures.CaseSensitiveSearch;
            fileSystemName = "MongoFs";
            maximumComponentLength = 0;

            return NtStatus.Success;
        }

        public NtStatus GetFileSecurity(
            string fileName,
            out FileSystemSecurity security,
            AccessControlSections sections,
            IDokanFileInfo info)
        {
            _logger.Verbose("GetFileSecurity({fileName}, {sections}, {@info}", fileName, sections, info);
            security = default!;
            return NtStatus.NotImplemented;
        }

        public NtStatus SetFileSecurity(
            string fileName,
            FileSystemSecurity security,
            AccessControlSections sections,
            IDokanFileInfo info)
        {
            _logger.Verbose("SetFileSecurity({fileName}, {@security}, {sections}, {@info}",
                fileName, security, sections, info);
            return NtStatus.NotImplemented;
        }

        public NtStatus Mounted(IDokanFileInfo info)
        {
            _logger.Verbose("Mounted({@info})", info);
            return NtStatus.Success;
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            _logger.Verbose("Unmounted({@info})", info);
            return NtStatus.Success;
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            _logger.Verbose("FindStreams({fileName}, {@info}", fileName, info);
            streams = Array.Empty<FileInformation>();
            return NtStatus.NotImplemented;
        }
    }
}
