using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class UDP : MonoBehaviour {
    public Action<string> callback;

    UDPThread thread;

    void Start() {
        thread = new UDPThread(callback);
        thread.Start();
        Debug.Log("UDP-Start: Created thread");
    }

    void Update() {
        if (callback == null)
            return;
        if (thread != null && !thread.IsDone) {
            thread.Update();
        }
        if (thread.IsDone) {
            thread.OnFinished();
            Debug.Log("UDP-Update: Thread is done. Starting another");
            thread = new UDPThread(callback);
            thread.Start();
        }
    }

    void Terminate() {
        callback = null;
    }
}