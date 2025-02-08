using UnityEngine;
using i5.VirtualAgents;
using i5.VirtualAgents.AgentTasks;
using i5.VirtualAgents.ScheduleBasedExecution;

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
    [Tooltip("The audio clips for the list")]
    [SerializeField] public AudioClip[] listAudios;
    [Tooltip("The audio clips for the story")]
    [SerializeField] public AudioClip[] storyAudios;
    [Tooltip("The audio clips for the number")]
    [SerializeField] public AudioClip[] numberAudios;
    [Tooltip("To check if the agent is following the user")]
    private bool _isFollowingUser = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivateAgent();
        // Get the task system of the agent
        taskSystem = (ScheduleBasedTaskSystem)agent.TaskSystem;
        // Turn to the user
        FaceUser();
        // TODO Play the introduction audio on the head layer
        AgentAudioTask audioTask = new AgentAudioTask(introductionAudio);
        taskSystem.ScheduleTask(audioTask);
        AgentAudioTask audioTask2 = new AgentAudioTask(MoLAudio);
        taskSystem.ScheduleTask(audioTask2);
    }

    // Update is called once per frame
    void Update()
    {
        // If the user is too far away, and there is no walking task already, follow the user
        if (Vector3.Distance(transform.position, user.transform.position) > 2 && !_isFollowingUser)
        {
            FollowUser();
        }
    }

    /// <summary>
    /// Turns the agent to face the user
    /// </summary>
    public void FaceUser()
    {
        float angle = Vector3.SignedAngle(agent.transform.forward, user.transform.position - agent.transform.position, Vector3.up);
        AgentRotationTask rotationTask = new AgentRotationTask(angle, true);
        taskSystem.ScheduleTask(rotationTask);
        _isFollowingUser = true;
        rotationTask.OnTaskFinished += OnTaskFinished;
    }

    /// <summary>
    /// Makes the agent point at a specific object
    /// </summary>
    /// <param name="gameObject">The object to point at</param>
    public void PointAtObject(GameObject gameObject)
    {
        FaceUser();
        taskSystem.Tasks.PointAt(gameObject, true);
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
            rotationTask.OnTaskFinished -= OnTaskFinished;
            rotationTask.OnTaskFinished += OnTaskFinished;
        }
    }

    /// <summary>
    /// To control that the agent only has one follow task at a time
    /// </summary>
    private void OnTaskFinished()
    {
        _isFollowingUser = false;
    }


    public void GuideUserFurnishing()
    {
        // Play the audio for the furniture setup

    }

    public void GuideUserEncodingInformation()
    {
        //TODO
    }

    public void GuideUserEmptyPalace()
    {
        //TODO
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
        agent.gameObject.SetActive(true);
    }
}
