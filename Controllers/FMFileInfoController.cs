using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using FileManage.DBContexts;
using FileManage.DBModels;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Net;
using FileManage.Attributes;
using FileManage.Utilities;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace FileManage.Controllers
{
    [Route("[controller]")]
    [ApiController]
    // [Authorize]
    public class FMFileInfoController : EntityController<FMFileInfo, FMFileInfoController>
    {
        private readonly string basePath;
        private readonly string basePathOut;
        private readonly ILogger<FMFileInfoController> _logger;

        private readonly string[] _permittedExtensions = { ".txt", ".apk", ".jpg" };
        private static readonly FormOptions _defaultFormOptions = new FormOptions();
        private readonly long _fileSizeLimit;
        private readonly EntityContext _context;
        public FMFileInfoController(EntityContext context, ILogger<FMFileInfoController> logger, IDistributedCache cache, IConfiguration config)
            : base(logger, context, cache)
        {
            _logger = logger;
            basePath = Path.Combine(AppContext.BaseDirectory, "FMFileInfo");
            basePathOut = Path.Combine(AppContext.BaseDirectory, "FMFileInfoOut");
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");
            _context = context;
        }


        /// <summary>
        /// 流式文件上传
        /// </summary>
        /// <returns></returns>
        [HttpPost("UploadingStream")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadingStream()
        {
            if (Directory.Exists(basePathOut))//如果存在文件删除
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(basePathOut);
                    FileInfo[] files = directoryInfo.GetFiles();
                    foreach(var f in files){
                        f.Delete();
                    } 
                }
                
                


            List<FMFileInfo> items = new List<FMFileInfo>();
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    $"The request couldn't be processed (Error 1).");
                // Log error

                return BadRequest(ModelState);
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // This check assumes that there's a file
                    // present without form data. If form data
                    // is present, this method immediately fails
                    // and returns the model error.
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File",
                            $"The request couldn't be processed (Error 2).");
                        // Log error

                        return BadRequest(ModelState);
                    }
                    else
                    {
                        // Don't trust the file name sent by the client. To display
                        // the file name, HTML-encode the value.
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName.Value);
                        var ext = Path.GetExtension(trustedFileNameForDisplay).ToLowerInvariant();
                        var trustedFileNameForFileStorage = Guid.NewGuid().ToString("N") + ext;

                        // **WARNING!**
                        // In the following example, the file is saved without
                        // scanning the file's contents. In most production
                        // scenarios, an anti-virus/anti-malware scanner API
                        // is used on the file before making the file available
                        // for download or for use by other systems. 
                        // For more information, see the topic that accompanies 
                        // this sample.

                        var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                            section, contentDisposition, ModelState,
                            _permittedExtensions, _fileSizeLimit);

                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }
                        var pathfull = Path.Combine(basePath, trustedFileNameForFileStorage);
                        if (!Directory.Exists(basePath))//判断是否存在
                        {
                            Directory.CreateDirectory(basePath);//创建新路径
                        }
                        using (var targetStream = System.IO.File.Create(pathfull))
                        {
                            await targetStream.WriteAsync(streamedFileContent);
                            items.Add(new FMFileInfo
                            {
                                Name = trustedFileNameForDisplay,
                                Path = pathfull,
                            });
                            _logger.LogInformation(
                                "Uploaded file '{TrustedFileNameForDisplay}' saved to " +
                                "'{TargetFilePath}' as {TrustedFileNameForFileStorage}",
                                trustedFileNameForDisplay, basePath,
                                trustedFileNameForFileStorage);
                        }
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }
            /*
            //获取boundary
            var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(Request.ContentType).Boundary).Value;
            //得到reader
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            //{ BodyLengthLimit = 2000 };//
            var section = await reader.ReadNextSectionAsync();

            List<FMFileInfo> items = new List<FMFileInfo>();
            //读取section
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDispositionHeader)
                {
                    var trustedFileNameForDisplay = WebUtility.HtmlEncode(value: contentDisposition.FileName.Value);

                    var trustedFileNameForFileStorage = Path.GetRandomFileName();
                    var pathfull = Path.Combine(basePath, trustedFileNameForFileStorage);
                    if (!Directory.Exists(basePath))//判断是否存在
                    {
                        Directory.CreateDirectory(basePath);//创建新路径
                    }
                    _logger.LogInformation(
                        $"Uploaded file '{trustedFileNameForDisplay}' saved to '{basePath}' as {trustedFileNameForFileStorage}");
                    using (var targetStream = System.IO.File.Create(pathfull))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await section.Body.CopyToAsync(memoryStream);
                            await targetStream.WriteAsync(memoryStream.ToArray());
                        }
                    }
                    items.Add(new FMFileInfo
                    {
                        Name = trustedFileNameForDisplay,
                        Path = pathfull,
                    });

                }
                section = await reader.ReadNextSectionAsync();
            }

            */
            var result = await base.createMultiEntity(items);
            return result;
        }

        [HttpGet("getallfile/{name}")]
        public async virtual Task<IActionResult> getAllfile(string name)
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                var dir = Path.Combine(AppContext.BaseDirectory, name);
                if (!Directory.Exists(dir))//判断是否存在
                {
                    Directory.CreateDirectory(dir);//创建新路径
                }
                //var query = from tb in _context.Set<FMFileInfo>();
                DirectoryInfo directoryInfo = new DirectoryInfo(dir);
                FileInfo[] files = directoryInfo.GetFiles();
                var result = from f in files
                             select new { f.Name, f.FullName };

                apiResult.resultCode = 0;
                apiResult.resultBody = result;
                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                apiResult.resultCode = ResultCodeEnum.Exception;
                apiResult.resultBody = ex.ToString();
                return BadRequest(apiResult);
            }

        }
        [HttpGet("download/FMFileInfoIn/{name}")]
        public async virtual Task<IActionResult> downfileIn(string name)
        {
            return await downfile(Path.Combine("FMFileInfo", name));
        }
    
        [HttpGet("download/FMFileInfoOut/{name}")]
        public async virtual Task<IActionResult> downfileOut(string name)
        {
            return await downfile(Path.Combine("FMFileInfoOut", name));
        }
        
        private async  Task<IActionResult> downfile(string name)
        {
            try
            {
                var fullPath = Path.Combine(AppContext.BaseDirectory, name);
                FileInfo fi = new FileInfo(fullPath);
                if (fi.Exists)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var stream = new FileStream(fullPath, FileMode.Open))
                        {
                            await stream.CopyToAsync(memoryStream);
                        }
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        
                        return new FileStreamResult(new MemoryStream(memoryStream.ToArray()), "application/octet-stream");
                    }
                }
                ModelState.AddModelError("File", "文件不存在");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("File", ex.Message);
            }



            // Log error

            return BadRequest(ModelState);
        }
    
    }
}