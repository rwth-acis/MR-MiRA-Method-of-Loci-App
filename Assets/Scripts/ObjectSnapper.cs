using Oculus.Interaction;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectSnapper : MonoBehaviour
{
    [Tooltip("The grabbable object to check for grabbing")]
    [SerializeField] public Grabbable grabbable;
    [Tooltip("Is the object a door")]
    [SerializeField] public bool isDoor = false;
    [Tooltip("Is the object an image")]
    [SerializeField] public bool isImage = false;
    [Tooltip("Is the object a vertical image")]
    [SerializeField] public bool isVerticalImage = false;
    [Tooltip("Used to determine if the door leads to the next scene")]
    [SerializeField] public bool next = false;
    [Tooltip("Is the object a tooltip")]
    [SerializeField] public bool isTooltip = false;
    [Tooltip("Is the object a light cone")]
    [SerializeField] public bool isCone = false;
    private bool _grabbed = false;
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        if (isImage)
        {
            SnapToWall();
            transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (isCone)
        {
            return;
        }
        if(grabbable == null)
        {
            return;
        }
        if (grabbable.SelectingPointsCount > 0)
        {
            if(!_grabbed)
            {
                _grabbed = true;
            }
        }
        else
        {
            if (_grabbed)
            {
                if (isImage)
                {
                    SnapToWall();
                    if (transform.position.y > 2)
                    {
                        transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
                    }
                    _grabbed = false;
                    return;
                }
                if (isDoor)
                {
                    SnapToWall();
                }
                SnapToFloor();
                _grabbed = false;
            }
        }
    }

    /// <summary>
    /// To change rooms when the door is touched
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.GameObject().name.Contains("PinchArea")) // pinch area contains the finger's collider
        {
            return;
        }
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
            if(RoomManager.Instance.IsDeleteMode)
            {
                RoomManager.Instance.DeleteObject(gameObject);
            }
        }
    }

    /// <summary>
    /// Snaps the object to the wall behind it
    /// </summary>
    public void SnapToWall()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }

        Vector3 raycastOrigin = transform.position;
        Vector3 raycastDirection = -transform.forward;

        for(int i = 0; i < 10; i++)
        {
            if (Physics.Raycast(raycastOrigin, raycastDirection, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    // Set the position to be on the wall
                    transform.position = hit.point;

                    // Find the right rotation to be parallel to the wall
                    transform.rotation = Quaternion.LookRotation(hit.normal);
                    if (isVerticalImage)
                    {
                        transform.Rotate(0, 0, -90);
                    }
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
    public void SnapToFloor()
    {
        if (isImage)
        {
            SnapToWall();
            if (transform.position.y > 2)
            {
                transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
            }

            return;
        }
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

        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            return;
        }
        // calculate the offset of the object to be actually standing on the ground
        // i.e. the distance between the position to the actual lowest point of the object that should be touching the ground
        float groundOffset = transform.position.y - _renderer.bounds.min.y;
        Vector3 basePosition = transform.position;
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
        }
        else
        {
            basePosition.y = groundOffset;
        }
        transform.position = basePosition;
    }

    /// <summary>
    /// Snaps the tooltip to the object
    /// </summary>
    /// <param name="gameObjectY"></param>
    public void SnapTooltip(float gameObjectY)
    {
        transform.position = new Vector3(transform.position.x, gameObjectY, transform.position.z);
    }

    /// <summary>
    /// Returns the highest point of the object
    /// </summary>
    /// <returns>The highest point of the object</returns>
    public float GetHighestPoint()
    {
        // check if the root object has a renderer at all
        if (_renderer != null && _renderer.bounds.max.y >= gameObject.transform.lossyScale.y) return _renderer.bounds.max.y;

        _renderer = GetComponent<Renderer>();
        // Search for the highest renderer in the children
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if(_renderer == null && renderers.Length > 0)
        {
            _renderer = renderers[0];
        }
        float highestY = _renderer.bounds.max.y;

        foreach (Renderer renderer in renderers)
        {
            if (renderer.bounds.max.y > highestY)
            {
                _renderer = renderer;
                highestY = renderer.bounds.max.y;
            }
        }
        if(highestY <= gameObject.transform.lossyScale.y)
        {
            return gameObject.transform.lossyScale.y;
        }
        return highestY;
    }
}
