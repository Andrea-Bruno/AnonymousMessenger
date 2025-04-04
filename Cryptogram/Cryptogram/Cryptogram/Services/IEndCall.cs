using System;
namespace Cryptogram.Services
{
    public interface IEndCall
    {
        void FinishCall(string chatId, string remoteName= "", bool isVideoCall = false);
    }
}
