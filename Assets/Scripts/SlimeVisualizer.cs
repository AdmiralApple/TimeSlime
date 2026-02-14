using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
public class SlimeVisualizer : MonoBehaviour
{
    [SerializeField] SlimeSpringGenerator slimeSpringGenerator;
    SpriteShapeController shape;

    private void Update()
    {
        if (shape == null) shape = GetComponent<SpriteShapeController>();
        if(shape.spline.GetPointCount() != slimeSpringGenerator.SlimePoints.Count)
        {

        }
        for (var i = 0; i < slimeSpringGenerator.SlimePoints.Count; i++)
        {
            shape.spline.SetPosition(i, slimeSpringGenerator.SlimePoints[i].position);
        }
    }
}
