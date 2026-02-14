using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(SlimeSpringGenerator))]
public class SlimeMover : MonoBehaviour
{
    SlimeSpringGenerator slimeGen;
    [ShowInInspector, PropertyRange(0, 100)]
    public float ForceMultiplier = 10f;

    [SerializeField]
    bool SoftCapVelocity = true;
    void Start()
    {
        slimeGen = GetComponent<SlimeSpringGenerator>();

    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.A))
        {
            //move each rigidbody towards its neighbour
            for(int i = 0; i < slimeGen.SlimeRigidbodies.Count; i++)
            {
                Rigidbody2D rb = slimeGen.SlimeRigidbodies[i];
                Rigidbody2D neighbour = slimeGen.SlimeRigidbodies[(i + 1) % slimeGen.SlimeRigidbodies.Count];
                Vector2 direction = (neighbour.position - rb.position).normalized;
                rb.AddForce(direction * ForceMultiplier); // Adjust the force as needed
            }

        }
        if (Input.GetKey(KeyCode.D))
        {
            //move each rigidbody towards its other neighbour
            for (int i = 0; i < slimeGen.SlimeRigidbodies.Count; i++)
            {
                Rigidbody2D rb = slimeGen.SlimeRigidbodies[i];
                Rigidbody2D neighbour = slimeGen.SlimeRigidbodies[(i - 1 + slimeGen.SlimeRigidbodies.Count) % slimeGen.SlimeRigidbodies.Count];
                Vector2 direction = (neighbour.position - rb.position).normalized;
                rb.AddForce(direction * ForceMultiplier); // Adjust the force as needed
            }

        }

        if (SoftCapVelocity)
        {
            //add a soft cap to the velocity of each rigidbody to prevent it from flying off
            foreach (Rigidbody2D rb in slimeGen.SlimeRigidbodies)
            {
                if (rb.linearVelocity.magnitude > 5f) // Adjust the max speed as needed
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * 5f;
                }
            }
        }
    }


}
