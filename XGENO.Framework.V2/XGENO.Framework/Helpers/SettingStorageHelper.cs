using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XGENO.Framework.Enums;

namespace XGENO.Framework.Helpers
{
    public static class SettingStorageHelper
    {
        public static bool SettingExists(string key, StorageStrategy location = StorageStrategy.Local)
        {
            switch (location)
            {
                case StorageStrategy.Local:
                    return Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
                case StorageStrategy.Roaming:
                    return Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey(key);
                default:
                    throw new NotSupportedException(location.ToString());
            }
        }

        public static T GetSetting<T>(string key, T otherwise = default(T), StorageStrategy location = StorageStrategy.Local, bool setSetting = false)
        {
            try
            {
                if (!(SettingExists(key, location)))
                {
                    if(setSetting)
                        SetSetting<T>(key, otherwise, location);

                    return otherwise;
                }

                switch (location)
                {
                    case StorageStrategy.Local:
                        return (T) Windows.Storage.ApplicationData.Current.LocalSettings.Values[key];
                    case StorageStrategy.Roaming:
                        return (T) Windows.Storage.ApplicationData.Current.RoamingSettings.Values[key];
                    default:
                        throw new NotSupportedException(location.ToString());
                }
            }
            catch 
            {
            }

            return otherwise;
        }

        public static void SetSetting<T>(string key, T value, StorageStrategy location = StorageStrategy.Local)
        {
            switch (location)
            {
                case StorageStrategy.Local:
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = value;
                    break;
                case StorageStrategy.Roaming:
                    Windows.Storage.ApplicationData.Current.RoamingSettings.Values[key] = value;
                    break;
                default:
                    throw new NotSupportedException(location.ToString());
            }
        }

        public static void DeleteSetting(string key, StorageStrategy location = StorageStrategy.Local)
        {
            switch (location)
            {
                case StorageStrategy.Local:
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove(key);
                    break;
                case StorageStrategy.Roaming:
                    Windows.Storage.ApplicationData.Current.RoamingSettings.Values.Remove(key);
                    break;
                default:
                    throw new NotSupportedException(location.ToString());
            }
        }

        //public static T GetSerializedSetting<T>(string key, T otherwise = default(T), StorageStrategy location = StorageStrategy.Local, bool setSetting = false)
        //{
        //    try
        //    {
        //        if (!(SettingExists(key, location)))
        //        {
        //            if (setSetting)
        //                SetSerializedSetting<T>(key, otherwise, location);

        //            return otherwise;
        //        }

        //        switch (location)
        //        {
        //            case StorageStrategy.Local:
        //                return SerializationHelper.Deserialize<T>(Windows.Storage.ApplicationData.Current.LocalSettings.Values[key].ToString());
        //            case StorageStrategy.Roaming:
        //                return SerializationHelper.Deserialize<T>(Windows.Storage.ApplicationData.Current.RoamingSettings.Values[key].ToString());
        //            default:
        //                throw new NotSupportedException(location.ToString());
        //        }
        //    }
        //    catch 
        //    {
        //    }

        //    return otherwise; 
        //}

        //public static void SetSerializedSetting<T>(string key, T value, StorageStrategy location = StorageStrategy.Local)
        //{
        //    switch (location)
        //    {
        //        case StorageStrategy.Local:
        //            Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = SerializationHelper.Serialize<T>(value);
        //            break;
        //        case StorageStrategy.Roaming:
        //            Windows.Storage.ApplicationData.Current.RoamingSettings.Values[key] = SerializationHelper.Serialize<T>(value);
        //            break;
        //        default:
        //            throw new NotSupportedException(location.ToString());
        //    }
        //}
    }
}
