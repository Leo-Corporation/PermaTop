/*
MIT License

Copyright (c) Léo Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. 
*/
using Microsoft.Win32;
using PermaTop.Enums;
using PermaTop.Pages;
using PeyrSharp.Enums;
using PeyrSharp.Env;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace PermaTop.Classes;
public static class Global
{
	public static PinWindowsPage? PinWindowsPage { get; set; }
	public static FavoritePage? FavoritePage { get; set; }
	public static SettingsPage? SettingsPage { get; set; }

	public static List<Favorite> Favorites { get; set; }
	internal static string SettingsPath => $@"{FileSys.AppDataPath}\Léo Corporation\PermaTop\Settings.xml";
	public static Settings Settings { get; set; } = XmlSerializerManager.LoadFromXml<Settings>(SettingsPath) ?? new();

	public static string Version => "1.1.0.2308";
	public static string LastVersionLink => "https://raw.githubusercontent.com/Leo-Corporation/LeoCorp-Docs/master/Liens/Update%20System/PermaTop/Version.txt";

	public static string GetHiSentence
	{
		get
		{
			if (DateTime.Now.Hour >= 21 && DateTime.Now.Hour <= 7) // If between 9PM & 7AM
			{
				return Properties.Resources.GoodNight + ", " + Environment.UserName + "."; // Return the correct value
			}
			else if (DateTime.Now.Hour >= 7 && DateTime.Now.Hour <= 12) // If between 7AM - 12PM
			{
				return Properties.Resources.Hi + ", " + Environment.UserName + "."; // Return the correct value
			}
			else if (DateTime.Now.Hour >= 12 && DateTime.Now.Hour <= 17) // If between 12PM - 5PM
			{
				return Properties.Resources.GoodAfternoon + ", " + Environment.UserName + "."; // Return the correct value
			}
			else if (DateTime.Now.Hour >= 17 && DateTime.Now.Hour <= 21) // If between 5PM - 9PM
			{
				return Properties.Resources.GoodEvening + ", " + Environment.UserName + "."; // Return the correct value
			}
			else
			{
				return Properties.Resources.Hi + ", " + Environment.UserName + "."; // Return the correct value
			}
		}
	}

	internal const int GWL_EXSTYLE = -20;
	internal const int WS_EX_TOPMOST = 0x0008;

	internal const int HWND_TOPMOST = -1;
	internal const int HWND_NOTOPMOST = -2;

	internal const uint SWP_NOMOVE = 0x0002;
	internal const uint SWP_NOSIZE = 0x0001;

	internal const int GWL_STYLE = -16;
	internal const int WS_VISIBLE = 0x10000000;

	internal const int SW_SHOWNORMAL = 1;
	internal const int SW_SHOWMINIMIZED = 2;
	internal const int SW_SHOWMAXIMIZED = 3;

	internal enum WindowPlacementFlags : int
	{
		WPF_SETMINPOSITION = 0x0001,
		WPF_RESTORETOMAXIMIZED = 0x0002,
		WPF_ASYNCWINDOWPLACEMENT = 0x0004
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct WINDOWPLACEMENT
	{
		public int length;
		public int flags;
		public int showCmd;
		public System.Drawing.Point ptMinPosition;
		public System.Drawing.Point ptMaxPosition;
		public System.Drawing.Rectangle rcNormalPosition;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
	
	internal static Rect GetWindowPosition(IntPtr windowHandle)
	{
		GetWindowRect(windowHandle, out RECT windowRect);
		return new Rect(windowRect.Left, windowRect.Top, windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top);
	}

	[DllImport("user32.dll")]
	internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

	internal static bool IsWindowMaximized(IntPtr windowHandle)
	{
		WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
		placement.length = Marshal.SizeOf(placement);
		GetWindowPlacement(windowHandle, ref placement);

		return placement.showCmd == SW_SHOWMAXIMIZED;
	}

	internal static SolidColorBrush GetSolidColor(string resource) => (SolidColorBrush)Application.Current.Resources[resource];

	// Import the SetWindowPos function from user32.dll
	[DllImport("user32.dll")]
	static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	// Import the FindWindow function from user32.dll
	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	// Import the GetClassName function from user32.dll
	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	public static List<WindowInfo> GetWindows()
	{
		List<WindowInfo> windowInfos = new();
		EnumWindows((hWnd, lParam) =>
		{
			if (IsWindowVisible(hWnd) && !IsWindowUwp(hWnd))
			{
				StringBuilder titleBuilder = new StringBuilder(256);
				GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
				string windowTitle = titleBuilder.ToString();

				StringBuilder classBuilder = new StringBuilder(256);
				GetClassName(hWnd, classBuilder, classBuilder.Capacity);
				string className = classBuilder.ToString();

				string? file = null;
				try
				{
					GetWindowThreadProcessId(hWnd, out uint processId);
					Process process = Process.GetProcessById((int)processId);
					file = process.MainModule.FileName;
				}
				catch { }

				if (!string.IsNullOrEmpty(windowTitle))
				{
					windowInfos.Add(new(windowTitle, className, IsWindowPinned(hWnd), hWnd, file));
				}
			}

			return true;
		}, IntPtr.Zero);
		return windowInfos;
	}

	[DllImport("user32.dll")]
	static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
	MonitorEnumProc lpfnEnum, IntPtr dwData);

