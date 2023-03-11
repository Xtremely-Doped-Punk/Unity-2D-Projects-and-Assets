using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.MLAgents;

public class GameSession : MonoBehaviour
{
    [SerializeField] private int[] NoOfTries = null;
    [Tooltip("Based on Average no. of obstacles passed each time")]
    [SerializeField] private float[] Scores = null;
    [SerializeField] private int SceneIdx=-1;

    private int SceneCount;
    static private GameSession Instance;
    public static GameSession GetInstance() { return Instance; }

    void Awake()
    {
        var sessions = FindObjectsOfType<GameSession>();
        if (sessions.Length > 1)
        {
            foreach (var session in sessions)
            {
                if (this == session)
                    continue;
                else
                {
                    this.NoOfTries = session.NoOfTries;
                    this.SceneIdx = session.SceneIdx;

                    Destroy(session.gameObject);
                }
            }
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    void Start()
    {
        SceneCount = SceneManager.sceneCountInBuildSettings;
        // this initialization could be simply avoided by removing the [SerializeField]
        // for the variable as the instpector cant overwrite the initializations of script
        if (NoOfTries.Length < SceneCount)
            NoOfTries = new int[SceneCount];
        if (Scores.Length < SceneCount)
            Scores = new float[SceneCount];

        SceneIdx = SceneManager.GetActiveScene().buildIndex;

    }

    public void SessionUpdate(int EpisodeScore)
    {
        Scores[SceneIdx] = (Scores[SceneIdx]*NoOfTries[SceneIdx]+EpisodeScore)/++NoOfTries[SceneIdx];
    }

    public void LoadNextSceneDelayed(float timer)
    {
        Debug.Log("Loading Next Scene in "+timer.ToString());
        SceneIdx++;
        if (SceneIdx >= SceneCount)
            SceneIdx = SceneCount - 1;
        Invoke(nameof(LoadNScene),timer);
    }
    public void LoadPrevSceneDelayed(float timer)
    {
        Debug.Log("Loading Prev Scene in " + timer.ToString());
        SceneIdx--;
        if (SceneIdx < 0)
            SceneIdx = 0;
        Invoke(nameof(LoadNScene), timer);
    }

    private void LoadNScene()
    {
        Debug.Log("Loaded Nxt Scene Idx: "+SceneIdx.ToString());
        SceneManager.LoadScene(SceneIdx);
    }

    public void LoadMainMenu()
    {
        SceneIdx = 0;
        LoadNScene();
    }

    public void LoadGameOver()
    {
        SceneIdx = SceneCount - 1;
        LoadNScene();
    }

    public void SceneReset() { Start(); }
}
