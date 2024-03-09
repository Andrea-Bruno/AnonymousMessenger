using System;
namespace MessageCompose.Model
{
    [Serializable]
    public class SerializableFileData
    {
        public byte[] Data { get; set; }
        public string FileName { get; set; }

        public SerializableFileData(byte[] data, string fileName)
        {
            Data = data;
            FileName = fileName;
        }
    }
}
