using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace MacTool.Mac
{
    public class MacUnbindEventArg : EventArgs
    {
        public string Msg { get; }
        public string Detail { get; set; }
        public MacUnbindEventArg(string msg)
        {
            this.Msg = msg;
        }
    }

    class MacUnbind
    {
        public event EventHandler<MacUnbindEventArg> OnChanged;
        public event EventHandler<MacUnbindEventArg> OnError;
        private string Path { get; set; }
        private string User { get; set; }
        private string PWD { get; set; }

        public int Interval { get; set; }

        private IList<string> accounts = new List<string>();
        HtmlHelper htmlHelper = new HtmlHelper();
        public MacUnbind(string text, string user, string pwd, bool isFile = true)
        {
            if (isFile)
            {
                this.Path = text;
                GetFileAccount();
            }
            else
            {
                this.accounts.Add(text);
            }
            this.Interval = 5;
            this.User = user;
            this.PWD = pwd;
        }

        protected void RaiseChanged(string msg)
        {
            if (null != this.OnChanged)
            {
                this.OnChanged(this, new MacUnbindEventArg(msg));
            }
        }

        protected void RaiseError(string msg, string detail = null)
        {
            if (null != this.OnChanged)
            {
                this.OnError(this, new MacUnbindEventArg(msg) { Detail = detail });
            }
        }

        string currentAccont = null;
        bool isRunning = true;
        public bool IsRunning { get { return this.isRunning; } }

        public Tuple<int, int, int> Start()
        {
            Tuple<int, int, int> result = new Tuple<int, int, int>(0, 0, 0);
            bool isLogined = Login();
            if (!isLogined)
            {
                return result;
            }
            if (!GetXToken())
            { return result; }

            isRunning = true;
            int total = 0, succeed = 0, failed = 0;
            foreach (string item in accounts)
            {
                total++;
                currentAccont = item;
                if (!isRunning)
                {
                    RaiseError(string.Format("程序执行被停止"));
                    break;
                }
                bool isUnbind = UnbindMac(item, this.XToken);
                if (isUnbind)
                {
                    RaiseChanged(string.Format("{0} 解绑成功", item));
                    succeed++;
                }
                else
                {
                    RaiseChanged(string.Format("---- >: {0} 解绑失败", item));
                    failed++;
                }
                Thread.Sleep(Interval * 1000);
            }
            isRunning = false;
            return new Tuple<int, int, int>(total, succeed, failed);

        }

        public void Stop()
        {
            isRunning = false;
        }

        private void GetFileAccount()
        {
            using (StreamReader sr = new StreamReader(new FileStream(this.Path, FileMode.Open, FileAccess.Read)))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    accounts.Add(line);
                }

            }
        }

        private HttpWebRequest CreateRequest(string url, string method)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = method;
            return request;
        }

        private HttpWebRequest GetRequest(string url)
        {
            return CreateRequest(url, "GET");
        }

        private HttpWebRequest PostRequest(string url)
        {
            return CreateRequest(url, "POST");
        }

        private CookieContainer cookies = new CookieContainer();
        private void SetHttpHeader(HttpWebRequest request)
        {
            if (request == null)
            {
                return;
            }

            request.KeepAlive = true;
            request.Host = "bbadmin.zjip.com:8002";
            request.Accept = "application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, */*";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Win64; x64; Trident/4.0; .NET CLR 2.0.50727; SLCC2; .NET CLR 3.5.30729; .NET CLR 3.0.30729)";
            request.ContentType = "application/x-www-form-urlencoded";
            //request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en;q=0.6");
            request.Headers.Add("Accept-Language", "en-us");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.CookieContainer = this.cookies;

            request.Referer = "http://bbadmin.zjip.com:8002/webkit_ui/ls_login.jsp";
            request.Host = "bbadmin.zjip.com:8002";
            request.Headers.Add("Pragma", "no-cache");
            request.Headers.Add("UA-CPU", "AMD64");
        }

        private string EncodeValue(string str)
        {
            return Uri.EscapeDataString(str).Replace("!", "%21");
        }
        private string GetHtmlFromReponse(HttpWebRequest request)
        {
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("GBK")))
                    {
                        string line = sr.ReadToEnd();
                        return line;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Assert(e != null, e.Message, null != e.InnerException ? e.InnerException.Message : e.StackTrace);
                // throw e;
                return string.Empty;
            }

        }


        private bool Login()
        {
            #region
            //POST http://bbadmin.zjip.com:8002/servlet/com.portal.web.PInfranetServlet HTTP/1.1
            //Accept: application/x-ms-application, image/jpeg, application/xaml+xml, image/gif, image/pjpeg, application/x-ms-xbap, */
            //            *
            //Referer: http://bbadmin.zjip.com:8002/webkit_ui/ls_login.jsp
            //            Accept - Language: en - us
            //User - Agent: Mozilla / 4.0(compatible; MSIE 8.0; Windows NT 6.1; Win64; x64; Trident / 4.0; .NET CLR 2.0.50727; SLCC2; .NET CLR 3.5.30729; .NET CLR 3.0.30729)
            //Content - Type: application / x - www - form - urlencoded
            //UA - CPU: AMD64
            //Accept - Encoding: gzip, deflate
            //    Connection: Keep - Alive
            //Content - Length: 146
            //Host: bbadmin.zjip.com:8002
            //Pragma: no - cache

            //login = wuchfj & password = qwert12 % 21 & csrlogin = yes & page = firstpage & sessionState = start & Component = com.hp.web.comp.PInitialBeanImpl & Submit = ++% B5 % C7 % C2 % BC++

            #endregion

            bool isLogined = false;
            string url = "http://bbadmin.zjip.com:8002/servlet/com.portal.web.PInfranetServlet";
            string txt = "login={0}&password={1}&csrlogin=yes&page=firstpage&sessionState=start&Component=com.hp.web.comp.PInitialBeanImpl&Submit=++%B5%C7%C2%BC++";
            txt = string.Format(txt, EncodeValue(this.User), EncodeValue(this.PWD));
            HttpWebRequest request = PostRequest(url);
            SetHttpHeader(request);
            using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(txt);
            }
            string html = GetHtmlFromReponse(request);
            //string html = GetTestHtml(@"E:\MacTool\文档\login.txt");
            isLogined = html.Contains("上次登录时间");
            if (!isLogined)
            {
                RaiseError("登录失败", html);
            }

            return isLogined;
        }

        private string XToken;
        private bool GetXToken()
        {
            bool result = false;
            HttpWebRequest request = this.GetRequest("http://bbadmin.zjip.com:8002/webkit_ui/main_title.jsp");
            SetHttpHeader(request);
            request.Referer = "http://bbadmin.zjip.com:8002/webkit_ui/admin.html";
            string html = GetHtmlFromReponse(request);
            //string html = GetTestHtml(@"E:\MacTool\文档\xtoken.txt");
            XToken = htmlHelper.GetPropertyFromHtml(@"<input name=""xtoken""", "/>", "value", html);
            if (string.IsNullOrEmpty(XToken))
            {
                RaiseError("进入业务受理获取Token失败", html);
            }
            else
            {
                result = true;
            }
            return result;

        }

        string Host = "http://bbadmin.zjip.com:8002";
        private bool UnbindMac(string account, string token)
        {
            bool result = false;
            //查询
            string url = "http://bbadmin.zjip.com:8002/webkit_ui/biz/pubAcntQueryRes.jsp";
            HttpWebRequest request = this.PostRequest(url);
            SetHttpHeader(request);
            request.Referer = "http://bbadmin.zjip.com:8002/webkit_ui/biz/pubAcntQuery.jsp?acntManagement=yes";
            string txt = "listSrv=no&acntManagement=yes&type=1&region1=57901%2B%BD%F0%BB%AA%2B%BD%F0%BB%AA%B5%D8%C7%F8&region=57901&cardtype=40000&login={0}&loginrelation=equal&custidlocal=&instlno=&accountstatus=&service=&firstname=&relation=equal&servicestatus=&Submit=%B2%E9%D1%AF&xtoken={1}";
            txt = string.Format(txt, account, EncodeValue(token));
            using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(txt);
            }
            string html = GetHtmlFromReponse(request);
            //string html = GetTestHtml(@"E:\MacTool\文档\jhdianyeju.txt");
            string detailUrl = htmlHelper.GetPropertyFromHtml("<a", @""">", "href", html);
            if (string.IsNullOrEmpty(detailUrl))
            {
                RaiseError("获取 金华电业局 失败", html);
                return result;
            }
            detailUrl = Host + detailUrl;

            //明细
            request = this.GetRequest(detailUrl);
            SetHttpHeader(request);
            request.Referer = url;
            html = GetHtmlFromReponse(request);
            //html = GetTestHtml(@"E:\MacTool\文档\dial.txt");
            string macUrl = htmlHelper.GetPropertyFromHtml(@"<td bgcolor=""#d1e3f5"" width=""10%""><a", @"宽带拨号", "href", html);
            if (string.IsNullOrEmpty(detailUrl))
            {
                RaiseError("获取 宽带拨号 失败", html);
                return result;
            }
            macUrl = Host + macUrl;

            //Mac地址列表
            request = this.GetRequest(macUrl);
            SetHttpHeader(request);
            request.Referer = detailUrl;
            html = GetHtmlFromReponse(request);
            //html = GetTestHtml(@"E:\MacTool\文档\mac_list.txt");
            string postData = GetUnbindText(html);

            //解绑
            url = "http://bbadmin.zjip.com:8002/webkit_ui/biz/acntModifyServiceInput.jsp";
            request = this.PostRequest(url);
            SetHttpHeader(request);
            request.Referer = macUrl;
            using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(postData);
            }
            html = GetHtmlFromReponse(request);
            //html = GetTestHtml(@"E:\MacTool\文档\post_unbind.txt");
            //获取解绑状态
            result = GetUnbindStatus(html);
            return result;
        }
        private bool GetUnbindStatus(string html)
        {
            string action = htmlHelper.GetPropertyFromHtml("action=", @"method=""post""", "action", html);

            IDictionary<string, string> resultDic = new Dictionary<string, string>();
            string[] items = { "homeid1", "homeid2", "page" ,
            "loginname","localid","Component","loadBean"};
            foreach (string name in items)
            {
                string startStr = string.Format(@"name=""{0}"" value=", name);
                string hidenItem = htmlHelper.GetSubString(html, startStr, ">");
                if (!string.IsNullOrEmpty(hidenItem))
                {
                    hidenItem = hidenItem.Replace("\"", "");
                    if (name == "loginname")
                    {
                        resultDic.Add(name, hidenItem);
                    }
                    else
                    {
                        resultDic.Add(name, EncodeValue(hidenItem));
                    }

                }
            }

            string token = htmlHelper.GetPropertyFromHtml(@"name=""xtoken""", @"/>", "value", html);
            if (!string.IsNullOrEmpty(token))
            {
                resultDic.Add("xtoken", EncodeValue(token));
            }

            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var kv in resultDic)
            {
                if (i++ < resultDic.Count - 1)
                {
                    sb.AppendFormat("{0}={1}&", kv.Key, kv.Value);
                }
                else
                { sb.AppendFormat("{0}={1}", kv.Key, kv.Value); }
            }
            string postData = sb.ToString();
            string url = Host + action;

            HttpWebRequest request = this.PostRequest(url);
            SetHttpHeader(request);
            request.Referer = "http://bbadmin.zjip.com:8002/webkit_ui/biz/acntModifyServiceInput.jsp";
            using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(postData);
            }
            html = GetHtmlFromReponse(request);
            //html = GetTestHtml(@"E:\MacTool\文档\get_unbind_status.txt");
            bool result = html.Contains("成功修改服务属性");
            return result;
        }

        private string GetUnbindText(string html)
        {
            IDictionary<string, string> resultDic = new Dictionary<string, string>();
            string[] items = { "Database", "ServiceFullName", "ServiceNo" ,
            "ServiceName"};
            foreach (string name in items)
            {
                string startStr = string.Format(@"name=""{0}"" value=""", name);
                string hidenItem = htmlHelper.GetPropertyFromHtml(startStr, ">", "value", html);
                if (!string.IsNullOrEmpty(hidenItem))
                {
                    resultDic.Add(name, EncodeValue(hidenItem));
                }
            }
            string item = htmlHelper.GetSubString(html, @"name=""AccountNo"" value=", ">");
            if (!string.IsNullOrEmpty(item))
            {
                resultDic.Add("AccountNo", EncodeValue(item));
            }

            string subband = htmlHelper.GetPropertyFromHtml(@"name=""subbandwidth"" value=""", ">", "value", html);
            if (!string.IsNullOrEmpty(subband))
            {
                resultDic.Add("subbandwidth", EncodeValue(subband));
            }


            item = htmlHelper.GetPropertyFromHtml(@"name=""bandwidth""", "/>", "value", html);
            if (!string.IsNullOrEmpty(item))
            {
                resultDic.Add("bandwidth", EncodeValue(item));
            }

            items = new string[] { "ismax", "ispppoewlanmax", "ispppoephsmax" };
            foreach (string name in items)
            {
                string startStr = string.Format(@"name=""{0}""", name);
                string hidenItem = htmlHelper.GetPropertyFromHtml(startStr, ">", "value", html);
                if (!string.IsNullOrEmpty(hidenItem))
                {
                    resultDic.Add(name, EncodeValue(hidenItem));
                }
            }
            resultDic.Add("xvccontrol", "2");
            resultDic.Add("xvcdomain_1", EncodeValue("JH-JH-CN-1.domain"));

            items = new string[] { "xvcxpi_1", "xvcxci_1" };
            foreach (string name in items)
            {
                string startStr = string.Format(@"name=""{0}""", name);
                string hidenItem = htmlHelper.GetPropertyFromHtml(startStr, @""">", "value", html);
                if (!string.IsNullOrEmpty(hidenItem))
                {
                    resultDic.Add(name, EncodeValue(hidenItem));
                }
            }

            string macStart = @"name=""mac_{0}""";
            int macCount = 0;
            for (int j = 1; j < 15; j++)
            {
                string itemStart = string.Format(macStart, j);
                string mac = htmlHelper.GetSubString(html, itemStart, ">");
                if (string.IsNullOrEmpty(mac))
                {
                    continue;
                }
                string valueProp = "value=";
                mac = mac.Substring(mac.IndexOf(valueProp) + valueProp.Length);
                string macCheck = "macck_{0}";
                string macAddr = "mac_{0}";
                resultDic.Add(string.Format(macCheck, j), "on");
                resultDic.Add(string.Format(macAddr, j), EncodeValue(mac));
                // html = html.Replace(itemStart, "");
                macCount++;
            }
            if (macCount <= 0)
            {
                RaiseChanged(string.Format("---> 无法获取：{0} 下MAC地址列表,可能已经解绑", currentAccont));
                return string.Empty;
            }
            items = new string[] { "xvcitemno", "macitemno", "cliditemno" };
            foreach (string name in items)
            {
                string startStr = string.Format(@"name=""{0}"" value=""", name);
                string hidenItem = htmlHelper.GetPropertyFromHtml(startStr, ">", "value", html);
                if (!string.IsNullOrEmpty(hidenItem))
                {
                    resultDic.Add(name, EncodeValue(hidenItem));
                }
            }
            resultDic.Add("how", EncodeValue("deletemac"));
            item = htmlHelper.GetPropertyFromHtml(@"name=""xtoken""", @"/>", "value", html);
            if (!string.IsNullOrEmpty(item))
            {
                resultDic.Add("xtoken", EncodeValue(item));
            }
            else
            {
                RaiseError("获取 xtoken 失败", html);
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var kv in resultDic)
            {
                if (i++ < resultDic.Count - 1)
                {
                    sb.AppendFormat("{0}={1}&", kv.Key, kv.Value);
                }
                else
                { sb.AppendFormat("{0}={1}", kv.Key, kv.Value); }
            }
            string result = sb.ToString();
            return result;
        }



        private string GetTestHtml(string filePath)
        {
            using (StreamReader sr = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read), Encoding.GetEncoding("GBK")))
            {
                string result = sr.ReadToEnd();
                return result;
            }
        }

    }

}
