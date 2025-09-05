using UnityEngine;

public class Utils : MonoBehaviour
{

    /// <summary>
    /// Rotates the origin to face the target in 2D space.  
    /// Uses Rigidbody2D rotation if available, otherwise sets Transform.eulerAngles.  
    /// </summary>
    /// <param name="origin">The Transform that will rotate.</param>
    /// <param name="target">The Transform to look at.</param>
    public static void LookToObject2D(Transform origin, Vector3 target) 
    {

        Vector3 direction = (target - origin.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (origin.GetComponent<Rigidbody2D>() != null)
        {
            origin.GetComponent<Rigidbody2D>().rotation = angle;
        }
        else 
        { 
            origin.eulerAngles = new Vector3 (0, 0, angle);
        }

    }
    public static void LookToObject2D(Transform origin, Transform target) 
    {
        LookToObject2D (origin, target.position);
    }
}
