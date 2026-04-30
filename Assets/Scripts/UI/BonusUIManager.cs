using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BonusUIManager : MonoBehaviour
{
    [Header("Jackpot Display")]
    [SerializeField] private TextMeshProUGUI _jackpotText;

    [Header("Streak Display")]
    [SerializeField] private TextMeshProUGUI _streakText;

    [Header("Free Spins Counter")]
    [SerializeField] private TextMeshProUGUI _freeSpinsText;
    [SerializeField] private GameObject _freeSpinsBadge;

    [Header("Free Spins Popup")]
    [SerializeField] private CanvasGroup _freeSpinsPopup;
    [SerializeField] private TextMeshProUGUI _freeSpinsPopupText;
    [SerializeField] private Button _freeSpinsYesBtn;
    [SerializeField] private Button _freeSpinsNoBtn;

    [Header("Game Over Popup")]
    [SerializeField] private CanvasGroup _gameOverPopup;
    [SerializeField] private Button _restartYesBtn;
    [SerializeField] private Button _restartNoBtn;

    private void Start()
    {
        // Hide badge by default (only shows when player actually has free spins)
        if (_freeSpinsBadge) _freeSpinsBadge.SetActive(false);

        // Subscribe to game events so UI reacts automatically
        GameManager.Instance.OnJackpotChanged += UpdateJackpot;
        GameManager.Instance.OnStreakChanged += UpdateStreak;
        GameManager.Instance.OnFreeSpinsAwarded += ShowFreeSpinsPopup;
        GameManager.Instance.OnFreeSpinCountChanged += UpdateFreeSpinCount;
        GameManager.Instance.OnGameOver += ShowGameOverPopup;

        // Hook up popup buttons
        _freeSpinsYesBtn.onClick.AddListener(OnFreeSpinsYes);
        _freeSpinsNoBtn.onClick.AddListener(OnFreeSpinsNo);
        _restartYesBtn.onClick.AddListener(OnRestartYes);
        _restartNoBtn.onClick.AddListener(OnRestartNo);
    }

    private void UpdateJackpot(long amount)
    {
        // Simple jackpot formatting
        if (_jackpotText)
            _jackpotText.text = $"JACKPOT\n{amount}G";
    }

    private void UpdateStreak(int streak)
    {
        if (_streakText == null) return;

        // Hide streak UI unless it actually matters
        if (streak <= 1)
        {
            _streakText.gameObject.SetActive(false);
            return;
        }

        _streakText.gameObject.SetActive(true);

        // Slight scaling multiplier based on streak (capped to avoid crazy numbers)
        int multiplier = Mathf.Min(1 + (streak / 3), 5);
        _streakText.text = $"{streak} WIN STREAK  x{multiplier}";
    }

    private void ShowFreeSpinsPopup(int count)
    {
        // Prompt player before starting free spins
        _freeSpinsPopupText.text = $"FREE SPINS!\n{count} spins awarded!\nPlay now?";
        StartCoroutine(FadePopup(_freeSpinsPopup, true));
    }

    private void UpdateFreeSpinCount(int remaining)
    {
        // Show badge only when spins are available
        if (_freeSpinsBadge)
            _freeSpinsBadge.SetActive(remaining > 0);

        if (_freeSpinsText)
            _freeSpinsText.text = $"FREE x{remaining}";
    }

    private void ShowGameOverPopup()
    {
        // Trigger game over UI
        StartCoroutine(FadePopup(_gameOverPopup, true));
    }

    private void OnFreeSpinsYes()
    {
        // For now just close popup (game logic already handles spins)
        StartCoroutine(FadePopup(_freeSpinsPopup, false));
    }

    private void OnFreeSpinsNo()
    {
        // Same behavior — just dismiss
        StartCoroutine(FadePopup(_freeSpinsPopup, false));
    }

    private void OnRestartYes()
    {
        // Reset game state
        StartCoroutine(FadePopup(_gameOverPopup, false));
        GameManager.Instance.RestartGame();
    }

    private void OnRestartNo()
    {
        // Just close popup, player stays stuck (intentional for now)
        StartCoroutine(FadePopup(_gameOverPopup, false));
    }

    private IEnumerator FadePopup(CanvasGroup cg, bool fadeIn)
    {
        float from = fadeIn ? 0f : 1f;
        float to = fadeIn ? 1f : 0f;
        float duration = 0.3f;

        float elapsed = 0f;

        // Enable/disable interaction depending on visibility
        cg.blocksRaycasts = fadeIn;
        cg.interactable = fadeIn;

        // Standard fade loop
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        cg.alpha = to;
    }
}