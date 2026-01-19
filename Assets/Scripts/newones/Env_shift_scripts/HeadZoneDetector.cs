using UnityEngine;

public class HeadZoneDetector : MonoBehaviour
{
    public bool inLeftZone { get; private set; }
    public bool inRightZone { get; private set; }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("LeftZone"))
    //         inLeftZone = true;

    //     if (other.CompareTag("RightZone"))
    //         inRightZone = true;
    // }

    void OnTriggerEnter(Collider other)
{
    Debug.Log($"ENTER: {other.name}, tag={other.tag}, layer={other.gameObject.layer}");

    if (other.CompareTag("LeftZone"))
        inLeftZone = true;

    if (other.CompareTag("RightZone"))
        inRightZone = true;
    
}

void OnTriggerExit(Collider other)
{
    Debug.Log($"EXIT: {other.name}, tag={other.tag}");

    if (other.CompareTag("LeftZone"))
        inLeftZone = false;

    if (other.CompareTag("RightZone"))
        inRightZone = false;
}


    public int ActiveZoneCount()
    {
        int count = 0;
        if (inLeftZone) count++;
        if (inRightZone) count++;
        return count;
    }
     
    void OnGUI()
{
    GUI.Label(new Rect(10, 10, 300, 20),
        $"Left: {inLeftZone} | Right: {inRightZone}");
}


}
// using UnityEngine;

// public class HeadZoneDetector : MonoBehaviour
// {
//     public bool inLeftZone;
//     public bool inRightZone;

//     void OnTriggerEnter(Collider other)
//     {
//         if (!other.CompareTag("LeftZone") && !other.CompareTag("RightZone"))
//             return;

//         Debug.Log($"HEAD ENTERED {other.tag}");

//         if (other.CompareTag("LeftZone"))
//             inLeftZone = true;

//         if (other.CompareTag("RightZone"))
//             inRightZone = true;
//     }

//     void OnTriggerExit(Collider other)
//     {
//         if (!other.CompareTag("LeftZone") && !other.CompareTag("RightZone"))
//             return;

//         Debug.Log($"HEAD EXITED {other.tag}");

//         if (other.CompareTag("LeftZone"))
//             inLeftZone = false;

//         if (other.CompareTag("RightZone"))
//             inRightZone = false;
//     }

//     public int ActiveZoneCount()
//     {
//         return (inLeftZone ? 1 : 0) + (inRightZone ? 1 : 0);
//     }
// }
