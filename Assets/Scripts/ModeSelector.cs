using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelector : MonoBehaviour
{
    [SerializeField] public GameObject UI;
    [SerializeField] public GameObject User;
    public bool layoutMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Object.DontDestroyOnLoad(this);
        UI.transform.position = User.transform.position + User.transform.forward + Vector3.up;
    }

    public void LoadPalace(bool value)
    {
        SceneManager.LoadScene("Room 1");
        layoutMode = false;
    }

    public void LearnLayout(bool value)
    {
        SceneManager.LoadScene("Room 1");
        layoutMode = true;
    }

}
