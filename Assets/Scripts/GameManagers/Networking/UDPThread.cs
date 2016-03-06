using UnityEngine;
using System;

public class UDPThread : ThreadedJob {
    Action<string> callback;

    public UDPThread(Action<string> callback) {
        this.callback = callback;
    }

    protected override void ThreadFunction() {
        // Do your threaded task. DON'T use the Unity API here
        for (int i = 0; i < 1000000000; i++)
            //for (int j = 0; j < 1000000000; j++)
                //for (int k = 0; k < 1000000000; k++)
                    //for (int l = 0; l < 1000000000; l++)
                        ;
    }

    public override void OnFinished() {
        // This is executed by the Unity main thread when the job is finished
        Debug.Log("Finished!");
        callback("[udpdata]");
    }
}