namespace Cryptogram.Services
{
    public interface ICallNotificationService
    {
        void FinishCall(string chatId, bool isVideoCall, bool isCallingByMe, int durationInSecond);

        void DeclineCall(string chatId, bool isCallDeclinedByReceiver = true); // decline call


        void SetRingingStatus(string chatId);
    }
}
