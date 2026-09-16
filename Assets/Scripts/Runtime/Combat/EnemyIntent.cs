using UnityEngine;

public class EnemyIntent : MonoBehaviour
{
    public IntentType currentIntent;
    public int intentValue;

    public void UpdateIntent()
    {
    }

    public void SetIntent(IntentType intent, int value)
    {
        currentIntent = intent;
        intentValue = value;
    }
}
