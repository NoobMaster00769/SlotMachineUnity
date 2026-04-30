using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Central access point for game state
    public static GameManager Instance { get; private set; }

    [Header("Starting Values")]
    [SerializeField] private int _startingBalance = 1000;
    [SerializeField] private int _minBet = 10;
    [SerializeField] private int _maxBet = 100;
    [SerializeField] private int _betStep = 10;

    public int Balance { get; private set; }
    public int CurrentBet { get; private set; }
    public bool IsSpinning { get; private set; }

    // Core gameplay events
    public event Action<int> OnBalanceChanged;
    public event Action<int> OnBetChanged;
    public event Action<int> OnWin;
    public event Action OnLoss;
    public event Action OnSpinStarted;

    // Bonus system events
    public event Action<int> OnFreeSpinsAwarded;
    public event Action<int> OnFreeSpinCountChanged;
    public event Action<int> OnStreakChanged;
    public event Action<long> OnJackpotChanged;
    public event Action OnGameOver;
    public event Action OnFreeSpinWin;

    [SerializeField] private SlotMachine _slotMachine;

    private void Awake()
    {
        // Prevent duplicate managers (common Unity pitfall)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Balance = _startingBalance;
        CurrentBet = _minBet;

        // Forward slot machine events to UI layer
        _slotMachine.OnSpinComplete += HandleSpinResult;
        _slotMachine.OnSpinStarted += () =>
        {
            IsSpinning = true;
            OnSpinStarted?.Invoke();
        };

        _slotMachine.OnFreeSpinsAwarded += count => OnFreeSpinsAwarded?.Invoke(count);
        _slotMachine.OnFreeSpinCountChanged += count => OnFreeSpinCountChanged?.Invoke(count);
        _slotMachine.OnStreakChanged += streak => OnStreakChanged?.Invoke(streak);
        _slotMachine.OnJackpotChanged += pool => OnJackpotChanged?.Invoke(pool);

        // Push initial UI state
        OnBalanceChanged?.Invoke(Balance);
        OnBetChanged?.Invoke(CurrentBet);
    }

    public void RequestSpin()
    {
        if (IsSpinning) return;

        // Free spins override normal betting
        if (_slotMachine.HasFreeSpins)
        {
            _slotMachine.SpinFree(CurrentBet);
            return;
        }

        // No money left → game over
        if (Balance < CurrentBet)
        {
            OnGameOver?.Invoke();
            return;
        }

        // Deduct bet and spin
        Balance -= CurrentBet;
        OnBalanceChanged?.Invoke(Balance);

        _slotMachine.Spin(CurrentBet);
    }

    public void IncreaseBet()
    {
        if (IsSpinning) return;

        CurrentBet = Mathf.Min(CurrentBet + _betStep, _maxBet);
        OnBetChanged?.Invoke(CurrentBet);
    }

    public void DecreaseBet()
    {
        if (IsSpinning) return;

        CurrentBet = Mathf.Max(CurrentBet - _betStep, _minBet);
        OnBetChanged?.Invoke(CurrentBet);
    }

    private void HandleSpinResult(int payout)
    {
        IsSpinning = false;

        if (payout > 0)
        {
            Balance += payout;
            OnBalanceChanged?.Invoke(Balance);
            OnWin?.Invoke(payout);
        }
        else
        {
            OnLoss?.Invoke();
        }

        // Soft fail condition (keeps tension instead of immediate loss)
        if (Balance <= 50 && !_slotMachine.HasFreeSpins)
            OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        // Reset everything to initial state
        Balance = _startingBalance;
        CurrentBet = _minBet;
        IsSpinning = false;

        OnBalanceChanged?.Invoke(Balance);
        OnBetChanged?.Invoke(CurrentBet);
    }
}