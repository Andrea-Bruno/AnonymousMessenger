using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraph.CallHandler
{
  public static class AgoraTestConstants
    {
        /// <summary>
        /// App ID from https://dashboard.agora.io/
        /// </summary>
        public static string AgoraAPI
        {
            get
            {
                return "71ef2cf4387646f5ba61fc663544bb95";
            }
        }

        /// <summary>
        /// Temp token generated in https://dashboard.agora.io/ or Token from your API
        /// </summary>
        public static string Token
        {
            get
            {
                return null;
            }
        }
        public const string ShareString = "Hey check out Xamarin Agora sample app at: http://drmtm.us/videosample";
    }
}
