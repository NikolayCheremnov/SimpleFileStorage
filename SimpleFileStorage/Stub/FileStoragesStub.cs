using SimpleFileStorage.Model;

namespace SimpleFileStorage.Stub
{
    public class FileStoragesStub : IFileMetadataStorage, IFileDataStorage
    {
        private readonly Dictionary<Guid, FileMetadata> _metadatas = new();
        private readonly Dictionary<Guid, FileData> _files = new();

        public FileStoragesStub()
        {

        }

        // IFileDataStorage

        async Task IFileDataStorage.Insert(FileData file)
        {
            _files[file.FileID] = file;
        }

        async Task<FileData?> IFileDataStorage.Get(Guid fileID)
        {
            return _files.GetValueOrDefault(fileID);
        }

        // IFileMetadataStorage

        async Task IFileMetadataStorage.Insert(FileMetadata metadata)
        {
            _metadatas[metadata.FileID] = metadata;
        }

        async Task<FileMetadata?> IFileMetadataStorage.Get(Guid fileID)
        {
            return _metadatas.GetValueOrDefault(fileID);
        }
    }
}
