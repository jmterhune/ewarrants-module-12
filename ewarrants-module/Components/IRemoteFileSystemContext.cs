using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tjc.Modules.eWarrants.Components
{
    internal interface IRemoteFileSystemContext
    {
        bool IsConnected();
        void Connect();
        void Disconnect();


        List<string> FileNames(string directory);
        List<string> FileNames(string directory, string filePrefix, string fileSuffixOrExtension = "");

        void UploadFile(string localFilePath, string remoteFilePath);
        void DownloadFile(string localFilePath, string remoteFilePath);

    }
    public interface IFileSystem
    {
        bool FileExists(string filePath);
        bool DirectoryExists(string directoryPath);
        void CreateDirectoryIfNotExists(string directoryPath);
        void DeleteFileIfExists(string filePath);
    }
}
