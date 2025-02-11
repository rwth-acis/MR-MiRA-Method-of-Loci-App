using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Oculus.Interaction;
using TMPro;
using GameObject = UnityEngine.GameObject;

public class RoomManager : MonoBehaviour
{
    [Tooltip("The instance of the RoomManager")]
    public static RoomManager Instance { get; private set; }
    [Tooltip("The agent controller")]
    public AgentController agentController;
    [Tooltip("The material for the walls")]
    public Material wallColour;
    [Tooltip("The list of all furniture prefabs")]
    public List<GameObject> furniturePrefabs = new List<GameObject>();
    [Tooltip("The list of all door prefabs")]
    public List<GameObject> doorPrefabs = new List<GameObject>();
    [Tooltip("The center eye anchor of the user")]
    [SerializeField] public GameObject user;
    [Tooltip("The Camera Rig")]
    [SerializeField] public GameObject cameraRig;
    [Tooltip("The locus halo prefab")]
    [SerializeField] public GameObject locusHalo;
    [Tooltip("The tooltip prefab")]
    [SerializeField] public GameObject tooltip;
    [Tooltip("The list for the evaluation")]
    public List<GameObject> evaluationList = new List<GameObject>();

    [Tooltip("Whether the layout mode is active")]
    public bool layoutMode { get; set; } = false;
    [Tooltip("Whether the reuse mode is active")]
    public bool reusePalace { get; set; } = false;

    // All buttons in the user menu
    public GameObject newRoomButton;
    public GameObject backButton;
    public GameObject forwardButton;
    public GameObject openLociStoreButton;
    public GameObject openFurnitureStoreButton;
    public GameObject nextWallColourButton;
    public GameObject previousWallColourButton;
    // public GameObject nextFurnitureButton;
    // public GameObject selectButton;
    // public GameObject previousFurnitureButton;
    public GameObject changeUser;
    public GameObject audioPauseButton;
    public GameObject audioReplayButton;
    public GameObject furniturePhaseButton;
    public GameObject endFurniturePhaseButton;
    public GameObject listPhaseButton;
    public GameObject storyPhaseButton;
    public GameObject numberPhaseButton;
    public GameObject confirmButton;

    private List<User> _users = new List<User>();
    private Camera _cam;
    private List<Color> _wallColours = new List<Color>();
    private int _colourPointer;
    private User _currentUser;
    private Room _currentRoom;
    private int _furniturePointer = 0;
    private GameObject _menu;
    private GameObject _lociMenu;
    private GameObject _devMenu;
    private GameObject _furnitureMenu;
    private bool _isObjectConfirmed = false;
    private int _deleteCounter = 0;
    private int furniturePlacementCounter = 0;
    private int _devMenuCounter;
    // whether doors can be used to switch rooms
    private bool _doorChangeRoom = false;

    //for testing
    [SerializeField] public GameObject furniture;
    [SerializeField] public GameObject furniture2;


    private void Awake()
    {
        // Ensure that there is only one instance of RoomManager
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
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
        cameraRig = GameObject.Find("[BuildingBlock] Camera Rig");
        _menu = GameObject.FindGameObjectWithTag("Menu");
        _lociMenu = GameObject.FindGameObjectWithTag("LociMenu");
        _lociMenu.SetActive(false);
        _devMenu = GameObject.FindGameObjectWithTag("DevMenu");
        _devMenu.SetActive(false);
        _furnitureMenu = GameObject.FindGameObjectWithTag("FurnitureMenu");
        _furnitureMenu.SetActive(false);
        ActivateStartButtons();
        _cam = user.GetComponent(typeof(Camera)) as Camera;
        ModeSelector mode = FindObjectOfType<ModeSelector>();
        layoutMode = mode.layoutMode;
        reusePalace = mode.reuseMode;
        GameObject.Destroy(mode);
        if (layoutMode && !reusePalace)
        {
            agentController.DeactivateAgent();
        }
        Debug.Log("Layout mode: " + layoutMode);
        //Set up the colour list
        colourListSetup();
        LoadUser("Lena");
    }

