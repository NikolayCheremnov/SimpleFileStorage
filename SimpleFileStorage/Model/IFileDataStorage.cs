namespace SimpleFileStorage.Model
{
    // IFileDataStorage - хранилище самих файлов
    public interface IFileDataStorage
    {
        Task Insert(FileData file);
        Task<FileData?> Get(Guid fileID);
    }
}
