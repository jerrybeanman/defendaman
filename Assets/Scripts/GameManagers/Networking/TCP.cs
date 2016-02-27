using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class TCP : MonoBehaviour {
    public Action<string> callback;

    TCPThread thread;

    void Start() {
        thread = new TCPThread(callback);
        thread.Start();
        
    }

    void Update() {
        if (callback == null)
            return;
        if (thread != null && !thread.IsDone)
            thread.Update();
        if (thread.IsDone)
        {
            thread.OnFinished();
            Debug.Log("TCP-Update: Thread is done. Starting another");
            thread = new TCPThread(callback);
            thread.Start();
        }
    }

    void Terminate() {
        callback = null;
    }
}