using System;
using System.ServiceModel;
using System.Threading;
using Amazon.Common.Models;
using Amazon.Core.Contracts;
using Amazon.Core.Exceptions;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using log4net;

namespace Amazon.Common.Helpers
{
    public static class RetryHelper
    {
        public static CallResult<T> ActionWithRetriesWithCallResult<T>(Func<T> action,
            ILogService logger,
            int retryCount = 3,
            int pauseMillesecond = 1000,
            RetryModeType retryMode = RetryModeType.None)
        {
            if (retryMode == RetryModeType.Fast)
            {
                pauseMillesecond = 1000;
                retryCount = 2;
            }
            
            var result = new RetriesResult<T>();
            var random = new Random(DateTime.Now.Millisecond);

            int pauseTime = pauseMillesecond;
            int attemptNumber = 0;
            bool isConversationOutOfSync = false;
            bool stopRetrying = false;

            //NOTE: Only conversation out of sync going repeats more 3 times
            //TODO: go to refactor, move to settings
            while (!stopRetrying &&
                ((isConversationOutOfSync && attemptNumber < retryCount)
                || (!isConversationOutOfSync && attemptNumber < retryCount)))
            {
                try
                {
                    return new CallResult<T>()
                    {
                        Data = action(),
                        Status = CallStatus.Success
                    };
                }
                catch (Exception ex)
                {
                    if (ex is FaultException || ex is StampsException)
                    {
                        //System.ServiceModel.FaultException: Conversation out-of-sync.
                        //System.ServiceModel.FaultException: Invalid conversation token.
                        //System.ServiceModel.FaultException: Invalid SOAP message due to XML Schema validation failure...
                        isConversationOutOfSync = ExceptionHelper.IsStampsConversationSyncEx(ex);// ex.Message.Contains("out-of-sync") || ex.Message.Contains("conversation token");
                        stopRetrying = ex.Message.Contains("Invalid SOAP message due to XML Schema validation failure");
                    }

                    result.LastAttemptException = ex;
                    logger.Error("Retry attempt failed, attempt number=" + attemptNumber
                        + ", pause time=" + pauseTime
                        + ", isConversationOutOfSync=" + isConversationOutOfSync
                        + ", stopRetrying=" + stopRetrying, ex);

                    if (retryMode == RetryModeType.Random)
                        Thread.Sleep(pauseTime
                            + (random.Next(0, 3) - 1) /* [-1, 0, 1] */
                            * pauseTime / 2);
                    else
                        Thread.Sleep(pauseTime);

                    pauseTime = UpdatePauseTime(pauseTime, retryMode);
                }

                attemptNumber++;
            }

            //NOTE: gets here unless have passed all attempts
            var exception = result.LastAttemptException ?? new Exception("All attempts fails");

            return new CallResult<T>
            {
                Data = default(T),
                Exception = exception,
                Message = exception.Message,
                Status = CallStatus.Fail
            };
        }

        public static void ActionWithRetries(Action action,
            ILogService logger,
            int retryCount = 3,
            int pauseMillesecond = 1000,
            RetryModeType retryMode = RetryModeType.None,
            bool throwException = false)
        {
            ActionWithRetries<bool>(() => { action(); return true; }, logger, retryCount, pauseMillesecond, retryMode, throwException);
        }

        public static T ActionWithRetries<T>(Func<T> action, 
            ILogService logger, 
            int retryCount = 3, 
            int pauseMillesecond = 1000, 
            RetryModeType retryMode = RetryModeType.None, 
            bool throwException = false)
        {
            if (retryMode == RetryModeType.Fast)
            {
                pauseMillesecond = 1000;
                retryCount = 2;
            }

            var result = new RetriesResult<T>();
            var random = new Random(DateTime.Now.Millisecond);

            int pauseTime = pauseMillesecond;
            int attemptNumber = 0;
            bool isConversationOutOfSync = false;
            bool stopRetrying = false;

            //NOTE: Only conversation out of sync going repeats more 3 times
            //TODO: go to refactor, move to settings
            while (!stopRetrying &&
                ((isConversationOutOfSync && attemptNumber < retryCount)
                || (!isConversationOutOfSync && attemptNumber < retryCount)))
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    if (ex is FaultException)
                    {
                        //System.ServiceModel.FaultException: Conversation out-of-sync.
                        //System.ServiceModel.FaultException: Invalid conversation token.
                        //System.ServiceModel.FaultException: Invalid SOAP message due to XML Schema validation failure...
                        isConversationOutOfSync = ExceptionHelper.IsStampsConversationSyncEx(ex);// ex.Message.Contains("out-of-sync") || ex.Message.Contains("conversation token");
                        stopRetrying = ex.Message.Contains("Invalid SOAP message due to XML Schema validation failure");
                    }

                    result.LastAttemptException = ex;
                    logger.Error("Retry attempt failed, attempt number=" + attemptNumber 
                        + ", pause time=" + pauseTime 
                        + ", isConversationOutOfSync=" + isConversationOutOfSync
                        + ", stopRetrying=" + stopRetrying, ex);

                    if (retryMode == RetryModeType.Random)
                        Thread.Sleep(pauseTime 
                            + (random.Next(0, 3) - 1) /* [-1, 0, 1] */ 
                            * pauseTime/2);
                    else
                        Thread.Sleep(pauseTime);

                    pauseTime = UpdatePauseTime(pauseTime, retryMode);
                }

                attemptNumber++;
            }

            //NOTE: gets here unless have passed all attempts
            if (throwException)
            {
                if (result.LastAttemptException != null)
                    throw result.LastAttemptException;
                throw new Exception("All attempts fails");
            }

            return default(T);
        }

        private static int UpdatePauseTime(int pauseTime, RetryModeType retryMode)
        {
            switch (retryMode)
            {
                case RetryModeType.Progressive:
                    pauseTime = (int)Math.Round(pauseTime*1.3M);
                    if (pauseTime > 15000)
                        pauseTime = 15000;
                    break;
            }
            return pauseTime;
        }

        static public T TryOrDefault<T>(Func<T> func, T defaultValue)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                return defaultValue;
            }
        }
    }
}
