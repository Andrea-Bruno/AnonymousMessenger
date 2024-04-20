namespace Anonymous.Services
{
    public interface ISharedPreference
    {
        void AddContact(string chatId, string name, bool isOsAndroid);
        string GetContactName(string chatId);
        bool GetContactOS(string chatId);
        bool GetValueByKey(string key);
        void SetValueByKey(string key, bool value);
    }

}
