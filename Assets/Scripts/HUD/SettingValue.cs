using UnityEngine;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class SettingValue : MonoBehaviour {
    public static SettingValue setting;

    public int movementType;
    public float volume;

	// Use this for initialization
	void Awake () {
        if (setting == null)
        {
            DontDestroyOnLoad(gameObject);
            setting = this;
        }
        else if(setting != this)
        {
            Destroy(gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/SettingInfo.dat", FileMode.Open);

        SettingData data = new SettingData();
        data.movementType = movementType;
        data.volume = volume;

        bf.Serialize(file, data);
        file.Close();

    }
    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/SettingInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/SettingInfo.dat", FileMode.Open);
            SettingData data = (SettingData)bf.Deserialize(file);
            file.Close();

            movementType = data.movementType;
            volume = data.volume;
        }

    }
}

[System.Serializable]
class SettingData
{
    public int movementType;
    public float volume;
}
