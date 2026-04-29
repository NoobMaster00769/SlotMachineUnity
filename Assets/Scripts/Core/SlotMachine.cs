using System;
using System.Collections;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    [Header("Reels")]
    [SerializeField] private ReelController[] _reels = new ReelController[3];

    [Header("Spin Timing")]
    [SerializeField] private float _reelStopDelay = 0.35f;

    [Header("Symbol Data")]
    [SerializeField] private SymbolData[] _availableSymbols;

    public event Action<int> OnSpinComplete;
    public event Action OnSpinStarted;

    private bool _isSpinning;

    private void Start()
    {
        // Build weighted strips once at start
        foreach (var reel in _reels)
            reel.BuildWeightedStrip(_availableSymbols);
    }

    public void Spin(int betAmount)
    {
        // Avoid overlapping spins 
        if (_isSpinning) return;

        StartCoroutine(SpinSequence(betAmount));
    }

    private IEnumerator SpinSequence(int betAmount)
    {
        _isSpinning = true;
        OnSpinStarted?.Invoke();

        // Clear previous highlights
        foreach (var reel in _reels)
            reel.SetHighlight(false);

        // Start reels with staggered delay 
        for (int i = 0; i < _reels.Length; i++)
        {
            StartCoroutine(_reels[i].SpinAndStop(delay: i * _reelStopDelay));
        }

        // Wait long enough for all reels to stop
        float totalWait = _reels[0].GetSpinDuration() +
                          (_reels.Length - 1) * _reelStopDelay + 0.1f;

        yield return new WaitForSeconds(totalWait);

        int payout = EvaluateResult(betAmount);

        OnSpinComplete?.Invoke(payout);
        _isSpinning = false;
    }

    private int EvaluateResult(int betAmount)
    {
        // Collect final symbols
        SymbolData[] results = new SymbolData[_reels.Length];
        for (int i = 0; i < _reels.Length; i++)
            results[i] = _reels[i].Result;

        // Find first non-wild symbol to define the combo
        SymbolData effectiveSymbol = null;
        foreach (var r in results)
        {
            if (!r.isWild)
            {
                effectiveSymbol = r;
                break;
            }
        }

        // Edge case: all wilds
        if (effectiveSymbol == null)
            effectiveSymbol = results[0];

        // Check if all symbols match (considering wilds)
        bool isWin = true;
        foreach (var r in results)
        {
            if (!r.isWild && r != effectiveSymbol)
            {
                isWin = false;
                break;
            }
        }

        if (!isWin) return 0;

        // Highlight winning line
        foreach (var reel in _reels)
            reel.SetHighlight(true);

        // Count wilds for multiplier boost
        int wildCount = 0;
        foreach (var r in results)
            if (r.isWild) wildCount++;

        int multiplier = effectiveSymbol.payoutMultiplier *
                         (int)Mathf.Pow(2, wildCount);

        return betAmount * multiplier;
    }
}