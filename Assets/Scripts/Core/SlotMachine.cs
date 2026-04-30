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
    public event Action<int> OnFreeSpinsAwarded;
    public event Action<int> OnFreeSpinCountChanged;
    public event Action<int> OnStreakChanged;
    public event Action<long> OnJackpotChanged;

    private bool _isSpinning;
    private int _freeSpinsRemaining;
    private int _winStreak;
    private long _jackpotPool;

    private const long _jackpotSeed = 500;

    private void Start()
    {
        // Initialize reels once
        foreach (var reel in _reels)
            reel.BuildWeightedStrip(_availableSymbols);

        // Seed jackpot so it’s never zero
        _jackpotPool = _jackpotSeed;
        OnJackpotChanged?.Invoke(_jackpotPool);
    }

    public void Spin(int betAmount)
    {
        if (_isSpinning) return;

        // Small % of every bet goes into jackpot pool
        _jackpotPool += (long)(betAmount * 0.05f);
        OnJackpotChanged?.Invoke(_jackpotPool);

        StartCoroutine(SpinSequence(betAmount));
    }

    public bool HasFreeSpins => _freeSpinsRemaining > 0;

    public void SpinFree(int betAmount)
    {
        if (_isSpinning || _freeSpinsRemaining <= 0) return;

        _freeSpinsRemaining--;
        OnFreeSpinCountChanged?.Invoke(_freeSpinsRemaining);

        StartCoroutine(SpinSequence(betAmount));
    }

    private IEnumerator SpinSequence(int betAmount)
    {
        _isSpinning = true;
        OnSpinStarted?.Invoke();

        // Reset highlights before new spin
        foreach (var reel in _reels)
            reel.SetHighlight(false);

        // Start reels with delay (adds anticipation)
        for (int i = 0; i < _reels.Length; i++)
            StartCoroutine(_reels[i].SpinAndStop(delay: i * _reelStopDelay));

        float totalWait = _reels[0].GetSpinDuration() +
                          (_reels.Length - 1) * _reelStopDelay + 0.1f;

        yield return new WaitForSeconds(totalWait);

        int payout = EvaluateResult(betAmount);

        OnSpinComplete?.Invoke(payout);
        _isSpinning = false;
    }

    private int EvaluateResult(int betAmount)
    {
        SymbolData[] results = new SymbolData[_reels.Length];
        for (int i = 0; i < _reels.Length; i++)
            results[i] = _reels[i].Result;

        // Special case: all wild → reward free spins instead of payout
        bool allWild = true;
        foreach (var r in results)
            if (!r.isWild) { allWild = false; break; }

        if (allWild)
        {
            int spinsAwarded = 5;
            _freeSpinsRemaining += spinsAwarded;
            OnFreeSpinsAwarded?.Invoke(spinsAwarded);

            // Reset streak (free spins act as a separate phase)
            _winStreak = 0;
            OnStreakChanged?.Invoke(_winStreak);

            return 0;
        }

        // Determine effective symbol (first non-wild)
        SymbolData effectiveSymbol = null;
        foreach (var r in results)
            if (!r.isWild) { effectiveSymbol = r; break; }

        if (effectiveSymbol == null)
            effectiveSymbol = results[0];

        // Check if it's a valid win
        bool isWin = true;
        foreach (var r in results)
            if (!r.isWild && r != effectiveSymbol) { isWin = false; break; }

        if (!isWin)
        {
            _winStreak = 0;
            OnStreakChanged?.Invoke(_winStreak);
            return 0;
        }

        // Highlight winning line
        foreach (var reel in _reels)
            reel.SetHighlight(true);

        _winStreak++;
        OnStreakChanged?.Invoke(_winStreak);

        // Combine multipliers (symbol + wilds + streak)
        int wildCount = 0;
        foreach (var r in results)
            if (r.isWild) wildCount++;

        int streakMultiplier = Mathf.Min(1 + (_winStreak / 5), 3);
        int wildMultiplier = (int)Mathf.Pow(2, wildCount);

        int totalMultiplier =
            effectiveSymbol.payoutMultiplier *
            wildMultiplier *
            streakMultiplier;

        return betAmount * totalMultiplier;
    }

    public long ClaimJackpot()
    {
        // Reset jackpot after claim
        long amount = _jackpotPool;

        _jackpotPool = _jackpotSeed;
        OnJackpotChanged?.Invoke(_jackpotPool);

        return amount;
    }

    public void ResetHighlights()
    {
        foreach (var reel in _reels)
            reel.SetHighlight(false);
    }

}