using System.Threading.Tasks;
using UnityEngine;
using i5.VirtualAgents;
using i5.VirtualAgents.AgentTasks;
using i5.VirtualAgents.ScheduleBasedExecution;
using TMPro;

public class AgentController : MonoBehaviour
{
    [Tooltip("The agent which is controlled by this controller")]
    public Agent agent;
    [Tooltip("The task system of the agent")]
    protected ScheduleBasedTaskSystem TaskSystem;
    [Tooltip("The center eye anchor of the user")]
    [SerializeField] public GameObject user;
    // Audio clips for the agent
    [Tooltip("The audio clip for the introduction")]
    [SerializeField] public AudioClip introductionAudio;
    [Tooltip("The audio clip for the MoL Explanation")]
    [SerializeField] public AudioClip MoLAudio;
    [Tooltip("Furniture phase introduction audio")]
    [SerializeField] public AudioClip furnitureIntroductionAudio;
    [Tooltip("After placement of 2 furniture items")]
    [SerializeField] public AudioClip furniturePlacement2Audio;
    [Tooltip("After creating a new room, explain doors")]
    [SerializeField] public AudioClip newRoomDoorAudio;
    [Tooltip("The furniture phase can be finished")]
    [SerializeField] public AudioClip furnitureCanBeFinishedAudio;
    [Tooltip("Finish Furniture phase")]
    [SerializeField] public AudioClip furnitureFinishAudio;
    [Tooltip("This audio explains that the learning part will start after the user finishes repeating the room layouts")]
    [SerializeField] public AudioClip furniturePhaseRememberLayoutLastRoomAudio;
    [Tooltip("The list introduction clip")]
    [SerializeField] public AudioClip listIntroductionAudio;
    [Tooltip("The audio clips for the list")]
    [SerializeField] public AudioClip[] listAudios;
    [Tooltip("To encourage the user to place 3 to 5 items per room")]
    [SerializeField] public AudioClip listPhase3ItemsAudio;
    [Tooltip("Introduction to the story")]
    [SerializeField] public AudioClip storyIntroductionAudio;
    [Tooltip("The audio clips for the story")]
    [SerializeField] public AudioClip[] storyAudios;
    [Tooltip("The introduction to the number phase")]
    [SerializeField] public AudioClip numberIntroductionAudio;
    [Tooltip("The audio clips for the number")]
    [SerializeField] public AudioClip[] numberAudios;
    [Tooltip("Tell the user that they can continue learning")]
    [SerializeField] public AudioClip backInLearningModeAudio;
    [Tooltip("Tell user to repeat the information, starting with the first room")]
    [SerializeField] public AudioClip repetitionAudio;

