[System.Serializable]
public class CardEffect
{
    public EffectType effectType;
    public int value;
    public bool isTemporary;
    public int duration;

    public enum EffectType
    {
        Attack,
        Move,
        Heal,
        Burn,
        Block
    }
}