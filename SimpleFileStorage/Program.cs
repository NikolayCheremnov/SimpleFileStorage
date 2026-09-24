using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using SimpleFileStorage.Model;
using SimpleFileStorage.ObjectStorage;
using SimpleFileStorage.Postgres;
using SimpleFileStorage.Stub;

var builder = WebApplication.CreateBuilder(args);

// ВАРИАНТ ПРИЛОЖЕНИЯ С ЗАГЛУШКАМИ
// builder.Services.AddSingleton<IFileDataStorage, FileStoragesStub>();
// builder.Services.AddSingleton<IFileMetadataStorage, FileStoragesStub>();
// builder.Services.AddSingleton<FileService>();

// ВАРИАНТ ПРИЛОЖЕНИЯ С только Postgres
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddTransient<IFileDataStorage, FileStorage>();
builder.Services.AddTransient<IFileMetadataStorage, FileStorage>();
builder.Services.AddTransient<FileService>();

// ВАРИАНТ ПРИЛОЖЕНИЯ С Postgres и S3
// builder.Services.AddDbContext<AppDbContext>();
// builder.Services.AddTransient<IFileMetadataStorage, FileStorage>();
// builder.Services.AddTransient<S3ServicesFactory>();
// builder.Services.AddTransient<IAmazonS3>(opts => opts.GetRequiredService<S3ServicesFactory>().CreateClient());
// builder.Services.AddTransient<IFileDataStorage>(opts => opts.GetRequiredService<S3ServicesFactory>().CreateStorage());
// builder.Services.AddTransient<FileService>();

// ДОБАВЛЕНИЕ КОНТРОЛЛЕРОВ В КОНТЕЙНЕР ЗАВИСИМОСТЕЙ (IoC-контейнер)
builder.Services.AddControllers();

var app = builder.Build();

// ВКЛЮЧЕНИЕ КОНТРОЛЛЕРОВ (МАППИНГ)
app.MapControllers();

// ВЫЗОВ АВТОМИГРАЦИЙ БД
await AutoApplyMigrationsWithBackoff();

// ВЫЗОВ АВТОСОЗДАНИЯ БАКЕТА В S3
// await AutoEnsureS3BucketExistsWithBackoff();

app.Run();

// AutoApplyMigrationsWithBackoff - автомиграция для postgres с backoff-ми
// TODO: прочитать что такое backoff
async Task AutoApplyMigrationsWithBackoff()
{
    using var scope = app.Services.CreateScope();
    Console.WriteLine("Starting migrations processing...");

    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // примитивно заданные параметры backoff-а
    var maxRetryCount = 5;
    var initialDelayMs = 1000; // 1 секунда
    var maxDelayMs = 30000; // 30 секунд

    for (int i = 0; i < maxRetryCount; i++)
    {
        try
        {
            Console.WriteLine($"Attempt {i + 1} to connect to database...");
            // программный вызов применения миграций
            await db.Database.MigrateAsync();
            Console.WriteLine("Migrations were applied successfully if it exists");
            break;
        }
        catch (Exception ex) when (i < maxRetryCount - 1)
        {
            int delay = Math.Min(initialDelayMs * (int)Math.Pow(2, i), maxDelayMs);
            Console.WriteLine($"Attempt {i + 1} failed: {ex.Message}");
            Console.WriteLine($"Waiting {delay}ms before next attempt...");
            // асинхронное ожидание
            await Task.Delay(delay);
        }
    }
}

// AutoEnsureS3BucketExistsWithBackoff - автоматическое обеспечение существования бакета с backoff-ми
async Task AutoEnsureS3BucketExistsWithBackoff()
{
    using var scope = app.Services.CreateScope();
    Console.WriteLine("Starting S3 bucket existence processing...");

    IAmazonS3 s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();

    string s3ConnectionProfile = Environment.GetEnvironmentVariable("S3_OPTIONS_PROFILE") ?? "default";
    IConfigurationSection s3Options = builder.Configuration.GetSection("S3Options").GetSection(s3ConnectionProfile);
    string bucketName = s3Options["BucketName"] ?? "default";

    var maxRetryCount = 5;
    var initialDelay = 1000; // 1 секунда
    var maxDelay = 30000; // 30 секунд

    for (int i = 0; i < maxRetryCount; i++)
    {
        try
        {
            Console.WriteLine($"Attempt {i + 1} to connect to s3 ...");
            // попытка проверить существование бакета или создать его
            await s3Client.EnsureBucketExistsAsync(bucketName);
            Console.WriteLine("S3 bucket existence processed");
            break;
        }
        catch (Exception ex) when (i < maxRetryCount - 1)
        {
            var delay = Math.Min(initialDelay * (int)Math.Pow(2, i), maxDelay);
            Console.WriteLine($"Attempt {i + 1} failed: {ex.Message}");
            Console.WriteLine($"Waiting {delay}ms before next attempt...");
            // асинхронное ожидание
            await Task.Delay(delay);
        }
    }
}

// Что плохо в данном проекте?

// 1. Все файлы обрабатываются целиком, поэтому достаточно большой файл может повлиять на производительность
// 2. При обработки 1 файла есть риск лишних операций, связанных с перевыделением памяти и повторным копированием
// данных, которых можно было бы избежать
// 3. Отсутствие транзакционности при выпонении операций с файлами
