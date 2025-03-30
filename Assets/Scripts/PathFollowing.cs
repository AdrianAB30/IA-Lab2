using UnityEngine;
using System.Collections.Generic;

public class PathFollowing : MonoBehaviour
{
    public List<Transform> paths = new List<Transform>();
    public int index = 0;
    public float valuedistance = 0.5f;
    public bool loop = true;

    public Transform NextPoint(Vector3 currentPosition)
    {
        if (paths.Count == 0) return null;

        float distance = Vector3.Distance(currentPosition, paths[index].position);

        if (distance < valuedistance)
        {
            if (loop) index = (index + 1) % paths.Count;
            else index = Mathf.Clamp(index + 1, 0, paths.Count - 1);
        }

        return paths[index];
    }
}