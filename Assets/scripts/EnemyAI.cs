using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Transform player;  // Assign player in Inspector
    private NavMeshAgent agent;
    public ThirdPersonCharacter character;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    void Update()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }

        if (agent.remainingDistance > agent.stoppingDistance) {
            character.Move(agent.desiredVelocity, false, false);
        }
        else {
            character.Move(Vector3.zero, false, false); 
        }
    }
}