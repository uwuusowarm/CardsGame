using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CardTemplate : MonoBehaviour
{
    [Header("Class")]
    public String cardClass;
    
    [Header("Card Name")]
    public String cardName;
    
    [Header("Attack Value")]
    [Range(0, 10)]
    public int attackVal = 1;  
    
    [Header("Block Value")]
    [Range(0, 10)]
    public int blockVal = 1; 
    
    [Header("Range Value")]
    [Range(0, 5)]
    public int rangeVal = 1; 
    
    private int tempAt;
    private int tempBlock;
    void Start()
    {
        tempAt = attackVal;
        tempBlock = blockVal;
        
        if (cardClass == null)
        {
            Debug.Log("Card Class not set");
        }
        else
        {
            Debug.Log("Class: " + cardClass);
        }

        if (cardName == null)
        {
            Debug.Log("Card Name not set");
        }
        else
        {
            Debug.Log("Card Name not set");
        }
        
        Debug.Log("Attack: " + attackVal + " Block: " + blockVal + " Range: " + rangeVal);

        
    }

    void Update()
    {
        if (tempAt != attackVal)
        {
            Debug.Log("New Attack Value: " + attackVal);
            tempAt = attackVal;
        }

        if (tempBlock != blockVal)
        {
            Debug.Log("New Block Value: " + blockVal);
            tempBlock = blockVal;
        }
    }
}
