
using System;
using System.Collections.Generic;
using UnityEngine;
/// An event that happens only once and can be listened to. 
/// Any listener that subscribes after it has completed will be called immediatelly.
/// Relies on corrutines, therefore requires a monobehaviour to create it.
public class TriggerEvent
{
    public bool HasFinished
    {
        get;
        private set;
    }

    private bool _hasTimedOut;
    private List<Action> _listeners = new List<Action>();
    private Coroutine _timeOutCoroutine;
    private Action _onTimeOut;
    private Action _onSuccess;
    
    public TriggerEvent(MonoBehaviour owner, Action onSuccess = null, float timeOutSeconds = 0, Action onTimeOut = null) {
        _listeners = new List<Action>();        
        _onSuccess = onSuccess;

        if(timeOutSeconds > 0) {
            _onTimeOut = onTimeOut;
            _timeOutCoroutine = CoroutineWait.ForSeconds(owner, timeOutSeconds, OnTimeout);     
        }
    }

    public void AddListener(Action callback) {
        if(HasFinished) {
            callback();
        }
        else {
            _listeners.Add(callback);
        }      
    }

    public void Invoke() {
        if(!HasFinished) { 
            if(!_hasTimedOut) {
                _onSuccess?.Invoke();
            }

            HasFinished = true;
            _timeOutCoroutine = null;

            for(int i=0; i < _listeners.Count; i++) {
                _listeners[i].Invoke();
            }

            _listeners = new List<Action>();
        }
    }

    private void OnTimeout() { 
        _hasTimedOut = true;
        _onTimeOut?.Invoke();      
        Invoke();
    }
}