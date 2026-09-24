namespace SimpleFileStorage.Model
{
    // IFileMetadataStorage - хранилище метаданных файлов
    public interface IFileMetadataStorage
    {
        Task Insert(FileMetadata metadata);
        Task<FileMetadata?> Get(Guid fileID);
        Task<List<FileMetadata>> GetAll();
    }
}
