using System;
using System.Threading;

namespace DBackup.Services
{
    public class SchedulerService
    {
        private Thread _backupThread;
        private bool _stopThread = false;
        private Action _tickAction;

        public void Start(Action tickAction)
        {
            _tickAction = tickAction;
            _backupThread = new Thread(ThreadLoop)
            {
                IsBackground = true
            };
            _backupThread.Start();
        }

        private void ThreadLoop()
        {
            while (!_stopThread)
            {
                try
                {
                    _tickAction?.Invoke();
                }
                catch (Exception)
                {
                    //
                }
                Thread.Sleep(60000);
            }
        }

        public void Stop()
        {
            _stopThread = true;
            if (_backupThread != null && _backupThread.IsAlive)
            {
                _backupThread.Join(1000);
            }
        }
    }
}