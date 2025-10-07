using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Captures screenshots once per physics step (FixedUpdate) during play.
/// Saves screenshots to Application.persistentDataPath with frame numbering.
/// </summary>
[AddComponentMenu("Utilities/Screen Recorder")]
public sealed class ScreenRecorder : MonoBehaviour
{
	[Header("Screenshot Control")]
	[SerializeField] private bool startRecordingOnPlay = false;
	[SerializeField] private KeyCode toggleRecordingKey = KeyCode.R;

	[Header("Screenshot Settings")]
	[SerializeField] private int screenshotWidth = 1920;
	[SerializeField] private int screenshotHeight = 1080;
	[SerializeField] private bool useFullScreenResolution = false;

	[Header("Output")]
	[SerializeField] private string filePrefix = "screenshots";
	[SerializeField] private string recordingDir = "Recordings";

	private bool isRecording;
	private string screenshotDirectory;
	private int frameCounter;
	private string savePath;
	private Texture2D screenshot;
	private int width;
	private int height;
	private int actionFrameRate = 1;
	private List<byte[]> screenshotData = new List<byte[]>();
	private List<string[]> filenames = new List<string[]>();
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
		screenshotDirectory = Path.Combine(savePath, recordingDir);
		width = useFullScreenResolution ? Screen.width : screenshotWidth;
		height = useFullScreenResolution ? Screen.height : screenshotHeight;
		screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
		// Create render texture
		renderTexture = new RenderTexture(width, height, 24);
	}
	private void Start()
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

	public void setParams(bool startRecordingOnPlay = false, int screenshotWidth = 1920, int screenshotHeight = 1080, string recordingDir = "Recordings", int actionFrameRate = 1, bool useFullScreenResolution = false)
	{
		this.startRecordingOnPlay = startRecordingOnPlay;
		this.actionFrameRate = actionFrameRate;
		this.screenshotWidth = screenshotWidth;
		this.screenshotHeight = screenshotHeight;
		this.recordingDir = recordingDir;
		this.useFullScreenResolution = useFullScreenResolution;
		width = useFullScreenResolution ? Screen.width : screenshotWidth;
		height = useFullScreenResolution ? Screen.height : screenshotHeight;
		screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
		renderTexture = new RenderTexture(width, height, 24);
		string assetsPath = Application.dataPath;
		string projectPath = Directory.GetParent(assetsPath).FullName;
		savePath = Path.Combine(projectPath, "data");
		screenshotDirectory = Path.Combine(savePath, recordingDir);
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
			if (frameCounter % actionFrameRate > 0)
			{
				frameCounter++;
				yield break;
			}

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
			// Debug.Log("FixedStepRecorder: Writing screenshot to " + filepath);

			filenames.Add(new string[] { filepath });
			screenshotData.Add(data);

			File.WriteAllBytes(filepath, data);

			frameCounter++;
		}
		catch (Exception ex)
		{
			Debug.LogError($"FixedStepRecorder failed to capture screenshot: {ex.Message}");
		}
	}

	public void BuildScreenshotDirectory(string savePath)
	{
		this.savePath = savePath;
		screenshotDirectory = Path.Combine(savePath, recordingDir);
		if (!Directory.Exists(screenshotDirectory))
		{
			Directory.CreateDirectory(screenshotDirectory);
		}
	}
	
	public void resetCounter()
	{
		frameCounter = 0;
	}
}
