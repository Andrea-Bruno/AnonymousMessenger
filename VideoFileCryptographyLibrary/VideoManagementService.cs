using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using VideoFileCryptographyLibrary.Models;
using Xamarin.Essentials;

namespace VideoFileCryptographyLibrary
{
    public static class VideoManagementService
    {
        public delegate void DownloadStatus(double percentage, bool isVideoDownloadSuccessful = true);

        public delegate void UploadStatus(double percentage);

        public static async Task<string> DownloadVideo(byte[] _privateKey, DownloadStatus downloadStatus)
        {
            Debug.WriteLine("[Downloading Video]: File download started");
            // Check internet connectivity
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
            {
                Debug.WriteLine("[Downloading Video]: Error - No internet connection available");
                return string.Empty;
            }

            // ---------------------
            // PREPARING VIDEO FILE
            // ---------------------

            // Generate File name from key (Filename is the hash of the private key)
            var sha256 = SHA256.Create();
            var fileName = ComputePsuedoHash.ToHex(sha256.ComputeHash(_privateKey));

            // Get list of file chunks uploaded to server
            var fileList = await FileService.GetFileChunks(fileName).ConfigureAwait(false);
            if (fileList.Count > 0)
            {
                var _fileNameWithType = fileList[0].Split('-').LastOrDefault();
                //Saving video file in phone storage
                var folder = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "downloads");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                var videoFile = Path.Combine(folder, _fileNameWithType);

                // Check if file already exists
                var chunk = 0;
                if (File.Exists(videoFile))
                {
                    using (var readStream = File.OpenRead(videoFile))
                    {
                        chunk = (int)Math.Floor(Convert.ToDouble(readStream.Length / FileService.ChunkSize));
                    }
                }

                // ----------------------
                // DOWNLOADING VIDEO FILE
                // ----------------------

                using (var outputStream = File.OpenWrite(videoFile))
                {
                    // Seek the file to last downloaded chunk (it will remain 0 if file don't exist)
                    outputStream.Seek(chunk * FileService.ChunkSize, SeekOrigin.Begin);
                    for (; chunk < fileList.Count; chunk++)
                    {
                        // Check internet connectivity
                        current = Connectivity.NetworkAccess;
                        if (current != NetworkAccess.Internet)
                        {
                            Debug.WriteLine("[Downloading Video]: Error - No internet connection available");
                            return string.Empty;
                        }

                        // Show download progress
                        var progressPercentAge = Math.Ceiling((chunk / Convert.ToDouble(fileList.Count)) * 100);
                        downloadStatus(progressPercentAge);
                        Debug.WriteLine(string.Format("{0} %", progressPercentAge));

                        var fileBlock = await FileService.DownloadFileChunk(fileList[chunk], _privateKey).ConfigureAwait(false);
                        if (fileBlock == null)
                        {
                            return string.Empty;
                        }

                        // Writing the downloaded chunk to file
                        await outputStream.WriteAsync(fileBlock, 0, fileBlock.Length).ConfigureAwait(false);
                        Debug.WriteLine(string.Format("[Downloading Video]: File Uploaded - {0}", fileList[chunk]));
                    }
                }

                Debug.WriteLine(string.Format("[Downloading Video]: File Uploaded - {0}", videoFile));
                return videoFile;
            }
            else
            {
                return string.Empty;
            }
        }

