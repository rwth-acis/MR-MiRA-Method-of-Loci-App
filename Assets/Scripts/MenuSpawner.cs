using UnityEngine;

public class MenuSpawner : MonoBehaviour
{
    [Tooltip("The menu prefab to spawn")]
    [SerializeField] private GameObject menuPrefab;
    [Tooltip("The user object")]
    [SerializeField] private GameObject user;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Spawn the menu prefab
        GameObject menu = Instantiate(menuPrefab, user.transform.position + user.transform.forward + Vector3.up, user.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Follow User
    }
}
