using UnityEngine;

public class TriggerHelper : MonoBehaviour
{
    public GameObject triggerArea;

    public void ShowTriggerArea()
    {
        triggerArea.SetActive(true);
    }

    public void HideTriggerArea()
    {
        triggerArea.SetActive(false);
    }
}
