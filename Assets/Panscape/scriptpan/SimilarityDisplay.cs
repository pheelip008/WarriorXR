using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimilarityDisplay : MonoBehaviour {
    public PoseMatcher matcher;
    public TMP_Text similarityText;
    public Slider similaritySlider;

    void Update() {
        if (matcher == null) return;
        float sim = matcher.ComputeSimilarity();
        if (similarityText != null) similarityText.text = $"Similarity: {Mathf.RoundToInt(sim*100)}%";
        if (similaritySlider != null) similaritySlider.value = sim;
    }
}