	[DllImport("user32.dll")]
	static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

	delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor,
		RECT lprcMonitor, IntPtr dwData);

	[StructLayout(LayoutKind.Sequential)]
	struct MonitorInfo
	{
		public uint Size;
		public RECT Monitor;
		public RECT WorkArea;
		public uint Flags;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string DeviceName;
	}

	public static List<ScreenInfo> GetScreenInfos()
	{
		var screenInfos = new List<ScreenInfo>();
		EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
			(hMonitor, hdcMonitor, lprcMonitor, dwData) =>
			{
				var mi = new MonitorInfo();
				mi.Size = (uint)Marshal.SizeOf(mi);
				if (GetMonitorInfo(hMonitor, ref mi))
				{
					screenInfos.Add(new ScreenInfo(mi.DeviceName, lprcMonitor));
				}
				return true; // Continue enumeration
			}, IntPtr.Zero);
		return screenInfos;
	}

	public static void SetWindowTopMost(IntPtr hWnd, bool topMost)
	{
		if (hWnd == IntPtr.Zero)
		{
			Console.WriteLine("Window not found!");
			return;
		}

		SetWindowPos(hWnd, topMost ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
	}

	public static bool IsWindowPinned(IntPtr hWnd)
	{
		if (hWnd == IntPtr.Zero)
		{
			Console.WriteLine("Window not found!");
			return false;
		}

		int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);

		return (exStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST;
	}

	public static bool IsWindowUwp(IntPtr hWnd)
	{
		int style = GetWindowLong(hWnd, GWL_STYLE);
		if ((style & WS_VISIBLE) == 0) // Exclude invisible windows
			return false;

		uint processId;
		GetWindowThreadProcessId(hWnd, out processId);
		Process process = Process.GetProcessById((int)processId);

		// UWP apps typically run under the "ApplicationFrameHost" process name
		return process.ProcessName.Equals("ApplicationFrameHost", StringComparison.OrdinalIgnoreCase);
	}

	public static void ChangeTheme()
	{
		App.Current.Resources.MergedDictionaries.Clear();
		ResourceDictionary resourceDictionary = new(); // Create a resource dictionary

		bool isDark = Settings.Theme == Themes.Dark;
		if (Settings.Theme == Themes.System)
		{
			isDark = IsSystemThemeDark(); // Set
		}

		if (isDark) // If the dark theme is on
		{
			resourceDictionary.Source = new Uri("..\\Themes\\Dark.xaml", UriKind.Relative); // Add source
		}
		else
		{
			resourceDictionary.Source = new Uri("..\\Themes\\Light.xaml", UriKind.Relative); // Add source
		}

		App.Current.Resources.MergedDictionaries.Add(resourceDictionary); // Add the dictionary
	}

	public static bool IsSystemThemeDark()
	{
		if (Sys.CurrentWindowsVersion != WindowsVersion.Windows10 && Sys.CurrentWindowsVersion != WindowsVersion.Windows11)
		{
			return false; // Avoid errors on older OSs
		}

		var t = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", "1");
		return t switch
		{
			0 => true,
			1 => false,
			_ => false
		}; // Return
	}

	public static void ChangeLanguage()
	{
		switch (Settings.Language) // For each case
		{
			case Languages.Default: // No language
				break;
			case Languages.en_US: // English (US)
				Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US"); // Change
				break;

			case Languages.fr_FR: // French (FR)
				Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr-FR"); // Change
				break;
			default: // No language
				break;
		}
	}
}
