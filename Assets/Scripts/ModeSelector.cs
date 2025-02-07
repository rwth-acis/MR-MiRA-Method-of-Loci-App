using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class ModeSelector : MonoBehaviour
{
    [Tooltip("The UI object")]
    [SerializeField] public GameObject UI;
    [Tooltip("The user object")]
    [SerializeField] public GameObject User;
    [Tooltip("Whether the layout mode is selected")]
    public bool layoutMode;
    [Tooltip("Whether the reuse mode is selected")]
    public bool reuseMode = false;
    [Tooltip("The camera object")]
    private Camera _cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Object.DontDestroyOnLoad(this);
        _cam = User.GetComponent(typeof(Camera)) as Camera;
        UI.transform.position = User.transform.position + User.transform.forward + Vector3.up;
    }

    private void Update()
    {
        // Check if the menu is within the user's field of view
        Vector3 viewPos = _cam.WorldToViewportPoint(UI.transform.position);
        bool isVisible = viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0;

        // If the menu is not visible, reposition it in front of the user
        if (!isVisible)
        {
            Vector3 velocity = Vector3.zero;
            Vector3 targetPosition = User.transform.TransformPoint(new Vector3(0, 0, 0.7f));
            targetPosition.y = UI.transform.position.y;
            UI.transform.position = Vector3.SmoothDamp(UI.transform.position, targetPosition, ref velocity, 0.3f);
            var lookAtPos = new Vector3(User.transform.position.x, UI.transform.position.y, User.transform.position.z);
            UI.transform.LookAt(lookAtPos);
            UI.transform.Rotate(0, 180, 0);
        }

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

    /// <summary>
    /// Loads the palace with no information objects and the user can choose new ones
    /// </summary>
    /// <param name="value">Teh value of the corresponding button</param>
    public void ReusePalace(bool value)
    {
        SceneManager.LoadScene("Room 1");
        layoutMode = true;
        reuseMode = true;
    }

}
