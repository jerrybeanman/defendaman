using UnityEngine;
using System;

public class TCPThread : ThreadedJob {
    Action<string> callback;

    public TCPThread(Action<string> callback) {
        this.callback = callback;
    }

    protected override void ThreadFunction() {
        // Do your threaded task. DON'T use the Unity API here
        for (int i = 0; i < 1000000000; i++)
                    ;
    }

    public override void OnFinished() {
        // This is executed by the Unity main thread when the job is finished
        Debug.Log("Finished!");
        callback("[tcpdata]");
    }
}