using UnityEngine;

public class InGameUI : MonoBehaviour
{
    public void ToggleOptionsMenu()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ToggleOptionsPanel();
        }
    }
}