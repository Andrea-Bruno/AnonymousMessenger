using System;
using System.Threading.Tasks;

namespace MessageCompose.Services
{
    public interface IVideoTrimmingService
    {
        Task<int> TrimAsync(int startMS, int lengthMS, string inputPath, string outputPath);

    }
}
