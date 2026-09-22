using Amazon.S3;

namespace SimpleFileStorage.ObjectStorage
{
    // S3ServicesFactory - создание и конфигурирование S3Storage
    public class S3ServicesFactory
    {
        private readonly IConfiguration _config;

        public S3ServicesFactory(IConfiguration config)
        {
            _config = config;
        }

        public IAmazonS3 CreateClient()
        {
            // TODO: прочитать про env vars (environment variables - переменные окружения)
            string s3ConnectionProfile = Environment.GetEnvironmentVariable("S3_OPTIONS_PROFILE") ?? "default";
            IConfigurationSection s3Options = _config.GetSection("S3Options").GetSection(s3ConnectionProfile);

            // подготовим конфигурацию s3
            AmazonS3Config config = new AmazonS3Config
            {
                ServiceURL = s3Options["URL"],
                ForcePathStyle = true,
                UseHttp = bool.Parse(s3Options["UseHttp"] ?? "false"),
            };
            string region = s3Options["Region"] ?? "";
            if (!string.IsNullOrEmpty(region) && string.IsNullOrEmpty(config.ServiceURL))
            {
                config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
            }
            string accessKey = s3Options["AccessKey"]!;
            string secretKey = s3Options["SecretKey"]!;

            // создать клиент
            IAmazonS3 s3 = new AmazonS3Client(accessKey, secretKey, config);
            return s3;
        }

        public FileS3Storage CreateStorage()
        {
            // создать клиент
            IAmazonS3 s3 = CreateClient();

            // считать имя бакета
            // TODO: читать S3-конфиг один раз
            string s3ConnectionProfile = Environment.GetEnvironmentVariable("S3_OPTIONS_PROFILE") ?? "default";
            IConfigurationSection s3Options = _config.GetSection("S3Options").GetSection(s3ConnectionProfile);
            string bucketName = s3Options["BucketName"] ?? "default";

            // создать сторадж
            return new FileS3Storage(s3, bucketName);
        }
    }
}
