using System;
using System.Collections.Generic;
using Foundation;
using CallKit;

namespace Telegraph.iOS.Call
{
    public class ActiveCallManager
    {
        #region Private Variables
        private CXCallController CallController = new CXCallController();
        #endregion

        #region Computed Properties
        public List<ActiveCall> Calls { get; set; }
        #endregion

        #region Constructors
        public ActiveCallManager()
        {
            // Initialize
            this.Calls = new List<ActiveCall>();
        }
        #endregion

        #region Private Methods
        private void SendTransactionRequest(CXTransaction transaction)
        {
            // Send request to call controller
            CallController.RequestTransaction(transaction, (error) => {
                // Was there an error?
                if (error == null)
                {
                    // No, report success
                    Console.WriteLine("Transaction request sent successfully.");
                }
                else
                {
                    // Yes, report error
                    Console.WriteLine("Error requesting transaction: {0}", error);
                }
            });
        }
        #endregion

        #region Public Methods
        public ActiveCall FindCall(NSUuid uuid)
        {
            // Scan for requested call
            foreach (ActiveCall call in Calls)
            {
                if (call.UUID.IsEqual(uuid)) return call;
            }

            // Not found
            return null;
        }

        public void StartCall(string contact)
        {
            // Build call action
            var handle = new CXHandle(CXHandleType.Generic, contact);
            var startCallAction = new CXStartCallAction(new NSUuid(), handle);

            // Create transaction
            var transaction = new CXTransaction(startCallAction);

            // Inform system of call request
            SendTransactionRequest(transaction);
        }

        public void EndCall(ActiveCall call)
        {
            if(AppDelegate.Instance.CallManager.Calls.Contains(call))
                AppDelegate.Instance.CallManager.Calls.Remove(call);
            // Build action
            var endCallAction = new CXEndCallAction(call.UUID);

            // Create transaction
            var transaction = new CXTransaction(endCallAction);
            // Inform system of call request
            AppDelegate.Instance?.CallProviderDelegate?.Provider?.ReportCall(call.UUID, null, CXCallEndedReason.Unanswered);
       //     SendTransactionRequest(transaction);

            //DependencyService.Get<ICallNotificationService>().DisableCall(Convert.ToUInt64(call.ChatId));
        }

        public void PlaceCallOnHold(ActiveCall call)
        {
            // Build action
            var holdCallAction = new CXSetHeldCallAction(call.UUID, true);

            // Create transaction
            var transaction = new CXTransaction(holdCallAction);

            // Inform system of call request
            SendTransactionRequest(transaction);
        }

        public void RemoveCallFromOnHold(ActiveCall call)
        {
            // Build action
            var holdCallAction = new CXSetHeldCallAction(call.UUID, false);

            // Create transaction
            var transaction = new CXTransaction(holdCallAction);

            // Inform system of call request
            SendTransactionRequest(transaction);
        }
        #endregion
    }
}
