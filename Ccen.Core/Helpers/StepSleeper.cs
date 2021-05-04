using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Amazon.Core.Models
{
    public class StepSleeper
    {
        private int _currentStep = 0;
        private int _stepBetweenSleep = 0;
        private TimeSpan _sleepTime;

        public StepSleeper(TimeSpan sleepTime, int numberOfStepBetweenSleeps)
        {
            _sleepTime = sleepTime;
            _stepBetweenSleep = numberOfStepBetweenSleeps;
        }

        public void NextStep()
        {
            _currentStep ++;
            if (_currentStep%_stepBetweenSleep == 0)
            {
                Thread.Sleep(_sleepTime);
            }
        }
    }
}
