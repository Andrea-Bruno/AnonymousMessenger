using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VideoFileCryptographyLibrary
{
    internal static class ApiService
    {
        //#if DEBUG
        //        private static readonly string Url = @"http://90.191.43.19:8081/api/VideoFile/";
        //#else
        //        private static readonly string Url = @"https://Anonymousvideoappservice.azurewebsites.net/api/VideoFile/";
        //#endif
        private static readonly string Url = @"https://Anonymousvideoappservice.azurewebsites.net/api/VideoFile/";

        public static async Task<bool> UploadFileAsync(Stream sourceStream, string fileName)
        {
            sourceStream.Position = 0;
            string uploadUrl = string.Format("{0}Upload?fileName={1}", Url, fileName);
            var fileStreamContent = new StreamContent(sourceStream);
            fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "fileName", FileName = fileName };
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            using (var client = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(fileStreamContent);
                    try
                    {
                        var response = await client.PostAsync(uploadUrl, formData).ConfigureAwait(false);
                        //debug
                        Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                        var result = response.IsSuccessStatusCode;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception Caught: " + ex.Message.ToString());
                        return false;
                    }
                }
            }
        }

        public static async Task<bool> IsFileExist(string fileName)
        {
            string targetUrl = string.Format("{0}FileExists?fileName={1}", Url, fileName);
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(targetUrl).ConfigureAwait(false);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception Caught: ", ex.Message.ToString());
                    return false;
                }
            }
            return false;
        }

        public static async Task<List<string>> GetFileListAsync(string fileName)
        {
            List<string> listOfFiles = new List<string>();
            string targetUrl = string.Format("{0}FileExists?fileName={1}", Url, fileName);
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetStringAsync(targetUrl).ConfigureAwait(false);
                    listOfFiles = JsonConvert.DeserializeObject<List<string>>(response);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception Caught: ", ex.Message.ToString());
                }
            }
            return listOfFiles;
        }

        public static async Task<MemoryStream> DownloadFileAsync(string fileName)
        {
            string downloadUrl = string.Format("{0}Download?filename={1}", Url, fileName);

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetStreamAsync(downloadUrl).ConfigureAwait(false);
                    MemoryStream ms = new MemoryStream();
                    await response.CopyToAsync(ms).ConfigureAwait(false);
                    return ms;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            }
        }

        public static async Task<bool> UploadFileThumbnailAsync(Stream sourceStream, string fileName)
        {
            sourceStream.Position = 0;
            string uploadUrl = string.Format("{0}UploadThumbnail?fileName={1}", Url, fileName);
            var fileStreamContent = new StreamContent(sourceStream);
            fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "fileName", FileName = fileName };
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            using (var client = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(fileStreamContent);
                    try
                    {
                        var response = await client.PostAsync(uploadUrl, formData).ConfigureAwait(false);
                        //debug
                        Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
                        var result = response.IsSuccessStatusCode;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exception Caught: " + ex.Message.ToString());
                        return false;
                    }
                }
            }
        }

        public static async Task<MemoryStream> DownloadFileThumbnailAsync(string fileName)
        {
            string downloadUrl = string.Format("{0}DownloadThumbnail?filename={1}", Url, fileName);
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetStreamAsync(downloadUrl).ConfigureAwait(false);
                    MemoryStream ms = new MemoryStream();
                    await response.CopyToAsync(ms).ConfigureAwait(false);
                    return ms;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            }
        }
    }
}
