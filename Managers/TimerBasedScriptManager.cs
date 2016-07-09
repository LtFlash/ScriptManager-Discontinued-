﻿using System;
using Rage;

namespace ScriptManager.Managers
{
    public class TimerBasedScriptManager : ScriptManagerBase
    {
        private System.Timers.Timer _scriptRunTimer = new System.Timers.Timer();
        private double _intervalMax = 3 * 60 * 1000;
        private double _intervalMin = 1 * 60 * 1000;

        public TimerBasedScriptManager(bool autoStart = false, double intervalMax = 180000, double intervalMin = 60000) : base()
        {
            _intervalMax = intervalMax;
            _intervalMin = intervalMin;

            if (autoStart) StartTimer();
        }

        public void StartTimer()
        {
            _scriptRunTimer.Interval = GetRandomTimerInterval();
            _scriptRunTimer.Elapsed += StartNextScript;
            _scriptRunTimer.AutoReset = true;
            _scriptRunTimer.Start();
        }

        private double GetRandomTimerInterval()
        {
            return MathHelper.GetRandomDouble(_intervalMin, _intervalMax);
        }

        private void StartNextScript(object sender, System.Timers.ElapsedEventArgs e)
        {
            _scriptRunTimer.Interval = GetRandomTimerInterval(); 

            //check if any script is running
            if (IsAnyScriptRunning()) return;

            ScriptStatus _scriptToRun = GetNextScriptReadyToRun();
            Game.LogVerbose("_script to run null: " + (_scriptToRun == null));

            //start new script inside a GameFiber of the main loop
            _scriptToRunInFiber = _scriptToRun;
             
            Game.LogVerbose("ScriptManager.StartNextScript | interval: " + _scriptRunTimer.Interval);
        }

        private ScriptStatus GetNextScriptReadyToRun()
        {
            //FindLastIndex returns -1 when no id found
            int _idLastScript = _scripts.FindLastIndex(s => s.FinishedSuccessfully);
            Game.LogVerbose("_idLastScript: " + _idLastScript);

            //no index found
            if (_idLastScript == -1) return _scripts.Count > 0 ? _scripts[0] : null;
            //last script on the list
            if (_scripts.Count - 1 == _idLastScript) return null;
            int _idNextScriptToRun = _scripts[_idLastScript].FinishedSuccessfully ? _idLastScript + 1 : _idLastScript;
            if (_scripts.Count - 1 >= _idNextScriptToRun) return _scripts[_idNextScriptToRun];
            else return null;
        } 

        private bool IsAnyScriptRunning()
        {
            int noOfRunningScripts = _scripts.FindAll(s => s.IsRunning).Count;
            Game.LogVerbose("IsAnyScriptRunning.Count: " + noOfRunningScripts);
            return noOfRunningScripts > 0;
        }
    }
}