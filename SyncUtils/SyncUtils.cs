﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncUtils
{
    public static class SyncUtils
    {

        public static void Wait(object mlock, object condition, int timeout)
        {
            if (mlock == condition)
            {
                Monitor.Wait(mlock, timeout);
                return;
            }
            bool wasInterrupted;
            Monitor.Enter(condition);
            Monitor.Exit(mlock);
            try
            {
                Monitor.Wait(condition, timeout);
            }
            catch (Exception)
            {
                Monitor.Exit(condition);
                EnterUninterruptibly(mlock, out wasInterrupted);
                throw;
            }
            Monitor.Exit(condition);
            EnterUninterruptibly(mlock, out wasInterrupted);
            if (wasInterrupted)
            {
                throw new ThreadInterruptedException();
            }
        }

        public static void Pulse(object mlock, object condition)
        {
            if (mlock == condition)
            {
                Monitor.Pulse(mlock);
                return;
            }
            bool wasInterrupted;
            EnterUninterruptibly(condition, out wasInterrupted);
            try
            {
                Monitor.Pulse(condition);
            }
            finally
            {
                Monitor.Exit(condition);
                if (wasInterrupted)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

        public static void PulseAll(object mlock, object condition)
        {
            if (mlock == condition)
            {
                Monitor.PulseAll(mlock);
                return;
            }
            bool wasInterrupted;
            EnterUninterruptibly(condition, out wasInterrupted);
            try
            {
                Monitor.PulseAll(condition);
            }
            finally
            {
                Monitor.Exit(condition);
                if (wasInterrupted)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

        public static void EnterUninterruptibly(object mon, out bool wasInterrupted)
        {
            wasInterrupted = false;
            while (true)
            {
                try
                {
                    Monitor.Enter(mon);
                    return;
                }
                catch (ThreadInterruptedException e)
                {
                    wasInterrupted = true;
                }
            }
        }
    }
}
