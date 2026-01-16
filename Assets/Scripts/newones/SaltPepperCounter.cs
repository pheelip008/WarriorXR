using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaltPepperCounter : MonoBehaviour
{
    public static SaltPepperCounter Instance;

    [Header("Limits")]
    public int maxSalt = 10;
    public int maxPepper = 10;

    [Header("UI")]
    public Slider saltBar;
    public Slider pepperBar;
    public TextMeshProUGUI saltText;
    public TextMeshProUGUI pepperText;

    int saltCount = 0;
    int pepperCount = 0;

    void Awake()
    {
        Instance = this;

        saltBar.maxValue = maxSalt;
        pepperBar.maxValue = maxPepper;

        UpdateUI();
    }

    public void OnCollected(CollectibleType type)
    {
        if (type == CollectibleType.Salt && saltCount < maxSalt)
            saltCount++;

        if (type == CollectibleType.Pepper && pepperCount < maxPepper)
            pepperCount++;

        UpdateUI();
    }

    void UpdateUI()
    {
        saltBar.value = saltCount;
        pepperBar.value = pepperCount;

        if (saltText) saltText.text = $"{saltCount} / {maxSalt}";
        if (pepperText) pepperText.text = $"{pepperCount} / {maxPepper}";
    }

    public bool IsComplete()
    {
        return saltCount >= maxSalt && pepperCount >= maxPepper;
    }
}
