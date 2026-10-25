using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using SoundSync.Models;

namespace SoundSync.Features
{
    public sealed record AudioEndpointRow(string Name, string Kind, string State, bool IsDefaultOutput, bool IsSelectedMirror);
    
    public sealed class AudioInventory
    {
        public DateTimeOffset ScannedAt { get; } = DateTimeOffset.UtcNow;
        public List<AudioEndpointRow> Endpoints { get; } = new();
        public List<string> Warnings { get; } = new();
        public int ActiveOutputs => Endpoints.Count(x => x.Kind == "Output" && x.State == "Active");
        public int ActiveInputs => Endpoints.Count(x => x.Kind == "Input" && x.State == "Active");
        public int SelectedOutputs => Endpoints.Count(x => x.Kind == "Output" && x.State == "Active" && x.IsSelectedMirror);

        public static AudioInventory Scan(IEnumerable<string> selectedMirrorIds)
        {
            var result = new AudioInventory();
            var selected = new HashSet<string>(selectedMirrorIds, StringComparer.OrdinalIgnoreCase);
            using var enumerator = new MMDeviceEnumerator();
            string? defaultId = null;
            try { defaultId = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID; }
            catch (Exception ex) { result.Warnings.Add("Default output unavailable: " + ex.Message); }
            foreach (var kind in new[] { (Flow: DataFlow.Render, Label: "Output"), (Flow: DataFlow.Capture, Label: "Input") })
            {
                try
                {
                    foreach (var endpoint in enumerator.EnumerateAudioEndPoints(kind.Flow, DeviceState.All))
                    {
                        try
                        {
                            var isDefault = kind.Label == "Output" && endpoint.ID == defaultId;
                            result.Endpoints.Add(new AudioEndpointRow(endpoint.FriendlyName,
                                kind.Label, endpoint.State.ToString(), isDefault,
                                kind.Label == "Output" && !isDefault && selected.Contains(endpoint.ID)));
                        }
                        catch (Exception ex) { result.Warnings.Add("Endpoint metadata unavailable: " + ex.Message); }
                    }
                }
                catch (Exception ex) { result.Warnings.Add("Cannot enumerate " + kind.Label + "s: " + ex.Message); }
            }
            return result;
        }
    }

    public sealed class SpeakerGroup
    {
        public string Name { get; set; } = "";
        public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
        public List<SavedDeviceSettings> Devices { get; set; } = new();
    }

