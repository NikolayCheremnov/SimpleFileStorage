namespace SimpleFileStorage.Model
{
    // FileMetadata - метаданные файла
    public class FileMetadata
    {
        public Guid FileID { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public long SizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }

        public FileMetadata() { }
    }
}
