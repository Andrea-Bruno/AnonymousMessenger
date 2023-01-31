using System;
using System.Collections.Generic;
using System.Text;

namespace VideoFileCryptographyLibrary.Models
{
    public class VideoFileInfo
    {
        public int ChunkSize { get; set; }

        public double TotalChunks { get; set; }

        public int LastChunkSize { get; set; }

        public string FileName { get; set; }

        public byte[] PrivateKey { get; set; }
    }
}
