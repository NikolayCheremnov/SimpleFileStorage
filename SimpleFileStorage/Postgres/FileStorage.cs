using Microsoft.EntityFrameworkCore;
using SimpleFileStorage.Model;
using static System.Net.WebRequestMethods;

namespace SimpleFileStorage.Postgres
{
    // FileStorage - postgres-хранилище файлов
    public class FileStorage : IFileDataStorage, IFileMetadataStorage
    {
        private readonly AppDbContext _db;

        public FileStorage(AppDbContext db)
        {
            _db = db;
        }

        // IFileDataStorage
        [Obsolete("use ObjectStorage.FileS3Storage instead")]
        async Task IFileDataStorage.Insert(FileData file)
        {
            await _db.Files.AddAsync(file);
            await _db.SaveChangesAsync();
        }

        [Obsolete("use ObjectStorage.FileS3Storage instead")]
        async Task<FileData?> IFileDataStorage.Get(Guid fileID)
        {
            return await _db.Files.FirstOrDefaultAsync(f => f.FileID == fileID);
        }

        // IFileMetadataStorage

        async Task IFileMetadataStorage.Insert(FileMetadata metadata)
        {
            await _db.Metadatas.AddAsync(metadata);
            await _db.SaveChangesAsync();
        }

        async Task<FileMetadata?> IFileMetadataStorage.Get(Guid fileID)
        {
            return await _db.Metadatas.FirstOrDefaultAsync(md => md.FileID == fileID);
        }

        async Task<List<FileMetadata>> IFileMetadataStorage.GetAll()
        {
            return await _db.Metadatas.ToListAsync();
        }
    }
}
