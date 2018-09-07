using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WebApp_Storage_DotNet.Interfaces
{
    public interface IFileService
    {
        Task<List<Uri>> ListFileUris();
        Task UploadFileFromStream(Stream stream, string filename);
    }
}