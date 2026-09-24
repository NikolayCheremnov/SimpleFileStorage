using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleFileStorage.Model;

namespace SimpleFileStorage.Api
{
    // FileController - контроллер для работы с методами файлов
    [Route("api/file")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly FileService _service;

        public FileController(FileService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file.Length == 0)
            {
                // 400
                return BadRequest(new StringMessage("empty file"));
            }
            // считать файл из http-запроса (IFormFile)
            using MemoryStream stream = new MemoryStream();
            await file.CopyToAsync(stream);
            // подготовить параметр для выгрузки файла
            UploadFileParam param = new UploadFileParam()
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Data = stream.ToArray(),
            };
            // выгрузить файл
            Guid fileID = await _service.Upload(param);
            // вернуть в качетве результата id созданного файла
            // 200
            return Ok(new { ID = fileID });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetFileMetadata(Guid id)
        {
            try
            {
                // получить метаданные файла
                FileMetadata metadata = await _service.GetFileMetadata(id);
                // 200
                return Ok(metadata);
            }
            catch (FileNotFoundException)
            {
                // 404
                return NotFound(new StringMessage("file not found"));
            }
        }

        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                // получить метаданные файла
                FileMetadata metadata = await _service.GetFileMetadata(id);
                // получить данные файла
                FileData data = await _service.GetFileData(id);
                // записать файл в http-ответ (stream закрыть нельзя)
                MemoryStream stream = new MemoryStream(data.Data);
                // 200 + file
                return File(stream, metadata.ContentType, metadata.FileName);
            }
            catch (FileNotFoundException)
            {
                // 404
                return NotFound(new StringMessage("file not found"));
            }
        }

        [HttpGet]
        public async Task<List<FileMetadata>> GetAllFilesData()
        {
            return await _service.GetAllFiles();
        }
    }
}