    public sealed class SpeakerGroupStore
    {
        private readonly string _path;
        public SpeakerGroupStore(string? path = null)
        {
            _path = path ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VinuVinu", "speaker-groups.json");
        }
        public List<SpeakerGroup> Load()
        {
            if (!File.Exists(_path)) return new();
            try { return JsonSerializer.Deserialize<List<SpeakerGroup>>(File.ReadAllText(_path)) ?? new(); }
            catch (JsonException ex) { throw new InvalidDataException("Speaker group file is invalid; it was not overwritten.", ex); }
        }
        public void Upsert(string name, IEnumerable<SavedDeviceSettings> devices)
        {
            name = (name ?? "").Trim();
            if (name.Length < 1 || name.Length > 48) throw new ArgumentException("Group name must be 1-48 characters.", nameof(name));
            var groups = Load();
            groups.RemoveAll(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            groups.Add(new SpeakerGroup { Name = name, Devices = devices.ToList() });
            Save(groups.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList());
        }
        public void Delete(string name)
        {
            var groups = Load();
            groups.RemoveAll(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            Save(groups);
        }
        private void Save(List<SpeakerGroup> groups)
        {
            var directory = Path.GetDirectoryName(_path) ?? throw new InvalidOperationException("Invalid group path.");
            Directory.CreateDirectory(directory);
            var temp = _path + ".tmp";
            try
            {
                File.WriteAllText(temp, JsonSerializer.Serialize(groups, new JsonSerializerOptions { WriteIndented = true }));
                File.Move(temp, _path, true);
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
    }

    public sealed class AudioHubWindow : Window
    {
        private readonly Func<IReadOnlyList<SavedDeviceSettings>> _current;
        private readonly Func<IReadOnlyCollection<string>> _selectedIds;
        private readonly Func<bool> _connected;
        private readonly Func<IReadOnlyList<SavedDeviceSettings>, string> _apply;
        private readonly SpeakerGroupStore _store = new();
        private readonly DataGrid _endpoints = new() { AutoGenerateColumns = false, IsReadOnly = true, CanUserAddRows = false };
        private readonly ListBox _groups = new() { DisplayMemberPath = nameof(SpeakerGroup.Name) };
        private readonly TextBox _newName = new() { MinWidth = 210, Margin = new Thickness(0, 0, 8, 0) };
        private readonly TextBlock _summary = new() { TextWrapping = TextWrapping.Wrap };
        private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap };
        private AudioInventory? _inventory;

        public AudioHubWindow(Func<IReadOnlyList<SavedDeviceSettings>> current,
            Func<IReadOnlyCollection<string>> selectedIds,
            Func<bool> connected,
            Func<IReadOnlyList<SavedDeviceSettings>, string> apply)
        {
            _current = current;
            _selectedIds = selectedIds;
            _connected = connected;
            _apply = apply;
            Title = "Vinu Vinu | Audio Hub";
            Width = 890; Height = 640; MinWidth = 680; MinHeight = 450;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var layout = new DockPanel { Margin = new Thickness(16) };
            Content = layout;
            var heading = new TextBlock { Text = "AUDIO HUB", FontSize = 22, FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10) };
            DockPanel.SetDock(heading, Dock.Top);
            layout.Children.Add(heading);
            _status.Margin = new Thickness(0, 10, 0, 0);
            DockPanel.SetDock(_status, Dock.Bottom);
            layout.Children.Add(_status);
            var tabs = new TabControl();
            tabs.Items.Add(new TabItem { Header = "Audio endpoints", Content = BuildInventoryPage() });
            tabs.Items.Add(new TabItem { Header = "Speaker groups", Content = BuildGroupsPage() });
            layout.Children.Add(tabs);
            RefreshInventory();
            RefreshGroups();
        }

        private static Button ActionButton(string label, Action onClick)
        {
            var button = new Button { Content = label, Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(0, 0, 8, 4) };
            button.Click += (_, _) => onClick();
            return button;
        }

        private UIElement BuildInventoryPage()
        {
            var root = new DockPanel { Margin = new Thickness(12) };
            DockPanel.SetDock(_summary, Dock.Top);
            _summary.Margin = new Thickness(0, 0, 0, 12);
            root.Children.Add(_summary);
            var actions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            actions.Children.Add(ActionButton("Rescan devices", RefreshInventory));
            actions.Children.Add(ActionButton("Export diagnostics", Export));
            DockPanel.SetDock(actions, Dock.Top);
            root.Children.Add(actions);
            foreach (var col in new[] {
                ("Kind", nameof(AudioEndpointRow.Kind), 80.0),
                ("Windows audio endpoint", nameof(AudioEndpointRow.Name), 300.0),
                ("State", nameof(AudioEndpointRow.State), 95.0),
                ("Default", nameof(AudioEndpointRow.IsDefaultOutput), 85.0),
                ("Selected", nameof(AudioEndpointRow.IsSelectedMirror), 85.0)})
                _endpoints.Columns.Add(new DataGridTextColumn {
                    Header = col.Item1, Binding = new Binding(col.Item2), Width = new DataGridLength(col.Item3) });
            root.Children.Add(_endpoints);
            return root;
        }

        private UIElement BuildGroupsPage()
        {
            var root = new DockPanel { Margin = new Thickness(12) };
            var intro = new TextBlock { Text = "Save a named group of outputs, delay and equalizer settings. Disconnect before switching groups.",
                TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 12) };
            DockPanel.SetDock(intro, Dock.Top); root.Children.Add(intro);
            var commands = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
            commands.Children.Add(_newName);
            commands.Children.Add(ActionButton("Save group", SaveGroup));
            commands.Children.Add(ActionButton("Apply selected", ApplyGroup));
            commands.Children.Add(ActionButton("Delete selected", DeleteGroup));
            DockPanel.SetDock(commands, Dock.Top); root.Children.Add(commands);
            _groups.SelectionChanged += (_, _) => {
                if (_groups.SelectedItem is SpeakerGroup group) _newName.Text = group.Name;
            };
            root.Children.Add(_groups);
            return root;
        }

        private void RefreshInventory()
        {
            try
            {
                _inventory = AudioInventory.Scan(_selectedIds());
                _endpoints.ItemsSource = _inventory.Endpoints.OrderBy(x => x.Kind).ThenBy(x => x.Name).ToList();
                _summary.Text = $"Active outputs: {_inventory.ActiveOutputs}   |   Active inputs: {_inventory.ActiveInputs}   |   Selected mirrors: {_inventory.SelectedOutputs}\n" +
                    (_connected() ? "Routing is active." : "Routing is stopped.") +
                    " Windows must first expose Bluetooth devices as audio endpoints. A selection does not prove sound is audible.";
                _status.Text = _inventory.Warnings.Count > 0 ? string.Join("; ", _inventory.Warnings)
                    : $"Scanned at {_inventory.ScannedAt:HH:mm:ss} UTC.";
            }
            catch (Exception ex) { ShowError("Device scan failed: " + ex.Message); }
        }

        private void Export()
        {
            if (_inventory is null) return;
            var dialog = new SaveFileDialog { Filter = "JSON (*.json)|*.json", FileName = "vinu-vinu-devices.json" };
            if (dialog.ShowDialog(this) != true) return;
            try
            {
                // Never export endpoint IDs or browser-access bearer tokens.
                var report = new {
                    _inventory.ScannedAt, _inventory.ActiveOutputs, _inventory.ActiveInputs,
                    _inventory.SelectedOutputs, SessionActive = _connected(), _inventory.Warnings,
                    Devices = _inventory.Endpoints
                };
                File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
                _status.Text = "Diagnostics saved.";
            }
            catch (Exception ex) { ShowError("Export failed: " + ex.Message); }
        }

        private void RefreshGroups()
        {
            try { _groups.ItemsSource = _store.Load(); }
            catch (Exception ex) { ShowError("Reading groups failed: " + ex.Message); }
        }
        private void SaveGroup()
        {
            try
            {
                _store.Upsert(_newName.Text, _current());
                RefreshGroups();
                _status.Text = "Speaker group saved.";
            }
            catch (Exception ex) { ShowError("Saving group failed: " + ex.Message); }
        }
        private void ApplyGroup()
        {
            if (_connected()) { ShowError("Stop audio sharing before switching groups."); return; }
            if (_groups.SelectedItem is not SpeakerGroup group) { ShowError("Choose a group first."); return; }
            try { _status.Text = _apply(group.Devices); RefreshInventory(); }
            catch (Exception ex) { ShowError("Applying group failed: " + ex.Message); }
        }
        private void DeleteGroup()
        {
            if (_groups.SelectedItem is not SpeakerGroup group) { ShowError("Choose a group first."); return; }
            if (MessageBox.Show(this, "Delete group '" + group.Name + "'?", "Delete group",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            try { _store.Delete(group.Name); RefreshGroups(); _status.Text = "Group deleted."; }
            catch (Exception ex) { ShowError("Deleting group failed: " + ex.Message); }
        }
        private void ShowError(string message)
        {
            _status.Text = message;
            MessageBox.Show(this, message, "Vinu Vinu", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}

namespace SoundSync
{
    public partial class MainWindow
    {
        private void AudioHubButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SoundSync.Features.AudioHubWindow(
                () => allItems.Select(i => new SavedDeviceSettings {
                    DeviceId = i.Device.ID,
                    IsSelected = i.IsSelected && !i.IsDefaultDevice,
                    Delay = i.Delay, Bass = i.Bass, Mid = i.Mid, Treble = i.Treble,
                    AutoAlignDelay = i.AutoAlignDelay, SyncVolumeWithDefault = i.SyncVolumeWithDefault,
                    VolumeRatioToDefault = i.VolumeRatioToDefault }).ToList(),
                () => allItems.Where(i => i.IsSelected && !i.IsDefaultDevice).Select(i => i.Device.ID).ToArray(),
                () => isConnected,
                saved => {
                    if (isConnected) return "Stop audio sharing before applying a group.";
                    var byId = saved.GroupBy(x => x.DeviceId, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
                    int selected = 0, found = 0;
                    foreach (var item in allItems)
                    {
                        if (!byId.TryGetValue(item.Device.ID, out var config))
                        {
                            item.IsSelected = false;
                            continue;
                        }
                        found++;
                        item.IsSelected = config.IsSelected && !item.IsDefaultDevice;
                        if (item.IsSelected) selected++;
                        item.Delay = Math.Clamp(config.Delay, 0, 500);
                        item.Bass = config.Bass; item.Mid = config.Mid; item.Treble = config.Treble;
                        item.AutoAlignDelay = config.AutoAlignDelay;
                        item.SyncVolumeWithDefault = config.SyncVolumeWithDefault;
                        item.VolumeRatioToDefault = config.VolumeRatioToDefault <= 0 ? 1 : config.VolumeRatioToDefault;
                    }
                    SaveCurrentProfile();
                    ApplyDeviceFilter();
                    RefreshHeaderToggles();
                    return $"Group applied: {selected} selected mirrors, {found} matching audio endpoints. " +
                        $"{Math.Max(0, byId.Count - found)} missing endpoint(s).";
                }) { Owner = this };
            dialog.ShowDialog();
        }
    }
}
