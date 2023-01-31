using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using VideoFileCryptographyLibrary.Models;

namespace VideoFileCryptographyLibrary
{
    public static class FileService
    {
        /// <summary>
        /// The chunk size of the video is set to 1MB of the data.
        /// </summary>
        private const int CHUNK_SIZE = 1048576;
        public static int ChunkSize { get { return CHUNK_SIZE; } }

        /// <summary>
        /// This method gets the file stream and split it in chunks an upload each chunk on server.
        /// It calculate the psudo-hash of the file and generate the key to encrypt the file chunks.
        /// </summary>
        /// <param name="fileStream"> File stream to upload</param>
        /// <returns>It returns the psudo-hash of the file, representing the file name.</returns>
        public static async Task<string> UploadFile(Stream fileStream)
        {
            // Calculate psudo-hash of file to create file name and to generate the key.
            var _FileName = ComputePsuedoHash.ToHex(ComputePsuedoHash.GetHash256(ComputePsuedoHash.GetHash256(fileStream)));
            var Encryptor = new EncryptionService(ComputePsuedoHash.HexToByteArray(_FileName));

            // Check if file already uploaded on the server
            var fileList = await ApiService.GetFileListAsync(_FileName).ConfigureAwait(false);

            // If the file doesn't exist on the server it will upload the file otherwise, it will simply return the file name.
            if (fileList.Count == 0)
            {
                var FSBytes = (int)fileStream.Length;
                var div = FSBytes / CHUNK_SIZE;
                var mod = FSBytes % CHUNK_SIZE;
                if (!(div == 0 && mod == 0))
                {
                    var i = 0;
                    for (; i < div; i++)
                    {
                        var chunk = new byte[CHUNK_SIZE];
                        fileStream.ReadAsync(chunk, 0, CHUNK_SIZE).Wait();
                        var encryptedData = Encryptor.Encrypt(chunk);
                        //Upload file to Server
                        var filename = string.Format("{0}-{1}.mp4", i, _FileName);
                        using (var stream = new MemoryStream())
                        {
                            await stream.WriteAsync(encryptedData, 0, encryptedData.Length).ConfigureAwait(false);
                            //Call API Here
                            await ApiService.UploadFileAsync(stream, filename).ConfigureAwait(false);
                        }
                    }
                    if (mod != 0)
                    {
                        var chunk = new byte[mod];
                        await fileStream.ReadAsync(chunk, 0, mod).ConfigureAwait(false);
                        var encryptedData = Encryptor.Encrypt(chunk);
                        //Upload file to Server
                        var filename = string.Format("{0}-{1}.mp4", i, _FileName);
                        using (var stream = new MemoryStream())
                        {
                            await stream.WriteAsync(encryptedData, 0, encryptedData.Length).ConfigureAwait(false);
                            //Call API Here
                            await ApiService.UploadFileAsync(stream, filename).ConfigureAwait(false);
                        }
                    }
                }
            }
            return string.Format("{0}.mp4", _FileName);
        }

        /// <summary>
        /// This method receives the file stream, create a file chunk and upload to the server after encryption.
        /// Most probably the last chunk will not the exact 10mb, so the LastChunkSize is the parameter that needs to be passed explicitly.
        /// </summary>
        /// <param name="fileStream"> File stream to upload</param>
        /// <param name="fileName"> Name of the file.</param>
        /// <param name="ChunkNumber"> The number of chunk that needs to be trim from stream</param>
        /// <param name="LastChunkSize"> Size of the last chunk</param>
        /// <returns>It will return "true" if the upload is successfull else the return value will be "false".</returns>
        public static async Task<bool> UploadFileChunk(Stream fileStream, string fileName, byte[] privateKey, int ChunkNumber = 0, int LastChunkSize = 0)
        {
            var result = false;

            // Validating the parameters
            if (fileStream == null)
                return result;
            if (ChunkNumber == 0)
                return result;

            try
            {
                var Encryptor = new EncryptionService(privateKey);
                byte[] chunk;
                if (LastChunkSize == 0)
                    chunk = new byte[CHUNK_SIZE];
                else
                    chunk = new byte[LastChunkSize];

                var _offSet = (ChunkNumber * CHUNK_SIZE) - CHUNK_SIZE;
                fileStream.Seek(_offSet, SeekOrigin.Begin);
                await fileStream.ReadAsync(chunk, 0, chunk.Length).ConfigureAwait(false);
                var encryptedData = Encryptor.Encrypt(chunk);
                //Upload file to Server
                var filename = string.Format("{0}-{1}", ChunkNumber, fileName);
                using (var stream = new MemoryStream())
                {
                    await stream.WriteAsync(encryptedData, 0, encryptedData.Length).ConfigureAwait(false);
                    //Call API Here
                    result = await ApiService.UploadFileAsync(stream, filename).ConfigureAwait(false);
                }
                fileStream.Seek(0, SeekOrigin.Begin);
                return result;
            }
            catch (Exception ex)
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                Debug.WriteLine(ex.Message);
                return result;
            }
        }

