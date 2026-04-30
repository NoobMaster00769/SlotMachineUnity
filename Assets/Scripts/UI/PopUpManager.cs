using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _winAmountText;
    [SerializeField] private CanvasGroup _popupCanvasGroup;
    [SerializeField] private float _displayDuration = 2.5f;

    private void Start()
    {
        // Make sure popup starts hidden and doesn't block clicks
        _popupCanvasGroup.alpha = 0f;
        _popupCanvasGroup.blocksRaycasts = false;
        _popupCanvasGroup.interactable = false;
    }

    public void ShowWin(int amount)
    {
        _winAmountText.text = $"+{amount}G\nYOU WIN!";

        // Trigger popup animation sequence
        StartCoroutine(ShowAndHide());
    }

    private IEnumerator ShowAndHide()
    {
        // Fade in quickly
        yield return StartCoroutine(Fade(0f, 1f, 0.3f));

        // Keep it visible for a bit so player can read it
        yield return new WaitForSeconds(_displayDuration);

        // Fade out again
        yield return StartCoroutine(Fade(1f, 0f, 0.3f));
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _popupCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        // Snap to final value 
        _popupCanvasGroup.alpha = to;
    }
}