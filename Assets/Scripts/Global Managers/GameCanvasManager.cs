using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvasManager : MonoBehaviour {
    
    public static GameCanvasManager singleton;

    [SerializeField] Slider healthSlider;
    GameObject fillObject;

	void Awake ()
    {
		if (singleton == null)
        {
            singleton = this;
            fillObject = healthSlider.fillRect.gameObject;
        }
        else
        {
            Destroy(gameObject);
        }
	}
	
    public void SetHealthMaximum(float max)
    {
        healthSlider.maxValue = max;
        healthSlider.value = max;
    }

	public void SetHealth(float value)
    {
        healthSlider.value = value;
        if (value == 0)
            fillObject.SetActive(false);
        else if (!fillObject.activeSelf)
        {
            fillObject.SetActive(true);
        }
    }
}
