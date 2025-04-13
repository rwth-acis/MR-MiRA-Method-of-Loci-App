using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class ModeSelector : MonoBehaviour
{
    [FormerlySerializedAs("UI")]
    [Tooltip("The UI object")]
    [SerializeField] public GameObject ui;
    [FormerlySerializedAs("User")]
    [Tooltip("The user object")]
    [SerializeField] public GameObject user;
    [Tooltip("Whether the layout mode is selected")]
    public bool layoutMode;
    [Tooltip("Whether the reuse mode is selected")]
    public bool reuseMode = false;
    [Tooltip("The camera object")]
    private Camera _cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Object.DontDestroyOnLoad(this);
        _cam = user.GetComponent(typeof(Camera)) as Camera;
        ui.transform.position = user.transform.position + user.transform.forward + Vector3.up;
    }

    private void Update()
    {
        // Check if the menu is within the user's field of view
        Vector3 viewPos = _cam.WorldToViewportPoint(ui.transform.position);
        bool isVisible = viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0;

        // If the menu is not visible, reposition it in front of the user
        if (!isVisible)
        {
            Vector3 velocity = Vector3.zero;
            Vector3 targetPosition = user.transform.TransformPoint(new Vector3(0, 0, 0.7f));
            targetPosition.y = ui.transform.position.y;
            ui.transform.position = Vector3.SmoothDamp(ui.transform.position, targetPosition, ref velocity, 0.3f);
            var lookAtPos = new Vector3(user.transform.position.x, ui.transform.position.y, user.transform.position.z);
            ui.transform.LookAt(lookAtPos);
            ui.transform.Rotate(0, 180, 0);
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
