using UnityEditor;
using UnityEngine;
using System.IO;

public class AudioEditor : EditorWindow
{
    private AudioClip audioClip;
    private float startTrim = 0f;
    private float endTrim = 0f;
    private float fadeStartDuration = 0f;
    private float fadeEndDuration = 0f;
    private bool isAudioAdded = false;

    [MenuItem("Tools/Simblend/Audio Editor")]
    public static void ShowWindow()
    {
        GetWindow<AudioEditor>("Audio Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Trim and Fade Audio Clip", EditorStyles.boldLabel);

        audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", audioClip, typeof(AudioClip), false);

        if (audioClip != null)
        {
            startTrim = EditorGUILayout.Slider("Start Trim", startTrim, 0f, audioClip.length);
            endTrim = EditorGUILayout.Slider("End Trim", endTrim, 0f, audioClip.length);
            fadeStartDuration = EditorGUILayout.Slider("Fade Start Duration", fadeStartDuration, 0f, endTrim - startTrim);
            fadeEndDuration = EditorGUILayout.Slider("Fade End Duration", fadeEndDuration, 0f, endTrim - startTrim);

            if (!isAudioAdded)
            {
                endTrim = audioClip.length;
                isAudioAdded = true;
            }

            if (GUILayout.Button("Trim, Fade, and Save"))
            {
                TrimAndFadeAudioClip();
            }
        }
        else
        {
            isAudioAdded = false;
        }
    }

    private void TrimAndFadeAudioClip()
    {
        if (audioClip == null)
        {
            Debug.LogError("No audio clip selected for trimming.");
            return;
        }

        float length = endTrim - startTrim;
        if (length <= 0)
        {
            Debug.LogError("Invalid trim values. The end trim must be greater than the start trim.");
            return;
        }

        string path = Path.Combine("Assets", "Sounds", audioClip.name + "_EDITED.wav");

        // Ensure the directory exists
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Create trimmed and faded AudioClip
        AudioClip trimmedClip = TrimAndFadeClip(audioClip, startTrim, length, fadeStartDuration, fadeEndDuration);
        SaveAsWav(trimmedClip, path);

        Debug.Log($"Trimmed and faded audio clip saved to {path}");
        AssetDatabase.Refresh();
    }

    private AudioClip TrimAndFadeClip(AudioClip clip, float startTime, float length, float fadeStartDuration, float fadeEndDuration)
    {
        float[] data = new float[clip.samples * clip.channels];
        clip.GetData(data, 0);

        int startSample = Mathf.FloorToInt(startTime * clip.frequency * clip.channels);
        int endSample = Mathf.FloorToInt((startTime + length) * clip.frequency * clip.channels);

        float[] trimmedData = new float[endSample - startSample];
        System.Array.Copy(data, startSample, trimmedData, 0, trimmedData.Length);

        // Apply fade-in
        if (fadeStartDuration > 0)
        {
            int fadeStartSamples = Mathf.FloorToInt(fadeStartDuration * clip.frequency * clip.channels);
            for (int i = 0; i < fadeStartSamples && i < trimmedData.Length; i++)
            {
                float fadeFactor = (float)i / fadeStartSamples;
                trimmedData[i] *= fadeFactor;
            }
        }

        // Apply fade-out
        if (fadeEndDuration > 0)
        {
            int fadeEndSamples = Mathf.FloorToInt(fadeEndDuration * clip.frequency * clip.channels);
            for (int i = 0; i < fadeEndSamples && i < trimmedData.Length; i++)
            {
                float fadeFactor = (float)(fadeEndSamples - i) / fadeEndSamples;
                trimmedData[trimmedData.Length - 1 - i] *= fadeFactor;
            }
        }

        AudioClip newClip = AudioClip.Create(clip.name + "_EDITED", trimmedData.Length / clip.channels, clip.channels, clip.frequency, false);
        newClip.SetData(trimmedData, 0);

        return newClip;
    }

    private void SaveAsWav(AudioClip clip, string path)
    {
        if (clip == null)
        {
            Debug.LogError("Clip is null, cannot save as WAV.");
            return;
        }

        byte[] wavData = WavUtility.FromAudioClip(clip);
        File.WriteAllBytes(path, wavData);
    }
}
