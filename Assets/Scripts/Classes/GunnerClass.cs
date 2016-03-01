using UnityEngine;
using System.Collections;

public class GunnerClass : BaseClass
{

    public GunnerClass()
	{
        this._className = "Gunner";
        this._classDescription = "Test class Gunner";
        this._classStat.CurrentHp = 50;
        this._classStat.MaxHp = 50;

        //placeholder numbers
        this._classStat.MoveSpeed = 5;
        this._classStat.AtkPower = 20;
    }
} 
