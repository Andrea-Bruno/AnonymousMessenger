using System;
namespace Anonymous.Services
{
    public interface IEndCall
    {
        void FinishCall(string chatId, string remoteName= "", bool isVideoCall = false);
    }
}