        public static async Task<byte[]> UploadVideo(FileResult selectedFile, UploadStatus uploadStatus, byte[] thumbnail, CancellationToken token)
        {
            Debug.WriteLine("[Uploading Video]: File upload started");
            var current = Connectivity.NetworkAccess;
            // Check internet connectivity
            if (current != NetworkAccess.Internet)
            {
                Debug.WriteLine("[Uploading Video]: Error - No internet connection available");
                return null;
            }

            var localFilePath = string.Empty;
            var fileInfo = new VideoFileInfo();

            // Implement File upload
            try
            {
                // ---------------------
                // PREPARING VIDEO FILE
                // ---------------------

                using (var fileStream = await selectedFile.OpenReadAsync().ConfigureAwait(false))
                {
                    // Get file information
                    fileInfo = FileService.GetFileInfo(fileStream, selectedFile.FileName.Split('.').LastOrDefault());
                    // Copy selected video file in local phone directory
                    try
                    {
                        var folder = Path.Combine(FileSystem.AppDataDirectory, "uploads");
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        localFilePath = Path.Combine(folder, fileInfo.FileName);
                        if (File.Exists(localFilePath))
                        {
                            // Verify if the file is not corrupted
                            long localFileLength = 0;
                            using (var local = File.OpenRead(localFilePath))
                            {
                                localFileLength = local.Length;
                            }
                            if (localFileLength != fileStream.Length)
                            {
                                File.Delete(localFilePath);
                                using (var outputStream = File.Create(localFilePath))
                                {
                                    await fileStream.CopyToAsync(outputStream).ConfigureAwait(false);
                                }
                            }
                        }
                        else
                        {
                            using (var outputStream = File.Create(localFilePath))
                            {
                                await fileStream.CopyToAsync(outputStream).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(string.Format("[Uploading Video]: Exception - {0}", ex.Message));
                        return null;
                    }
                }

                // ---------------------
                // UPLOADING VIDEO FILE
                // ---------------------

                // Check if file exists
                var fileList = await ApiService.GetFileListAsync(fileInfo.FileName).ConfigureAwait(false);
                if (fileList.Count == fileInfo.TotalChunks)
                {
                    Debug.WriteLine(string.Format("[Uploading Video]: File Uploaded - {0}", ComputePsuedoHash.ToHex(fileInfo.PrivateKey)));
                    return fileInfo.PrivateKey;
                }
                else
                {
                    // Check if some of the file chunks already uploaded to the server due to internet loss or application crash.
                    var lastUploadedChunk = 0;
                    foreach (var item in fileList)
                    {
                        var number = Convert.ToInt32(item.Split('-')[0]);
                        if (lastUploadedChunk < number)
                            lastUploadedChunk = number;
                    }

                    using (var fileStream = File.OpenRead(localFilePath))
                    {
                        // Uploading file chunks
                        var i = lastUploadedChunk + 1;
                        for (; i <= fileInfo.TotalChunks; i++)
                        {
                            // For cancelling Upload
                            if (token.IsCancellationRequested)
                            {
                                return null;
                            }
                            if (i == fileInfo.TotalChunks)
                            {
                                if (!await FileService.UploadFileChunk(fileStream, fileInfo.FileName, fileInfo.PrivateKey, i, fileInfo.LastChunkSize).ConfigureAwait(false))
                                    return null;
                            }
                            else
                            {
                                if (!await FileService.UploadFileChunk(fileStream, fileInfo.FileName, fileInfo.PrivateKey, i).ConfigureAwait(false))
                                    return null;
                            }

                            // Show uploading Progress
                            var progressPercentAge = Math.Ceiling((i / (fileInfo.TotalChunks + 1)) * 100);
                            uploadStatus(progressPercentAge);
                            Debug.WriteLine(string.Format("{0} %", progressPercentAge));
                        }
                    }

                    // Uploading file Thumbnail
                    //byte[] thumbnail = DependencyService.Get<IThumbnailService>().GenerateThumbImage(localFilePath, 1);
                    if (thumbnail != null)
                    {
                        await FileService.UploadFileThumbnail(thumbnail, fileInfo.FileName.Split('.')[0] + ".png", fileInfo.PrivateKey).ConfigureAwait(false);
                    }

                    // Updating file status
                    Debug.WriteLine(string.Format("[Uploading Video]: File Uploaded - {0}", fileInfo.FileName));
                    return fileInfo.PrivateKey;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("[Uploading Video]: Exception - {0}", e.Message));
                return null;
            }
        }
    }
}
