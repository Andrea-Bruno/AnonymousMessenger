using System;
namespace Telegraph.Services
{
    public interface IEndCall
    {
        void FinishCall(string chatId, string remoteName= "", bool isVideoCall = false);
    }
}
