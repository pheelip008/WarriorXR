using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour {
    public PoseRecorderUI recorder;
    public PoseMatcher matcher;
    public PoseApplier ghost;
    public TMP_Text modeText;
    public Button switchToPlayButton;
    public Button switchToRecordButton;
    public GameObject doorToOpen;       // example in-scene object to move

    void Start() {
        if (switchToPlayButton != null) switchToPlayButton.onClick.AddListener(EnterPlayMode);
        if (switchToRecordButton != null) switchToRecordButton.onClick.AddListener(EnterRecordMode);
        EnterRecordMode();
    }

    public void EnterRecordMode() {
        UpdateModeUI("Record Mode: perform pose and press Record");
        if (matcher != null) matcher.OnPoseSucceeded = null;
        if (ghost != null) ghost.gameObject.SetActive(false);
    }

    public void EnterPlayMode() {
        string fname = (recorder != null ? recorder.lastSavedFileName : null);
        if (string.IsNullOrEmpty(fname)) {
            string folder = Path.Combine(Application.persistentDataPath, "RecordedPoses");
            if (Directory.Exists(folder)) {
                var files = Directory.GetFiles(folder, "*.json");
                if (files.Length > 0) fname = Path.GetFileName(files[files.Length-1]);
            }
        }
        if (string.IsNullOrEmpty(fname)) {
            UpdateModeUI("No saved pose found. Record first.");
            return;
        }

        if (matcher != null) {
            matcher.loadFileName = fname;
            matcher.LoadPoseFromFile(fname);
            matcher.OnPoseSucceeded += OnMatched;
        }

        if (ghost != null) {
            ghost.gameObject.SetActive(true);
            ghost.LoadAndApply(fname);
        }

        UpdateModeUI("Play Mode: match pose " + fname);
    }

    void OnMatched() {
        Debug.Log("[GameManager] Match succeeded!");
        UpdateModeUI("Matched! Triggered event.");
        TriggerEvent();
    }

    void TriggerEvent() {
        if (doorToOpen != null) {
            // quick example: lift door up slightly
            doorToOpen.transform.position += Vector3.up * 1.6f;
        }
    }

    void UpdateModeUI(string s) {
        if (modeText != null) modeText.text = s;
    }
}
