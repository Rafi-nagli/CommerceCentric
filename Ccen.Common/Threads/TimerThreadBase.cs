using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.Common.Helpers;
using Amazon.Common.Services;
using Amazon.Core.Contracts;
using Amazon.Core.Models.SystemMessages;
using log4net;

namespace Amazon.Common.Threads
{
    public class TimerThreadBase : IThread
    {
        private Timer _timer;
        protected readonly string _threadName;

        private ThreadCmd _threadCmd;
        protected ThreadPriority ThreadPriority = ThreadPriority.Lowest;

        private readonly ILogService _logger;
        private ITime _time;
        private ISystemMessageService _messageService;

        protected readonly long CompanyId;
        private IList<TimeSpan> _callTimeList;

        public TimerThreadBase(string threadName, 
            long companyId, 
            ISystemMessageService messageService,
            IList<TimeSpan> callTimeList,
            ITime time,
            ILogService overrideLogger = null)
        {
            _threadName = threadName;
            _callTimeList = callTimeList ?? new List<TimeSpan>();
            CompanyId = companyId;

            _time = time;
            _logger = overrideLogger ?? LogFactory.From(threadName);
            _messageService = messageService;
            LogWrite("Construct, companyId=" + companyId + ", _callTimes=" + String.Join(",", _callTimeList.Select(t => t.ToString()).ToList()));
        }

        public TimerThreadBase(string threadName,
            long companyId,
            IList<TimeSpan> callTimeList,
            ITime time,
            ILogService overrideLogger = null)
        {
            _threadName = threadName;
            _callTimeList = callTimeList ?? new List<TimeSpan>();
            CompanyId = companyId;

            _time = time;
            _logger = overrideLogger ?? LogFactory.From(threadName);
            LogWrite("Construct, companyId=" + companyId + ", _callTimes=" + String.Join(",", _callTimeList.Select(t => t.ToString()).ToList()));
        }

        public void Start()
        {
            LogWrite("Start");

            try
            {
                Init();
            }
            catch (Exception ex)
            {
                LogError("Start->Init", ex);
            }

            SetTimer(); //NOTE: after init, may call callback while Init in progress
            _threadCmd = ThreadCmd.Run;
        }

        private void SetTimer()
        {
            if (_callTimeList == null || !_callTimeList.Any())
                return;

            var nextLaunch = GetNextCallTime();
            _logger.Info("Set launch after period=" + nextLaunch);
            if (_timer != null)
                _timer.Change((int)nextLaunch.TotalMilliseconds, Timeout.Infinite);
            else
                _timer = new Timer(state => { Run(); }, null, (int)nextLaunch.TotalMilliseconds, Timeout.Infinite);
        }

        private TimeSpan GetNextCallTime()
        {
            var now = _time.GetAppNowTime(); 
            var nowTime = now.TimeOfDay;
            var times = _callTimeList.OrderBy(t => t).ToList();

            var todayCalls = times.Where(t => t > nowTime.Add(TimeSpan.FromMinutes(1))).ToList(); //NOTE: Threashold 1 minute
            TimeSpan? callTime = todayCalls.Count > 0 ? todayCalls[0] : (TimeSpan?)null;
            if (callTime.HasValue)
            {
                return callTime.Value - nowTime;
            }
            //NOTE: get min Call Time on the next day
            return times.First().Add(TimeSpan.FromDays(1)) - nowTime;
        }

        public void Join(TimeSpan time)
        {
            LogWrite("Join");
            if (_timer != null)
            {
                _threadCmd = ThreadCmd.Stop;
                try
                {
                    _timer.Dispose();
                }
                catch (Exception ex)
                {
                    LogError("Join", ex);
                }
                _timer = null;
            }
        }

        public void Stop()
        {
            try
            {
                Finish();
            }
            catch (Exception ex)
            {
                LogError("Finish", ex);
            }

            LogWrite("Stop");
            Join(TimeSpan.FromSeconds(20));
        }

        public void Run()
        {
            if (_threadCmd != ThreadCmd.Stop)
            {
                try
                {
                    LogWrite("RunCallback begin");
                    RunCallback();
                    LogWrite("RunCallback end");

                    if (_messageService != null)
                        _messageService.AddOrUpdate("ServiceStatus", _threadName, "Success", null, Core.Models.Calls.MessageStatus.Success);
                }
                catch (Exception ex)
                {
                    _logger.Info("Run", ex);

                    if (_messageService != null)
                        _messageService.AddOrUpdate("ServiceStatus",
                            _threadName,
                            "Service processing error",
                            new ExceptionMessageData()
                            {
                                Message = ExceptionHelper.GetAllMessages(ex)
                            },
                            Core.Models.Calls.MessageStatus.Error);
                }

                SetTimer();
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
