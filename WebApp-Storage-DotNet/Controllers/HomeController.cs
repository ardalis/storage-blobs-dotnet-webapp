//---------------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved. 
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,  
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES  
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
//---------------------------------------------------------------------------------- 
// The example companies, organizations, products, domain names, 
// e-mail addresses, logos, people, places, and events depicted 
// herein are fictitious.  No association with any real company, 
// organization, product, domain name, email address, logo, person, 
// places, or events is intended or should be inferred. 

namespace WebApp_Storage_DotNet.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using System.Web;
    using System.Threading.Tasks;
    using System.IO;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.Azure;
    using System.Configuration;
    using WebApp_Storage_DotNet.Interfaces;

    /// <summary> 
    /// Azure Blob Storage Photo Gallery - Demonstrates how to use the Blob Storage service.  
    /// Blob storage stores unstructured data such as text, binary data, documents or media files.  
    /// Blobs can be accessed from anywhere in the world via HTTP or HTTPS. 
    /// 
    /// Note: This sample uses the .NET 4.5 asynchronous programming model to demonstrate how to call the Storage Service using the  
    /// storage client libraries asynchronous API's. When used in real applications this approach enables you to improve the  
    /// responsiveness of your application. Calls to the storage service are prefixed by the await keyword.  
    ///  
    /// Documentation References:  
    /// - What is a Storage Account - http://azure.microsoft.com/en-us/documentation/articles/storage-whatis-account/ 
    /// - Getting Started with Blobs - http://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/ 
    /// - Blob Service Concepts - http://msdn.microsoft.com/en-us/library/dd179376.aspx  
    /// - Blob Service REST API - http://msdn.microsoft.com/en-us/library/dd135733.aspx 
    /// - Blob Service C# API - http://go.microsoft.com/fwlink/?LinkID=398944 
    /// - Delegating Access with Shared Access Signatures - http://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-shared-access-signature-part-1/ 
    /// </summary> 

    public class HomeController : Controller
    {
        static CloudBlobClient blobClient;
        const string blobContainerName = "webappstoragedotnet-imagecontainer";
        static CloudBlobContainer blobContainer;
        private readonly IFileService _fileService;

        public HomeController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary> 
        /// Task<ActionResult> Index() 
        /// Documentation References:  
        /// - What is a Storage Account: http://azure.microsoft.com/en-us/documentation/articles/storage-whatis-account/ 
        /// - Create a Storage Account: https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/#create-an-azure-storage-account
        /// - Create a Storage Container: https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/#create-a-container
        /// - List all Blobs in a Storage Container: https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/#list-the-blobs-in-a-container
        /// </summary> 
        public async Task<ActionResult> Index()
        {
            try
            {
                return View(await _fileService.ListFileUris());
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        /// <summary> 
        /// Task<ActionResult> UploadAsync() 
        /// Documentation References:  
        /// - UploadFromFileAsync Method: https://msdn.microsoft.com/en-us/library/azure/microsoft.windowsazure.storage.blob.cloudpageblob.uploadfromfileasync.aspx
        /// </summary> 
        [HttpPost]
        public async Task<ActionResult> UploadAsync()
        {
            try
            {
                HttpFileCollectionBase files = Request.Files;
                int fileCount = files.Count;

                if (fileCount > 0)
                {
                    var tasks = new List<Task>();
                    // TODO: make parallel
                    for (int i = 0; i < fileCount; i++)
                    {
                        tasks.Add(
                        _fileService.UploadFileFromStream(files[i].InputStream,
                            files[i].FileName));
                    }
                    await Task.WhenAll(tasks);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        /// <summary> 
        /// Task<ActionResult> DeleteImage(string name) 
        /// Documentation References:  
        /// - Delete Blobs: https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/#delete-blobs
        /// </summary> 
        [HttpPost]
        public async Task<ActionResult> DeleteImage(string name)
        {
            try
            {
                Uri uri = new Uri(name);
                string filename = Path.GetFileName(uri.LocalPath);

                var blob = blobContainer.GetBlockBlobReference(filename);
                var result = await blob.DeleteIfExistsAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        /// <summary> 
        /// Task<ActionResult> DeleteAll(string name) 
        /// Documentation References:  
        /// - Delete Blobs: https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/#delete-blobs
        /// </summary> 
        [HttpPost]
        public async Task<ActionResult> DeleteAll()
        {
            try
            {
                foreach (var blob in blobContainer.ListBlobs())
                {
                    if (blob.GetType() == typeof(CloudBlockBlob))
                    {
                        var result = await ((CloudBlockBlob)blob).DeleteIfExistsAsync();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        /// <summary> 
        /// string GetRandomBlobName(string filename): Generates a unique random file name to be uploaded  
        /// </summary> 
        private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }
    }
}
