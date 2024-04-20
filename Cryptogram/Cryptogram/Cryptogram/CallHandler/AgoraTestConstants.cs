using System;
using System.Collections.Generic;
using System.Text;

namespace Anonymous.CallHandler
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
                return "ad647cd2961f4a659a08dba11ef01f01";
            }
        }

        //new appid for wegetltd = ad647cd2961f4a659a08dba11ef01f01
        //old that was in code = 71ef2cf4387646f5ba61fc663544bb95
        // new app certficate = 076c7c9d53ec4e9990e6e956bbf40638
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
