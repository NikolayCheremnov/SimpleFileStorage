using Amazon.S3;
using Amazon.S3.Model;
using SimpleFileStorage.Model;
using System.Net;

namespace SimpleFileStorage.ObjectStorage
{
    public class FileS3Storage : IFileDataStorage
    {
        private readonly IAmazonS3 _s3Client; // клиент для работы с S3-хранилищем
        private readonly string _bucketName;

        public FileS3Storage(IAmazonS3 s3Client, string bucketName)
        {
            _s3Client = s3Client;
            _bucketName = bucketName;
        }

        public async Task<FileData?> Get(Guid fileID)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileID.ToString()
                };
                using GetObjectResponse response = await _s3Client.GetObjectAsync(request);
                using MemoryStream memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);
                return new FileData
                {
                    FileID = fileID,
                    Data = memoryStream.ToArray()
                };
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task Insert(FileData file)
        {
            PutObjectRequest request = new PutObjectRequest()
            {
                BucketName = _bucketName,
                Key = file.FileID.ToString(),
                InputStream = new MemoryStream(file.Data)
            };
            await _s3Client.PutObjectAsync(request);
        }
    }
}
