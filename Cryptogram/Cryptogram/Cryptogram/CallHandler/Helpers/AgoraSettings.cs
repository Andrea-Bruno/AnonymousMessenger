using System;
using System.Collections.Generic;
using System.Text;
namespace Cryptogram.CallHandler.Helpers
{
    public class AgoraSettings : BasePropertyChanged
    {
        public const int MaxPossibleBitrate = 300;
        public const int MinPossibleBitrate = 100;

        static AgoraSettings _settings;
        public static AgoraSettings Current
        {
            get { return _settings ?? (_settings = new AgoraSettings()); }
        }

        #region Setting Constants

        private static int ProfileDefault = 30;

        private static bool UseMySettingsDefault = false;

        private static string RoomNameDefault = "112221";

        private static string EncryptionPhraseDefault = "";

        private static int EncryptionTypeDefault = (int)EncryptionType.xts128;

        private static bool IsAudioDefaultCall = true;

        private static string UsernameDefault = "";

        private static bool IsCallingByMeDefault = false;

        private static bool IsGroupCallDefault = false;

        private static byte[] AvatarDefault = null;
        #endregion

        public int Profile
        {
            get
            {
                return ProfileDefault;
            }
            set
            {
                ProfileDefault = value;
                OnPropertyChanged();
            }
        }

        public bool UseMySettings
        {
            get
            {
                return UseMySettingsDefault;
            }
            set
            {
                UseMySettingsDefault = value;
                OnPropertyChanged();
            }
        }

        public string RoomName
        {
            get
            {
                return RoomNameDefault;
            }
            set
            {
                RoomNameDefault = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get
            {
                return UsernameDefault;
            }
            set
            {
                UsernameDefault = value;
                OnPropertyChanged();
            }
        }

        public string EncryptionPhrase
        {
            get
            {
                return EncryptionPhraseDefault;
            }
            set
            {
                EncryptionPhraseDefault = value;
                OnPropertyChanged();
            }
        }

        public EncryptionType EncryptionType
        {
            get
            {
                return (EncryptionType)EncryptionTypeDefault;
            }
            set
            {
                EncryptionTypeDefault = (int)value;
                OnPropertyChanged();
            }
        }
        public bool IsAudioCall
        {
            get
            {
                return IsAudioDefaultCall;
            }
            set
            {
                IsAudioDefaultCall = value ;
                OnPropertyChanged();
            }
        }

        public bool IsCallingByMe
        {
            get
            {
                return IsCallingByMeDefault;
            }
            set
            {
                IsCallingByMeDefault = value;
                OnPropertyChanged();
            }
        }

        public bool IsGroupCall
        {
            get
            {
                return IsGroupCallDefault;
            }
            set
            {
                IsGroupCallDefault = value;
                OnPropertyChanged();
            }
        }

        public byte[] Avatar
        {
            get
            {
                return AvatarDefault;
            }
            set
            {
                AvatarDefault = value;
                OnPropertyChanged();
            }
        }

    }
}