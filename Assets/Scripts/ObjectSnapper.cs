using System;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectSnapper : MonoBehaviour
{
    [Tooltip("The grabbable object to check for grabbing")]
    [SerializeField] public Grabbable grabbable;
    [Tooltip("Is the object a door")]
    [SerializeField] public bool isDoor = false;
    [SerializeField] public bool next = false;
    [SerializeField] public bool isTooltip = false;
    private bool grabbed = false;
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }
    // Update is called once per frame
    void Update()
    {
        if(grabbable == null)
        {
            return;
        }
        if (grabbable.SelectingPointsCount > 0)
        {
            if(!grabbed)
            {
                Debug.Log("I have been grabbed!");
                grabbed = true;
            }
        }
        else
        {
            if (grabbed)
            {
                if (isDoor)
                {
                    snapToWall();
                }
                Debug.Log("I have been released!");
                snapToFloor();
                grabbed = false;
            }
        }
    }

    /// <summary>
    /// To change rooms when the door is touched
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (isDoor)
        {
            if (next)
            {
                RoomManager.Instance.DoorNextScene();
            }
            else
            {
                RoomManager.Instance.DoorPreviousScene();
            }
        }
        // If delete button is pressed delete the object
        else
        {
            Debug.Log("DELETE: Delete mode: " + RoomManager.Instance.isDeleteMode);
            if(RoomManager.Instance.isDeleteMode)
            {
                Debug.Log("DELETE: Object deleted");
                RoomManager.Instance.DeleteObject(gameObject);
            }
        }
    }

    /// <summary>
    /// Snaps the object to the wall behind it
    /// </summary>
    public void snapToWall()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }

        Vector3 raycastOrigin = transform.position;
        Vector3 raycastDirection = -transform.forward;
        RaycastHit hit;

        for(int i = 0; i < 10; i++)
        {
            if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    // Set the position to be on the wall
                    transform.position = hit.point;

                    // Find the right rotation to be parallel to the wall
                    transform.rotation = Quaternion.LookRotation(hit.normal);
                    return;
                }
                // Update the origin to the hit point and continue the raycast
                raycastOrigin = hit.point;
            }
        }
    }

    /// <summary>
    /// Snaps the object to the floor and makes it upright
    /// </summary>
    public void snapToFloor()
    {
        _renderer = GetComponent<Renderer>();
        // check if the root object has a renderer at all
        float lowestY = Mathf.Infinity;
        if (_renderer != null)
        {
            lowestY = _renderer.bounds.min.y;
        }
        // Search for the lowest renderer in the children
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.bounds.min.y < lowestY)
            {
                _renderer = renderer;
                lowestY = renderer.bounds.min.y;
            }
        }

        // set the rotation to be always upright
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

        // Collider collider = GetComponent<Collider>();
        // if (collider == null)
        // {
        //     Debug.LogError("No collider found on the grabbable object.");
        //     return;
        // }
        // calculate the offset of the object to be actually standing on the ground
        // i.e. the distance between the position to the actual lowest point of the object that should be touching the ground
        float groundOffset = transform.position.y - _renderer.bounds.min.y;
        Debug.Log("SNAP: Ground offset: " + groundOffset);
        Vector3 basePosition = transform.position;
        Debug.Log("SNAP: Base position y: " + basePosition.y);

        if (isDoor)
        {
            // Doors should always be on the floor to be reachable for the user
            basePosition.y = groundOffset;
            transform.position = basePosition;
            return;
        }
        if (Physics.Raycast(basePosition, Vector3.down, out RaycastHit groundHit, Mathf.Infinity))
        {
            basePosition.y = groundOffset + groundHit.point.y;
            Debug.Log("SNAP: hit  @ " + Mathf.Round(groundHit.point.y));
            Debug.Log("SNAP: Final base position y: " + basePosition.y);
        }
        else
        {
            basePosition.y = groundOffset;
        }
        transform.position = basePosition;
    }

    public void snapTooltip(float gameObjectY)
    {
        transform.position = new Vector3(transform.position.x, gameObjectY, transform.position.z);
        // // Tooltips should be snapped to the object they are hovering over
        // if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit objectHit, Mathf.Infinity))
        // {
        //     float y = objectHit.point.y;
        //     transform.position = new Vector3(transform.position.x, y, transform.position.z);
        //     return;
        // }
    }

    public float getHighestPoint()
    {
        // check if the root object has a renderer at all
        if (_renderer != null) return _renderer.bounds.max.y;

        _renderer = GetComponent<Renderer>();
        // Search for the highest renderer in the children
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float highestY = _renderer.bounds.max.y;
        foreach (Renderer renderer in renderers)
        {
            if (renderer.bounds.max.y > highestY)
            {
                _renderer = renderer;
                highestY = renderer.bounds.max.y;
            }
        }
        return highestY;
    }
}
