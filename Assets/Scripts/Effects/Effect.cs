using UnityEngine;
using System.Collections;

public abstract class Effect : MonoBehaviour
{
    public int source;  //source player ID
    public BaseClass player;
    public int magnitude;
    public int duration;

    public Effect()
    {
        player = gameObject.GetComponent<BaseClass>();
    }
    
    protected virtual void FixedUpdate()
    {
        if(--duration < 0)
        {
            Destroy(this);
        }
    }
}
