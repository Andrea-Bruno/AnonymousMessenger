using System;
using EncryptedMessaging;

namespace CloudClient
{
    public static  class CloudClientConnection
    {
#if DEBUG
        public static bool Reset = false;
#endif
        /// <summary>
        /// Initializer: Initialize the cloud client for use
        /// </summary>
        /// <param name="context"></param>
        public static void Initialize(Context context, string serverCloudPublicKey = default)
        {
            if (serverCloudPublicKey == default)
                serverCloudPublicKey = DefaultServerCloudPublicKey;
            var communication = new Communication(context, serverCloudPublicKey);
            context.AddCloudManager(communication);            
        }
#if DEBUG        
        const string DefaultServerCloudPublicKey = @"AoT0MTjFnw4bVT1ma74N/dK3Za5A1Iv5TDC4vKtJ6Ukm";
#elif DEBUG_RAM
        const string DefaultServerCloudPublicKey = @"ApkrRQUe7qbaKY05Lbs5z+o001UNzXlfHgm+9KEN41vE";
#else
        const string DefaultServerCloudPublicKey = @"A4f7EZyD/lVQd5P4r0H3haPCdQJNOU/6sm7LsZoIT+XH"; // Production
#endif     
    }
}
