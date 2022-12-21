using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] public GameObject HUD;
    [SerializeField] public GameObject Menu;

    [Header("Audio")]
    [SerializeField] public AudioSource BGM;

    private bool _isPaused;

    void Start()
    {
        // Call UnPause on Start to prevent timeScale from setting to 0 when user clicks Restart button
        UnPause();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
                UnPause();
            else
                Pause();
        }
    }

    public void Pause()
    {
        _isPaused = true;
        Menu.SetActive(true);
        HUD.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = _isPaused;
        Time.timeScale = 0;
        BGM.Pause();
    }
    public void UnPause()
    {
        _isPaused = false;
        Menu.SetActive(false);
        HUD.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = _isPaused;
        Time.timeScale = 1f;
        BGM.Play();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}