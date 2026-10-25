using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace SoundSync.Tests;

/// <summary>
/// Checks the design contract without requiring desktop audio hardware. Compiling the
/// WPF project in CI additionally validates actual XAML and code-behind references.
/// </summary>
public class SkyThemeContractTests
{
    private static string Markup => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "MainWindow.xaml"));

    [Fact]
    public void DesktopControls_AreStillWiredAfterThemeRedesign()
    {
        var xml = XDocument.Parse(Markup);
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var keys = new[] {
            "AudioHubButton", "RefreshButton", "StartupButton", "IndependentVolumesButton",
            "RemoteControlButton", "ShowHiddenButton", "ThemeButton", "ThemeButtonText",
            "ThemeIconPath", "DeviceListBox", "LinkUrlBox", "ConnectButton", "StatusText"
        };
        var named = xml.Descendants()
            .Select(el => (string?)el.Attribute(x + "Name"))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray();
        foreach (var name in keys) Assert.Equal(1, named.Count(item => item == name));
        Assert.Contains("Click=\"ConnectButton_Click\"", Markup);
        Assert.Contains("Click=\"AudioHubButton_Click\"", Markup);
        Assert.Contains("Click=\"RefreshButton_Click\"", Markup);
    }

    [Fact]
    public void DaySky_UsesFriendlyReadablePalette()
    {
        var xml = XDocument.Parse(Markup);
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var brushes = xml.Descendants()
            .Where(node => node.Name.LocalName == "SolidColorBrush")
            .Where(node => node.Attribute(x + "Key") != null)
            .ToDictionary(node => (string)node.Attribute(x + "Key")!,
                          node => (string)node.Attribute("Color")!);
        foreach (var key in new[] {
            "HeroBrush", "HeroTextBrush", "ButtonTextBrush", "PanelBackgroundBrush",
            "ChannelCardBgBrush", "TextForegroundBrush", "TextSecondaryBrush",
            "TextMutedBrush", "AccentBlueBrush", "ConnectButtonBgBrush",
            "ConnectButtonConnectedBgBrush", "BadgeBgBrush"
        }) Assert.True(brushes.ContainsKey(key), "Missing sky-theme token " + key);

        // Normal text on a white card should exceed WCAG AA contrast (4.5:1).
        Assert.True(Contrast(brushes["TextForegroundBrush"], brushes["ChannelCardBgBrush"]) >= 4.5);
        Assert.True(Contrast(brushes["ButtonTextBrush"], brushes["ConnectButtonBgBrush"]) >= 4.5);
        Assert.Contains("Your sound, your friends, all together!", Markup);
        Assert.DoesNotContain("RED DEAD EDITION", Markup);
    }

    [Fact]
    public void ThemeHasOnePrimaryActionAndBothThemes()
    {
        Assert.Contains("START AUDIO SHARING", Markup);
        Assert.Contains("NIGHT SKY", Markup);
        Assert.Contains("CornerRadius=\"22\"", Markup);
        Assert.Contains("HeroBrush", Markup);
    }

    private static double Contrast(string fg, string bg)
    {
        double L(string hex)
        {
            double Channel(int offset)
            {
                int value = Convert.ToInt32(hex.Substring(offset, 2), 16);
                double normalized = value / 255.0;
                return normalized <= 0.04045
                    ? normalized / 12.92
                    : Math.Pow((normalized + 0.055) / 1.055, 2.4);
            }
            return Channel(1) * .2126 + Channel(3) * .7152 + Channel(5) * .0722;
        }
        var a = L(fg); var b = L(bg);
        return (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
    }
}
