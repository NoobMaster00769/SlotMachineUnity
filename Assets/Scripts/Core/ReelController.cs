using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReelController : MonoBehaviour
{
    [Header("Symbol Configuration")]
    [SerializeField] private SymbolData[] _symbolStrip;
    [SerializeField] private Image[] _visibleSlots;

    [Header("Spin Settings")]
    [SerializeField] private float _spinDuration = 2f;
    [SerializeField] private float _symbolScrollSpeed = 800f;

    private int _currentStopIndex;

    // The symbol that ends up in the center slot
    public SymbolData Result => _symbolStrip[_currentStopIndex];

    private bool _isSpinning;

    public void BuildWeightedStrip(SymbolData[] availableSymbols)
    {
        var strip = new List<SymbolData>();

        // Add symbols multiple times based on weight (simple probability system)
        foreach (var symbol in availableSymbols)
        {
            for (int i = 0; i < symbol.weight; i++)
                strip.Add(symbol);
        }

        for (int i = strip.Count - 1; i > 0; i--)
        {
            int j = RNGService.Next(i + 1);
            (strip[i], strip[j]) = (strip[j], strip[i]);
        }

        _symbolStrip = strip.ToArray();
    }

    public float GetSpinDuration()
    {
        return _spinDuration;
    }

    public IEnumerator SpinAndStop(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        _isSpinning = true;

        float elapsed = 0f;
        float spinTime = _spinDuration;

        // Spin with easing (fast to slow)
        while (elapsed < spinTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spinTime;

            float speed = t < 0.7f
                ? _symbolScrollSpeed
                : Mathf.Lerp(_symbolScrollSpeed, 0f, (t - 0.7f) / 0.3f);

            ScrollSymbols(speed * Time.deltaTime);
            yield return null;
        }

        // Pick final stop index randomly
        _currentStopIndex = RNGService.Next(_symbolStrip.Length);

        DisplaySymbols(_currentStopIndex);

        _isSpinning = false;
    }

    private void ScrollSymbols(float amount)
    {
        foreach (var slot in _visibleSlots)
        {
            slot.rectTransform.anchoredPosition += Vector2.up * amount;

            // When a symbol goes out of view, recycle it to the bottom
            if (slot.rectTransform.anchoredPosition.y > 150f)
            {
                slot.rectTransform.anchoredPosition -=
                    new Vector2(0, _visibleSlots.Length * 150f);

                int randomIdx = RNGService.Next(_symbolStrip.Length);
                slot.sprite = _symbolStrip[randomIdx].sprite;
            }
        }
    }

    private void DisplaySymbols(int centerIndex)
    {
        int count = _symbolStrip.Length;

        // Align symbols so selected one is centered
        for (int i = 0; i < _visibleSlots.Length; i++)
        {
            int offset = i - 1;
            int stripIdx = ((centerIndex + offset) % count + count) % count;

            _visibleSlots[i].sprite = _symbolStrip[stripIdx].sprite;
            _visibleSlots[i].rectTransform.anchoredPosition =
                new Vector2(0, -offset * 150f);
        }
    }

    [SerializeField] private Animator _midSymbolAnimator;

    public void SetHighlight(bool on)
    {
        if (_visibleSlots.Length > 1)
            _visibleSlots[1].color = on ? Color.yellow : Color.white;
        if (_midSymbolAnimator != null)
            _midSymbolAnimator.SetBool("IsWinning", on);
    }
}