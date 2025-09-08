using UnityEngine;

public class Cam : MonoBehaviour
{
    public Transform target;
    public Vector3 offSet = new Vector3(0, 0, -15);
    public float smooth = 5;

    void Start()
    {
        if (target == null)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
    }

    void LateUpdate()
    {
        Vector3 targetPos = target.position + offSet;
        transform.position = Vector3.Lerp(transform.position, targetPos, smooth * Time.deltaTime);

    }

    public void TrocarPos(Transform Target)
    {
        transform.position = Target.position + offSet;
    }
}
