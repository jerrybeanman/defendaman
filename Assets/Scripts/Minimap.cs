using UnityEngine;
using System.Collections;

public class Minimap : MonoBehaviour {

    public float mini_map_size = 2.0f;
    public float offset_x = 10.0f;
    public float offset_y = 10.0f;
    float new_size;

    public Texture border_texture;
    public Camera mini_map_camera;
	
	// Update is called once per frame
	void Update () {
	    
        if (mini_map_camera == null)
        {
            return;
        }

        new_size = Mathf.RoundToInt(Screen.width / 10);
        mini_map_camera.pixelRect = new Rect(offset_x, (Screen.height - (mini_map_size * new_size)) - offset_y, mini_map_size * new_size, mini_map_size * new_size);
	}

    void OnGUI()
    {
        if (border_texture != null)
        {
            mini_map_camera.Render();
            GUI.DrawTexture(new Rect(offset_x, offset_y, mini_map_size * new_size, mini_map_size * new_size), border_texture);
        }
    }
}
