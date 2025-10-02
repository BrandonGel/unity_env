using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Captures screenshots once per physics step (FixedUpdate) during play.
/// Saves screenshots to Application.persistentDataPath with frame numbering.
/// </summary>
[AddComponentMenu("Utilities/Fixed Step Recorder")]
public sealed class FixedStepRecorder : MonoBehaviour
{
	[Header("Screenshot Control")]
	[SerializeField] private bool startRecordingOnPlay = true;
	[SerializeField] private KeyCode toggleRecordingKey = KeyCode.R;

	[Header("Screenshot Settings")]
	[SerializeField] private int screenshotWidth = 1920;
	[SerializeField] private int screenshotHeight = 1080;
	[SerializeField] private bool useFullScreenResolution = false;

	[Header("Output")]
	[SerializeField] private string filePrefix = "screenshots";

	private bool isRecording;
	private string screenshotDirectory;
	private int frameCounter;
	private string savePath;
    private Texture2D screenshot;
    private int width;
    private int height;
    private RenderTexture renderTexture;

	private void Reset()
	{
		// No initialization needed for screenshot-only mode
	}

	private void Awake()
	{
		// No initialization needed for screenshot-only mode
        string assetsPath = Application.dataPath;
        string projectPath = Directory.GetParent(assetsPath).FullName;
        savePath = Path.Combine(projectPath, "data");
        width = useFullScreenResolution ? Screen.width : screenshotWidth;
        height = useFullScreenResolution ? Screen.height : screenshotHeight;
        screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
        // Create render texture
        renderTexture = new RenderTexture(width, height, 24);
	}

	private void OnEnable()
	{
		if (startRecordingOnPlay)
		{
			StartRecording();
		}
	}

	private void OnDisable()
	{
		StopRecording();
	}

	private void Update()
	{
		if (toggleRecordingKey != KeyCode.None && Input.GetKeyDown(toggleRecordingKey))
		{
			if (isRecording)
			{
				StopRecording();
			}
			else
			{
				StartRecording();
			}
		}
	}

	private void FixedUpdate()
	{
		if (!isRecording)
		{
			return;
		}

		// Capture screenshot
		CaptureScreenshot();
	}

	public void StartRecording()
	{


		frameCounter = 0;
		isRecording = true;
		screenshotDirectory = BuildScreenshotDirectory();
		
		// Create screenshot directory
		if (!Directory.Exists(screenshotDirectory))
		{
			Directory.CreateDirectory(screenshotDirectory);
		}
	}

	public void StopRecording()
	{
		if (!isRecording)
		{
			return;
		}

		isRecording = false;
	}

	private void CaptureScreenshot()
	{
		// Wait for end of frame to ensure all rendering is complete
		StartCoroutine(CaptureScreenshotCoroutine());
	}

	private IEnumerator CaptureScreenshotCoroutine()
	{
		// Wait for end of frame
		yield return new WaitForEndOfFrame();

		try
		{

			
			Camera mainCamera = Camera.main;
			
			if (mainCamera == null)
			{
				Debug.LogWarning("FixedStepRecorder: No main camera found for screenshot");
				yield break;
			}

			// Render camera to texture
			mainCamera.targetTexture = renderTexture;
			mainCamera.Render();

			// Read pixels from render texture
			RenderTexture.active = renderTexture;
			screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			screenshot.Apply();

			// Reset camera
			mainCamera.targetTexture = null;
			RenderTexture.active = null;

			// Save to file
			string filename = $"frame_{frameCounter:D6}.png";
			string filepath = Path.Combine(screenshotDirectory, filename);
			byte[] data = screenshot.EncodeToPNG();
            Debug.Log("FixedStepRecorder: Writing screenshot to " + filepath);
			File.WriteAllBytes(filepath, data);

			// Cleanup
			// DestroyImmediate(screenshot);
			// DestroyImmediate(renderTexture);

			frameCounter++;
		}
		catch (Exception ex)
		{
			Debug.LogError($"FixedStepRecorder failed to capture screenshot: {ex.Message}");
		}
	}


	private string BuildScreenshotDirectory()
	{
		var safePrefix = string.IsNullOrWhiteSpace(filePrefix) ? "screenshots" : filePrefix.Trim();
		var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
		var dirName = $"{safePrefix}_{name}_{timestamp}";
		return Path.Combine(savePath, dirName);
	}
}
