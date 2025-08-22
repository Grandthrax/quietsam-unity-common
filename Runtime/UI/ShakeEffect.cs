using UnityEngine;
using System.Collections;

public class ShakeEffect : MonoBehaviour
{
    [SerializeField] private Transform shakeTransform;

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 10f;
    [SerializeField] private int shakeVibrato = 10;
    
    [Header("Color Flash")]
    [SerializeField] private bool enableColorFlash = true;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;
    
    private RectTransform rectTransform;
    private UnityEngine.UI.Graphic graphic;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;
    private Coroutine flashCoroutine;
    
    public bool IsIdle => shakeCoroutine == null;
    
    private void Awake()
    {
        // Try to get UI components first
        rectTransform = GetComponent<RectTransform>();
        graphic = GetComponent<UnityEngine.UI.Graphic>();

        // If no UI components, try SpriteRenderer
        if (rectTransform == null)
        {
            if (shakeTransform == null)
                shakeTransform = GetComponent<Transform>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Store original color
        if (graphic != null)
        {
            originalColor = graphic.color;
        }
        else if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Store original position
        if (rectTransform != null)
        {
            originalPosition = rectTransform.localPosition;
        }
        else if (shakeTransform != null)
        {
            originalPosition = shakeTransform.localPosition;
        }
    }
    
    public void Shake()
    {
        if (rectTransform == null && shakeTransform == null) return;
        
        // Stop any existing shake
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        // Reset position
        if (rectTransform != null)
            rectTransform.localPosition = originalPosition;
        else if (shakeTransform != null)
            shakeTransform.localPosition = originalPosition;
        
        
        // Start shake
        shakeCoroutine = StartCoroutine(ShakeCoroutine());
        
        // Color flash
        if (enableColorFlash && (graphic != null || spriteRenderer != null))
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashCoroutine());
        }
    }
    
    public void ShakeWithCustomSettings(float duration, float strength, Color flashColor)
    {
        if (rectTransform == null && shakeTransform == null) return;
        
        // Stop any existing shake
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        // Reset position
        if (rectTransform != null)
            rectTransform.localPosition = originalPosition;
        else if (shakeTransform != null)
            shakeTransform.localPosition = originalPosition;
        
        // Start shake with custom settings
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, strength));
        
        // Custom color flash
        if (enableColorFlash && (graphic != null || spriteRenderer != null))
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashCoroutine(flashColor));
        }
    }
    
    private IEnumerator ShakeCoroutine(float duration = -1, float strength = -1)
    {
        if (duration < 0) duration = shakeDuration;
        if (strength < 0) strength = shakeStrength;
        
        float elapsed = 0f;
        Vector3 previousOffset = Vector3.zero;
        
        while (elapsed < duration)
        {
            // Calculate the base position by removing the previous offset
            Vector3 basePosition;
            if (rectTransform != null)
                basePosition = rectTransform.localPosition - previousOffset;
            else
                basePosition = shakeTransform.localPosition - previousOffset;
            
            // Generate new random offset
            Vector3 newOffset = new Vector3(Random.Range(-strength, strength), Random.Range(-strength, strength), 0);
            
            // Apply new position
            if (rectTransform != null)
                rectTransform.localPosition = basePosition + newOffset;
            else
                shakeTransform.localPosition = basePosition + newOffset;
            
            // Store offset for next iteration
            previousOffset = newOffset;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Reset to base position (without any offset)
        if (rectTransform != null)
            rectTransform.localPosition = rectTransform.localPosition - previousOffset;
        else if (shakeTransform != null)
            shakeTransform.localPosition = shakeTransform.localPosition - previousOffset;
        
        shakeCoroutine = null;
    }
    
    private IEnumerator FlashCoroutine(Color? customColor = null)
    {
        Color targetColor = customColor ?? flashColor;
        
        // Flash to target color
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            Color newColor = Color.Lerp(originalColor, targetColor, elapsed / flashDuration);
            
            if (graphic != null)
                graphic.color = newColor;
            else if (spriteRenderer != null)
                spriteRenderer.color = newColor;
                
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Flash back to original color
        elapsed = 0f;
        while (elapsed < flashDuration)
        {
            Color newColor = Color.Lerp(targetColor, originalColor, elapsed / flashDuration);
            
            if (graphic != null)
                graphic.color = newColor;
            else if (spriteRenderer != null)
                spriteRenderer.color = newColor;
                
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Reset to original color
        if (graphic != null)
            graphic.color = originalColor;
        else if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
            
        flashCoroutine = null;
    }
  
} 