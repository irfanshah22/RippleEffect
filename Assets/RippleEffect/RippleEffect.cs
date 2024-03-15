using UnityEngine;
using UnityEngine.UI;
using System.Collections;
  
public class RippleEffect : MonoBehaviour
{
    public AnimationCurve waveform = new AnimationCurve(
        new Keyframe(0.00f, 0.50f, 0, 0),
        new Keyframe(0.05f, 1.00f, 0, 0),
        new Keyframe(0.15f, 0.10f, 0, 0),
        new Keyframe(0.25f, 0.80f, 0, 0),
        new Keyframe(0.35f, 0.30f, 0, 0),
        new Keyframe(0.45f, 0.60f, 0, 0),
        new Keyframe(0.55f, 0.40f, 0, 0),
        new Keyframe(0.65f, 0.55f, 0, 0),
        new Keyframe(0.75f, 0.46f, 0, 0),
        new Keyframe(0.85f, 0.52f, 0, 0),
        new Keyframe(0.99f, 0.50f, 0, 0)
    );

    [Range(0.01f, 1.0f)]
    public float refractionStrength = 0.5f;

    public Color reflectionColor = Color.gray;

    [Range(0.01f, 1.0f)]
    public float reflectionStrength = 0.7f;

    [Range(1.0f, 3.0f)]
    public float waveSpeed = 1.25f;

    [Range(0.0f, 2.0f)]
    public float dropInterval = 0.5f;
    private float _tempdropInterval = 0f;

    public static Vector3 MousePos;
    public Text _androidText;
    private int Counter;

    [SerializeField, HideInInspector]
    Shader shader;

    class Droplet
    {
        Vector2 position;
        float time;

        public Droplet()
        {
            time = 1000;
        }

        public void Reset()
        {
            position = new Vector2(Random.value, Random.value);
            time = 0;
        }

        public void Update()
        {
            time += Time.deltaTime;
        }

        public Vector4 MakeShaderParameter(float aspect)
        {
            //  (-0.3, 0.4, 0.0)
          //  (0.3, 0.5, 0.0)
           // return new Vector4(position.x * aspect, position.y, time, 0);
            return new Vector4(MousePos.x+.9f, MousePos.y+.5f, time, 0);   
        } 
    }
    public static RippleEffect Instance;
    Droplet[] droplets;
    Texture2D gradTexture;
    Material material;
    float timer;
    int dropCount;
    private bool _MouseHit;
    void UpdateShaderParameters()
    {
        var c = GetComponent<Camera>();

        material.SetVector("_Drop1", droplets[0].MakeShaderParameter(c.aspect));
        //material.SetVector("_Drop2", droplets[1].MakeShaderParameter(c.aspect));
        //material.SetVector("_Drop3", droplets[2].MakeShaderParameter(c.aspect));
         material.SetColor("_Reflection", reflectionColor);
        material.SetVector("_Params1", new Vector4(c.aspect, 1, 1 / waveSpeed, 0));
        material.SetVector("_Params2", new Vector4(1, 1 / c.aspect, refractionStrength, reflectionStrength));
    }

    void Awake()
    {
        Instance = this;
        _tempdropInterval = dropInterval;  
        _MouseHit = false;
        Counter = 0;
        droplets = new Droplet[1]; 
        droplets[0] = new Droplet();
        //droplets[1] = new Droplet();
        //droplets[2] = new Droplet();
         gradTexture = new Texture2D(2048, 1, TextureFormat.Alpha8, false);
        gradTexture.wrapMode = TextureWrapMode.Clamp;
        gradTexture.filterMode = FilterMode.Bilinear;
        for (var i = 0; i < gradTexture.width; i++)
        {
            var x = 1.0f / gradTexture.width * i;
            var a = waveform.Evaluate(x);
            gradTexture.SetPixel(i, 0, new Color(a, a, a, a));
        }
        gradTexture.Apply();    
         material = new Material(shader);
        material.hideFlags = HideFlags.DontSave;
        material.SetTexture("_GradTex", gradTexture);  
        // UpdateShaderParameters();
    }

    void Update()
    {
#if UNITY_EDITOR_WIN
         if (Input.GetMouseButtonDown(0))
        {
            _androidText.text = "Mouse Clicked";
             RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                _androidText.text = "Mouse Hit editor or PC";

                if (hit.collider.gameObject.tag == "Plane")
                {
                    Counter += 1;
                     _androidText.text = "Plane Hited  = " + Counter;
                      MousePos = hit.point;
                    FishController.Instance.MouseTouchPoint(MousePos);
                    _MouseHit = true;
                    dropInterval = _tempdropInterval;
                    timer = dropInterval; 
                    StopCoroutine(Stopbool());
                    StopCoroutine(ReturntoPath());
                    StartCoroutine(Stopbool());
                }  
            }  
        }  
#endif
#if UNITY_ANDROID
        if (Input.touchCount > 0)  
        {
              if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Touch touch = Input.GetTouch(0);
                // Construct a ray from the current touch coordinates
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                // Create a particle if hit
                if (Physics.Raycast(ray, out hit, 100.0f))    
                {  
                    if (hit.collider.gameObject.tag == "Plane")  
                    {
                        Counter += 1;
                        _androidText.text = "touch plane hited Android= " + Counter;
                         print(hit.point);
                         MousePos = hit.point;
                        FishController.Instance.MouseTouchPoint(MousePos);
                        _MouseHit = true;
                        dropInterval = _tempdropInterval;  
                        StopCoroutine(Stopbool());
                        StopCoroutine(ReturntoPath());
                         StartCoroutine(Stopbool());
                    }  
                }  
            }
        }
#endif

        if (!_MouseHit) 
            return; 
        if (dropInterval > 0)
        {

            timer += Time.deltaTime;
             while (timer > dropInterval)
            {
                 Emit();
                timer -= dropInterval;
            }
        }  
         foreach (var d in droplets) d.Update(); 

        UpdateShaderParameters();
    }

    public void ReachedMousePoint(Vector3 _MouseReachedPoint)
    {
             print("same Point");
        // StopCoroutine(ReturntoPath());
     }
     IEnumerator Stopbool()
    {
        yield return new WaitForSeconds(1);
        dropInterval = 0;
        StartCoroutine(ReturntoPath());
     }
    IEnumerator ReturntoPath()
    {
        yield return new WaitForSeconds(1);
         FishController.Instance.ReturntoPath();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }

    public void Emit()
    {
         droplets[dropCount++ % droplets.Length].Reset();
    }
}