    /// <summary>
    /// Only show user change, audio buttons and option to start the furniture phase at the beginning
    /// </summary>
    private void ActivateStartButtons()
    {
        // Deactivate and activate only necessary buttons
        newRoomButton.SetActive(false);
        backButton.SetActive(false);
        forwardButton.SetActive(false);
        openLociStoreButton.SetActive(false);
        nextWallColourButton.SetActive(false);
        previousWallColourButton.SetActive(false);
        // nextFurnitureButton.SetActive(false);
        // selectButton.SetActive(false);
        // previousFurnitureButton.SetActive(false);
        openFurnitureStoreButton.SetActive(false);
        changeUser.SetActive(true);
        audioPauseButton.SetActive(true);
        audioReplayButton.SetActive(true);
        furniturePhaseButton.SetActive(true);
        endFurniturePhaseButton.SetActive(false);
        listPhaseButton.SetActive(false);
        storyPhaseButton.SetActive(false);
        numberPhaseButton.SetActive(false);
        confirmButton.SetActive(false);
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
    public async void LoadUser(string username)
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
        await LoadRoom(_currentRoom, true);
    }

    /// <summary>
    /// Creates a new room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "New Room"</param>
    public async void CreateRoom(bool value)
    {
        // Position for door raycast
        Vector3 raycastOrigin = user.transform.position;
        // add door to the current room
        // TODO make sure that the rooms are actually connected
        Quaternion rotation = Quaternion.LookRotation(- user.transform.forward);
        GameObject door = GameObject.Instantiate(doorPrefabs[0], raycastOrigin, rotation);
        ObjectSnapper objectsnapper = door.GetComponent<ObjectSnapper>();
        objectsnapper.snapToFloor();
        objectsnapper.snapToWall();
        _currentRoom.AddFurniture(doorPrefabs[0], door);
        _currentRoom.UpdateTransforms();

        // Create a new room
        int newRoomID = _currentUser.GetFreeRoomID();
        Room room = new Room(newRoomID);
        _currentUser.AddRoom(room);
        _currentRoom = _currentUser.GetCurrentRoom();
        await LoadRoom(room);
        // After creating 6 rooms the user is allowed to end the furniture phase
        if (newRoomID == 6)
        {
            endFurniturePhaseButton.SetActive(true);
            agentController.PlayAudio(agentController.furnitureCanBeFinishedAudio);
        }

        // Add a door to the new room
        door = GameObject.Instantiate(doorPrefabs[1], raycastOrigin, user.transform.rotation);
        objectsnapper = door.GetComponent<ObjectSnapper>();
        objectsnapper.snapToFloor();
        objectsnapper.snapToWall();
        room.AddFurniture(doorPrefabs[1], door);
        room.UpdateTransforms();
        //await room.UpdateAnchors();
        _currentRoom.SaveRoom();
        agentController.PlayAudio(agentController.newRoomDoorAudio);
        agentController.PointAtObject(door);
    }

    /// <summary>
    /// Changes the scene to the next room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Next Room"</param>
    public async void NextScene(bool value)
    {
        Room nextRoom = _currentUser.NextRoom();
        if (nextRoom == null)
        {
            Debug.Log("There is no next room available in RoomManager");
            return; // TODO: Error Message or Grey out Button
        }
        await LoadRoom(nextRoom);
        _currentRoom = _currentUser.GetCurrentRoom();
    }

    /// <summary>
    /// Changes the scene to the previous room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Previous Room"</param>
    public async void PreviousScene(bool value)
    {
        Room previousRoom = _currentUser.PreviousRoom();
        if (previousRoom == null)
        {
            Debug.Log("There is no previous room available in RoomManager");
            return; // TODO: Error Message or Grey out Button
        }
        await LoadRoom(previousRoom);
        _currentRoom = _currentUser.GetCurrentRoom();
    }

