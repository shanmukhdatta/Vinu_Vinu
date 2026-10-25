using System.Drawing;
using System.Windows.Forms;
using SyncCast.Audio;
using SyncCast.Diagnostics;

namespace SyncCast.UI;

public sealed class MainForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(17, 22, 32);
    private static readonly Color PanelColor = Color.FromArgb(28, 35, 48);
    private static readonly Color FieldColor = Color.FromArgb(37, 45, 59);
    private static readonly Color TextColor = Color.FromArgb(239, 243, 248);
    private static readonly Color MutedColor = Color.FromArgb(157, 169, 186);
    private static readonly Color AccentColor = Color.FromArgb(255, 147, 60);

    private readonly ComboBox _source = new();
    private readonly DataGridView _grid = new();
    private readonly Button _start = new();
    private readonly Button _stop = new();
    private readonly Button _refresh = new();
    private readonly Label _status = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 600 };
    private readonly AudioEngine _engine = new();
    private List<AudioDeviceInfo> _devices = new();
    private bool _closing;

    public MainForm()
    {
        Text = "SyncCast | Multi-Device Audio Sharing";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1020, 710);
        MinimumSize = new Size(800, 640);
        BackColor = BackgroundColor;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 10f);
        BuildUi();
        _source.SelectedIndexChanged += (_, _) => PopulateGrid();
        _refresh.Click += (_, _) => RefreshDevices();
        _start.Click += (_, _) => StartAudio();
        _stop.Click += (_, _) => StopAudio();
        _engine.Faulted += OnEngineFault;
        _timer.Tick += (_, _) => RefreshTelemetry();
        _timer.Start();
        Shown += (_, _) => RefreshDevices();
        FormClosing += (_, _) =>
        {
            _closing = true;
            _timer.Stop();
            _engine.Dispose();
            _timer.Dispose();
        };
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(24, 22, 24, 18),
            BackColor = BackgroundColor
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 102));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        Controls.Add(root);

        var header = new Panel { Dock = DockStyle.Fill };
        header.Controls.Add(new Label
        {
            Text = "SYNCCAST",
            Font = new Font("Segoe UI", 23, FontStyle.Bold),
            ForeColor = AccentColor,
            AutoSize = true,
            Location = new Point(0, 0)
        });
        header.Controls.Add(new Label
        {
            Text = "One audio source. Your headphones. Together.",
            ForeColor = MutedColor,
            AutoSize = true,
            Location = new Point(2, 52)
        });
        root.Controls.Add(header, 0, 0);

        var sourceArea = new TableLayoutPanel
        {
            BackColor = PanelColor,
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(14, 12, 14, 12),
            Margin = new Padding(0, 0, 0, 14)
        };
        sourceArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        sourceArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16));
        sourceArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 166));
        sourceArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));
        sourceArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sourceArea.Controls.Add(MakeLabel("1  SOURCE AUDIO DEVICE (Windows playback endpoint)"), 0, 0);
        ConfigureCombo(_source);
        sourceArea.Controls.Add(_source, 0, 1);
        ConfigureButton(_refresh, "Refresh devices", FieldColor, TextColor);
        _refresh.Dock = DockStyle.Fill;
        sourceArea.Controls.Add(_refresh, 2, 1);
        root.Controls.Add(sourceArea, 0, 1);

        var destinationArea = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, 0, 14)
        };
        destinationArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        destinationArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        destinationArea.Controls.Add(MakeLabel("2  DESTINATIONS     ✓ Select two or more, set volume and optional delay"), 0, 0);
        ConfigureGrid();
        destinationArea.Controls.Add(_grid, 0, 1);
        root.Controls.Add(destinationArea, 0, 2);

        var actionArea = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Margin = new Padding(0, 0, 0, 8)
        };
        actionArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        actionArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        actionArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 47));
        actionArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        ConfigureButton(_start, "▶  Start sharing", AccentColor, BackgroundColor);
        ConfigureButton(_stop, "■  Stop", FieldColor, TextColor);
        _start.Dock = DockStyle.Fill;
        _stop.Dock = DockStyle.Fill;
        _start.Margin = new Padding(0, 0, 8, 0);
        _stop.Margin = new Padding(8, 0, 0, 0);
        _stop.Enabled = false;
        actionArea.Controls.Add(_start, 0, 0);
        actionArea.Controls.Add(_stop, 1, 0);
        actionArea.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = MutedColor,
            Text = "Tip: A virtual audio cable lets BOTH earbuds be destinations without audible laptop speakers. Delay changes need a restart.",
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 1);
        actionArea.SetColumnSpan(actionArea.GetControlFromPosition(0, 1)!, 2);
        root.Controls.Add(actionArea, 0, 3);

        _status.Text = "Ready. Connect your audio devices in Windows, then click Refresh devices.";
        _status.ForeColor = MutedColor;
        _status.BackColor = PanelColor;
        _status.Dock = DockStyle.Fill;
        _status.TextAlign = ContentAlignment.MiddleLeft;
        _status.AutoEllipsis = true;
        _status.Padding = new Padding(12, 0, 12, 0);
        root.Controls.Add(_status, 0, 4);
    }

    private static Label MakeLabel(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        ForeColor = TextColor,
        Font = new Font("Segoe UI", 10f, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static void ConfigureCombo(ComboBox box)
    {
        box.Dock = DockStyle.Fill;
        box.DropDownStyle = ComboBoxStyle.DropDownList;
        box.BackColor = FieldColor;
        box.ForeColor = TextColor;
        box.FlatStyle = FlatStyle.Flat;
        box.Font = new Font("Segoe UI", 10f);
    }

    private static void ConfigureButton(Button button, string text, Color back, Color foreground)
    {
        button.Text = text;
        button.BackColor = back;
        button.ForeColor = foreground;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.BorderStyle = BorderStyle.None;
        _grid.BackgroundColor = PanelColor;
        _grid.GridColor = FieldColor;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = FieldColor;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        _grid.ColumnHeadersHeight = 42;
        _grid.RowTemplate.Height = 45;
        _grid.DefaultCellStyle.BackColor = PanelColor;
        _grid.DefaultCellStyle.ForeColor = TextColor;
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(54, 65, 82);
        _grid.DefaultCellStyle.SelectionForeColor = TextColor;
        _grid.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "Enabled", HeaderText = "Use", FillWeight = 11
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Device", HeaderText = "Headphones / speakers", ReadOnly = true, FillWeight = 54
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Volume", HeaderText = "Volume %", FillWeight = 15
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Delay", HeaderText = "Extra delay (ms)", FillWeight = 21
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Buffer", HeaderText = "Queue", FillWeight = 13, ReadOnly = true
        });
        _grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_grid.IsCurrentCellDirty)
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        _grid.DataError += (_, e) =>
        {
            e.ThrowException = false;
            ShowStatus("Invalid cell value. Use numbers for volume and delay.", true);
        };
    }

    private void RefreshDevices()
    {
        if (_engine.IsRunning) return;
        try
        {
            string? previous = (_source.SelectedItem as AudioDeviceInfo)?.Id;
            _devices = AudioEngine.GetActiveRenderDevices();
            _source.BeginUpdate();
            _source.Items.Clear();
            foreach (var device in _devices) _source.Items.Add(device);
            _source.EndUpdate();
            if (_source.Items.Count > 0)
            {
                int index = _devices.FindIndex(d => d.Id == previous);
                _source.SelectedIndex = index >= 0 ? index : 0;
            }
            else _grid.Rows.Clear();
            ShowStatus($"Found {_devices.Count} active Windows playback device(s).");
            AppLog.Write($"Refreshed playback endpoints: {_devices.Count}");
        }
        catch (Exception exception)
        {
            AppLog.Write("Could not list audio devices", exception);
            ShowStatus($"Could not list audio devices: {exception.Message}", true);
        }
    }

    private void PopulateGrid()
    {
        if (_engine.IsRunning) return;
        string? selected = (_source.SelectedItem as AudioDeviceInfo)?.Id;
        _grid.Rows.Clear();
        foreach (var device in _devices.Where(d => d.Id != selected))
        {
            int i = _grid.Rows.Add(false, device.Name, 100, 0, "—");
            _grid.Rows[i].Tag = device.Id;
        }
    }

    private void StartAudio()
    {
        _grid.EndEdit();
        if (_source.SelectedItem is not AudioDeviceInfo source)
        {
            ShowStatus("Select a source device first.", true);
            return;
        }
        var targets = new List<OutputSettings>();
        foreach (DataGridViewRow row in _grid.Rows)
        {
            bool enabled = row.Cells["Enabled"].Value is true;
            if (!enabled) continue;
            if (!int.TryParse(row.Cells["Volume"].Value?.ToString(), out int volume) ||
                !int.TryParse(row.Cells["Delay"].Value?.ToString(), out int delay))
            {
                ShowStatus("Volume and delay must be whole numbers.", true);
                return;
            }
            if (volume is < 0 or > 100 || delay is < 0 or > 1000)
            {
                ShowStatus("Volume range: 0–100%. Delay range: 0–1000 ms.", true);
                return;
            }
            targets.Add(new OutputSettings((string)row.Tag!, volume, delay));
        }
        if (targets.Count == 0)
        {
            ShowStatus("Select at least one destination using the Use checkbox.", true);
            return;
        }
        try
        {
            AppLog.Write($"Start requested: source={source.Name}; destinations={targets.Count}");
            _engine.Start(source.Id, targets);
            _source.Enabled = false;
            _grid.ReadOnly = true;
            _refresh.Enabled = false;
            _start.Enabled = false;
            _stop.Enabled = true;
            ShowStatus($"Sharing from '{source.Name}' to {targets.Count} device(s).");
            AppLog.Write("Audio sharing started");
        }
        catch (Exception exception)
        {
            AppLog.Write("Could not start audio", exception);
            ShowStatus($"Could not start audio: {exception.Message}", true);
        }
    }

    private void StopAudio()
    {
        try { _engine.Stop(); }
        catch (Exception exception) { ShowStatus($"Stop warning: {exception.Message}", true); }
        _source.Enabled = true;
        _grid.ReadOnly = false;
        _refresh.Enabled = true;
        _start.Enabled = true;
        _stop.Enabled = false;
        foreach (DataGridViewRow row in _grid.Rows) row.Cells["Buffer"].Value = "—";
        ShowStatus("Stopped. You can edit destination settings and start again.");
        AppLog.Write("Audio sharing stopped");
    }

    private void OnEngineFault(string message)
    {
        AppLog.Write("Audio engine fault: " + message);
        if (_closing || !IsHandleCreated) return;
        try
        {
            BeginInvoke((Action)(() =>
            {
                if (_closing) return;
                StopAudio();
                ShowStatus(message, true);
            }));
        }
        catch (InvalidOperationException) { /* window closed during device event */ }
    }

    private void RefreshTelemetry()
    {
        if (!_engine.IsRunning) return;
        var statuses = _engine.GetStatuses().ToDictionary(s => s.DeviceId);
        bool disconnected = false;
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.Tag is string id && statuses.TryGetValue(id, out var status))
            {
                row.Cells["Buffer"].Value = status.IsPlaying
                    ? $"{status.BufferedMilliseconds} ms · {status.ResyncCount} reset(s)" : "Stopped";
                if (!status.IsPlaying) disconnected = true;
            }
        }
        if (disconnected) ShowStatus("An output has stopped. Disconnect/reconnect in Windows, then restart sharing.", true);
    }

    private void ShowStatus(string message, bool isError = false)
    {
        _status.Text = message;
        _status.ForeColor = isError ? Color.FromArgb(255, 190, 142) : MutedColor;
    }
}
