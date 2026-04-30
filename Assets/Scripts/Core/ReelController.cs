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
    [SerializeField] private float _spinDuration = 10f;

    [Header("Layout")]
    [SerializeField] private float _symbolSpacing = 95f;

    [Header("Win Animation")]
    [SerializeField] private Animator _midSymbolAnimator;

    private int _currentStopIndex;
    private bool _isSpinning;
    private const float PeakSpeed = 450f;

    public SymbolData Result => _symbolStrip[_currentStopIndex];
    public float GetSpinDuration() => _spinDuration;

    public void BuildWeightedStrip(SymbolData[] availableSymbols)
    {
        var strip = new List<SymbolData>();

        // Duplicate symbols based on weight (basic probability setup)
        foreach (var symbol in availableSymbols)
            for (int i = 0; i < symbol.weight; i++)
                strip.Add(symbol);

        // Shuffle so it’s not predictable
        for (int i = strip.Count - 1; i > 0; i--)
        {
            int j = RNGService.Next(i + 1);
            (strip[i], strip[j]) = (strip[j], strip[i]);
        }

        _symbolStrip = strip.ToArray();

        // Pick a random starting point so reels don’t all look identical
        _currentStopIndex = RNGService.Next(_symbolStrip.Length);
        DisplaySymbols(_currentStopIndex);
    }

    public IEnumerator SpinAndStop(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        _isSpinning = true;
        float elapsed = 0f;

        // Spin loop (accelerate → hold → decelerate)
        while (elapsed < _spinDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / _spinDuration);
            float speed = PeakSpeed * SpeedCurve(t);

            ScrollSymbols(speed * Time.deltaTime);
            yield return null;
        }

        // Figure out which symbol is visually closest to center
        _currentStopIndex = GetClosestSymbolToCenter();

        // Tiny correction so it lands perfectly on the payline
        yield return StartCoroutine(NudgeToCenter(_currentStopIndex));

        // Re-sync index with what's actually visible (handles duplicate sprites)
        for (int i = 0; i < _symbolStrip.Length; i++)
        {
            if (_symbolStrip[i].sprite == _visibleSlots[1].sprite)
            {
                _currentStopIndex = i;
                break;
            }
        }

        _isSpinning = false;
    }

    private int GetClosestSymbolToCenter()
    {
        float minDist = float.MaxValue;
        int closestSlotIndex = 1; // assume middle unless proven otherwise

        // Check which slot is nearest to Y=0
        for (int i = 0; i < _visibleSlots.Length; i++)
        {
            float dist = Mathf.Abs(_visibleSlots[i].rectTransform.anchoredPosition.y);
            if (dist < minDist)
            {
                minDist = dist;
                closestSlotIndex = i;
            }
        }

        int slotOffset = closestSlotIndex - 1;

        Sprite closestSprite = _visibleSlots[closestSlotIndex].sprite;
        Sprite aboveSprite = closestSlotIndex > 0
            ? _visibleSlots[closestSlotIndex - 1].sprite : null;

        int count = _symbolStrip.Length;
        int bestMatch = -1;

        // Try to match not just the symbol, but also its neighbor
        // (helps when multiple identical symbols exist in strip)
        for (int i = 0; i < count; i++)
        {
            if (_symbolStrip[i].sprite != closestSprite) continue;

            if (aboveSprite != null)
            {
                int neighborIdx = ((i - 1) % count + count) % count;
                if (_symbolStrip[neighborIdx].sprite == aboveSprite)
                {
                    bestMatch = i;
                    break;
                }
            }

            if (bestMatch == -1)
                bestMatch = i; // fallback if no perfect match
        }

        if (bestMatch == -1)
            return RNGService.Next(count);

        // Convert from slot index → center index
        int centerIndex = ((bestMatch - slotOffset) % count + count) % count;
        return centerIndex;
    }

    private IEnumerator NudgeToCenter(int targetIndex)
    {
        Image midSlot = _visibleSlots[1];
        float startY = midSlot.rectTransform.anchoredPosition.y;

        // Already aligned, nothing to fix
        if (Mathf.Abs(startY) < 0.5f)
        {
            SnapPositionsOnly();
            yield break;
        }

        float elapsed = 0f;
        float duration = 0.04f;

        // Tiny smooth correction — shouldn’t be noticeable to player
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float smooth = 1f - (1f - t) * (1f - t);

            float newY = Mathf.Lerp(startY, 0f, smooth);
            float delta = newY - midSlot.rectTransform.anchoredPosition.y;

            foreach (var slot in _visibleSlots)
                slot.rectTransform.anchoredPosition += new Vector2(0f, delta);

            yield return null;
        }

        SnapPositionsOnly();
    }

    private float SpeedCurve(float t)
    {
        // Ease in
        if (t < 0.25f)
        {
            float x = t / 0.25f;
            return x * x * (3f - 2f * x);
        }
        // Constant speed
        else if (t < 0.55f)
        {
            return 1f;
        }
        // Ease out
        else
        {
            float x = (t - 0.55f) / 0.45f;
            float smoothed = x * x * (3f - 2f * x);
            return 1f - smoothed;
        }
    }

    private void ScrollSymbols(float amount)
    {
        foreach (var slot in _visibleSlots)
        {
            slot.rectTransform.anchoredPosition += Vector2.up * amount;

            // When symbol exits top, recycle it to bottom with a new sprite
            if (slot.rectTransform.anchoredPosition.y
                > _symbolSpacing * (_visibleSlots.Length - 1) / 2f
                + _symbolSpacing * 0.5f)
            {
                slot.rectTransform.anchoredPosition -=
                    new Vector2(0f, _visibleSlots.Length * _symbolSpacing);

                int randomIdx = RNGService.Next(_symbolStrip.Length);
                slot.sprite = _symbolStrip[randomIdx].sprite;
            }
        }
    }

    private void DisplaySymbols(int centerIndex)
    {
        int count = _symbolStrip.Length;

        // Position symbols so chosen one sits in the middle
        for (int i = 0; i < _visibleSlots.Length; i++)
        {
            int offset = i - 1;
            int stripIdx = ((centerIndex + offset) % count + count) % count;

            _visibleSlots[i].sprite = _symbolStrip[stripIdx].sprite;
            _visibleSlots[i].rectTransform.anchoredPosition =
                new Vector2(0f, -offset * _symbolSpacing);
        }
    }

    private void SnapPositionsOnly()
    {
        // Just fix positions without touching sprites
        for (int i = 0; i < _visibleSlots.Length; i++)
        {
            int offset = i - 1;
            _visibleSlots[i].rectTransform.anchoredPosition =
                new Vector2(0f, -offset * _symbolSpacing);
        }
    }

    public void SetHighlight(bool on)
    {
        // Highlight middle symbol (payline)
        if (_visibleSlots.Length > 1)
            _visibleSlots[1].color = on ? Color.yellow : Color.white;

        // Trigger win animation if available
        if (_midSymbolAnimator != null)
            _midSymbolAnimator.SetBool("IsWinning", on);
    }
}