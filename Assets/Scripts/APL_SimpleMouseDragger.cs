using UnityEngine;

public class APL_SimpleMouseDragger : MonoBehaviour
{
    private Vector3 offset;
    private float zCoord;
    void OnMouseDown()
    {
        zCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        offset = gameObject.transform.position - GetMouseWorldPos();
    }
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
    void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + offset;
    }
}
