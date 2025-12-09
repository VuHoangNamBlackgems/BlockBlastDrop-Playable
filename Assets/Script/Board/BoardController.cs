using UnityEngine;

public class BoardController : MonoBehaviour
{
    public static BoardController Instance;
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private CanonManager canonManager;
    [SerializeField] private EnemyGridManager enemyGridManager;

    [SerializeField] public GameObject bulletPrefab;
    [SerializeField] public GameObject blastParticle;

    [SerializeField] private Transform levelSpawn;

    [Header("Data"), Space(3)]
    public EnemyGridData enemyGridData;
    public ThemeData themeData;

    [Header("VFX"), Space(3)]
    [SerializeField] ParticleSystem comboVFX;
    [SerializeField] bool useVFXCombo;

    [Header("Tutorial"), Space(3)]
    [SerializeField] bool useTutorial;
    [SerializeField] float timeMaxShow;

    [Header("Setting"), Space(3)]
    public DifficultyType difficultyLevel;
    public ThemeType theme;

    [HideInInspector]
    public int currentLevel => (int)difficultyLevel;
    [HideInInspector]
    public int currentTheme => (int)theme;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1.5f;
        QualitySettings.vSyncCount = 0;
        //  Application.targetFrameRate = 60;
    }

    public void Start()
    {
        LoadLevel();
    }

    public void SpawnCanon(int colorId)
    {
        if (canonManager != null)
            canonManager.SpawnCanon(colorId);
    }

    public void ResetBoard()
    {
        if (lobbyManager != null) lobbyManager.ResetLobby();
        if (canonManager != null) canonManager.ClearAllCanons();
        if (enemyGridManager != null) enemyGridManager.ClearGrid();
        if (levelSpawn.childCount > 0)
            Destroy(levelSpawn.GetChild(0).gameObject);
    }
    public GameObject[] objLevels;
    public int indexLevel;
    public void LoadLevel()
    {
        // Time.timeScale = 1.5f;
        ResetBoard();
        enemyGridManager.LoadEnemyGrid(currentLevel - 1);
        ThemeController.Instance.SetupTheme(currentTheme);
        // Object level = Instantiate(Resources.Load("Levels/Level" + currentLevel), levelSpawn);
        Object level = Instantiate(objLevels[indexLevel], levelSpawn);

        if (UIEndLevel.Instance)
            UIEndLevel.Instance.Hide();
        timeCountShowTut = 0;
        startCountTime = true;
    }

    public void PlayCombo()
    {
        if (!useVFXCombo) return;
        int index = Random.Range(0, 10);
        var texture = comboVFX.textureSheetAnimation;
        texture.startFrame = new ParticleSystem.MinMaxCurve((index - 2) * 0.1f);
        comboVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        comboVFX.Play(true);
    }

    [HideInInspector]
    public bool startCountTime = false;

    float timeCountShowTut = 0;
    private void Update()
    {

        if (startCountTime)
            timeCountShowTut += Time.deltaTime;
        else
            timeCountShowTut = 0;
        CheckShowTutorial();
    }
    void CheckShowTutorial()
    {
        if (timeCountShowTut > timeMaxShow)
        {
            timeCountShowTut = 0;
        }
    }
}
