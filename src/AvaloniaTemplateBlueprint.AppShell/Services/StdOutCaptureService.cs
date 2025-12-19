using System.Text;

namespace AvaloniaTemplateBlueprint.AppShell.Services;

/// <summary>
/// Event arguments for captured output.
/// </summary>
public class OutputCapturedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the captured text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets whether this is from stderr.
    /// </summary>
    public bool IsError { get; }

    public OutputCapturedEventArgs(string text, bool isError)
    {
        Text = text;
        IsError = isError;
    }
}

/// <summary>
/// Service to capture stdout/stderr output and route to debug console.
/// This is OPT-IN - not enabled by default.
/// </summary>
/// <remarks>
/// WARNING: Capturing stdout/stderr can expose sensitive information.
/// Only enable this during development/debugging.
/// </remarks>
public class StdOutCaptureService : IDisposable
{
    private TextWriter? _originalStdOut;
    private TextWriter? _originalStdErr;
    private CaptureWriter? _captureStdOut;
    private CaptureWriter? _captureStdErr;
    private bool _isCapturing;

    /// <summary>
    /// Raised when output is captured.
    /// </summary>
    public event EventHandler<OutputCapturedEventArgs>? OutputCaptured;

    /// <summary>
    /// Gets whether capture is currently active.
    /// </summary>
    public bool IsCapturing => _isCapturing;

    /// <summary>
    /// Starts capturing stdout and stderr.
    /// </summary>
    public void StartCapture()
    {
        if (_isCapturing)
            return;

        _originalStdOut = Console.Out;
        _originalStdErr = Console.Error;

        _captureStdOut = new CaptureWriter(_originalStdOut, text => OnOutputCaptured(text, false));
        _captureStdErr = new CaptureWriter(_originalStdErr, text => OnOutputCaptured(text, true));

        Console.SetOut(_captureStdOut);
        Console.SetError(_captureStdErr);

        _isCapturing = true;
    }

    /// <summary>
    /// Stops capturing and restores original console.
    /// </summary>
    public void StopCapture()
    {
        if (!_isCapturing)
            return;

        if (_originalStdOut != null)
            Console.SetOut(_originalStdOut);

        if (_originalStdErr != null)
            Console.SetError(_originalStdErr);

        _captureStdOut?.Dispose();
        _captureStdErr?.Dispose();

        _captureStdOut = null;
        _captureStdErr = null;
        _isCapturing = false;
    }

    private void OnOutputCaptured(string text, bool isError)
    {
        OutputCaptured?.Invoke(this, new OutputCapturedEventArgs(text, isError));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        StopCapture();
    }

    /// <summary>
    /// TextWriter that captures output while also writing to original.
    /// </summary>
    private class CaptureWriter : TextWriter
    {
        private readonly TextWriter _original;
        private readonly Action<string> _onWrite;
        private readonly StringBuilder _lineBuffer = new();

        public CaptureWriter(TextWriter original, Action<string> onWrite)
        {
            _original = original;
            _onWrite = onWrite;
        }

        public override Encoding Encoding => _original.Encoding;

        public override void Write(char value)
        {
            _original.Write(value);

            if (value == '\n')
            {
                var line = _lineBuffer.ToString();
                _lineBuffer.Clear();
                if (!string.IsNullOrEmpty(line))
                {
                    _onWrite(line);
                }
            }
            else if (value != '\r')
            {
                _lineBuffer.Append(value);
            }
        }

        public override void Write(string? value)
        {
            if (value == null)
                return;

            _original.Write(value);

            foreach (var c in value)
            {
                if (c == '\n')
                {
                    var line = _lineBuffer.ToString();
                    _lineBuffer.Clear();
                    if (!string.IsNullOrEmpty(line))
                    {
                        _onWrite(line);
                    }
                }
                else if (c != '\r')
                {
                    _lineBuffer.Append(c);
                }
            }
        }

        public override void WriteLine(string? value)
        {
            _original.WriteLine(value);
            _onWrite(value ?? string.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _lineBuffer.Length > 0)
            {
                _onWrite(_lineBuffer.ToString());
                _lineBuffer.Clear();
            }
            base.Dispose(disposing);
        }
    }
}
