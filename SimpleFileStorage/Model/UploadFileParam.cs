namespace SimpleFileStorage.Model
{
    // UploadFileParam - параметр для загрузки файла
    public class UploadFileParam
    {
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public required byte[] Data { get; set; }

        public UploadFileParam() { }
    }
}
