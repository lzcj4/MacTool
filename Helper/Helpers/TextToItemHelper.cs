using System;

namespace PublicUtilities
{
    public class TextToItemHelper
    {
        public static char[] SplitChars = { ' ', '\t', ',', '，', ':', '：', ';', '；', '\n', '\r' };

        public static char[] AccountSplitChars = { ' ', '\t', '\n', '\r' };

        #region Mail / user/ pwd

        public static bool GetLoginAccountItem(string rawString, ref string account, ref string pwd, ref string email,
            DataFormat dataFormat, LogManagerBase logManager)
        {
            if (string.IsNullOrEmpty(rawString))
                return false;

            account = string.Empty;
            pwd = string.Empty;
            email = string.Empty;

            string[] values = rawString.Split(TextToItemHelper.SplitChars, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim();
            }
            if (values.Length == 2)
            {
                if (dataFormat == DataFormat.MailPassword)
                {
                    email = values[0];
                    account = email;
                    pwd = values[1];
                    return true;
                }
                else if (dataFormat == DataFormat.AccountPassword)
                {
                    account = values[0];
                    email = account;
                    pwd = values[1];
                    return true;
                }
            }
            else if (values.Length >= 3)
            {
                // user name may contains space
                int emailIndex = GetEmailIndex(values, logManager);
                if (emailIndex < 0)
                {
                    logManager.Error(string.Format("GetAccountInfo({0}): Email address can't be the first item", rawString));
                }
                //Trace.Assert(emailIndex >= 0, "GetAccountInfo(): Email address can't be the first item");
                if (emailIndex >= 0)
                {
                    if (dataFormat == DataFormat.AccountMailPassword)
                    {
                        email = values[emailIndex];
                        account = GetUser(rawString, email, logManager);
                        if (emailIndex + 1 != (values.Length - 1))
                        {
                            logManager.Error("GetAccountInfo(): Email address can't be the last item. it's miss password item");
                        }
                        //Trace.Assert(emailIndex + 1 == (values.Length - 1), "GetAccountInfo(): Email address can't be the last item. it's miss password item");
                        pwd = values[emailIndex + 1];
                        return true;
                    }

                    if (dataFormat == DataFormat.AccountPasswordMail)
                    {
                        email = values[emailIndex];
                        if (emailIndex - 1 < 0)
                        {
                            logManager.Error(string.Format("GetAccountInfo({0}):  Email address can't be the first item. it's miss password item", rawString));
                        }
                        //Trace.Assert(emailIndex - 1 >= 0, "GetAccountInfo(): Email address can't be the first item. it's miss password item");
                        pwd = values[emailIndex - 1];
                        account = GetUser(rawString, pwd, logManager);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Index from last
        /// </summary>
        /// <param name="rawContent"></param>
        /// <param name="subString"></param>
        /// <returns></returns>
        public static string GetUser(string rawContent, string subString, LogManagerBase logManager)
        {
            int index = rawContent.LastIndexOf(subString);
            if (index >= 0)
            {
                return rawContent.Substring(0, index);
            }

            logManager.Error(string.Format("GetUser({0}): password index must bigger than 0", rawContent));
            return string.Empty;
        }

        public static int GetEmailIndex(string[] values, LogManagerBase logManager)
        {
            if (values.Length == 0)
            {
                logManager.Error(string.Format("GetEmailIndex():the raw data is incorrect ,this is a empty line"));
                return -1;
            }

            for (int i = values.Length - 1; i >= 0; i--)
            {
                if (TextHelper.IsMail(values[i]))
                {
                    return i;
                }
            }

            string valueStr = string.Empty;
            foreach (string item in values)
            {
                valueStr += item;
            }
            logManager.Error(string.Format("GetEmailIndex({0}):不是用效的邮箱格式，请检查数据格式是否正确", valueStr));
            return -1;
        }

        #endregion

        #region Mail/First name/ second name

        public static PwdResetItem GetPwdResetItem(string rawString, DataFormat dataFormat, LogManagerBase logManager)
        {
            if (string.IsNullOrEmpty(rawString))
                return null;

            string[] values = rawString.Split(TextToItemHelper.AccountSplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length >= 3)
            {
                // user name may contains space
                int emailIndex = GetEmailIndex(values, logManager);
                if (emailIndex != 0)
                {
                    logManager.Error(string.Format("GetAccountInfo({0}): Email address can't be the first item", rawString));
                    return null;
                }
                else
                {
                    PwdResetItem item = new PwdResetItem();
                    item.EMail = values[0];
                    if (dataFormat == DataFormat.MailFstNSecN)
                    {
                        item.FirstName = values[1];
                        item.SecondName = values[2];
                    }
                    else if (dataFormat == DataFormat.MailSecNFstN)
                    {
                        item.SecondName = values[1];
                        item.FirstName = values[2];
                    }
                    return item;
                }
            }

            return null;
        }

        #endregion


        #region IP / Port


        public static bool GetProxy(string rawString, ref string ip, ref int port)
        {
            if (string.IsNullOrEmpty(rawString))
                return false;

            ip = string.Empty;
            port = 0;

            string[] values = rawString.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length == 2)
            {
                ip = values[0];
                bool isGet = int.TryParse(values[1], out port);
                return isGet;
            }

            return false;
        }

        #endregion

    }
}