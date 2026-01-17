using UnityEngine;

public class PlaneBendDriver : MonoBehaviour
{
    PlaneBendController_Part2 bendController;
    LeanZoneDetector zoneDetector;

    void Start()
    {
        bendController = GetComponent<PlaneBendController_Part2>();
        zoneDetector = FindObjectOfType<LeanZoneDetector>();
    }

    void Update()
    {
        if (ExperimentConfig.Instance.condition == "Static")
        {
            bendController.SetBend(0f, 0);
            return;
        }

        string zone = zoneDetector.GetActiveZone();

        if (zone == "Left")
            bendController.SetBend(1f, -1);
        else if (zone == "Right")
            bendController.SetBend(1f, +1);
        else
            bendController.SetBend(0f, 0);
    }
}
