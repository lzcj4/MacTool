using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NI = System.Net.NetworkInformation;

namespace PublicUtilities
{
    public static class CmdHelper
    {
        public static bool Ping163()
        {
            return Ping("www.163.com");
        }

        public static bool PingBaidu()
        {
            return Ping("www.baidu.com");
        }

        public static bool Ping(string IP)
        {
            //string content = RunCmd("Ping.exe", IP);
            //bool isPinged = TextHelper.IsContains(content, "TTL=", "MS", "Reply");
            //return isPinged;

            using (NI.Ping ping = new NI.Ping())
            {
                try
                {
                    NI.PingReply pr = ping.Send(IP);
                    return pr.Status == NI.IPStatus.Success;
                }
                catch (System.Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Run command list in cmd.exe
        /// </summary>
        /// <param name="cmdList">commands</param>
        public static void RunCmd(IList<string> cmdList, LogManagerBase logManager)
        {
            Process p = new Process();
            p.StartInfo.FileName = @"cmd.exe";

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;

            p.Start();
            foreach (string cmd in cmdList)
            {
                p.StandardInput.WriteLine(cmd);
                //string content = p.StandardOutput.ReadToEnd();
                //string errmsg = p.StandardError.ReadToEnd();
            }

            p.StandardInput.WriteLine("exit");
            string content = p.StandardOutput.ReadToEnd();
            string errmsg = p.StandardError.ReadToEnd();
            logManager.Info(string.Format("重拔ADSL结果：{0}", content));
            p.WaitForExit();
            p.Close();
        }

        public static string RunCmd(string cmd, string args)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = cmd;
            if (!string.IsNullOrEmpty(args))
            {
                proc.StartInfo.Arguments = args;
            }
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();

            string content = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            proc.Close();
            return content;
        }
    }
}