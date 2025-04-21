using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquidFishEnvironment : MonoBehaviour
{

    [SerializeField] private SquidFishAgent SquidFishAgent;
    [SerializeField] private Transform pfFish;
    [SerializeField] private Transform pfSquid;
    [SerializeField] private Transform FishPenTransform;
    [SerializeField] private Transform SquidPenTransform;

    private SquidFishMover agentSquidFishMover;
    private List<Transform> FishTransformList;
    private List<Transform> SquidTransformList;
    private List<Transform> animalTransformList;

    private bool lastTrainingIsSquid;
    private Transform lastTrainingTransform;

    private void Awake()
    {
        FishTransformList = new List<Transform>();
        SquidTransformList = new List<Transform>();
        animalTransformList = new List<Transform>();

        agentSquidFishMover = SquidFishAgent.GetComponent<SquidFishMover>();
        agentSquidFishMover.OnReachedTargetPosition += AgentSquidFishMover_OnReachedTargetPosition;

        SpawnInitialAnimals();
        //FunctionPeriodic.Create(() => SquidFishAgent.RequestDecision(), 1f); // Used for Heuristic Testing
    }

    private void AgentSquidFishMover_OnReachedTargetPosition(object sender, System.EventArgs e)
    {
        Debug.Log("Agent reached target, requesting decision.");
        SquidFishAgent.RequestDecision();
    }

    private void SpawnInitialAnimals()
    {
        for (int i = 0; i < 10; i++)
        {
            SpawnFish();
            SpawnSquid();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MoveToNextAnimal();
        }
    }

    // private void FixedUpdate() { SquidFishAgent.RequestDecision(); } // Used for Training

    public void MoveToNextAnimal()
    {
        if (animalTransformList.Count == 0) return; // No more animals!

        Transform animalTransform = animalTransformList[Random.Range(0, animalTransformList.Count)];
        animalTransformList.Remove(animalTransform);

        if (SquidTransformList.Contains(animalTransform))
        {
            // Next Animal is a Squid
            Debug.Log("Next Animal is a Squid");
            lastTrainingIsSquid = true;
        }
        else
        {
            // Next Animal is a Fish
            Debug.Log("Next Animal is a Fish");
            lastTrainingIsSquid = false;
        }

        agentSquidFishMover.SetTargetPosition(animalTransform.position + (GetRandomDir() * 1.5f));
        agentSquidFishMover.SetLookAtPosition(animalTransform.position);

        lastTrainingTransform = animalTransform;
    }

    public void TeleportToNextAnimal()
    {
        if (animalTransformList.Count == 0) return; // No more animals!

        Transform animalTransform = animalTransformList[Random.Range(0, animalTransformList.Count)];
        animalTransformList.Remove(animalTransform);

        if (SquidTransformList.Contains(animalTransform))
        {
            // Next Animal is a Squid
            Debug.Log("Next Animal is a Squid");
            lastTrainingIsSquid = true;
        }
        else
        {
            // Next Animal is a Fish
            Debug.Log("Next Animal is a Fish");
            lastTrainingIsSquid = false;
        }

        SquidFishAgent.transform.position = animalTransform.position + (GetRandomDir() * 1.5f);
        SquidFishAgent.transform.rotation = Quaternion.LookRotation(animalTransform.position - SquidFishAgent.transform.position);

        lastTrainingTransform = animalTransform;
    }

    private Vector3 GetRandomDir()
    {
        return new Vector3(Random.Range(-1f, +1f), 0, Random.Range(-1f, +1f)).normalized;
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 spawnPosition;
        int safety = 0;
        int safetyMax = 1000;

        do
        {
            spawnPosition = new Vector3(Random.Range(-10f, +10f), 0, Random.Range(-13f, 13f));
            safety++;
        } while (Physics.OverlapBox(spawnPosition, new Vector3(3f, .5f, 3f)).Length != 0 && safety < safetyMax);

        if (safety >= safetyMax)
        {
            Debug.Log("SAFETY!");
        }

        return spawnPosition;
    }

    private Vector3 GetFishPenPosition()
    {
        return FishPenTransform.position + new Vector3(Random.Range(-8f, +8f), 0, Random.Range(-10f, +10f));
    }

    private Vector3 GetSquidPenPosition()
    {
        return SquidPenTransform.position + new Vector3(Random.Range(-8f, +8f), 0, Random.Range(-10f, +10f));
    }

    private void SendAnimalToPen(Transform animalTransform, bool sendToSquidPen)
    {
        Vector3 penPosition = sendToSquidPen ? GetSquidPenPosition() : GetFishPenPosition();
        animalTransform.GetComponent<SquidFishMover>().SetTargetPosition(penPosition);
        animalTransform.GetComponent<SquidFishMover>().SetLookAtPosition(penPosition + GetRandomDir());
    }

    private void SpawnFish()
    {
        Vector3 spawnPosition = GetSpawnPosition();
        Transform FishTransform = Instantiate(pfFish, spawnPosition, Quaternion.Euler(0, Random.Range(0, 360f), 0));

        FishTransformList.Add(FishTransform);
        animalTransformList.Add(FishTransform);
    }

    private void SpawnSquid()
    {
        Vector3 spawnPosition = GetSpawnPosition();
        Transform SquidTransform = Instantiate(pfSquid, spawnPosition, Quaternion.Euler(0, Random.Range(0, 360f), 0));

        SquidTransformList.Add(SquidTransform);
        animalTransformList.Add(SquidTransform);
    }

    public void SpawnTraining()
    {
        if (lastTrainingTransform != null)
        {
            Destroy(lastTrainingTransform.gameObject);
        }

        lastTrainingIsSquid = Random.Range(0, 100) < 50;
        Vector3 spawnPosition = SquidFishAgent.transform.position + (SquidFishAgent.transform.forward * 1.5f);

        Transform prefab = lastTrainingIsSquid ? pfSquid : pfFish;
        lastTrainingTransform = Instantiate(prefab, spawnPosition, SquidFishAgent.transform.rotation);
        animalTransformList.Add(lastTrainingTransform);
        if (lastTrainingIsSquid)
        {
            SquidTransformList.Add(lastTrainingTransform);
        }
        else
        {
            FishTransformList.Add(lastTrainingTransform);
        }
    }

    public bool IsLastTrainingSquid()
    {
        return lastTrainingIsSquid;
    }

    public bool TrySelectAnimal(bool isSquid)
    {
        Debug.Log((isSquid ? "Squid" : "Fish") + " :: " + (lastTrainingIsSquid ? "Squid" : "Fish"));

        SendAnimalToPen(lastTrainingTransform, isSquid);

        if (isSquid == lastTrainingIsSquid)
        {
            // Correct!
            return true;
        }
        else
        {
            // Wrong!
            return false;
        }
    }

}