using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Balance & Bet Display")]
    [SerializeField] private TextMeshProUGUI _balanceText;
    [SerializeField] private TextMeshProUGUI _betText;

    [Header("Buttons")]
    [SerializeField] private Button _spinButton;
    [SerializeField] private Button _betIncreaseButton;
    [SerializeField] private Button _betDecreaseButton;

    [Header("Win Display")]
    [SerializeField] private PopupManager _popupManager;

    private void Start()
    {
        // Hook into GameManager events so UI stays in sync
        GameManager.Instance.OnBalanceChanged += UpdateBalance;
        GameManager.Instance.OnBetChanged += UpdateBet;
        GameManager.Instance.OnWin += HandleWin;
        GameManager.Instance.OnLoss += HandleLoss;
        GameManager.Instance.OnSpinStarted += DisableButtons;

        // Wire up button clicks
        _spinButton.onClick.AddListener(GameManager.Instance.RequestSpin);
        _betIncreaseButton.onClick.AddListener(GameManager.Instance.IncreaseBet);
        _betDecreaseButton.onClick.AddListener(GameManager.Instance.DecreaseBet);
    }

    private void UpdateBalance(int balance)
    {
        // Update balance display
        _balanceText.text = $"Balance: {balance}G";
    }

    private void UpdateBet(int bet)
    {
        // Update bet display
        _betText.text = $"Bet: {bet}G";
    }

    private void HandleWin(int payout)
    {
        // Re-enable controls after spin ends
        EnableButtons();

        // Show win popup
        _popupManager.ShowWin(payout);
    }

    private void HandleLoss()
    {
        // No popup for loss (keeping it simple for now)
        EnableButtons();
    }

    private void DisableButtons()
    {
        // Prevent spam clicking during spin
        _spinButton.interactable = false;
        _betIncreaseButton.interactable = false;
        _betDecreaseButton.interactable = false;
    }

    private void EnableButtons()
    {
        // Restore interactivity once spin is done
        _spinButton.interactable = true;
        _betIncreaseButton.interactable = true;
        _betDecreaseButton.interactable = true;
    }
}