using System.Threading.Tasks;
using UnityEngine;
using i5.VirtualAgents;
using i5.VirtualAgents.AgentTasks;
using i5.VirtualAgents.ScheduleBasedExecution;

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

    private bool _isFollowingUser = false;
    private AudioClip _currentAudio;
    private AudioSource _audioSource;
    private bool paused = false;
    private bool replaying = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivateAgent();
        // Get the task system of the agent
        TaskSystem = (ScheduleBasedTaskSystem)agent.TaskSystem;
        _audioSource = agent.transform.gameObject.GetComponent<AudioSource>();
        // Turn to the user
        FaceUser();
        // Wave to the user
        TaskSystem.Tasks.PlayAnimation("WaveLeft", 5, "", 0, "Left Arm");

        PlayAudio(introductionAudio);
        PlayAudio(MoLAudio);
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
    /// <param name="targetObject"></param>
    public void GoToObject(GameObject targetObject)
    {
        // let the agent stand a bit away from the object
        Vector3 position = targetObject.transform.position;
        AgentMovementTask task = new AgentMovementTask(targetObject, default, true);
        TaskSystem.ScheduleTask(task);
    }

    /// <summary>
    /// Instructs the agent to go to a specific object and point at it for 5 seconds
    /// </summary>
    /// <param name="targetObject"></param>
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
    /// <param name="index"></param>
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
