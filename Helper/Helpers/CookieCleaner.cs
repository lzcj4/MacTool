using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PublicUtilities.Helpers
{
    public enum Explorer
    {
        IE,
        FireFox
    }

    public static class CookieCleaner
    {
        public static bool CleanCookies(string webKeyword, Explorer explorer)
        {
            //定义变量
            string cookiesPath, findWords=webKeyword, osName;
            string UserProfile;
            string XPCookiesPath, VistaCookiesPath;
            long osType;

            //获取用户配置路径
            UserProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            //获取操作系统类型
            osType = Environment.OSVersion.Version.Major;

            //解析地址
            if (webKeyword.Contains("."))
            {
                //用"."分割字符
                char[] separator = { '.' };
                string[] splitWords = webKeyword.Split(separator);
                findWords = (splitWords != null && splitWords.Length > 0) ? splitWords[0] : string.Empty;
            }

            //判断浏览器类型
            if (explorer == Explorer.IE)
            {
                //IE浏览器
                XPCookiesPath = @"\Cookies\";
                VistaCookiesPath = @"\AppData\Roaming\Microsoft\Windows\Cookies\";
            }
            else if (explorer == Explorer.FireFox)
            {
                //FireFox浏览器
                XPCookiesPath = @"\Application Data\Mozilla\Firefox\Profiles\";
                VistaCookiesPath = @"\AppData\Roaming\Mozilla\Firefox\Profiles\";
                findWords = "cookies";
            }
            else
            {
                XPCookiesPath = "";
                VistaCookiesPath = "";
                return false;
            }

            //判断操作系统类型
            if (osType == 5)
            {
                //系统为XP
                osName = "Microsoft Windows XP";
                cookiesPath = UserProfile + XPCookiesPath;
            }
            else if (osType == 6)
            {
                //系统为Vista
                osName = "Microsoft Windows Vista";
                cookiesPath = UserProfile + VistaCookiesPath;
            }
            else if (osType == 7)
            {
                //系统为Win 7
                osName = "Microsoft Windows 7";
                cookiesPath = UserProfile + VistaCookiesPath;
            }
            else
            {
                //未识别之操作系统
                osName = "Other OS Version";
                cookiesPath = "";
                return false;
            }

            //删除文件
            return DeleteFileFunction(cookiesPath, findWords);
        }

        //重载函数

        public static bool CleanCookies(string webKeyword)
        {
            return CleanCookies(webKeyword, Explorer.IE);
        }

        private static bool DeleteFileFunction(string filepath, string findWords)
        {           

            //下面这些字串例外
            string exceptFileName = "index.dat";
            //解析删除关键字
            string findFileWildStr= string.IsNullOrEmpty(findWords) ? "*.*" : "*" + findWords + "*";

            //删除cookie文件夹
            try
            {
                foreach (string dFileName in Directory.GetFiles(filepath, findFileWildStr))
                {
                    if (dFileName == filepath + exceptFileName)
                    {
                        continue;
                    }
                    File.Delete(dFileName);
                }
            }
            catch
            {
                return false;
            }

            //深层遍历（解决Vista Low权限问题）
            string[] subFolders = Directory.GetDirectories(filepath);
            foreach (string subItem in subFolders)
            {
                try
                {
                    foreach (string dFileName in Directory.GetFiles(subItem, findFileWildStr))
                    {
                        if (dFileName == filepath + exceptFileName)
                            continue;
                        File.Delete(dFileName);
                    }
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
    }
}
