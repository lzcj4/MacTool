using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace PublicUtilities
{
    public static class TextHelper
    {
        private const string HTMLSAPCE = "&nbsp;";

        private static object lockObject = new object();

        public static bool IsNumber(string line)
        {
            lock (lockObject)
            {
                if (string.IsNullOrEmpty(line)) return false;
                bool b = Regex.IsMatch(line, @"^\d+$");
                return b;
            }
        }

        public static bool IsNumber(char c)
        {
            lock (lockObject)
            {
                if (c >= 0x30 && c <= 0x39)
                {
                    return true;
                }
                return false;
            }
        }

        public static char CharReversal(char c)
        {
            lock (lockObject)
            {
                if (c >= 0x41 && c <= 0x5A)
                {
                    return (char)(((byte)c )+ 32); 
                }
                else if(c >= 0x61 && c <= 0x7A)
                {
                    return (char)(((byte)c) - 32); 
                }
                return c;
            }
        }

        public static int StringToInt(string line)
        {
            lock (lockObject)
            {
                return Convert.ToInt32(line);
            }
        }

        public static string ToTitleCase(string s)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
        }

        public static bool IsAllChar(string line)
        {
            lock (lockObject)
            {
                if (string.IsNullOrEmpty(line)) return false;
                bool b = Regex.IsMatch(line, @"^[A-Za-z]+$");
                return b;
            }
        }

        public static bool IsChar(char c)
        {
            lock (lockObject)
            {
                if ((c >= 0x41 && c <= 0x5A) || (c >= 0x61 && c <= 0x7A))
                {
                    return true;
                }
                return false;
            }
        }

        public static bool IsMail(string content)
        {
            lock (lockObject)
            {
                if (string.IsNullOrEmpty(content)) return false;
                //Regex mail = new Regex("^\\w+((-\\w+)|(\\.\\w+))*\\@[A-Za-z0-9]+((\\.|-)[A-Za-z0-9]+)*\\.[A-Za-z0-9]+$");
                //Regex mail = new Regex(@"^.+@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$");
                Regex mail = new Regex(@"^.+@.*\..*$");
                return mail.IsMatch(content);
            }
        }

        public static bool IsIP(string content)
        {
            lock (lockObject)
            {

                if (string.IsNullOrEmpty(content)) return false;
                //Regex Ip = new Regex("^(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5])$");

                // Regex Ip = new Regex("^(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5]).(d{1,2}|1dd|2[0-4]d|25[0-5])$");
                Regex Ip = new Regex(@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
                return Ip.IsMatch(content);
            }
        }

        public static bool IsContains(string rawContent, params string[] subStrings)
        {
            lock (lockObject)
            {
                if (string.IsNullOrEmpty(rawContent) || (null == subStrings)) return false;
                foreach (string item in subStrings)
                {
                    if (rawContent.IndexOf(item.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static string TrimHtml(string content)
        {
            lock (lockObject)
            {
                if (string.IsNullOrEmpty(content)) return content;
                return content.Replace(TextHelper.HTMLSAPCE, "").Trim();
            }
        }
    }

    public static class FileHelper
    {
        public static string GetFilePath(string rawFilePath)
        {
            string dir = Path.GetDirectoryName(rawFilePath);
            return dir;
        }

        public static string GetFileName(string rawFilePath)
        {
            string file = Path.GetFileNameWithoutExtension(rawFilePath);
            return file;
        }

        /// <summary>
        /// If dir existed ,then delete it and create a new
        /// </summary>
        /// <param name="path"></param>
        public static void CreateNewDir(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }
    }
}