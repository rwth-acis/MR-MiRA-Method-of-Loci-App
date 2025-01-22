using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelector : MonoBehaviour
{
    [Tooltip("The UI object")]
    [SerializeField] public GameObject UI;
    [Tooltip("The user object")]
    [SerializeField] public GameObject User;
    [Tooltip("Whether the layout mode is selected")]
    public bool layoutMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Object.DontDestroyOnLoad(this);
        UI.transform.position = User.transform.position + User.transform.forward + Vector3.up;
    }

    /// <summary>
    /// Loads the palace scene with all objects
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public void LoadPalace(bool value)
    {
        SceneManager.LoadScene("Room 1");
        layoutMode = false;
    }

    /// <summary>
    /// Loads the palace with no information objects to learn the layout
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public void LearnLayout(bool value)
    {
        SceneManager.LoadScene("Room 1");
        layoutMode = true;
    }

}
