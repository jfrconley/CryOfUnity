using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class CameraControl : MonoBehaviour {

    public static CameraControl singleton;
    PostProcessingProfile post;
    Camera cam;
    Transform player;

    Vector3 lastPlayer = Vector3.zero;
    List<Vector3> positions = new List<Vector3>();
    float shakeTarget;
    float shakeSmooth;
    Vector3 velocity;
    float anglularVelocity;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            cam = GetComponent<Camera>();
            post = GetComponent<PostProcessingBehaviour>()?.profile;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update ()
    {
        if (positions.Count > 0)
            positions.Clear();

        Vector3 v = Input.mousePosition;
        v.z = transform.position.y;

        //Player Position
        if (player != null)
            lastPlayer = player.position;
        positions.Add(lastPlayer);
        
        //Cursor Position
        positions.Add(cam.ScreenToWorldPoint(v));

        Vector3 target = FlattenY(GetAveragePosition(positions.ToArray()));
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, 0.04f);

        shakeSmooth = Mathf.SmoothDamp(shakeSmooth, shakeTarget, ref anglularVelocity, 0.06f);
        Vector3 angle = transform.eulerAngles;
        angle.y = shakeSmooth;
        transform.eulerAngles = angle;
    }

    Vector3 GetAveragePosition(Vector3[] v)
    {
        Vector3 average = Vector3.zero;
        foreach (Vector3 pos in v)
        {
            average += pos;
        }
        average /= v.Length;

        return average;
    }

    public void CameraShake(float amount, float time)
    {
        //StartCoroutine(CameraCoroutine(amount, Random.Range(2f,4f), time));
        StartCoroutine(CameraAberration(amount, time));
    }

    IEnumerator CameraCoroutine(float amplitude, float frequency, float time)
    {
        float percent = 0;
        while (percent < time)
        {
            percent += Time.deltaTime;
            shakeTarget = amplitude * Mathf.Sin(Mathf.PI * percent * frequency) * (1 - percent);
            yield return null;
        }
        shakeTarget = 0f;
    }

    IEnumerator CameraAberration(float amount, float time)
    {
        float percent = 0;
        float fov = cam.fieldOfView;
        ChromaticAberrationModel.Settings settings = post.chromaticAberration.settings;
        while(percent < time)
        {
            percent += Time.deltaTime;
            float total = Mathf.Sin(Mathf.PI * percent / time) * amount;

            settings.intensity = total;
            post.chromaticAberration.settings = settings;

            cam.fieldOfView = fov - total;
            yield return null;
        }
        settings.intensity = 0;
        post.chromaticAberration.settings = settings;
    }

    //positional
    /*public void CameraShake(Vector3 dir)
    {
        StartCoroutine(CameraCoroutine(dir));
    }

    IEnumerator CameraCoroutine(Vector3 offset)
    {
        shakeTarget += offset;
        float percent = 0;
        while(percent < 1)
        {
            percent += Time.deltaTime * 4f;
            shakeSmooth = shakeTarget * Mathf.Sin(Mathf.PI * percent);
            //shakeSmooth.y += transform.position.y;
            yield return null;
        }
        shakeSmooth = Vector3.zero;
    }*/

    Vector3 FlattenY(Vector3 v)
    {
        v.y = transform.position.y;
        return v;
    }

    public Transform PlayerTarget
    {
        set
        {
            player = value;
        }
    }
}
