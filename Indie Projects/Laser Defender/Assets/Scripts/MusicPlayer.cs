using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    private void Awake()
    {
        // set up Singleton,
        // i.e., when scene changes, make sure only only music-player stays in the scene
        // and to ensure that song continues in each scene as same object is left using DontDestroyOnLoad()

        // note Find Object "s" of type, so that we can check no. of music-systems at the time new scene is loaded
        if (FindObjectsOfType(GetType()).Length > 1)
        // GetType == current class name of this component == MusicPlayer
        {
            // if no of music-players > 1,  then destroy this new music-system as the old music-system still exists
            Destroy(gameObject);
        }
        else // if this is the first instance of a music-system,
        {
            DontDestroyOnLoad(gameObject);  // then define not to destroy this obj on load next scenes
        }
    }

    [SerializeField] float hSliderValue = 1.0f;
    [SerializeField] AudioClip[] clips;
    [SerializeField] int hideGUItime = 5;
    private bool GUIhidden = false;
    private float mouseMoveTime = 0f;
    [SerializeField] bool mute = false;

    private int cnt = -1;
    private Vector3 tmpMousePos;
    private AudioSource audiosrc;
    [SerializeField] bool randomSeed = true;
    [SerializeField] Vector2 MinMaxSound = new Vector2(0.005f,0.25f);

    [SerializeField] Vector2Int AspectRatio = new Vector2Int(9, 16);

    void Start()
    {
        audiosrc = GetComponent<AudioSource>();
        audiosrc.loop = false;
        SetRatio(AspectRatio.x, AspectRatio.y);
    }
    
    void SetRatio(float w, float h)
    {
        if ((((float)Screen.width) / ((float)Screen.height)) > w / h)
        {
            Screen.SetResolution((int)(((float)Screen.height) * (w / h)), Screen.height, true);
        }
        else
        {
            Screen.SetResolution(Screen.width, (int)(((float)Screen.width) * (h / w)), true);
        }
    }

    void Update()
    {
        DetectMouseMov();

        mouseMoveTime = mouseMoveTime + Time.deltaTime;

        if (audiosrc.volume != hSliderValue)
            audiosrc.volume = hSliderValue; // update music sound in real time using mouse

        if (mouseMoveTime > hideGUItime)
            GUIhidden = true;
        else
            GUIhidden = false;

        if (Input.GetKeyUp(KeyCode.M))
        {
            mute = !mute;
        }
        if (mute)
            audiosrc.Pause();
        else
        {
            audiosrc.UnPause();
            PlaySounds();
        }
    }

    void PlaySounds()
    {
        if (!audiosrc.isPlaying)
        {
            if (randomSeed)
                cnt = Random.Range(0,clips.Length);
            else
            {
                cnt++;
                if ((cnt >= clips.Length)||(cnt<0))
                    cnt = 0;
            }

            audiosrc.clip = clips[cnt];
            audiosrc.Play();
        }
    }

    void OnGUI()
    {
        if (!GUIhidden)
        {
            GUI.Label(new Rect(5, Screen.height - 40, 80, 20), "Volume");
            hSliderValue = GUI.HorizontalSlider(new Rect(5, Screen.height - 20, 80, 20), hSliderValue, MinMaxSound.x, MinMaxSound.y);

            mute = GUI.Toggle(new Rect(Screen.width - 55, Screen.height - 45, 50, 20), mute, "Mute");
            if (GUI.Button(new Rect(Screen.width - 95, Screen.height - 25, 90, 20), "Change Song"))
                audiosrc.Stop();
        }
        Cursor.visible = !GUIhidden;
    }

    void DetectMouseMov() // sound bar gui appears only when mouse moves
    {
        if (tmpMousePos != Input.mousePosition)
        {
            mouseMoveTime = 0;
            tmpMousePos = Input.mousePosition;
        }
    }
}