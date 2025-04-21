using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class SquidFishAgent : Agent
{

    [SerializeField] private SquidFishEnvironment SquidFishEnvironment;

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode has begun.");
        SquidFishEnvironment.MoveToNextAnimal();
        // SquidFishEnvironment.SpawnTraining();
        // SquidFishEnvironment.TeleportToNextAnimal();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Action: 0 = Squid; 1 = Fish;
        bool selectSquid = actions.DiscreteActions[0] == 0;

        if (SquidFishEnvironment.TrySelectAnimal(selectSquid))
        {
            AddReward(+1f);
            Debug.Log("Correct!");
        }
        else
        {
            AddReward(-1f);
            Debug.Log("Wrong!");
        }

        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        // KeyDown = Squid; KeyUp = Fish;
        discreteActions[0] = Input.GetKey(KeyCode.T) ? 0 : 1;
    }

}
