using UnityEngine;

[RequireComponent(typeof(GunLibrary))]
public class GunLocker : MonoBehaviour {

    public static GunLocker singleton;
    GunLibrary library;
    
	void Awake ()
    {
        if (singleton == null)
        {
            singleton = this;
            library = GetComponent<GunLibrary>();
        }
        else
        {
            Destroy(gameObject);
        }
	}

    public GunSettings GetGun(string name)
    {
        return library.GetGunFromName(name);
    }
}
