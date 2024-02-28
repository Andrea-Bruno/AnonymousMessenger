using System;
using Xamarin.Forms;

namespace Telegraph.Services
{
    public class CrossSimpleAudioPlayer
    {
      
        public IAudioPlayer CreateSimpleAudioPlayer()
        {
            return DependencyService.Get<IAudioPlayer>();
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the .NET standard version of this assembly. Reference the NuGet package from your platform-specific (head) application project in order to reference the platform-specific implementation.");
        }
    }
}