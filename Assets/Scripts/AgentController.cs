using UnityEngine;
using i5.VirtualAgents;
using i5.VirtualAgents.AgentTasks;
using i5.VirtualAgents.ScheduleBasedExecution;

public class AgentController : MonoBehaviour
{
    // The agent which is controlled by this controller, set in the inspector
    public Agent agent;
    // The taskSystem of the agent
    protected ScheduleBasedTaskSystem taskSystem;
    [SerializeField] public AudioClip introductionAudio;
    [SerializeField] public AudioClip MoLAudio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the task system of the agent
        taskSystem = (ScheduleBasedTaskSystem)agent.TaskSystem;
        // Play the introduction audio
        AgentAudioTask audioTask = new AgentAudioTask(introductionAudio);
        taskSystem.ScheduleTask(audioTask);
        AgentAudioTask audioTask2 = new AgentAudioTask(MoLAudio);
        taskSystem.ScheduleTask(audioTask2);
    }

    // Update is called once per frame
    void Update()
    {
        // Turn to the user
    }
}
