using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class RoomManager : MonoBehaviour
{
    [Tooltip("The instance of the RoomManager")]
    public static RoomManager Instance { get; private set; }
    [Tooltip("The agent controller")]
    public AgentController agentController;
    public Material wallColour;
    public List<GameObject> furniturePrefabs = new List<GameObject>();
    [Tooltip("The center eye anchor of the user")]
    [SerializeField] public GameObject user;

    private List<User> _users = new List<User>();
    private Camera _cam;
    private List<Color> _wallColours = new List<Color>();
    private int _colourPointer;
    private User _currentUser;
    private Room _currentRoom;
    private int _furniturePointer = 0;
    private GameObject _menu;

    //for testing
    [SerializeField] public GameObject furniture;
    [SerializeField] public GameObject furniture2;

    private void Awake()
    {
        // Ensure that there is only one instance of RoomManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // TODO get name as input from user
        // Find the agentcontroller in the scene
        agentController = FindObjectOfType<AgentController>();
        user = GameObject.Find("CenterEyeAnchor");
        _menu = GameObject.FindGameObjectWithTag("Menu");
        _cam = user.GetComponent(typeof(Camera)) as Camera;
        //Set up the colour list
        colourListSetup();
        LoadUser("Lena");
    }

    private void Update()
    {
        // Check if the menu is within the user's field of view
        Vector3 viewPos = _cam.WorldToViewportPoint(_menu.transform.position);
        bool isVisible = viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0;

        // If the menu is not visible, reposition it in front of the user
        if (!isVisible)
        {
            Vector3 velocity = Vector3.zero;
            Vector3 targetPosition = user.transform.TransformPoint(new Vector3(0, 0, 0.7f));
            targetPosition.y = _menu.transform.position.y;
            _menu.transform.position = Vector3.SmoothDamp(_menu.transform.position, targetPosition, ref velocity, 0.3f);
            var lookAtPos = new Vector3(user.transform.position.x, _menu.transform.position.y, user.transform.position.z);
            _menu.transform.LookAt(lookAtPos);
            _menu.transform.Rotate(0, 180, 0);
        }
    }

    /// <summary>
    /// Loads a user from a save file or creates a new user if the save file does not exist.
    /// The first room of the user is loaded.
    /// </summary>
    /// <param name="username">The user that should be loaded</param>
    public void LoadUser(string username)
    {
        string path = Path.Combine(Application.persistentDataPath, username + ".json");
        User newUser;
        if (!File.Exists(path))
        {
            Debug.Log("File not found");
            newUser = new User(username);
            Debug.Log("First room is null");
            Room room = new Room(0);
            newUser.AddRoom(room);
        }
        else
        {
            // Read the user's save file
            string userjson = File.ReadAllText(path);
            newUser = new User(username, JsonUtility.FromJson<SaveData>(userjson));
        }
        _users.Add(newUser);
        _currentUser = newUser;
        _currentRoom = _currentUser.GetFirstRoom();
        LoadRoom(_currentRoom);
    }

    /// <summary>
    /// Creates a new room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "New Room"</param>
    public void CreateRoom(bool value)
    {
        int newRoomID = _currentUser.GetFreeRoomID();
        Room room = new Room(newRoomID);
        _currentUser.AddRoom(room);
        _currentRoom = _currentUser.GetCurrentRoom();
        LoadRoom(room);
        _currentRoom.SaveRoom();
    }

    /// <summary>
    /// Changes the scene to the next room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Next Room"</param>
    public void NextScene(bool value)
    {
        Room nextRoom = _currentUser.NextRoom();
        if (nextRoom == null)
        {
            Debug.Log("There is no next room available in RoomManager");
            return; // TODO: Error Message or Grey out Button
        }
        LoadRoom(nextRoom);
        _currentRoom = _currentUser.GetCurrentRoom();
    }

    /// <summary>
    /// Changes the scene to the previous room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Previous Room"</param>
    public void PreviousScene(bool value)
    {
        Room previousRoom = _currentUser.PreviousRoom();
        if (previousRoom == null)
        {
            Debug.Log("There is no previous room available in RoomManager");
            return; // TODO: Error Message or Grey out Button
        }
        LoadRoom(previousRoom);
        _currentRoom = _currentUser.GetCurrentRoom();
    }

    /// <summary>
    /// Saves the current room to JSON and loads the given room
    /// </summary>
    /// <param name="room">The room to be loaded</param>
    public void LoadRoom(Room room)
    {
        Debug.Log("Furniture instances :" + room.FurnitureInstances.Count);
        room.FurnitureInstances.Clear();
        // Save the current room to JSON
        _currentRoom.SaveRoom();
        _currentUser.SaveUser();
        // First remove all furniture from the scene
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Furniture");
        foreach (GameObject obj in allObjects)
        {
            GameObject.Destroy(obj);
        }
        // Load the room
        Debug.Log("LENA: We are loading room " + room.ID);
        if (room != null && room.HasFurniture())
        {
            // List of the prefabs of the furniture in the room
            List<GameObject> roomFurniture = room.Furniture;
            Debug.Log("Furniture instances :" + room.FurnitureInstances.Count);
            for (int i = 0; i < roomFurniture.Count; i++)
            {
                GameObject myObject = GameObject.Instantiate(roomFurniture[i]);
                myObject.tag = "Furniture";
                room.AddFurnitureInstance(myObject);
            }
            room.LoadTransforms();
        }

        if (room != null && room.WallColour != null)
        {
            wallColour.color = room.WallColour;
            Debug.Log("Wall colour: " + room.WallColour);
        }
    }

    /// <summary>
    /// Adds furniture to the current room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Add Furniture"</param>
    public void AddFurniture(bool value)
    {
        Debug.Log("LENA: Furniture added to room " + _currentUser.GetCurrentRoom().ID);
        // Instantiate the furniture in the scene
        GameObject newObject = GameObject.Instantiate(furniture, Vector3.zero, Quaternion.identity);
        GameObject newObject2 = GameObject.Instantiate(furniture2, Vector3.zero, Quaternion.identity);
        newObject.tag = "Furniture";
        newObject2.tag = "Furniture";
        // Add furniture to the current room's list of furniture
        _currentRoom.AddFurniture(furniture, newObject);
        _currentRoom.AddFurniture(furniture2, newObject2);
        _currentRoom.UpdateTransforms();
        _currentRoom.SaveRoom();
    }

    /// <summary>
    /// Save all the rooms of the current user and switch to the next user
    /// </summary>
    /// <param name="username">The user to switch to</param>
    public void SwitchUser(string username)
    {
        _currentUser.SaveUser();
        LoadUser(username);
    }

    public void NextWallColour(bool value)
    {
        _currentRoom.ChangeWallColour(_wallColours[_colourPointer]);;
        if (_colourPointer == _wallColours.Count - 1)
        {
            _colourPointer = 0;
        }
        else
        {
            _colourPointer++;
        }
        wallColour.color = _currentRoom.WallColour;
        _currentRoom.SaveRoom();
        Debug.Log("Wall colour: " + _currentRoom.WallColour);
    }

    public void PreviousWallColour(bool value)
    {
        _currentRoom.ChangeWallColour(_wallColours[_colourPointer]);;
        if (_colourPointer == 0)
        {
            _colourPointer = _wallColours.Count - 1;
        }
        else
        {
            _colourPointer--;
        }
        wallColour.color = _currentRoom.WallColour;
        _currentRoom.SaveRoom();
        Debug.Log("Wall colour: " + _currentRoom.WallColour);
    }

    private void colourListSetup()
    {
        _wallColours.Add(Color.white);
        _wallColours.Add(Color.yellow);
        _wallColours.Add(new Color(1f,0.5f,0));
        _wallColours.Add(Color.magenta);
        _wallColours.Add(Color.red);
        _wallColours.Add(Color.green);
        _wallColours.Add(Color.cyan);
        _wallColours.Add(Color.blue);
        _wallColours.Add(new Color(0.6f,0.3f,1f));
        _wallColours.Add(Color.grey);
        _wallColours.Add(Color.black);
        _colourPointer = 0;
    }

    /// <summary>
    /// Shows the next furniture prefab in the list
    /// </summary>
    /// <param name="value"></param>
    public void NextFurniture(bool value)
    {
        // Remove the last preview object
        GameObject[] previewObjects = GameObject.FindGameObjectsWithTag("Preview");
        foreach (GameObject obj in previewObjects)
        {
            GameObject.Destroy(obj);
        }
        if (_furniturePointer >= furniturePrefabs.Count - 1)
        {
            _furniturePointer = 0;
        }
        else
        {
            _furniturePointer ++;
        }
        // Instantiate the new preview object
        GameObject newObject = GameObject.Instantiate(furniturePrefabs[_furniturePointer], findFreeFloatingSpace(), Quaternion.identity);
        newObject.transform.parent = _menu.transform;
        newObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        newObject.tag = "Preview";
    }

    /// <summary>
    /// Show the previous furniture prefab in the list
    /// </summary>
    /// <param name="value"></param>
    public void PreviousFurniture(bool value)
    {
        // Remove the last preview object
        GameObject[] previewObjects = GameObject.FindGameObjectsWithTag("Preview");
        foreach (GameObject obj in previewObjects)
        {
            GameObject.Destroy(obj);
        }
        if(_furniturePointer == 0)
        {
            _furniturePointer = furniturePrefabs.Count - 1;
        }
        else
        {
            _furniturePointer--;
        }
        // Instantiate the new preview object
        GameObject newObject = GameObject.Instantiate(furniturePrefabs[_furniturePointer], findFreeFloatingSpace(), Quaternion.identity);
        newObject.transform.parent = _menu.transform;
        newObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        newObject.tag = "Preview";
    }

    /// <summary>
    /// Lets the user place the selected furniture in the room
    /// </summary>
    /// <param name="value"></param>
    public void SelectFurniture(bool value)
    {
        // Remove the last preview object
        GameObject[] previewObjects = GameObject.FindGameObjectsWithTag("Preview");
        foreach (GameObject obj in previewObjects)
        {
            GameObject.Destroy(obj);
        }
        GameObject newObject = GameObject.Instantiate(furniturePrefabs[_furniturePointer], findFreeFloorSpace(furniturePrefabs[_furniturePointer]), Quaternion.identity);
        newObject.tag = "Furniture";
        // Add furniture to the current room's list of furniture
        _currentRoom.AddFurniture(furniturePrefabs[_furniturePointer], newObject);
        _currentRoom.UpdateTransforms();
        _currentRoom.SaveRoom();
    }

    private Vector3 findFreeFloorSpace(GameObject newObject)
    {
        Vector3 basePosition = user.transform.position + user.transform.forward * 2;
        basePosition.y = newObject.transform.position.y; // use the y position of the object to be placed

        float offset = 0.2f; // Distance to move left or right
        int maxAttempts = 20; // Maximum number of attempts to find a free space

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 freeSpace = basePosition + user.transform.right * (i % 2 == 0 ? offset * (i / 2) : -offset * (i / 2));

            if (!Physics.Raycast(user.transform.position, freeSpace - user.transform.position, out RaycastHit hit, 2f))
            {
                // Check if the space is on the ground
                if (Physics.Raycast(freeSpace, Vector3.down, out RaycastHit groundHit, Mathf.Infinity))
                {
                    freeSpace.y = basePosition.y + groundHit.point.y;
                    return freeSpace;
                }
            }
        }
        Debug.Log("No free space found");
        return basePosition;
    }

    private Vector3 findFreeFloatingSpace()
    {
        Vector3 basePosition = user.transform.position + user.transform.forward * 1;
        basePosition.y = user.transform.position.y;

        float offset = 0.2f; // Distance to move left or right
        int maxAttempts = 20; // Maximum number of attempts to find a free space

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 freeSpace = basePosition + user.transform.right * (i % 2 == 0 ? offset * (i / 2) : -offset * (i / 2));

            if (!Physics.Raycast(user.transform.position, freeSpace - user.transform.position, out RaycastHit hit, 2f))
            {
                return freeSpace;
            }
        }
        Debug.Log("No free space found");
        return basePosition;
    }
}
