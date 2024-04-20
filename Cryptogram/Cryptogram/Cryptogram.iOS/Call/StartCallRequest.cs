using Foundation;
using Intents;
namespace Anonymous.iOS.Call
{
    public static class StartCallRequest
    {
        public static string URLScheme
        {
            get { return "monkeycall"; }
        }

        public static string ActivityType
        {
            get { return INIntentIdentifier.StartAudioCall.GetConstant().ToString(); }
        }

        public static string CallHandleFromURL(NSUrl url)
        {
            // Is this a MonkeyCall handle?
            if (url.Scheme == URLScheme)
            {
                // Yes, return host
                return url.Host;
            }
            else
            {
                // Not handled
                return null;
            }
        }

        public static string CallHandleFromActivity(NSUserActivity activity)
        {
            // Is this a start call activity?
            if (activity.ActivityType == ActivityType)
            {
                // Yes, trap any errors
                try
                {
                    // Get first contact
                    var interaction = activity.GetInteraction();
                    var startAudioCallIntent = interaction.Intent as INStartAudioCallIntent;
                    var contact = startAudioCallIntent.Contacts[0];

                    // Get the person handle
                    return contact.PersonHandle.Value;
                }
                catch
                {
                    // Error, report null
                    return null;
                }
            }
            else
            {
                // Not handled
                return null;
            }
        }
    }
}