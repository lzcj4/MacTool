
using log4net;
using log4net.Config;
namespace PublicUtilities
{

    public enum GameType
    {
        [Comment("魔兽")]
        Wow,
        [Comment("裂隙")]
        Rift,
        [Comment("江湖")]
        RS
    }

    public delegate void LogEventHandle(string log);

    public enum LoggerType
    { Debug, Error, Fatal, Info, Warn }

    public abstract class LogManagerBase
    {
        protected ILog gameLog = null;
        public event LogEventHandle LogEvent = null;

        static LogManagerBase()
        {
            XmlConfigurator.Configure();
        }

        public virtual void Log(LoggerType type, string text)
        {
            switch (type)
            {
                case LoggerType.Debug:
                    this.Debug(text);
                    break;
                case LoggerType.Error:
                    this.Error(text);
                    break;
                case LoggerType.Fatal:
                    this.Fatal(text);
                    break;
                case LoggerType.Info:
                    this.Info(text);
                    break;
                case LoggerType.Warn:
                    this.Warn(text);
                    break;
            }
        }

        public virtual void Debug(string log)
        {
            if (this.gameLog.IsDebugEnabled)
            {
                this.gameLog.Debug(log);
            }
        }
        public virtual void Error(string text)
        {
            if (this.gameLog.IsErrorEnabled)
            {
                this.gameLog.Error(text);
            }
        }
        public virtual void Fatal(string text)
        {
            if (this.gameLog.IsFatalEnabled)
            {
                this.gameLog.Fatal(text);
            }
        }
        public virtual void Info(string text)
        {
            if (this.gameLog.IsInfoEnabled)
            {
                this.gameLog.Info(text);
            }
        }
        public virtual void Warn(string text)
        {
            if (this.gameLog.IsWarnEnabled)
            {
                this.gameLog.Warn(text);
            }
        }

        public virtual void InfoWithCallback(string text)
        {
            Info(text);
            OnLogEvent(text);
        }

        public virtual void ErrorWithCallback(string text)
        {
            Error(text);
            OnLogEvent(text);
        }

        private void OnLogEvent(string log)
        {
            if (null != this.LogEvent)
            {
                this.LogEvent(log);
            }
        }
    }

    /// <summary>
    /// 魔兽日志
    /// </summary>
    public class WowLogManager : LogManagerBase
    {
        public readonly static WowLogManager Instance = new WowLogManager();
        public WowLogManager()
        {
            gameLog = log4net.LogManager.GetLogger("WowLog");
        }
    }

    /// <summary>
    /// 裂隙日志
    /// </summary>
    public class RiftLogManager : LogManagerBase
    {
        public readonly static RiftLogManager Instance = new RiftLogManager();
        public RiftLogManager()
        {
            gameLog = log4net.LogManager.GetLogger("RiftLog");
        }
    }

    /// <summary>
    /// 江湖日志
    /// </summary>
    public class RSLogManager : LogManagerBase
    {
        public readonly static RSLogManager Instance = new RSLogManager();
        public RSLogManager()
        {
            gameLog = log4net.LogManager.GetLogger("RSLog");
        }
    }


    /// <summary>
    /// 苹果日志
    /// </summary>
    public class AppleLogManager : LogManagerBase
    {
        public readonly static AppleLogManager Instance = new AppleLogManager();
        public AppleLogManager()
        {
            gameLog = log4net.LogManager.GetLogger("AppleLog");
        }
    }



    /// <summary>
    /// XBOX日志
    /// </summary>
    public class XBOXLogManager : LogManagerBase
    {
        public readonly static XBOXLogManager Instance = new XBOXLogManager();
        public XBOXLogManager()
        {
            gameLog = log4net.LogManager.GetLogger("XBOXLog");
        }
    }

    /// <summary>
    /// Figth日志
    /// </summary>
    public class FightLogManager : LogManagerBase
    {
        public readonly static FightLogManager Instance = new FightLogManager();
        public FightLogManager()
        {
            gameLog = log4net.LogManager.GetLogger("FightLog");
        }
    }

    /// <summary>
    /// 所有游戏打印日志
    /// </summary>
    public class GameLogManager : LogManagerBase
    {
        public static GameLogManager Instance = new GameLogManager();
        public override void Debug(string log)
        {
            WowLogManager.Instance.Debug(log);
            RiftLogManager.Instance.Debug(log);
            RSLogManager.Instance.Debug(log);
        }
        public override void Error(string log)
        {
            WowLogManager.Instance.Error(log);
            RiftLogManager.Instance.Error(log);
            RSLogManager.Instance.Error(log);
        }
        public override void Fatal(string log)
        {
            WowLogManager.Instance.Fatal(log);
            RiftLogManager.Instance.Fatal(log);
            RSLogManager.Instance.Fatal(log);
        }
        public override void Info(string log)
        {
            WowLogManager.Instance.Info(log);
            RiftLogManager.Instance.Info(log);
            RSLogManager.Instance.Info(log);
        }
        public override void Warn(string log)
        {
            WowLogManager.Instance.Warn(log);
            RiftLogManager.Instance.Warn(log);
            RSLogManager.Instance.Warn(log);
        }

        public override void InfoWithCallback(string log)
        {
            WowLogManager.Instance.InfoWithCallback(log);
            RiftLogManager.Instance.InfoWithCallback(log);
            RSLogManager.Instance.InfoWithCallback(log);
        }

        public override void ErrorWithCallback(string log)
        {
            WowLogManager.Instance.ErrorWithCallback(log);
            RiftLogManager.Instance.ErrorWithCallback(log);
            RSLogManager.Instance.ErrorWithCallback(log);
        }
    }
}
