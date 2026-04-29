using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Starting Values")]
    [SerializeField] private int _startingBalance = 1000;
    [SerializeField] private int _minBet = 10;
    [SerializeField] private int _maxBet = 100;
    [SerializeField] private int _betStep = 10;

    // Core gameplay values
    public int Balance { get; private set; }
    public int CurrentBet { get; private set; }
    public bool IsSpinning { get; private set; }

    // Events for UI / feedback
    public event Action<int> OnBalanceChanged;
    public event Action<int> OnBetChanged;
    public event Action<int> OnWin;
    public event Action OnLoss;
    public event Action OnSpinStarted;

    [SerializeField] private SlotMachine _slotMachine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Initialize starting state
        Balance = _startingBalance;
        CurrentBet = _minBet;

        // Hook into slot machine events
        _slotMachine.OnSpinComplete += HandleSpinResult;
        _slotMachine.OnSpinStarted += () =>
        {
            IsSpinning = true;
            OnSpinStarted?.Invoke();
        };

        // Push initial values to UI
        OnBalanceChanged?.Invoke(Balance);
        OnBetChanged?.Invoke(CurrentBet);
    }

    public void RequestSpin()
    {
        // Prevent double spins
        if (IsSpinning) return;

        // Basic balance check
        if (Balance < CurrentBet)
        {
            Debug.Log("Insufficient balance!");
            return;
        }

        // Deduct bet immediately (feels more responsive)
        Balance -= CurrentBet;
        OnBalanceChanged?.Invoke(Balance);

        _slotMachine.Spin(CurrentBet);
    }

    public void IncreaseBet()
    {
        if (IsSpinning) return;

        // Clamp to max so we don’t overshoot
        CurrentBet = Mathf.Min(CurrentBet + _betStep, _maxBet);
        OnBetChanged?.Invoke(CurrentBet);
    }

    public void DecreaseBet()
    {
        if (IsSpinning) return;

        // Clamp to min for same reason
        CurrentBet = Mathf.Max(CurrentBet - _betStep, _minBet);
        OnBetChanged?.Invoke(CurrentBet);
    }

    private void HandleSpinResult(int payout)
    {
        IsSpinning = false;

        if (payout > 0)
        {
            // Add winnings back
            Balance += payout;
            OnBalanceChanged?.Invoke(Balance);
            OnWin?.Invoke(payout);
        }
        else
        {
            OnLoss?.Invoke();
        }
    }
}