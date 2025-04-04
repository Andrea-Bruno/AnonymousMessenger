using System;
namespace Cryptogram.Models
{
    public class FileData
    {
        public FileData()
        {

        }

        public FileData(string _name, byte[] _content, string _id)
        {
            Name = _name;
            Content = _content;
            Id = _id;
        }
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public string Id { get; set; }
    }
}
