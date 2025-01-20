using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectSnapper : MonoBehaviour
{
    [Tooltip("The grabbable object to check for grabbing")]
    [SerializeField] public Grabbable grabbable;
    [Tooltip("Is the object a door")]
    [SerializeField] public bool isDoor = false;
    private bool grabbed = false;
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }
    // Update is called once per frame
    void Update()
    {
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
    /// Snaps the object to the wall behind it
    /// </summary>
    public void snapToWall()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }
        // find the wall behind the object
        if( Physics.Raycast(transform.position, -transform.forward, out RaycastHit hit, Mathf.Infinity))
        {
            // set the position to be on the wall
            transform.position = hit.point;
        }
        // find the right rotation to be parallel to the wall
        transform.rotation = Quaternion.LookRotation(hit.normal);
    }

    /// <summary>
    /// Snaps the object to the floor and makes it upright
    /// </summary>
    public void snapToFloor()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }
        // set the rotation to be always upright
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);

        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError("No collider found on the grabbable object.");
            return;
        }
        // calculate the offset of the object to be actually standing on the ground
        // i.e. the distance between the position to the actual lowest point of the object that should be touching the ground
        float groundOffset = transform.position.y - _renderer.bounds.min.y;
        Debug.Log("SNAP: Ground offset: " + groundOffset);
        Vector3 basePosition = transform.position;
        Debug.Log("SNAP: Base position y: " + basePosition.y);
        basePosition.y = groundOffset;


        if (Physics.Raycast(basePosition, Vector3.down, out RaycastHit groundHit, Mathf.Infinity))
        {
            basePosition.y += groundHit.point.y;
            Debug.Log("SNAP: hit  @ " + Mathf.Round(groundHit.point.y));
            Debug.Log("SNAP: Final base position y: " + basePosition.y);
        }
        transform.position = basePosition;
    }
}