        /// <summary>
        /// This method will download the file chunk and decrypt it using the psudo-hash of the filename
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        /// <param name="fileChunkName">The chunk name that belongs to the file</param>
        /// <param name="privateKey">The private key is to decrypt the downloaded file</param>
        /// <returns>It will return "true" if the download is successfull else the return value will be "false".</returns>
        public static async Task<byte[]> DownloadFileChunk(string fileChunkName, byte[] privateKey)
        {
            var Decryptor = new EncryptionService(privateKey);
            try
            {
                var encFile = await ApiService.DownloadFileAsync(fileChunkName).ConfigureAwait(false);
                if (encFile != null)
                {
                    return Decryptor.Decrypt(encFile.ToArray());
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// This method will get the list of file chunks already uploaded on the server of a single file.
        /// </summary>
        /// <param name="fileName"> The name of the file required to download the list of chunks belongs to this file.</param>
        /// <returns>It will return the list of chunks of requested file if the file exists in the server else the return value will be an empty list.</returns>
        public static async Task<List<string>> GetFileChunks(string fileName)
        {
            var chunks = new List<string>();
            try
            {
                chunks = await ApiService.GetFileListAsync(fileName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return chunks;
        }

        /// <summary>
        /// This method will calculate the file properties, file name, total chunks, last chunk size and minimum chunk size of the file.
        /// </summary>
        /// <param name="fileStream">The stream of the file</param>
        /// <param name="fileType">This parameter holds the type of file, e.g. .mp4, .MOV etc.</param>
        /// <returns> Returns the object representing file information.</returns>
        public static VideoFileInfo GetFileInfo(Stream fileStream, string fileType)
        {
            // Private Key of the file
            var pseudoHash = ComputePsuedoHash.GetHash256(fileStream);

            // Filename is the hash of the private key
            var sha256 = SHA256.Create();
            var FileName = string.Format("{0}.{1}", ComputePsuedoHash.ToHex(sha256.ComputeHash(pseudoHash)), fileType);

            var FSBytes = (int)fileStream.Length;
            var div = FSBytes / CHUNK_SIZE;
            var mod = FSBytes % CHUNK_SIZE;

            // If there is last chunk present with less than chunk size
            if (mod > 0)
            {
                div++;
            }

            var info = new VideoFileInfo()
            {
                ChunkSize = CHUNK_SIZE,
                FileName = FileName,
                LastChunkSize = mod,
                TotalChunks = div,
                PrivateKey = pseudoHash
            };
            fileStream.Seek(0, SeekOrigin.Begin);
            return info;
        }

        /// <summary>
        /// This method is responsible to upload the video file thumbnail only.
        /// </summary>
        /// <param name="data">In this parameter we need to pass the byte array of video file thumbnail</param>
        /// <param name="fileName">In this parameter we need to pas the file name, exactly same as the file name of video to which this thumbnail belongs.</param>
        /// <param name="privateKey">The private key is to encrypt the uploading file</param>
        /// <returns>It will return "true" if the download is successfull else the return value will be "false".</returns>
        public static async Task<bool> UploadFileThumbnail(byte[] data, string fileName, byte[] privateKey)
        {
            var result = false;
            // Validating the parameters
            if (data == null)
                return result;
            try
            {
                //Encrypting Thumbnail
                var Encryptor = new EncryptionService(privateKey);
                var encryptedData = Encryptor.Encrypt(data);
                using (var stream = new MemoryStream())
                {
                    await stream.WriteAsync(encryptedData, 0,
                        encryptedData.Length).ConfigureAwait(false);
                    //Upload file to Server
                    //Call API Here
                    result = await ApiService.UploadFileThumbnailAsync(stream, fileName).ConfigureAwait(false);
                }
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return result;
            }
        }

        /// <summary>
        /// This method is responsible to download the thumbnail of video file with the help of video file name.
        /// </summary>
        /// <param name="fileName">The name of the thumbnail file, exactly same as the name of coresponding video file.</param>
        /// <param name="privateKey">The private key is to decrypt the downloaded file</param>
        /// <returns>It will return file in the form of "Byte Array" if the download is successfull else the return value will be "null".</returns>
        public static async Task<byte[]> DownloadFileThumbnail(string fileName, byte[] privateKey)
        {
            var Decryptor = new EncryptionService(privateKey);
            try
            {
                var encFile = await ApiService.DownloadFileThumbnailAsync(fileName).ConfigureAwait(false);
                if (encFile != null)
                {
                    return Decryptor.Decrypt(encFile.ToArray());
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

    }
}