    /// <summary>
    /// Saves the current room to JSON and loads the given room
    /// </summary>
    /// <param name="room">The room to be loaded</param>
    public async Task LoadRoom(Room room, bool firstLoad = false)
    {
        await room.LoadUnboundAnchors(room.FurnitureAnchors, room.RepresentationAnchors);
        // We clear the instances of the *new* room, to forgo issues with old instances having no transforms, etc...
        room.FurnitureInstances.Clear();
        room.RepresentationInstances.Clear();
        if (!firstLoad)
        {
            ReturnToRealPosition();
            Debug.Log("1");
            Debug.Log("5");
            //_currentRoom.UpdateTransforms();
            room.Loci.Clear();
            // Save the current room to JSON
            _currentRoom.UpdateTransforms();
            await _currentRoom.UpdateAnchors();
            _currentRoom.SaveRoom();
            _currentUser.SaveUser();
            // First remove all furniture from the scene
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Furniture");
            foreach (GameObject obj in allObjects)
            {
                GameObject.Destroy(obj);
            }
            room.RepresentationInstances.Clear();
            room.Loci.Clear();
            // Deletes either the representations or the loci halo objects
            GameObject[] allRepresentations = GameObject.FindGameObjectsWithTag("Information");
            foreach (GameObject obj in allRepresentations)
            {
                GameObject.Destroy(obj);
            }
        }

        // Load the room
        Debug.Log("LENA: We are loading room " + room.ID);
        if (room != null && room.HasFurniture())
        {
            // List of the prefabs of the furniture in the room
            List<GameObject> roomFurniture = room.Furniture;
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
        if(!layoutMode)
        {
            if (room != null && room.HasRepresentations())
            {
                List<GameObject> roomRepresentations = room.Representations;
                for (int i = 0; i < roomRepresentations.Count; i++)
                {
                    GameObject myRepresentation = GameObject.Instantiate(roomRepresentations[i]);
                    myRepresentation.tag = "Information";
                    room.AddRepresentationInstance(myRepresentation);
                }
                room.LoadTransforms();
                //room.LoadAnchors();
            }
        }
        else if (layoutMode)
        {
            // mark the loci on the transform positions of the representations
            foreach (var loci in room.Representations)
            {
                GameObject myLocus = GameObject.Instantiate(locusHalo, loci.transform.position, Quaternion.identity);
                myLocus.tag = "Information";
                room.AddLocusInstance(myLocus);
            }
            room.LoadTransformsLoci();
            foreach (var loci in room.Loci)
            {
                ObjectSnapper objectsnapper = loci.GetComponent<ObjectSnapper>();
                objectsnapper.snapToFloor();
            }
            // Make sure the furniture is not grabbable in the layout mode
            foreach (var furniture in room.FurnitureInstances)
            {
                furniture.GetComponentInChildren<Grabbable>().enabled = false;
            }
        }

        if (reusePalace)
        {
            // Show all the representations that have been replaced already
            if (room != null && room.HasRepresentations())
            {
                List<GameObject> roomRepresentations = room.Representations;
                for (int i = 0; i < room.ReplacingLocusIndex; i++)
                {
                    GameObject myRepresentation = GameObject.Instantiate(roomRepresentations[i]);
                    myRepresentation.tag = "Information";
                    room.AddRepresentationInstance(myRepresentation);
                }
                room.LoadTransforms();
                //room.LoadAnchors();
            }
        }
        room.LoadAnchors();
        Debug.Log("6");
    }

    /// <summary>
    /// Adds furniture to the current room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Open Loci Store"</param>
    public void OpenLociMenu(bool value)
    {
        _lociMenu.SetActive(true);
    }

    /// <summary>
    /// Adds furniture to the current room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Open Furniture Store"</param>
    public void OpenFurnitureMenu(bool value)
    {
        _furnitureMenu.SetActive(true);
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

    /// <summary>
    ///  Change to the next wall colour in the list
    /// </summary>
    /// <param name="value">The value of the button "Next Wallcolour"</param>
    public void NextWallColour(bool value)
    {
        _currentRoom.ChangeWallColour(_wallColours[_colourPointer]);
        if (_colourPointer == _wallColours.Count - 1)
        {
            _colourPointer = 0;
        }
        else
        {
            _colourPointer++;
        }
        wallColour.color = _currentRoom.WallColour;
        _currentRoom.UpdateTransforms();
        _currentRoom.SaveRoom();
        Debug.Log("Wall colour: " + _currentRoom.WallColour);
    }

    /// <summary>
    /// Changes to the previous wall colour in the list
    /// </summary>
    /// <param name="value">The value of the button "Previous Wallcolour"</param>
    public void PreviousWallColour(bool value)
    {
        _currentRoom.ChangeWallColour(_wallColours[_colourPointer]);
        if (_colourPointer == 0)
        {
            _colourPointer = _wallColours.Count - 1;
        }
        else
        {
            _colourPointer--;
        }
        wallColour.color = _currentRoom.WallColour;
        _currentRoom.UpdateTransforms();
        _currentRoom.SaveRoom();
        Debug.Log("Wall colour: " + _currentRoom.WallColour);
    }

    /// <summary>
    /// To set up the list of colours for the walls
    /// </summary>
    private void colourListSetup()
    {
        _wallColours.Add(Color.white);
        _wallColours.Add(Color.yellow);
        _wallColours.Add(new Color(0.94f, 0.9f, 0.55f));
        _wallColours.Add(new Color(1f,0.5f,0));
        _wallColours.Add(new Color(1f, 0.87f, 0.68f));
        _wallColours.Add(Color.magenta);
        _wallColours.Add(new Color(1f, 0.71f, 0.76f));
        _wallColours.Add(Color.red);
        _wallColours.Add(new Color(0.5f, 0f, 0f));
        _wallColours.Add(Color.green);
        _wallColours.Add(new Color(0f, 0.5f, 0f));
        _wallColours.Add(new Color(0.77f, 0.88f, 0.78f));
        _wallColours.Add(Color.cyan);
        _wallColours.Add(new Color(0.69f, 0.77f, 0.87f));
        _wallColours.Add(Color.blue);
        _wallColours.Add(new Color(0.6f,0.3f,1f));
        _wallColours.Add(new Color(0.87f, 0.68f, 0.98f));
        _wallColours.Add(Color.grey);
        _wallColours.Add(Color.black);
        _colourPointer = 0;
    }

    /// <summary>
    /// Shows the next furniture prefab in the list
    /// </summary>
    /// <param name="value">The value of the button "Next Furniture"</param>
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
    /// <param name="value">The value of the button "Previous Furniture"</param>
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
    /// <param name="value">The value of the button "Select Furniture"</param>
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
        // Let agent play an audio when the first two items have been placed
        if(_currentRoom==_currentUser.GetFirstRoom())
        {
            // Increment the counter and check if two items have been placed
            furniturePlacementCounter++;
            if (furniturePlacementCounter == 2)
            {
                agentController.PlayAudio(agentController.furniturePlacement2Audio);
            }
        }
    }

    /// <summary>
    /// Finds a free space on the floor to place the object
    /// </summary>
    /// <param name="newObject">The object to be placed</param>
    /// <returns>A free position on the ground</returns>
    private Vector3 findFreeFloorSpace(GameObject newObject)
    {
        Vector3 basePosition = user.transform.position + user.transform.forward;
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

    /// <summary>
    /// Finds a free space to place an object in the air
    /// </summary>
    /// <returns>A free position to float in front of the user</returns>
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

    /// <summary>
    /// Adds a GameObject to the current room, its transform will be saved as a loci
    /// </summary>
    /// <param name="loci">The GameObject to place</param>
    public async void AddLoci(GameObject loci, string tooltipText = null)
    {
        if (reusePalace)
        {
            //TODO finish this with new menu
            // Replacing the old representation with a new one
            GameObject newObject2 = GameObject.Instantiate(loci, findFreeFloatingSpace(), Quaternion.identity);
            newObject2.tag = "Information";
            _currentRoom.ReplaceRepresentation(0, loci, newObject2);
        }
        else
        {
            GameObject newObject = GameObject.Instantiate(loci, findFreeFloatingSpace(), Quaternion.identity);
            newObject.transform.position = findFreeFloorSpace(newObject);
            newObject.tag = "Information";

            // Add a tooltip as a child the new object
            if (tooltipText != null)
            {
                // TODO give the tooltip texts or let the user input them
                GameObject tooltipObject = GameObject.Instantiate(tooltip, newObject.transform.position, Quaternion.identity);
                // Set the tooltip to be on top of the object
                ObjectSnapper objectsnapper = newObject.GetComponent<ObjectSnapper>();
                float TooltipY = objectsnapper.getHighestPoint();
                tooltipObject.transform.position = new Vector3(tooltipObject.transform.position.x, TooltipY, tooltipObject.transform.position.z);


                tooltipObject.transform.SetParent(newObject.transform);
                TextMeshProUGUI [] texts = tooltipObject.GetComponentsInChildren<TextMeshProUGUI>();
                string name = newObject.name;
                // remove clone from the name
                name = name.Substring(0, name.Length - 7);
                texts[0].text = name;
                texts[1].text = tooltipText;
            }
            _currentRoom.AddRepresentation(loci, newObject);
        }
        _currentRoom.UpdateTransforms();
        _currentRoom.SaveRoom();
    }

    public void AddFurniture(GameObject furniture)
    {
        GameObject newObject = GameObject.Instantiate(furniturePrefabs[_furniturePointer], findFreeFloorSpace(furniturePrefabs[_furniturePointer]), Quaternion.identity);
        newObject.tag = "Furniture";

        // Add furniture to the current room's list of furniture
        _currentRoom.AddFurniture(furniturePrefabs[_furniturePointer], newObject);
        _currentRoom.UpdateTransforms();
        _currentRoom.SaveRoom();
        // Let agent play an audio when the first two items have been placed
        if(_currentRoom==_currentUser.GetFirstRoom())
        {
            // Increment the counter and check if two items have been placed
            furniturePlacementCounter++;
            if (furniturePlacementCounter == 2)
            {
                agentController.PlayAudio(agentController.furniturePlacement2Audio);
            }
        }
    }

    public void ReturnToRealPosition()
    {
        cameraRig.transform.position = new Vector3(user.transform.localPosition.x, cameraRig.transform.position.y, user.transform.localPosition.z);
        cameraRig.transform.localPosition = Vector3.zero;
        user.transform.position = Vector3.zero;
        cameraRig.transform.rotation = Quaternion.identity;
        user.transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Starts the setup of furniture in the memory palace
    /// </summary>
    /// <param name="value">The value of the button "Furniture Phase"</param>
    public void FurniturePhase(bool value)
    {
        // Deactivate and activate only necessary buttons
        newRoomButton.SetActive(true);
        backButton.SetActive(true);
        forwardButton.SetActive(true);
        openLociStoreButton.SetActive(false);
        nextWallColourButton.SetActive(true);;
        previousWallColourButton.SetActive(true);
        // nextFurnitureButton.SetActive(true);
        // selectButton.SetActive(true);
        // previousFurnitureButton.SetActive(true);
        openFurnitureStoreButton.SetActive(true);
        changeUser.SetActive(true);
        audioPauseButton.SetActive(true);
        audioReplayButton.SetActive(true);
        furniturePhaseButton.SetActive(false);
        endFurniturePhaseButton.SetActive(false);
        listPhaseButton.SetActive(false);
        storyPhaseButton.SetActive(false);
        numberPhaseButton.SetActive(false);
        confirmButton.SetActive(false);
        // Let the agent play the introduction to the furniture setup phase
        agentController.ActivateAgent();
        agentController.PlayAudio(agentController.furnitureIntroductionAudio);
        // TODO
    }

    /// <summary>
    /// Shows one object at a time to the user to place in the memory palace
    /// </summary>
    /// <param name="value"></param>
    public async void ListPhase(bool value)
    {
        _doorChangeRoom = true;
        // Deactivate and activate only necessary buttons
        newRoomButton.SetActive(false);
        backButton.SetActive(true);
        forwardButton.SetActive(true);
        openLociStoreButton.SetActive(false);
        nextWallColourButton.SetActive(false);
        previousWallColourButton.SetActive(false);
        // nextFurnitureButton.SetActive(false);
        // selectButton.SetActive(false);
        // previousFurnitureButton.SetActive(false);
        openFurnitureStoreButton.SetActive(false);
        changeUser.SetActive(true);
        furniturePhaseButton.SetActive(false);
        listPhaseButton.SetActive(false);
        storyPhaseButton.SetActive(true);
        numberPhaseButton.SetActive(false);
        confirmButton.SetActive(true);

        // Let the agent play the introduction to the list phase
        agentController.ActivateAgent();
        agentController.PlayAudio(agentController.listIntroductionAudio);
        for(int i = 0; i < evaluationList.Count; i++)
        {
            AddLoci(evaluationList[i], "");
            agentController.PlayAudioListPhase(i);
            await WaitForUserConfirmation();
            _isObjectConfirmed = false;
        }
    }

    private async Task WaitForUserConfirmation()
    {
        while (!_isObjectConfirmed)
        {
            await Task.Delay(100);
        }
    }

    public void OnConfirmButtonClick(bool value)
    {
        _isObjectConfirmed = value;
    }

    public void StoryPhase(bool value)
    {
        _doorChangeRoom = true;
        // Deactivate and activate only necessary buttons
        newRoomButton.SetActive(false);
        backButton.SetActive(true);
        forwardButton.SetActive(true);
        openLociStoreButton.SetActive(true);
        // Keep the wall colour changeable to connect colour to the objects
        nextWallColourButton.SetActive(true);
        previousWallColourButton.SetActive(true);
        // nextFurnitureButton.SetActive(false);
        // selectButton.SetActive(false);
        // previousFurnitureButton.SetActive(false);
        openFurnitureStoreButton.SetActive(false);
        changeUser.SetActive(true);
        audioPauseButton.SetActive(true);
        audioReplayButton.SetActive(true);
        furniturePhaseButton.SetActive(false);
        endFurniturePhaseButton.SetActive(false);
        listPhaseButton.SetActive(false);
        storyPhaseButton.SetActive(false);
        numberPhaseButton.SetActive(true);
        confirmButton.SetActive(true);
        // Let the agent play the introduction to the story phase
        agentController.ActivateAgent();
        // TODO
    }

    public void NumberPhase(bool value)
    {
        _doorChangeRoom = true;
        // Deactivate and activate only necessary buttons
        newRoomButton.SetActive(false);
        backButton.SetActive(true);
        forwardButton.SetActive(true);
        openLociStoreButton.SetActive(true);
        // Keep the wall colour changeable to connect colour to the objects
        nextWallColourButton.SetActive(true);
        previousWallColourButton.SetActive(true);
        // nextFurnitureButton.SetActive(false);
        // selectButton.SetActive(false);
        // previousFurnitureButton.SetActive(false);
        openFurnitureStoreButton.SetActive(false);
        changeUser.SetActive(true);
        audioPauseButton.SetActive(true);
        audioReplayButton.SetActive(true);
        furniturePhaseButton.SetActive(false);
        endFurniturePhaseButton.SetActive(false);
        listPhaseButton.SetActive(false);
        storyPhaseButton.SetActive(false);
        numberPhaseButton.SetActive(false);
        confirmButton.SetActive(true);
        // Let the agent play the introduction to the number phase
        agentController.ActivateAgent();
        // TODO
    }

    public void DeleteJSON(bool value)
    {
        _deleteCounter++;
        if (_deleteCounter == 2)
        {
            string path = Path.Combine(Application.persistentDataPath, _currentUser._name + ".json");
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("File deleted: " + path);
            }
            else
            {
                Debug.Log("File not found: " + path);
            }
            _deleteCounter = 0;
            // delete all objects from the scene
            GameObject[] allFurnitureObjects = GameObject.FindGameObjectsWithTag("Furniture");
            foreach (GameObject obj in allFurnitureObjects)
            {
                GameObject.Destroy(obj);
            }
            GameObject[] allRepresentationObjects = GameObject.FindGameObjectsWithTag("Information");
            foreach (GameObject obj in allRepresentationObjects)
            {
                GameObject.Destroy(obj);
            }
            LoadUser(_currentUser._name);
        }
    }

    //Audio Control
    public void PauseAudio(bool value)
    {
        agentController.PauseAudio();
    }

    public void ReplayAudio(bool value)
    {
        agentController.ReplayAudio();
    }

    public void OpenDevMenu(bool value)
    {
        // Only activate when pressed 4 times
        _devMenuCounter++;
        if(_devMenuCounter == 4)
        {
            _devMenuCounter = 0;
            _devMenu.SetActive(true);
        }
    }

    public void CloseDevMenu(bool value)
    {
        _devMenu.SetActive(false);
    }

    public void FinishFurniturePhase(bool value)
    {
        LoadRoom(_currentUser.GetFirstRoom());
        endFurniturePhaseButton.SetActive(false);
        listPhaseButton.SetActive(true);
        agentController.PlayAudio(agentController.furnitureFinishAudio);
    }

    /// <summary>
    /// To switch to the next room when the user is grabbing a door
    /// Only possible when the rooms are set up
    /// </summary>
    public void DoorNextScene()
    {
        if(_doorChangeRoom)
        {
            NextScene(true);
        }
    }

    /// <summary>
    /// To switch to the previous room when the user is grabbing a door
    /// Only possible when the rooms are set up
    /// </summary>
    public void DoorPreviousScene()
    {
        if (_doorChangeRoom)
        {
            PreviousScene(true);
        }
    }
}
