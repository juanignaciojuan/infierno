using UnityEngine;
using System.Text;

/// <summary>
/// Lightweight on-screen debug overlay (no UI Canvas) using OnGUI for quick testing in editor & device.
/// Set watched components in inspector.
/// </summary>
public class XRDebugOverlay : MonoBehaviour
{
    [Tooltip("Show logs captured (recent)." )]
    public bool showRecentLogs = true;
    [Tooltip("Max number of log lines to display.")]
    public int maxLines = 8;
    [Tooltip("Font size.")]
    public int fontSize = 14;
    [Tooltip("Corner offset.")]
    public Vector2 offset = new Vector2(10, 10);
    [Tooltip("Screen corner (0=TL,1=TR,2=BL,3=BR).")]
    public int corner = 0;

    private static readonly System.Collections.Generic.Queue<string> _logQueue = new();

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }
    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (!showRecentLogs) return;
        string prefix = type switch { LogType.Warning => "[W] ", LogType.Error => "[E] ", LogType.Exception => "[EX] ", _ => "" };
        _logQueue.Enqueue(prefix + condition);
        while (_logQueue.Count > maxLines) _logQueue.Dequeue();
    }

    private void OnGUI()
    {
        if (!showRecentLogs) return;
        GUI.skin.label.fontSize = fontSize;
        StringBuilder sb = new StringBuilder();
        foreach (var line in _logQueue)
        {
            sb.AppendLine(line);
        }
        string text = sb.ToString();
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        float x = offset.x;
        float y = offset.y;
        switch (corner)
        {
            case 1: x = Screen.width - size.x - offset.x; break;
            case 2: y = Screen.height - size.y - offset.y; break;
            case 3: x = Screen.width - size.x - offset.x; y = Screen.height - size.y - offset.y; break;
        }
        GUI.Box(new Rect(x - 4, y - 4, size.x + 8, size.y + 8), "");
        GUI.Label(new Rect(x, y, size.x, size.y), text);
    }
}