    private bool _isFollowingUser;
    private AudioClip _currentAudio;
    private AudioSource _audioSource;
    private bool paused;
    private bool replaying;
    private GameObject _storyPlane;
    private readonly string _storyText =
        "Gestern habe ich sehr viel erlebt. " +
        "Ich war auf dem Weg zum Air-Hockey spielen, mit meinen Freunden, Mira und Loki, " +
        "da sah ich mitten in der Stadt einen roten Bison. " +
        "Das ist komisch, dachte ich mir und drehte um, um das Tier nicht zu erschrecken. " +
        "Da lief mir auch schon ein Geist mit blauer Sonnenbrille entgegen. " +
        "Dieser stolperte und verlor eine kleine Münze. " +
        "Die Münze steckte ich in meine gelbe Tüte und ging weiter. " +
        "Später kann ich mir eine Zitrone von dem Geld kaufen, dachte ich. " +
        "Zwei Feuerwehrautos, ein Rennauto und ein Rentier warten schon am Bahnhof. " +
        "Nach 3 Stunden Warten gehe ich wieder nach Hause, weil der Zug ausgefallen ist. " +
        "Ein Taxi ist mir zu teuer und der Schlitten fährt nicht ohne mein Pferd. " +
        "Das ist gerade im Urlaub.";
    private readonly string _introductionText =
        "Hallo, ich werde versuchen dir heute die Method of Loci beizubringen. " +
        "Ich begleite dich bei allen Schritten auf dem Weg. " +
        "Zunächst erkläre ich dir wie die Methode funktioniert. " +
        "Wenn du willst kannst du dir diesen Teil mehrfach anhören.";
    private readonly string _MoLText =
        "Die Method of Loci ist eine beliebte Gedächtnis Strategie aus dem alten Griechenland. " +
        "Sie benutzt das Raumgedächtnis damit du dir etwas merken kannst. " +
        "Du stellst dir die Information als Objekt oder Bild vor und platzierst sie in deinem Kopf. " +
        "Die Orte an denen Objekte platziert werden heißen loci. " +
        "Mehrere Informationen können entlang einem Pfad von loci angeordnet werden und so kannst du dir auch die Reihenfolge merken. " +
        "Um viele Informationen zu lernen, benötigst du eine Vielzahl an Räumen in denen du Objekte platzieren kannst. " +
        "Diese Räume nennen wir Gedächtnispalast. " +
        "Traditionell findet das Lernen nur in deinem Kopf statt, " +
        "hier in dieser App versuche ich dir die Aufgabe des Erstellens und Einrichten eines Gedächtnispalastes zu vereinfachen.";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        ActivateAgent();
        // Get the task system of the agent
        TaskSystem = (ScheduleBasedTaskSystem)agent.TaskSystem;
        _audioSource = agent.transform.gameObject.GetComponent<AudioSource>();
        // Turn to the user
        FaceUser();
        // Wave to the user
        TaskSystem.Tasks.PlayAnimation("WaveLeft", 5, "", 0, "Left Arm");
        // Play the introduction audios and display the introduction texts
        _storyPlane = GameObject.FindGameObjectWithTag("Story");
        await Task.Delay(100);
        await PlayAudio(introductionAudio);
        await PlayAudio(MoLAudio);
    }

    /// <summary>
    /// Turns the agent to face the user
    /// </summary>
    public void FaceUser()
    {
        float angle = Vector3.SignedAngle(agent.transform.forward, user.transform.position - agent.transform.position, Vector3.up);
        AgentRotationTask rotationTask = new AgentRotationTask(angle, true);
        TaskSystem.ScheduleTask(rotationTask);
    }

    /// <summary>
    /// Turns the agent to face the game object
    /// </summary>
    public void FaceObject(GameObject faceObject)
    {
        float angle = Vector3.SignedAngle(agent.transform.forward, faceObject.transform.position - agent.transform.position, Vector3.up);
        AgentRotationTask rotationTask = new AgentRotationTask(angle, true);
        TaskSystem.ScheduleTask(rotationTask);
    }

    /// <summary>
    /// Makes the agent point at a specific object
    /// </summary>
    /// <param name="pointObject">The object to point at</param>
    public void PointAtObject(GameObject pointObject)
    {
        TaskSystem.Tasks.PointAt(pointObject, true);
    }

    /// <summary>
    /// Instructs the agent to go to a specific object
    /// </summary>
    /// <param name="targetObject">The object the agent goes to</param>
    public void GoToObject(GameObject targetObject)
    {
        // let the agent stand a bit away from the object
        AgentMovementTask task = new AgentMovementTask(targetObject, default, true);
        TaskSystem.ScheduleTask(task);
    }

    /// <summary>
    /// Instructs the agent to go to a specific object and point at it for 5 seconds
    /// </summary>
    /// <param name="targetObject">The object the agnet points at</param>
    public void GoToAndPointAtObject(GameObject targetObject)
    {
        FaceObject(targetObject);
        AgentAnimationTask pointing = null;
        pointing = new AgentAnimationTask("PointingLeft", 5, "", "Left Arm", targetObject);
        TaskSystem.ScheduleTask(pointing, 0, "Left Arm");
        Vector3 position = targetObject.transform.position;
        position = position + (agent.transform.position - position).normalized * 0.5f;
        AgentMovementTask task = new AgentMovementTask(position);
        TaskSystem.ScheduleTask(task,0);
    }

    /// <summary>
    /// Makes the agent follow the user if the user is too far away and turn to face the user if the user is close
    /// </summary>
    public void FollowUser()
    {
        // Rotate if close enough to the user
        if (Vector3.Distance(transform.position, user.transform.position) < 1)
        {
            FaceUser();
        }
        else
        {
            AgentMovementTask task = new AgentMovementTask(user, default, true);
            task.MinDistance = 1;
            TaskSystem.ScheduleTask(task);
            _isFollowingUser = true;
            float angle = Vector3.SignedAngle(agent.transform.forward, user.transform.position - agent.transform.position, Vector3.up);
            AgentRotationTask rotationTask = new AgentRotationTask(angle, true);
            TaskSystem.ScheduleTask(rotationTask);
        }
    }

    /// <summary>
    /// To control that the agent only has one follow task at a time
    /// </summary>
    private void OnTaskFinished()
    {
        _isFollowingUser = false;
    }

    /// <summary>
    /// Deactivate the agent
    /// </summary>
    public void DeactivateAgent()
    {
        agent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Activate the agent
    /// </summary>
    public void ActivateAgent()
    {
        if (agent.gameObject.activeSelf)
        {
            return;
        }
        agent.gameObject.SetActive(true);
    }

    /// <summary>
    /// Plays the audio clip from the list at the given index
    /// </summary>
    /// <param name="index">The place in the list of audios</param>
    public void PlayAudioListPhase(int index)
    {
        PlayAudio(listAudios[index]);
    }

    /// <summary>
    /// Plays the given audio clip
    /// </summary>
    /// <param name="audioClip">The audio clip to be played</param>
    public async Task PlayAudio(AudioClip audioClip)
    {
        while (_audioSource.isPlaying)
        {
            await Task.Delay(100);
        }
        // Display the subtitles for first two audios
        if (audioClip == introductionAudio)
        {
            _storyPlane.GetComponentInChildren<TextMeshProUGUI>().text = _introductionText;
        }
        else if (audioClip == MoLAudio)
        {
            _storyPlane.GetComponentInChildren<TextMeshProUGUI>().text = _MoLText;
        }
        else
        {
            // Hide the subtitles
            _storyPlane.GetComponentInChildren<TextMeshProUGUI>().text = _storyText;
        }
        _currentAudio = audioClip;
        _audioSource.clip = audioClip;
        _audioSource.Play();
    }

    /// <summary>
    /// To toggle the pause state of the audio
    /// </summary>
    public void PauseAudio()
    {
        if(_audioSource.isPlaying)
        {
            paused = true;
            _audioSource.Pause();
        }
        else if(_audioSource.clip != null)
        {
            paused = false;
            _audioSource.UnPause();
        }
    }

    /// <summary>
    /// To replay the audio clip or restart the audio clip if it is already playing
    /// </summary>
    public void ReplayAudio()
    {
        if (_audioSource.isPlaying)
        {
            replaying = true;
            _audioSource.Stop();
            _audioSource.Play();
        }
        else if (_currentAudio != null)
        {
            replaying = false;
            _audioSource.clip = _currentAudio;
            _audioSource.Play();
        }
    }
}
