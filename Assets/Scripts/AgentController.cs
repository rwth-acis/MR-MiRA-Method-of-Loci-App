using System.Threading.Tasks;
using UnityEngine;
using i5.VirtualAgents;
using i5.VirtualAgents.AgentTasks;
using i5.VirtualAgents.ScheduleBasedExecution;
using Unity.VisualScripting;

public class AgentController : MonoBehaviour
{
    [Tooltip("The agent which is controlled by this controller")]
    public Agent agent;
    [Tooltip("The task system of the agent")]
    protected ScheduleBasedTaskSystem taskSystem;
    // The "center" eye anchor of the user
    [Tooltip("The center eye anchor of the user")]
    [SerializeField] public GameObject user;
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
    [Tooltip("To check if the agent is following the user")]
    private bool _isFollowingUser = false;

    private AudioClip _currentAudio;
    private AudioSource _audioSource;
    //private AgentAudioTask _currentAudio = new AgentAudioTask(null);
    private bool paused = false;
    private bool replaying = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivateAgent();
        // Get the task system of the agent
        taskSystem = (ScheduleBasedTaskSystem)agent.TaskSystem;

        _audioSource = agent.transform.gameObject.GetComponent<AudioSource>();

        // Turn to the user
        FaceUser();
        // Wave to the user
        taskSystem.Tasks.PlayAnimation("WaveLeft", 5, "", 0, "Left Arm");

        PlayAudio(introductionAudio);
        // AgentAudioTask audioTask = new AgentAudioTask(introductionAudio);
        // taskSystem.ScheduleTask(audioTask, 0, "Head");
        // _currentAudio = audioTask;
        PlayAudio(MoLAudio);
        // AgentAudioTask audioTask2 = new AgentAudioTask(MoLAudio);
        // taskSystem.ScheduleTask(audioTask2, 0, "Head");
        // audioTask2.OnTaskStarted += OnAudioTaskStarted(audioTask2);
    }

    // Update is called once per frame
    void Update()
    {
        // // If the user is too far away, and there is no walking task already, follow the user
        // if (Vector3.Distance(transform.position, user.transform.position) > 2 && !_isFollowingUser)
        // {
        //     FollowUser();
        // }
    }

    /// <summary>
    /// Turns the agent to face the user
    /// </summary>
    public void FaceUser()
    {
        float angle = Vector3.SignedAngle(agent.transform.forward, user.transform.position - agent.transform.position, Vector3.up);
        AgentRotationTask rotationTask = new AgentRotationTask(angle, true);
        taskSystem.ScheduleTask(rotationTask);
        // _isFollowingUser = true;
        // rotationTask.OnTaskFinished += OnTaskFinished;
    }

    /// <summary>
    /// Turns the agent to face the game object
    /// </summary>
    public void FaceObject(GameObject faceObject)
    {
        float angle = Vector3.SignedAngle(agent.transform.forward, faceObject.transform.position - agent.transform.position, Vector3.up);
        AgentRotationTask rotationTask = new AgentRotationTask(angle, true);
        taskSystem.ScheduleTask(rotationTask);
    }

    /// <summary>
    /// Makes the agent point at a specific object
    /// </summary>
    /// <param name="pointObject">The object to point at</param>
    public void PointAtObject(GameObject pointObject)
    {
        Debug.Log("Agent: Pointing at object");
        taskSystem.Tasks.PointAt(pointObject, true);
    }

    public void GoToObject(GameObject targetObject)
    {
        Debug.Log("Agent: Going to object");
        // let the agent stand a bit away from the object
        Vector3 position = targetObject.transform.position;
        AgentMovementTask task = new AgentMovementTask(targetObject, default, true);
        taskSystem.ScheduleTask(task);
    }

    public void GoToAndPointAtObject(GameObject targetObject)
    {
        FaceObject(targetObject);
        AgentAnimationTask pointing = null;
        pointing = new AgentAnimationTask("PointingLeft", 5, "", "Left Arm", targetObject);
        taskSystem.ScheduleTask(pointing, 0, "Left Arm");
        // // wait before moving
        // AgentWaitTask waitTask = new AgentWaitTask(3);
        // taskSystem.ScheduleTask(waitTask, 9);
        Vector3 position = targetObject.transform.position;
        //change position so that agent can stand next to object
        position = position + (agent.transform.position - position).normalized * 0.5f;
        AgentMovementTask task = new AgentMovementTask(position);
        taskSystem.ScheduleTask(task,0);
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
            //AgentMovementTask task = (AgentMovementTask)taskSystem.Tasks.GoTo(user, default, default, true);
            AgentMovementTask task = new AgentMovementTask(user, default, true);
            task.MinDistance = 1;
            taskSystem.ScheduleTask(task);
            _isFollowingUser = true;
            float angle = Vector3.SignedAngle(agent.transform.forward, user.transform.position - agent.transform.position, Vector3.up);
            AgentRotationTask rotationTask = new AgentRotationTask(angle, true);
            taskSystem.ScheduleTask(rotationTask);
            //rotationTask.OnTaskFinished += OnTaskFinished;
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

    public void PlayAudioListPhase(int index)
    {
        // Play the audioclip on head layer
        //AgentAudioTask audioTask = new AgentAudioTask(listAudios[index]);
        //taskSystem.ScheduleTask(audioTask, 0, "Head");
        //audioTask.OnTaskStarted += () => OnAudioTaskStarted(audioTask);
        PlayAudio(listAudios[index]);
    }

    public async Task PlayAudio(AudioClip audioClip)
    {
        //AgentAudioTask audioTask = new AgentAudioTask(audioClip);
        //taskSystem.ScheduleTask(audioTask, 0, "Head");
        while (_audioSource.isPlaying)
        {
            await Task.Delay(100);
        }
        _currentAudio = audioClip;
        _audioSource.clip = audioClip;
        _audioSource.Play();
        //audioTask.OnTaskStarted += () => OnAudioTaskStarted(audioTask);
    }


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
