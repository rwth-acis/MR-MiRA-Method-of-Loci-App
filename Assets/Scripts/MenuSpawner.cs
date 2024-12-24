using UnityEngine;

public class MenuSpawner : MonoBehaviour
{
    [SerializeField] private GameObject menuPrefab;
    [SerializeField] private GameObject user;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instantiate(menuPrefab, user.transform.position + user.transform.forward + Vector3.up, user.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Follow User
    }
}
