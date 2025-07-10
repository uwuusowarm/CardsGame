using UnityEngine;

public class OptionsIngameUI : MonoBehaviour
{
    public void OnOptionsButtonPressed()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ToggleOptionsPanel();
        }
        else
        {
            Debug.LogWarning("SettingsManager.Instance nicht gefunden.");
        }
    }
}