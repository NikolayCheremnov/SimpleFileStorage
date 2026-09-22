namespace SimpleFileStorage.Model
{
    // FileData - данные файла
    public class FileData
    {
        public Guid FileID { get; set; }
        public required byte[] Data { get; set; }

        public FileData() { }
    }
}
