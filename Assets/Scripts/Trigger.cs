using UnityEngine;
using System.Collections;
using SimpleJSON;

public abstract class Trigger : MonoBehaviour
{
    //player ID
    public int playerID;
    public int teamID;
    public float damage;
    public bool IsDestroyable = true;
    public static int currentTriggerID = 1;
    public int triggerID;

    public Trigger()
    {
        triggerID = currentTriggerID++;
        NetworkingManager.Subscribe(killed, DataType.TriggerKilled, triggerID);
    }

    void killed(JSONClass packet)
    {
        NetworkingManager.Unsubscribe(DataType.TriggerKilled, triggerID);
        Destroy(gameObject);
    }

    protected abstract void OnTriggerEnter2D(Collider2D other);
}
