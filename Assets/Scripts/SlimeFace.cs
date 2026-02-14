using UnityEngine;

public class SlimeFace : MonoBehaviour
{
    [SerializeField] SlimeSpringGenerator slimeSpringGenerator;
    void Update()
    {
        transform.position = new Vector3(slimeSpringGenerator.SlimeCenter.x, slimeSpringGenerator.SlimeCenter.y, 0);
    }
}
