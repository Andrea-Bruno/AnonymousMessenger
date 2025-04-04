using Android.Content;
using Android.Preferences;
using Cryptogram.Droid.Services;
using Cryptogram.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(SharedPrefernceHandler))]
namespace Cryptogram.Droid.Services
{
    public class SharedPrefernceHandler : ISharedPreference
    {
        public void AddContact(string chatId, string name, bool isOsAndroid)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString(chatId, name);
            editor.PutBoolean(chatId + "OS", isOsAndroid);
            editor.Apply();
        }

        public string GetContactName(string chatId)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
            return prefs.GetString(chatId, null);
        }

        public bool GetContactOS(string chatId)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
            return prefs.GetBoolean(chatId + "OS", true);
        }

        public string GetMyId()
        {
#pragma warning disable CS0618 // 'PreferenceManager.GetDefaultSharedPreferences(Context?)' è obsoleto: 'deprecated'
#pragma warning disable CS0618 // 'PreferenceManager' è obsoleto: 'This class is obsoleted in this android platform'
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
#pragma warning restore CS0618 // 'PreferenceManager' è obsoleto: 'This class is obsoleted in this android platform'
#pragma warning restore CS0618 // 'PreferenceManager.GetDefaultSharedPreferences(Context?)' è obsoleto: 'deprecated'
            return prefs.GetString("MyId", null);
        }

        public bool GetValueByKey(string key)
        {
#pragma warning disable CS0618 // 'PreferenceManager.GetDefaultSharedPreferences(Context?)' è obsoleto: 'deprecated'
#pragma warning disable CS0618 // 'PreferenceManager' è obsoleto: 'This class is obsoleted in this android platform'
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
#pragma warning restore CS0618 // 'PreferenceManager' è obsoleto: 'This class is obsoleted in this android platform'
#pragma warning restore CS0618 // 'PreferenceManager.GetDefaultSharedPreferences(Context?)' è obsoleto: 'deprecated'
            return prefs.GetBoolean(key, false);
        }

        public void SetValueByKey(string key, bool value)
        {
#pragma warning disable CS0618 // 'PreferenceManager.GetDefaultSharedPreferences(Context?)' è obsoleto: 'deprecated'
#pragma warning disable CS0618 // 'PreferenceManager' è obsoleto: 'This class is obsoleted in this android platform'
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
#pragma warning restore CS0618 // 'PreferenceManager' è obsoleto: 'This class is obsoleted in this android platform'
#pragma warning restore CS0618 // 'PreferenceManager.GetDefaultSharedPreferences(Context?)' è obsoleto: 'deprecated'
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean(key, value);
            editor.Apply();
        }

        public void StoreMyId(string id)
        {
#pragma warning disable CS0618 // 'PreferenceManager.GetDefaultSharedPreferences(Context?)' è obsoleto: 'deprecated'
#pragma warning disable CS0618 // 'PreferenceManager' è obsoleto: 'This class is obsoleted in this android platform'
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
#pragma warning restore CS0618 // 'PreferenceManager' è obsoleto: 'This class is obsoleted in this android platform'
#pragma warning restore CS0618 // 'PreferenceManager.GetDefaultSharedPreferences(Context?)' è obsoleto: 'deprecated'
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString("MyId", id);
            editor.Apply();
        }

    }
}
