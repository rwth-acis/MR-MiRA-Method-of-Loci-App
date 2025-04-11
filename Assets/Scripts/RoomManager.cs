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
    [SerializeField] public List<GameObject> evaluationList = new List<GameObject>();
    [Tooltip("The wood texture in colour")]
    [SerializeField] public Texture woodTexture;
    [Tooltip("The wood texture in black and white")]
    [SerializeField] public Texture woodTextureBW;
    [Tooltip("The wood  material")]
    [SerializeField] public Material woodMaterial;

    [Tooltip("Whether the layout mode is active")]
    public bool layoutMode { get; set; } = false;
    [Tooltip("Whether the reuse mode is active")]
    public bool reusePalace { get; set; } = false;

    [Tooltip("If true, the next touched object will be deleted")]
    public bool isDeleteMode { get; private set; } = false;

    // All buttons in the user menu
    public GameObject newRoomButton;
    public GameObject backButton;
    public GameObject forwardButton;
    public GameObject openLociStoreButton;
    public GameObject openFurnitureStoreButton;
    public GameObject deleteButton;
    public GameObject nextWallColourButton;
    public GameObject previousWallColourButton;
    public GameObject changeUser;
    public GameObject audioPauseButton;
    public GameObject audioReplayButton;
    public GameObject greyOutFurnitureButton;
    public GameObject furniturePhaseButton;
    public GameObject endFurniturePhaseButton;
    public GameObject listPhaseButton;
    public GameObject storyPhaseButton;
    public GameObject numberPhaseButton;
    public GameObject confirmButton;
    public GameObject devmenuButton;
    public GameObject resetPositionButton;
    public GameObject goToFirstRoomButton;
    public GameObject addMoreRoomsButton;
    public GameObject enoughRoomsButton;
    public GameObject spotLight;

    private List<User> _users = new List<User>();
    private Camera _cam;
    private List<Color> _wallColours = new List<Color>();
    private int _colourPointer;
    private User _currentUser;
    private Room _currentRoom;
    private int _furniturePointer = 0;
    private List<string> _evaluationStory = new List<string>();
    private string _currentStoryPart = "";

    // The different parts of the menu that can be shown
    private GameObject _menu;
    private GameObject _lociMenu;
    private GameObject _devMenu;
    private GameObject _storyText;
    private GameObject _tooltipMenu;
    private GameObject _furnitureMenu;

    // Counter
    private int _deleteCounter = 0;
    private int furniturePlacementCounter = 0;
    private int _devMenuCounter;
    private int _recommendationsIndex;

    // Flags
    private bool _doorChangeRoom = false; // whether doors can be used to switch rooms
    private bool _furniturePhaseRememberLayout = false; // used to find out if the user is finished placing the furniture and looks at it again to remember the layout
    private bool _addingMoreRooms = false; // used to find out if the user wants to add more rooms
    private bool _finished = false; // used to find out if the user is finished setting up the representations
    private bool _learning = false; // used to find out if the user is in one of the learning phase
    private bool _continueEvaluation = false; // used to find out if the user wants to continue the evaluation

    // used to await other methods or action of the user
    private bool _isObjectConfirmed = false;
    private bool _isNextRoom = false;
    private bool _finishAddingMoreRooms = false;


    private void Awake()
    {
        // Ensure that there is only one instance of RoomManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        agentController = FindObjectOfType<AgentController>();
        user = GameObject.Find("CenterEyeAnchor");
        cameraRig = GameObject.Find("[BuildingBlock] Camera Rig");
        _menu = GameObject.FindGameObjectWithTag("Menu");
        _cam = user.GetComponent(typeof(Camera)) as Camera;
        ActivateStartButtons();

        _lociMenu = GameObject.FindGameObjectWithTag("LociMenu");
        _lociMenu.SetActive(false);
        _devMenu = GameObject.FindGameObjectWithTag("DevMenu");
        _devMenu.SetActive(false);
        _furnitureMenu = GameObject.FindGameObjectWithTag("FurnitureMenu");
        _furnitureMenu.SetActive(false);
        _storyText = GameObject.FindGameObjectWithTag("Story");
        _storyText.SetActive(false);
        _tooltipMenu = GameObject.FindGameObjectWithTag("Tooltip");
        _tooltipMenu.SetActive(false);
        spotLight.SetActive(false);

        ModeSelector mode = FindObjectOfType<ModeSelector>();
        layoutMode = mode.layoutMode;
        reusePalace = mode.reuseMode;
        if (reusePalace)
        {
            _lociMenu.SetActive(true);
            goToFirstRoomButton.SetActive(true);
        }
        GameObject.Destroy(mode);
        if (layoutMode && !reusePalace)
        {
            agentController.DeactivateAgent();
            setButtonsPhases("layout");
        }

        storyListSetup();
        colourListSetup();
        woodMaterial.mainTexture = woodTexture;
        LoadUser("Lena");
    }

    /// <summary>
    /// Only show user menu buttons that are necessary for the start
    /// </summary>
    private void ActivateStartButtons()
    {
        newRoomButton.SetActive(false);
        backButton.SetActive(false);
        forwardButton.SetActive(false);
        openLociStoreButton.SetActive(false);
        nextWallColourButton.SetActive(false);
        previousWallColourButton.SetActive(false);
        openFurnitureStoreButton.SetActive(false);
        deleteButton.SetActive(false);
        changeUser.SetActive(true);
        audioPauseButton.SetActive(false);
        audioReplayButton.SetActive(false);
        greyOutFurnitureButton.SetActive(false);
        furniturePhaseButton.SetActive(true);
        endFurniturePhaseButton.SetActive(false);
        listPhaseButton.SetActive(false);
        storyPhaseButton.SetActive(false);
        numberPhaseButton.SetActive(false);
        confirmButton.SetActive(false);
        devmenuButton.SetActive(true);
        resetPositionButton.SetActive(true);
        goToFirstRoomButton.SetActive(false);
        addMoreRoomsButton.SetActive(false);
        enoughRoomsButton.SetActive(false);
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
            newUser = new User(username);
            Room room = new Room(0);
            newUser.AddRoom(room);
        }
        else
        {
            // Read the user's save file
            string userjson = File.ReadAllText(path);
            newUser = new User(username, JsonUtility.FromJson<SaveData>(userjson));
            if (!layoutMode)
            {
                goToFirstRoomButton.SetActive(true);
                listPhaseButton.SetActive(true);
                storyPhaseButton.SetActive(true);
                numberPhaseButton.SetActive(true);
            }
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
        Quaternion rotation = Quaternion.LookRotation(- user.transform.forward);
        GameObject door = Instantiate(doorPrefabs[0], raycastOrigin, rotation);
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
        if (newRoomID == 5)
        {
            endFurniturePhaseButton.SetActive(true);
            agentController.PlayAudio(agentController.furnitureCanBeFinishedAudio);
        }

        // Add a door to the new room
        door = Instantiate(doorPrefabs[1], raycastOrigin, user.transform.rotation);
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
    /// Changes to the next room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Next Room"</param>
    public async void NextRoom(bool value)
    {
        Room nextRoom = _currentUser.NextRoom();
        if (nextRoom == null)
        {
            return;
        }
        // Deactivate the buttons to prevent multiple presses
        forwardButton.SetActive(false);
        backButton.SetActive(false);
        await LoadRoom(nextRoom);
        forwardButton.SetActive(true);
        backButton.SetActive(true);
        _currentRoom = _currentUser.GetCurrentRoom();

        // If we are in the last room, we deactivate the forward button
        if (_currentRoom.ID == _currentUser.GetFreeRoomID() -1)
        {
            forwardButton.SetActive(false);
        }
        // If we are not in the first room, we activate the back button
        if (_currentRoom.ID != 0)
        {
            backButton.SetActive(true);
        }
    }

    /// <summary>
    /// Changes to the previous room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Previous Room"</param>
    public async void PreviousRoom(bool value)
    {
        Room previousRoom = _currentUser.PreviousRoom();
        if (previousRoom == null)
        {
            return;
        }
        // Deactivate the buttons to prevent multiple presses
        backButton.SetActive(false);
        forwardButton.SetActive(false);
        await LoadRoom(previousRoom);
        backButton.SetActive(true);
        forwardButton.SetActive(true);
        _currentRoom = _currentUser.GetCurrentRoom();
        // If we are not in the last room, we activate the forward button
        if (_currentRoom.ID != _currentUser.GetFreeRoomID() -1)
        {
            forwardButton.SetActive(true);
        }
        // If we are in the first room, we deactivate the back button
        if (_currentRoom.ID == 0)
        {
            backButton.SetActive(false);
        }
    }

    /// <summary>
    /// Saves the current room to JSON and loads the given room
    /// </summary>
    /// <param name="room">The room to be loaded</param>
    /// <param name="firstLoad">If this is the first load at app start</param>
    private async Task LoadRoom(Room room, bool firstLoad = false)
    {
        await room.LoadUnboundAnchors(room.FurnitureAnchors, room.RepresentationAnchors);
        // We clear the instances of the *new* room, to forgo issues with old instances having no transforms, etc...
        room.FurnitureInstances.Clear();
        room.RepresentationInstances.Clear();
        if (!firstLoad)
        {
            // Reset the position of the user
            ReturnToRealPosition(true);
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
        if (room != null && room.HasFurniture())
        {
            // List of the prefabs of the furniture in the room
            List<GameObject> roomFurniture = room.Furniture;
            for (int i = 0; i < roomFurniture.Count; i++)
            {
                GameObject myObject = Instantiate(roomFurniture[i]);
                myObject.tag = "Furniture";
                room.AddFurnitureInstance(myObject);
            }
            room.LoadTransforms();
        }

        if (room != null && room.WallColour != null)
        {
            wallColour.color = room.WallColour;
        }

        if(!layoutMode)
        {
            if (room != null && room.HasRepresentations())
            {
                List<GameObject> roomRepresentations = room.Representations;
                for (int i = 0; i < roomRepresentations.Count; i++)
                {
                    GameObject myRepresentation = Instantiate(roomRepresentations[i]);
                    myRepresentation.tag = "Information";
                    room.AddRepresentationInstance(myRepresentation);
                }
                room.LoadTransforms();
            }
        }
        else if (layoutMode)
        {
            // mark the loci on the transform positions of the representations
            foreach (var loci in room.Representations)
            {
                GameObject myLocus = Instantiate(locusHalo, loci.transform.position, Quaternion.identity);
                myLocus.tag = "Information";
                room.AddLocusInstance(myLocus);
            }
            room.LoadTransformsLoci();
            foreach (var loci in room.Loci)
            {
                ObjectSnapper objectSnapper = loci.GetComponent<ObjectSnapper>();
                objectSnapper.snapToFloor();
            }
            // Make sure the furniture is not grabbable in the layout mode
            foreach (GameObject instance in room.FurnitureInstances)
            {
                instance.GetComponentInChildren<Grabbable>().enabled = false;
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
                    GameObject myRepresentation = Instantiate(roomRepresentations[i]);
                    myRepresentation.tag = "Information";
                    room.AddRepresentationInstance(myRepresentation);
                }
                room.LoadTransforms();
            }
        }
        room.LoadAnchors();
        if (_furniturePhaseRememberLayout && room.ID == _currentUser.GetFreeRoomID()-1)
        {
            // If we are in the last room again after trying to remember the layout
            // we play an audio to prepare for the learning part
            agentController.PlayAudio(agentController.furniturePhaseRememberLayoutLastRoomAudio);
        }
        else if (!_finished && _learning && room.ID == _currentUser.GetFreeRoomID()-1)
        {
            // If we are in the last room in any of the learning phases and need more rooms
            addMoreRoomsButton.SetActive(true);
        }
        _isNextRoom = true;
    }

    /// <summary>
    /// Opens the loci store when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Open Loci Store"</param>
    public void OpenLociMenu(bool value)
    {
        _lociMenu.SetActive(true);
    }

    /// <summary>
    /// Activate the furniture menu when the corresponding button is pressed
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
        if (_colourPointer == _wallColours.Count - 1)
        {
            _colourPointer = 0;
        }
        else
        {
            _colourPointer++;
        }
        _currentRoom.ChangeWallColour(_wallColours[_colourPointer]);
        wallColour.color = _currentRoom.WallColour;
        _currentRoom.UpdateTransforms();
        _currentRoom.SaveRoom();
    }

    /// <summary>
    /// Changes to the previous wall colour in the list
    /// </summary>
    /// <param name="value">The value of the button "Previous Wallcolour"</param>
    public void PreviousWallColour(bool value)
    {
        if (_colourPointer == 0)
        {
            _colourPointer = _wallColours.Count - 1;
        }
        else
        {
            _colourPointer--;
        }
        _currentRoom.ChangeWallColour(_wallColours[_colourPointer]);
        wallColour.color = _currentRoom.WallColour;
        _currentRoom.UpdateTransforms();
        _currentRoom.SaveRoom();
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
    ///  To set up the list of story parts for the evaluation
    /// </summary>
    private void storyListSetup()
    {
        _evaluationStory.Add("Gestern habe ich sehr viel erlebt.");
        _evaluationStory.Add("Ich war auf dem Weg zum Air-Hockey spielen, mit meinen Freunden, Mira und Loki");
        _evaluationStory.Add("da sah ich mitten in der Stadt einen roten Bison.");
        _evaluationStory.Add("Das ist komisch, dachte ich mir und drehte um, um das Tier nicht zu erschrecken.");
        _evaluationStory.Add("Da lief mir auch schon ein Geist mit blauer Sonnenbrille entgegen.");
        _evaluationStory.Add("Dieser stolperte und verlor eine kleine Münze.");
        _evaluationStory.Add("Die Münze steckte ich in meine gelbe Tüte und ging weiter. ");
        _evaluationStory.Add("Später kann ich mir eine Zitrone von dem Geld kaufen, dachte ich. ");
        _evaluationStory.Add("Zwei Feuerwehrautos, ");
        _evaluationStory.Add("ein Rennauto ");
        _evaluationStory.Add("und ein Rentier warten schon am Bahnhof. ");
        _evaluationStory.Add("Nach 3 Stunden Warten gehe ich wieder nach Hause");
        _evaluationStory.Add("weil der Zug ausgefallen ist. ");
        _evaluationStory.Add("Ein Taxi ist mir zu teuer ");
        _evaluationStory.Add("und der Schlitten fährt nicht ohne mein Pferd. ");
        _evaluationStory.Add("Das ist gerade im Urlaub.");
    }

    /// <summary>
    /// To grey out the furniture objects in the room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Grey out furniture"</param>
    public void GreyOutFurnitureButton(bool value)
    {
        if(woodMaterial.mainTexture == woodTexture)
        {
            woodMaterial.mainTexture = woodTextureBW;
        }
        else
        {
            woodMaterial.mainTexture = woodTexture;
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
        return basePosition;
    }

    /// <summary>
    /// Finds a free space to place an object in the air
    /// </summary>
    /// <returns>A free position to float in front of the user</returns>
    private Vector3 findFreeFloatingSpace()
    {
        Vector3 basePosition = user.transform.position + user.transform.forward * 1;
        basePosition.y = 1.5f; // Start at a height of 1.5m

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
        return basePosition;
    }

    /// <summary>
    /// Adds a GameObject to the current room, its transform will be saved as a loci
    /// </summary>
    /// <param name="loci">The GameObject to place</param>
    public void AddLoci(GameObject loci, string tooltipText = null)
    {
        // for evaluation story part
        if (_evaluationStory.Contains(_currentStoryPart) && _evaluationStory.IndexOf(_currentStoryPart) < _evaluationStory.Count)
        {
            tooltipText = _currentStoryPart;
        }
        else if (_currentStoryPart != "")
        {
            tooltipText = _currentStoryPart;
        }
        if (reusePalace)
        {
            if(_currentRoom.Representations.Count <= _currentRoom.ReplacingLocusIndex)
            {
                return;
            }
            // Replacing the old representation with a new one
            GameObject newObject2 = Instantiate(loci, findFreeFloatingSpace(), Quaternion.identity);
            newObject2.tag = "Information";
            _currentRoom.ReplaceRepresentation(loci, newObject2);
            newObject2.transform.position = _currentRoom.Loci[0].transform.position;
            // Remove the loci halo
            GameObject halo = _currentRoom.Loci[0];
            _currentRoom.Loci.RemoveAt(0);
            Destroy(halo);

        }
        else
        {
            GameObject newObject = Instantiate(loci, findFreeFloatingSpace(), Quaternion.identity);
            ObjectSnapper objectSnapper = newObject.GetComponent<ObjectSnapper>();
            objectSnapper.snapToFloor();
            newObject.tag = "Information";

            // Add a tooltip as a child the new object
            if (tooltipText != null)
            {
                GameObject tooltipObject = Instantiate(tooltip, newObject.transform.position, Quaternion.identity);
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
        GameObject newObject = GameObject.Instantiate(furniture, findFreeFloorSpace(furniture), Quaternion.identity);
        newObject.tag = "Furniture";

        // Add furniture to the current room's list of furniture
        _currentRoom.AddFurniture(furniture, newObject);
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
    /// Delete the next game object that the user touches, or deactivates the delete mode if active
    /// </summary>
    /// <param name="value">The value of the corresponding button "Delete Object"</param>
    public void DeleteButton(bool value)
    {
        // await the user to touch an object
        isDeleteMode = !isDeleteMode;
    }

    /// <summary>
    /// Called by the ObjectSnapper when the user touches an object after the delete button has been pressed
    /// </summary>
    /// <param name="obj">The object to be deleted</param>
    public void DeleteObject(GameObject obj)
    {
        if (isDeleteMode)
        {
            if (_currentRoom.FurnitureInstances.Contains(obj))
            {
                int index = _currentRoom.FurnitureInstances.IndexOf(obj);
                _currentRoom.FurnitureInstances.RemoveAt(index);
                _currentRoom.Furniture.RemoveAt(index);
                // As anchors all get deleted and re-detected when changing rooms (UpdateAnchors), we don't need to (or should) delete them here
            }
            else if (_currentRoom.RepresentationInstances.Contains(obj))
            {
                int index = _currentRoom.RepresentationInstances.IndexOf(obj);
                _currentRoom.RepresentationInstances.RemoveAt(index);
                _currentRoom.Representations.RemoveAt(index);
            }
            Destroy(obj);
            _currentRoom.UpdateTransforms();
            _currentRoom.SaveRoom();
            isDeleteMode = false;
        }
    }

    /// <summary>
    /// Resets the position of the user and the camera rig to the real position in the room.
    /// Important for the user to be able to move around in the real world again after teleporting.
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public void ReturnToRealPosition(bool value)
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
        _storyText.SetActive(false);
        // Deactivate and activate only necessary buttons
        newRoomButton.SetActive(true);
        backButton.SetActive(true);
        forwardButton.SetActive(true);
        openLociStoreButton.SetActive(false);
        nextWallColourButton.SetActive(true);
        previousWallColourButton.SetActive(true);
        openFurnitureStoreButton.SetActive(true);
        deleteButton.SetActive(true);
        changeUser.SetActive(false);
        // audioPauseButton.SetActive(true);
        // audioReplayButton.SetActive(true);
        greyOutFurnitureButton.SetActive(false);
        furniturePhaseButton.SetActive(false);
        endFurniturePhaseButton.SetActive(false);
        listPhaseButton.SetActive(false);
        storyPhaseButton.SetActive(false);
        numberPhaseButton.SetActive(false);
        confirmButton.SetActive(false);
        devmenuButton.SetActive(true);
        resetPositionButton.SetActive(true);
        addMoreRoomsButton.SetActive(false);
        enoughRoomsButton.SetActive(false);
        // Let the agent play the introduction to the furniture setup phase
        agentController.ActivateAgent();
        agentController.PlayAudio(agentController.furnitureIntroductionAudio);
    }

    /// <summary>
    /// Shows one object at a time to the user to place in the memory palace
    /// </summary>
    /// <param name="value"></param>
    public async void ListPhase(bool value)
    {
        await LoadRoom(_currentUser.GetFirstRoom());
        _currentUser.CurrentRoomID = 0;
        _currentRoom = _currentUser.GetCurrentRoom();

        setButtonsPhases("list");
        // Let the agent play the introduction to the list phase
        agentController.ActivateAgent();
        agentController.PlayAudio(agentController.listIntroductionAudio);
        for(int i = 0; i < evaluationList.Count; i++)
        {
            AddLoci(evaluationList[i], "");
            agentController.PlayAudioListPhase(i);
            if (_currentRoom.RepresentationInstances.Count == 3)
            {
                // Play audio: Place about 3 to 5 items in each room
                agentController.PlayAudio(agentController.listPhase3ItemsAudio);
            }

            if (_addingMoreRooms)
            {
                await WaitForContinueEvaluation();
                _storyText.SetActive(false);
                _tooltipMenu.SetActive(false);
                setButtonsPhases("list");
                agentController.PlayAudio(agentController.backInLearningModeAudio);
            }
            else
            {
                await WaitForUserConfirmation();
            }
            _isObjectConfirmed = false;
        }
        storyPhaseButton.SetActive(true);
    }

    /// <summary>
    /// Used to wait for the user to press the continue button in the evaluation phase
    /// </summary>
    private async Task WaitForUserConfirmation()
    {
        while (!_isObjectConfirmed)
        {
            await Task.Delay(100);
        }
    }

    /// <summary>
    /// Used to wait for the user to press the continue button in the evaluation phase
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public void OnConfirmButtonClick(bool value)
    {
        _isObjectConfirmed = true;
    }

    /// <summary>
    /// Shows the user part of the story to remember in the evaluation phase.
    /// The user has to pick an object that represents the story part.
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public async void StoryPhase(bool value)
    {
        setButtonsPhases("story");
        // Let the agent play the introduction to the story phase
        agentController.ActivateAgent();
        agentController.PlayAudio(agentController.storyIntroductionAudio);
        // Play the story in parts and show a tooltip with the text part
        _tooltipMenu.SetActive(true);
        TextMeshProUGUI [] texts = _tooltipMenu.GetComponentsInChildren<TextMeshProUGUI>();
        for(int i = 0; i < _evaluationStory.Count; i++)
        {
            agentController.PlayAudio(agentController.storyAudios[i]);
            _currentStoryPart = _evaluationStory[i];
            // Show story part on the menu
            texts[0].text = "Merke dir als nächstes:";
            texts[1].text =_evaluationStory[i];
            if (_addingMoreRooms)
            {
                await WaitForContinueEvaluation();
                _storyText.SetActive(true);
                _tooltipMenu.SetActive(true);
                setButtonsPhases("story");
                agentController.PlayAudio(agentController.backInLearningModeAudio);
            }
            else
            {
                await WaitForUserConfirmation();
                _isObjectConfirmed = false;
                _recommendationsIndex++;
            }
        }
        numberPhaseButton.SetActive(true);
        _currentStoryPart = "";
        texts[0].text = "Ende der Geschichte";
        texts[1].text = "Drücke auf 'Zahlen merken' um fortzufahren";
        _isObjectConfirmed = false;
    }

    /// <summary>
    /// The user has to remember a number and pick an object that represents the number.
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public async void NumberPhase(bool value)
    {
        _recommendationsIndex = 16;
        setButtonsPhases("number");
        // Let the agent play the introduction to the number phase
        agentController.ActivateAgent();
        agentController.PlayAudio(agentController.numberIntroductionAudio);

        _tooltipMenu.SetActive(true);
        TextMeshProUGUI [] texts = _tooltipMenu.GetComponentsInChildren<TextMeshProUGUI>();
        _isObjectConfirmed = false;
        for(int i = 0; i < agentController.numberAudios.Length; i++)
        {
            agentController.PlayAudio(agentController.numberAudios[i]);
            _currentStoryPart = "Steht für die Zahl " + agentController.numberAudios[i].name;
            // Show story part on the menu
            texts[0].text = "74920318475061238756";
            texts[1].text ="Merke dir als nächstes: Die Zahl " + agentController.numberAudios[i].name;
            if (_addingMoreRooms)
            {
                await WaitForContinueEvaluation();
                _storyText.SetActive(false);
                _tooltipMenu.SetActive(true);
                setButtonsPhases("number");
                agentController.PlayAudio(agentController.backInLearningModeAudio);
            }
            else
            {
                await WaitForUserConfirmation();
                _isObjectConfirmed = false;
                _recommendationsIndex++;
            }
        }
        _finished = true;
        _currentStoryPart = "";
        texts[0].text = "Ende der Zahlen";
        texts[1].text = "Drücke auf 'Erster Raum' um fortzufahren";
        goToFirstRoomButton.SetActive(true);
        agentController.PlayAudio(agentController.repetitionAudio);
    }

    /// <summary>
    /// Sets the current room to the first room of the user.
    /// Used in the evaluation phase to start repeating the information from the beginning.
    /// And to guide the user through the rooms.
    /// </summary>
    /// <param name="value">The value of the button "Erster Raum"</param>
    public async void GoToFirstRoom(bool value)
    {
        _isNextRoom = false;
        if (!reusePalace)
        {
            _storyText.SetActive(true);
        }
        else
        {
            _storyText.SetActive(false);
        }
        _doorChangeRoom = true;
        _tooltipMenu.SetActive(false);
        newRoomButton.SetActive(false);
        backButton.SetActive(true);
        forwardButton.SetActive(true);
        openLociStoreButton.SetActive(false);
        nextWallColourButton.SetActive(false);
        previousWallColourButton.SetActive(false);
        openFurnitureStoreButton.SetActive(false);
        greyOutFurnitureButton.SetActive(true);
        deleteButton.SetActive(false);
        changeUser.SetActive(false);
        // audioPauseButton.SetActive(true);
        // audioReplayButton.SetActive(true);
        furniturePhaseButton.SetActive(false);
        endFurniturePhaseButton.SetActive(false);
        listPhaseButton.SetActive(false);
        storyPhaseButton.SetActive(false);
        numberPhaseButton.SetActive(false);
        confirmButton.SetActive(true);
        devmenuButton.SetActive(true);
        resetPositionButton.SetActive(true);
        addMoreRoomsButton.SetActive(false);
        enoughRoomsButton.SetActive(false);
        if (_currentRoom != _currentUser.GetFirstRoom())
        {
            await LoadRoom(_currentUser.GetFirstRoom());
            _currentRoom.SaveRoom();
            _currentUser.CurrentRoomID = 0;
            _currentRoom = _currentUser.GetCurrentRoom();
        }
        GameObject spotlight = Instantiate(spotLight, Vector3.zero, Quaternion.Euler(90,0,0));
        spotlight.SetActive(false);
        for(int i = 0; i < _currentUser.GetFreeRoomID(); i++)
        {
            foreach (var representation in _currentRoom.RepresentationInstances)
            {
                spotlight.SetActive(true);
                spotlight.transform.position = new Vector3(representation.transform.position.x, 2.5f, representation.transform.position.z);
                agentController.GoToAndPointAtObject(representation);
                await WaitForUserConfirmation();
                _isObjectConfirmed = false;
                if(_isNextRoom)
                {
                    break;
                }
            }
            await WaitForNextRoom();
            _isNextRoom = false;
        }
        goToFirstRoomButton.SetActive(true);
        spotLight.SetActive(false);
    }

    /// <summary>
    /// Used to wait for the user to go to the next room
    /// </summary>
    private async Task WaitForNextRoom()
    {
        while (!_isNextRoom)
        {
            await Task.Delay(100);
        }
    }

    /// <summary>
    /// Used to go back to the evaluation phase after the user has added more rooms
    /// </summary>
    private async Task WaitForContinueEvaluation()
    {
        while (!_continueEvaluation)
        {
            await Task.Delay(100);
        }
    }

    /// <summary>
    /// Called when the user reaches the end of the rooms too early and wants to create more rooms
    /// </summary>
    /// <param name="value"></param>
    public async void ReturnToFurniturePhase(bool value)
    {
        _storyText.SetActive(false);
        _lociMenu.SetActive(false);
        _furnitureMenu.SetActive(false);
        _tooltipMenu.SetActive(false);

        newRoomButton.SetActive(true);
        // do not let the user go back and look at the other rooms
        backButton.SetActive(false);
        forwardButton.SetActive(true);
        openLociStoreButton.SetActive(false);
        nextWallColourButton.SetActive(false);
        previousWallColourButton.SetActive(false);
        openFurnitureStoreButton.SetActive(true);
        deleteButton.SetActive(true);
        changeUser.SetActive(false);
        // audioPauseButton.SetActive(true);
        // audioReplayButton.SetActive(true);
        greyOutFurnitureButton.SetActive(false);
        furniturePhaseButton.SetActive(false);
        endFurniturePhaseButton.SetActive(false);
        listPhaseButton.SetActive(false);
        storyPhaseButton.SetActive(false);
        numberPhaseButton.SetActive(false);
        confirmButton.SetActive(false);
        devmenuButton.SetActive(true);
        resetPositionButton.SetActive(true);
        addMoreRoomsButton.SetActive(false);
        enoughRoomsButton.SetActive(true);
        agentController.ActivateAgent();
        // Load the new room
        Room evaluationRoom = _currentRoom;
        CreateRoom(true);
        _currentRoom.SaveRoom();
        await WaitForAddMoreRooms();
        await LoadRoom(evaluationRoom);
        _continueEvaluation = true;
    }

    /// <summary>
    /// Used to wait for the user to add more rooms
    /// </summary>
    private async Task WaitForAddMoreRooms()
    {
        while (!_finishAddingMoreRooms)
        {
            await Task.Delay(100);
        }
        _finishAddingMoreRooms = false;
    }

    /// <summary>
    /// Called when the user is finished adding more rooms and wants to return to the evaluation phases
    /// </summary>
    /// <param name="value"></param>
    public void FinishAddingMoreRooms(bool value)
    {
        _finishAddingMoreRooms = true;
        addMoreRoomsButton.SetActive(true);
        enoughRoomsButton.SetActive(false);
        backButton.SetActive(true);
    }

    /// <summary>
    /// Used to set the buttons for the different phases of the evaluation
    /// </summary>
    /// <param name="phase">either "list", "story", "number" or "layout"</param>
    private void setButtonsPhases(string phase)
    {
        _learning = true;
        switch (phase)
        {
            case "list":
                _storyText.SetActive(false);
                _furniturePhaseRememberLayout = false;
                _doorChangeRoom = true;
                newRoomButton.SetActive(false);
                backButton.SetActive(true);
                forwardButton.SetActive(true);
                openLociStoreButton.SetActive(false);
                nextWallColourButton.SetActive(false);
                previousWallColourButton.SetActive(false);
                openFurnitureStoreButton.SetActive(false);
                greyOutFurnitureButton.SetActive(true);
                deleteButton.SetActive(false);
                changeUser.SetActive(false);
                furniturePhaseButton.SetActive(false);
                listPhaseButton.SetActive(false);
                storyPhaseButton.SetActive(false);
                numberPhaseButton.SetActive(false);
                confirmButton.SetActive(true);
                devmenuButton.SetActive(true);
                resetPositionButton.SetActive(true);
                addMoreRoomsButton.SetActive(false);
                enoughRoomsButton.SetActive(false);
                break;
            case "story":
                _storyText.SetActive(true);
                _furniturePhaseRememberLayout = false;
                _doorChangeRoom = true;
                newRoomButton.SetActive(false);
                backButton.SetActive(true);
                forwardButton.SetActive(true);
                openLociStoreButton.SetActive(true);
                nextWallColourButton.SetActive(false);
                previousWallColourButton.SetActive(false);
                openFurnitureStoreButton.SetActive(false);
                greyOutFurnitureButton.SetActive(true);
                deleteButton.SetActive(true);
                changeUser.SetActive(false);
                // audioPauseButton.SetActive(true);
                // audioReplayButton.SetActive(true);
                furniturePhaseButton.SetActive(false);
                endFurniturePhaseButton.SetActive(false);
                listPhaseButton.SetActive(false);
                storyPhaseButton.SetActive(false);
                numberPhaseButton.SetActive(false);
                confirmButton.SetActive(true);
                devmenuButton.SetActive(true);
                resetPositionButton.SetActive(true);
                addMoreRoomsButton.SetActive(false);
                enoughRoomsButton.SetActive(false);
                break;
            case "number":
                _storyText.SetActive(false);
                _furniturePhaseRememberLayout = false;
                _doorChangeRoom = true;
                // Deactivate and activate only necessary buttons
                newRoomButton.SetActive(false);
                backButton.SetActive(true);
                forwardButton.SetActive(true);
                openLociStoreButton.SetActive(true);
                nextWallColourButton.SetActive(false);
                previousWallColourButton.SetActive(false);
                openFurnitureStoreButton.SetActive(false);
                greyOutFurnitureButton.SetActive(true);
                deleteButton.SetActive(true);
                changeUser.SetActive(false);
                // audioPauseButton.SetActive(true);
                // audioReplayButton.SetActive(true);
                furniturePhaseButton.SetActive(false);
                endFurniturePhaseButton.SetActive(false);
                listPhaseButton.SetActive(false);
                storyPhaseButton.SetActive(false);
                numberPhaseButton.SetActive(false);
                confirmButton.SetActive(true);
                devmenuButton.SetActive(true);
                resetPositionButton.SetActive(true);
                addMoreRoomsButton.SetActive(false);
                enoughRoomsButton.SetActive(false);
                break;
            case "layout":
                _doorChangeRoom = true;
                backButton.SetActive(true);
                forwardButton.SetActive(true);
                deleteButton.SetActive(false);
                furniturePhaseButton.SetActive(false);
                devmenuButton.SetActive(true);
                resetPositionButton.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// Deletes the JSON file of the current user
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public void DeleteJSON(bool value)
    {
        _deleteCounter++;
        if (_deleteCounter == 2)
        {
            string path = Path.Combine(Application.persistentDataPath, _currentUser._name + ".json");
            if (File.Exists(path))
            {
                File.Delete(path);
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
    /// <summary>
    /// To pause the audio of the agent
    /// </summary>
    /// <param name="value"></param>
    public void PauseAudio(bool value)
    {
        agentController.PauseAudio();
    }

    /// <summary>
    /// To replay the audio of the agent
    /// </summary>
    /// <param name="value"></param>
    public void ReplayAudio(bool value)
    {
        agentController.ReplayAudio();
    }

    /// <summary>
    /// Opens the dev menu when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
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

    /// <summary>
    /// Close the dev menu when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public void CloseDevMenu(bool value)
    {
        _devMenu.SetActive(false);
    }

    /// <summary>
    /// For the user to finish the furniture phase and start the evaluation phase
    /// </summary>
    /// <param name="value">The value of the corresponding button</param>
    public async void FinishFurniturePhase(bool value)
    {
        _furniturePhaseRememberLayout = true;
        await LoadRoom(_currentUser.GetFirstRoom());
        _currentUser.CurrentRoomID = 0;
        _currentRoom = _currentUser.GetCurrentRoom();
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
            NextRoom(true);
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
            PreviousRoom(true);
        }
    }

    /// <summary>
    /// To set the height of the menu to match the user's height
    /// </summary>
    /// <param name="value"></param>
    public void ChangeMenuHeight(bool value)
    {
        _menu.transform.position = new Vector3(_menu.transform.position.x, user.transform.position.y - 0.2f, _menu.transform.position.z);
    }

    /// <summary>
    /// Used to determine which recommendations to load in the loci store
    /// </summary>
    /// <returns>The index of the recommendations</returns>
    public int LoadRecommendations()
    {
        return _recommendationsIndex;
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
        GameObject newObject = Instantiate(furniturePrefabs[_furniturePointer], findFreeFloatingSpace(), Quaternion.identity);
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
        GameObject newObject = Instantiate(furniturePrefabs[_furniturePointer], findFreeFloatingSpace(), Quaternion.identity);
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
        GameObject newObject = Instantiate(furniturePrefabs[_furniturePointer], findFreeFloorSpace(furniturePrefabs[_furniturePointer]), Quaternion.identity);
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

}
