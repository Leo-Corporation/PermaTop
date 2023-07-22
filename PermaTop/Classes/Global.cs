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
using PermaTop.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PermaTop.Classes;
public static class Global
{
	public static PinWindowsPage? PinWindowsPage { get; set; }

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

	public static SolidColorBrush GetSolidColor(string resource) => (SolidColorBrush)Application.Current.Resources[resource];

	// Import the SetWindowPos function from user32.dll
	[DllImport("user32.dll")]
	static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	// Import the FindWindow function from user32.dll
	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

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

	public static List<WindowInfo> GetWindows()
	{
		List<WindowInfo> windowInfos = new();
		EnumWindows((hWnd, lParam) =>
		{
			if (IsWindowVisible(hWnd))
			{
				StringBuilder titleBuilder = new StringBuilder(256);
				GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);
				string windowTitle = titleBuilder.ToString();

				StringBuilder classBuilder = new StringBuilder(256);
				GetClassName(hWnd, classBuilder, classBuilder.Capacity);
				string className = classBuilder.ToString();

				if (!string.IsNullOrEmpty(windowTitle))
				{
					windowInfos.Add(new(windowTitle, className, false));
				}
			}

			return true;
		}, IntPtr.Zero);
		return windowInfos;
	}
}
