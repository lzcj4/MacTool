using System;
using System.Collections.Generic;
using System.Text;
using MacTool;
using MacTool.Properties;

namespace MacTool
{
    class AppSetting
    {
        static Settings appSetting;
        static AppSetting()
        {
            appSetting = MacTool.Properties.Settings.Default;
        }

        public static void Save()
        {
            appSetting.Save();
        }

        public static string User
        {
            get { return appSetting.User; }
            set { appSetting.User = value; }
        }

        public static string Pwd
        {
            get { return appSetting.Pwd; }
            set { appSetting.Pwd = value; }
        }

        public static string FilePath
        {
            get { return appSetting.FilePath; }
            set { appSetting.FilePath = value; }
        }

        public static string Interval
        {
            get { return appSetting.Interval; }
            set { appSetting.Interval = value; }
        }
        public static string Hour
        {
            get { return appSetting.Hour; }
            set { appSetting.Hour = value; }
        }

        public static bool IsTimerEnabled
        {
            get { return appSetting.IsTimerEnabled; }
            set { appSetting.IsTimerEnabled = value; }
        }

    }
}
