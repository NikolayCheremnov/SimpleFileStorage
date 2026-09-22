using Moq;
using SimpleFileStorage.Model;

namespace SimpleFilesStorageUnitTests;

[TestClass]
public class FileServiceTests
{
    private Mock<IFileMetadataStorage> _metadatas = null!;
    private Mock<IFileDataStorage> _files = null!;
    private FileService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _metadatas = new Mock<IFileMetadataStorage>(MockBehavior.Strict);
        _files = new Mock<IFileDataStorage>(MockBehavior.Strict);
        _sut = new FileService(_metadatas.Object, _files.Object);
    }

    [TestMethod]
    public async Task Upload_InsertsMetadataAndData_ReturnsFileId()
    {
        var param = new UploadFileParam
        {
            FileName = "report.pdf",
            ContentType = "application/pdf",
            Data = [1, 2, 3, 4],
        };

        FileMetadata? savedMetadata = null;
        FileData? savedData = null;

        _metadatas
            .Setup(m => m.Insert(It.IsAny<FileMetadata>()))
            .Callback<FileMetadata>(m => savedMetadata = m)
            .Returns(Task.CompletedTask);
        _files
            .Setup(f => f.Insert(It.IsAny<FileData>()))
            .Callback<FileData>(d => savedData = d)
            .Returns(Task.CompletedTask);

        Guid fileId = await _sut.Upload(param);

        Assert.AreNotEqual(Guid.Empty, fileId);
        Assert.IsNotNull(savedMetadata);
        Assert.IsNotNull(savedData);
        Assert.AreEqual(fileId, savedMetadata.FileID);
        Assert.AreEqual(fileId, savedData.FileID);
        Assert.AreEqual(param.FileName, savedMetadata.FileName);
        Assert.AreEqual(param.ContentType, savedMetadata.ContentType);
        Assert.AreEqual(param.Data.Length, savedMetadata.SizeBytes);
        CollectionAssert.AreEqual(param.Data, savedData.Data);
        _metadatas.Verify(m => m.Insert(It.IsAny<FileMetadata>()), Times.Once);
        _files.Verify(f => f.Insert(It.IsAny<FileData>()), Times.Once);
    }

    [TestMethod]
    public async Task GetFileMetadata_WhenFound_ReturnsMetadata()
    {
        var fileId = Guid.NewGuid();
        var expected = new FileMetadata
        {
            FileID = fileId,
            FileName = "photo.png",
            ContentType = "image/png",
            SizeBytes = 10,
            UploadedAt = DateTime.UtcNow,
        };

        _metadatas.Setup(m => m.Get(fileId)).ReturnsAsync(expected);

        FileMetadata actual = await _sut.GetFileMetadata(fileId);

        Assert.AreSame(expected, actual);
        _metadatas.Verify(m => m.Get(fileId), Times.Once);
    }

    [TestMethod]
    public async Task GetFileMetadata_WhenMissing_ThrowsFileNotFoundException()
    {
        var fileId = Guid.NewGuid();
        _metadatas.Setup(m => m.Get(fileId)).ReturnsAsync((FileMetadata?)null);

        await Assert.ThrowsExactlyAsync<FileNotFoundException>(() => _sut.GetFileMetadata(fileId));
    }

    [TestMethod]
    public async Task GetFileData_WhenFound_ReturnsData()
    {
        var fileId = Guid.NewGuid();
        var expected = new FileData
        {
            FileID = fileId,
            Data = [9, 8, 7],
        };

        _files.Setup(f => f.Get(fileId)).ReturnsAsync(expected);

        FileData actual = await _sut.GetFileData(fileId);

        Assert.AreSame(expected, actual);
        _files.Verify(f => f.Get(fileId), Times.Once);
    }

    [TestMethod]
    public async Task GetFileData_WhenMissing_ThrowsFileNotFoundException()
    {
        var fileId = Guid.NewGuid();
        _files.Setup(f => f.Get(fileId)).ReturnsAsync((FileData?)null);

        await Assert.ThrowsExactlyAsync<FileNotFoundException>(() => _sut.GetFileData(fileId));
    }
}
