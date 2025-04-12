using UnityEngine;

public class MenuSpawner : MonoBehaviour
{
    [Tooltip("The menu prefab to spawn")]
    [SerializeField] private GameObject menuPrefab;
    [Tooltip("The user object")]
    [SerializeField] private GameObject user;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Spawn the menu prefab
        Instantiate(menuPrefab, user.transform.position + user.transform.forward + Vector3.up, user.transform.rotation);
    }
}
