using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Text;

namespace PublicUtilities
{
    public class CaptchaHelper : HttpHelperBase
    {
        public static CaptchaHelper Instance = new CaptchaHelper(WowLogManager.Instance);

        public string CurrentCaptchaPath
        {
            get;
            set;
        }

        public CaptchaHelper(LogManagerBase logManager)
            : base(logManager)
        {
        }

        #region Captcha

        private object lockObject = new object();

        public string GetCaptcha(string url)
        {
            string captchaCode = this.GetCaptchaCodeFormUrl(url);
            return captchaCode;
        }

        protected string GetCaptchaCode(string htmlContent, string webSiteUrl)
        {
            //only need captcha need get session id back from cookie
            if (htmlContent.Contains("captcha.jpg"))
            {
                string captchaUrl = this.GetCaptchaUrl(htmlContent);
                if (!string.IsNullOrEmpty(captchaUrl))
                {
                    return this.GetCaptchaCodeFormUrl(string.Format("{0}{1}", webSiteUrl, captchaUrl));
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get captcha code from url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetCaptchaCodeFormUrl(string url)
        {
            string captchaImagePath = this.SaveCaptchaImage(url);
            return GetCaptchaFromFile(captchaImagePath);
        }

        private static object obj = new object();
        /// <summary>
        /// Get captcha from a file ,and the max captcha len is 8
        /// </summary>
        /// <param name="filePath">file path of captcha</param>
        /// <returns>captcha from image</returns>
        public static string GetCaptchaFromFile(string filePath)
        {
            lock (obj)
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    return string.Empty;
                }
                byte[] chars = new byte[8];
                int code = NativeMethods.GetCaptchaFromFile(filePath, ref chars[0], ref chars[1], ref chars[2], ref chars[3],
                                                        ref chars[4], ref chars[5], ref chars[6], ref chars[7]);
                char[] codes = ASCIIEncoding.Default.GetChars(chars);
                StringBuilder sb = new StringBuilder();
                sb.Append(codes);
                string captcha = sb.ToString();
                captcha = captcha.Replace("\0", "");
                return captcha;
            }
        }

        /// <summary>
        /// save captcha from url and return the file path
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string SaveCaptchaImage(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                LogManager.Error("captcha url can't be null or empty");
                return string.Empty;
            }
            //Trace.Assert(!string.IsNullOrEmpty(url), "captcha url can't be null or empty");
            //https://us.battle.net/login/captcha.jpg?random=-1235
            HttpWebRequest httpRequest = this.GetHttpWebRequest(url, false);
            if (null != httpRequest)
            {
                try
                {
                    using (HttpWebResponse httpResponse = httpRequest.GetResponse() as HttpWebResponse)
                    {
                        using (Stream stream = httpResponse.GetResponseStream())
                        {
                            Bitmap bitmap = new Bitmap(stream);
                            string imagePath = string.Format("{0}{1}{2}", GetCurrentAppCaptchaPath(), GetImageNameFromUrl(url), IMAGEEXTENSION);
                            bitmap.Save(imagePath);
                            LogManager.Info(string.Format("Cpatha url:{0} save to :{1}", url, imagePath));
                            return imagePath;
                        }
                    }
                }
                finally
                {
                    this.DisposeHttpRequest(httpRequest);
                }
            }

            LogManager.Error("captcha image path can't be null or empty");
            return string.Empty;
        }

        HtmlParseHelper htmlParser = new HtmlParseHelper();
        private string GetCaptchaUrl(string htmlContent)
        {
            string outerHtml = htmlParser.GetOuterTextFromHtml("<div class=\"captcha\">", @"<img", @"/>", 1, htmlContent);
            string captchaImage = htmlParser.GetOutterPropertyFromHtml(outerHtml, "src");
            return captchaImage;
        }

        private string GetCurrentAppCaptchaPath()
        {
            const string captchaFolderName = "TempCaptcha";
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            appPath = string.Format(@"{0}\{1}\", appPath, captchaFolderName);

            if (!Directory.Exists(appPath))
            {
                Directory.CreateDirectory(appPath);
            }
            return appPath;
        }

        private string GetImageNameFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                LogManager.Error("captcha url can't be null or empty");
                return string.Empty;
            }
            string randomId = string.Empty;
            if (!string.IsNullOrEmpty(url))
            {
                randomId = url.Substring(url.IndexOf("=") + 1);
            }

            if (string.IsNullOrEmpty(randomId))
            {
                LogManager.Error("captcha random id can't be null or empty");
            }

            return randomId;
        }

        public void ClearCaptchaFolder()
        {
            string path = GetCurrentAppCaptchaPath();
            Directory.Delete(path, true);
        }

        #endregion
    }
}
