using UnityEngine;

[CreateAssetMenu(fileName = "NewSymbol", menuName = "SlotGame/SymbolData")]
public class SymbolData : ScriptableObject
{
    [Header("Identity")]
    public string symbolName;
    public Sprite sprite;

    [Header("Game Logic")]
    [Tooltip("Payout multiplier when 3 of this symbol appear")]
    public int payoutMultiplier = 1;
    [Range(1, 20)]
    [Tooltip("Higher = appears more often on the reel")]
    public int weight = 5;
    [Tooltip("If true, substitutes for any symbol")]
    public bool isWild = false;
}