using System;
using System.Threading;
using Amazon.Common.Helpers;
using Amazon.Common.Services;
using Amazon.Core.Contracts;
using Amazon.Core.Models.SystemMessages;
using log4net;

namespace Amazon.Common.Threads
{
    public class ThreadBase : IThread
    {
        private Thread _thread;
        private readonly ManualResetEvent _eventState = new ManualResetEvent(false);
        protected readonly string _threadName;
        protected readonly long CompanyId;
        private TimeSpan? _callbackInterval;
        private ThreadCmd _threadCmd;
        private readonly ILogService _logger;
        private IEmailService _emailService;
        private ISystemMessageService _messageService;
        protected ThreadPriority ThreadPriority = ThreadPriority.Lowest;

        public ThreadBase(string threadName, 
            long companyId, 
            ISystemMessageService messageService,
            TimeSpan? callbackInterval = null,
            ILogService overrideLogger = null,
            IEmailService emailService = null)
        {
            _threadName = threadName;
            _callbackInterval = callbackInterval;
            CompanyId = companyId;
            
            _logger = overrideLogger ?? LogFactory.From(threadName);
            _emailService = emailService;
            _messageService = messageService;
            LogWrite("Construct, companyId=" + companyId + ", _callbackInterval=" + callbackInterval.ToString());
        }

        public ThreadBase(string threadName,
            long companyId,
            TimeSpan? callbackInterval = null,
            ILogService overrideLogger = null,
            IEmailService emailService = null)
        {
            _threadName = threadName;
            _callbackInterval = callbackInterval;
            CompanyId = companyId;

            _logger = overrideLogger ?? LogFactory.From(threadName);
            _emailService = emailService;
            LogWrite("Construct, companyId=" + companyId + ", _callbackInterval=" + callbackInterval.ToString());
        }

        public void Start()
        {
            LogWrite("Start");
            if (_thread != null && _thread.IsAlive)
                Stop();

            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken = _cancellationTokenSource.Token;

            _threadCmd = ThreadCmd.Run;
            _thread = new Thread(Run);
            _thread.Priority = ThreadPriority;
            _thread.IsBackground = true;
            _thread.Start();
        }

        protected CancellationToken CancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsCancelledRequested
        {
            get { return CancellationToken != null && CancellationToken.IsCancellationRequested; }
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Join(TimeSpan time)
        {
            LogWrite("Join");
            if (_thread != null && _thread.IsAlive)
            {
                _threadCmd = ThreadCmd.Stop;
                _eventState.Set();
                try
                {
                    if (!_thread.Join(time))
                    {
                        _thread.Abort();
                        LogWrite("Abort");
                    }
                    else
                    {
                        LogWrite("Join");
                    }
                }
                catch (Exception ex)
                {
                    LogWrite("Join", ex);
                }
            }
        }

        public void Stop()
        {
            LogWrite("Stop");
            Cancel();
            Join(TimeSpan.FromSeconds(20));

            _cancellationTokenSource.Dispose();
        }

        protected void Sleep(TimeSpan span)
        {
            LogWrite("Thread event sleeping time: " + span.Minutes.ToString("00") + ":" + span.Seconds.ToString("00"));
            ThreadHelper.Sleep(_eventState, span);
        }

        protected void Awake()
        {
            _eventState.Set();
        }

        public void Run()
        {
            try
            {
                Init();

                while (_threadCmd != ThreadCmd.Stop)
                {
                    try
                    {
                        //LogWrite("RunCallback begin");
                        RunCallback();
                        //LogWrite("RunCallback end");

                        if (_messageService != null)
                            _messageService.AddOrUpdate("ServiceStatus", _threadName, "Success", null, Core.Models.Calls.MessageStatus.Success);
                    }
                    catch (Exception ex)
                    {                        
                        _logger.Fatal("Run." + _threadName, ex);

                        if (_messageService != null)
                            _messageService.AddOrUpdate("ServiceStatus", 
                                _threadName, 
                                "Service processing error", 
                                new ExceptionMessageData()
                                {
                                    Message = ExceptionHelper.GetAllMessages(ex)
                                },
                                Core.Models.Calls.MessageStatus.Error);

                        if (_emailService != null)
                            _emailService.SendSystemEmailToAdmin("Fail." + _threadName, "Details: " + ExceptionHelper.GetAllMessages(ex));
                    }
                    if (_callbackInterval.HasValue)
                        _eventState.WaitOne(_callbackInterval.Value);
                }
            }
            finally
            {
                Finish();
            }
        }

        virtual protected void Init()
        {

        }

        virtual protected void Finish()
        {

        }

        virtual protected void RunCallback()
        {

        }
        
        #region Logger
        protected ILogService GetLogger()
        {
            return _logger;
        }

        protected void LogWrite(string message)
        {
            _logger.Info(message);
        }

        protected void LogWrite(string message, Exception ex)
        {
            _logger.Info(message, ex);
        }

        protected void LogDebug(string message)
        {
            _logger.Debug(message);
        }

        protected void LogError(string message, Exception ex)
        {
            _logger.Error(message, ex);
        }
        #endregion
    }
}
