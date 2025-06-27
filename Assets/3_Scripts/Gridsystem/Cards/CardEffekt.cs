using UnityEngine;

[System.Serializable]
public class CardEffect
{
    public EffectType effectType;
    public int value;
    public bool isTemporary;
    public int duration;
    [Tooltip("0 = melee, 1+ = range")]
    public int range;

    public enum EffectType
    {
        Attack,
        Move,
        Heal,
        Burn,
        Block,
        
        Draw,
        Discard,
        Freeze,
        Poison,
        ActionPlus,
        None
    }
}