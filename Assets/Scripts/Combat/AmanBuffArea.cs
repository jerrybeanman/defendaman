using UnityEngine;
using System.Collections;
using System;

public class AmanBuffArea : Area
{
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        var target = other.GetComponent<BaseClass>();
        if (target != null && teamID == target.team)
        {
            var buff = other.gameObject.GetComponent<AmanTeamBuff>();
            if (buff != null)
            {
                buff.duration = 75;
            }
            else
            {
                other.gameObject.AddComponent<AmanTeamBuff>();
            }
        }
    }
}
