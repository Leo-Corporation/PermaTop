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
using PermaTop.Classes;
using PeyrSharp.Env;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace PermaTop.UserControls;
/// <summary>
/// Interaction logic for WindowPropertyItem.xaml
/// </summary>
public partial class WindowPropertyItem : UserControl
{
	WindowInfo WindowInfo { get; set; }
	StackPanel ParentElement { get; init; }
	public WindowPropertyItem(WindowInfo windowInfo, StackPanel parent)
	{
		InitializeComponent();
		WindowInfo = windowInfo;
		ParentElement = parent;

		InitUI();
	}

	internal void MoveWindow(IntPtr windowHandle, int x, int y)
	{
		Global.SetWindowPos(windowHandle, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
	}

	private void InitUI()
	{
		TitleTxt.Text = WindowInfo.Title.Length > 50 ? WindowInfo.Title[0..50] + "..." : WindowInfo.Title;

		Global.GetWindowRect(WindowInfo.Hwnd, out Global.RECT bounds);

		TitleToolTip.Content = $"{WindowInfo.Title}\n{bounds.Right - bounds.Left}x{bounds.Bottom - bounds.Top}";

		PinBtn.Content = WindowInfo.IsPinned ? "\uF604" : "\uF602";

		Favorite item = new(WindowInfo.ClassName, WindowInfo.Title, WindowInfo.ProcessName);
		FavBtn.Content = Global.Favorites.Contains(item) ? "\uF71B" : "\uF710";
		MaxRestoreBtn.Content = Global.IsWindowMaximized(WindowInfo.Hwnd) ? "\uF670" : "\uFA40";
		MaxRestoreBtn.FontSize = Global.IsWindowMaximized(WindowInfo.Hwnd) ? 18 : 14;

		IntPtr iconHandle = GetWindowIconHandle(WindowInfo.Hwnd, true); // Set 'true' for large icon, 'false' for small icon
		BitmapSource iconSource = GetIconFromHandle(iconHandle);
		if (iconSource != null)
		{
			// Set the Image control's Source property to display the icon
			IconImg.Source = iconSource;
		}
		Rect windowPosition = Global.GetWindowPosition(WindowInfo.Hwnd);
		XTxt.Text = windowPosition.Left.ToString();
		YTxt.Text = windowPosition.Top.ToString();
	}

	internal IntPtr GetWindowIconHandle(IntPtr windowHandle, bool largeIcon)
	{
		int index = largeIcon ? ICON_BIG : ICON_SMALL;
		return (IntPtr)Global.SendMessage(windowHandle, WM_GETICON, index, 0);
	}

	internal BitmapSource GetIconFromHandle(IntPtr iconHandle)
	{
		if (iconHandle == IntPtr.Zero)
			return null;

		return Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
	}

	private void PinBtn_Click(object sender, RoutedEventArgs e)
	{
		WindowInfo.IsPinned = !WindowInfo.IsPinned;
		PinBtn.Content = WindowInfo.IsPinned ? "\uF604" : "\uF602";
		Global.SetWindowTopMost(WindowInfo.Hwnd, WindowInfo.IsPinned);
	}

	private void FavBtn_Click(object sender, RoutedEventArgs e)
	{
		Favorite item = new(WindowInfo.ClassName, WindowInfo.Title, WindowInfo.ProcessName);
		if (Global.Favorites.Contains(item))
		{
			Global.Favorites.RemoveAt(Global.Favorites.IndexOf(item));
			FavBtn.Content = "\uF710";
		}
		else
		{
			Global.Favorites.Add(item);
			FavBtn.Content = "\uF71B";
		}
		XmlSerializerManager.SaveToXml(Global.Favorites, $@"{FileSys.AppDataPath}\Léo Corporation\PermaTop\Favs.xml");
	}

	private const int WM_SYSCOMMAND = 0x0112;
	private const int SC_CLOSE = 0xF060;
	private const int SC_MAXIMIZE = 0xF030;
	private const int SC_RESTORE = 0xF120;
	private const int SC_MINIMIZE = 0xF020;
	private const int GCL_HICON = -14;
	private const int ICON_SMALL = 0;
	private const int ICON_BIG = 1;
	private const uint WM_GETICON = 0x007F;
	private const uint SWP_NOSIZE = 0x0001;
	private const uint SWP_NOZORDER = 0x0004;

	private void CloseBtn_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			Global.SendMessage(WindowInfo.Hwnd, WM_SYSCOMMAND, (IntPtr)SC_CLOSE, IntPtr.Zero);
			ParentElement.Children.Remove(this);
		}
		catch { }
	}

	private void MaxRestoreBtn_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (!Global.IsWindowMaximized(WindowInfo.Hwnd))
			{
				Global.SendMessage(WindowInfo.Hwnd, WM_SYSCOMMAND, (IntPtr)SC_MAXIMIZE, IntPtr.Zero);
				MaxRestoreBtn.Content = "\uF670";
				MaxRestoreBtn.FontSize = 18;
				return;
			}
			Global.SendMessage(WindowInfo.Hwnd, WM_SYSCOMMAND, (IntPtr)SC_RESTORE, IntPtr.Zero);
			MaxRestoreBtn.Content = "\uFA40";
			MaxRestoreBtn.FontSize = 14;
		}
		catch { }
	}

	private void MinBtn_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			Global.SendMessage(WindowInfo.Hwnd, WM_SYSCOMMAND, (IntPtr)SC_MINIMIZE, IntPtr.Zero);
		}
		catch { }
	}

	private void MoreBtn_Click(object sender, RoutedEventArgs e)
	{
		ContextMenu.IsOpen = !ContextMenu.IsOpen;
	}

	private void ApplyBtn_Click(object sender, RoutedEventArgs e)
	{
		bool xValid = int.TryParse(XTxt.Text, out int x);
		bool yValid = int.TryParse(YTxt.Text, out int y);

		if (!xValid || !yValid)
		{
			return;
		}

		MoveWindow(WindowInfo.Hwnd, x, y);
	}

	private void SetPosBtn_Click(object sender, RoutedEventArgs e)
	{
		SizePopup.IsOpen = true;
		ContextMenu.IsOpen = false;
	}
	List<ScreenInfo> _screens = Global.GetScreenInfos();
	private void MoveBtn_Click(object sender, RoutedEventArgs e)
	{
		if (ScreenSelector.SelectedIndex >= 0)
		{
			Global.GetWindowRect(WindowInfo.Hwnd, out Global.RECT rect);
			var screen = _screens[ScreenSelector.SelectedIndex];

			int centerX = (screen.Bounds.Left + screen.Bounds.Right) / 2;
			int centerY = (screen.Bounds.Top + screen.Bounds.Bottom) / 2;

			int width = rect.Right - rect.Left;
			int height = rect.Bottom - rect.Top;
			// Adjust the coordinates to account for the window size
			int windowX = centerX - (int)(width / 2);
			int windowY = centerY - (int)(height / 2);

			MoveWindow(WindowInfo.Hwnd, windowX, windowY);
		}
	}

	private void SetScreenBtn_Click(object sender, RoutedEventArgs e)
	{
		ScreenPopup.IsOpen = true;
		ContextMenu.IsOpen = false;
		LoadScreens();
	}

	private void LoadScreens()
	{
		ScreenSelector.Items.Clear();
		for (int i = 0; i < _screens.Count; i++)
		{
			ScreenSelector.Items.Add($"{_screens[i].Name} ({_screens[i].Bounds.Right - _screens[i].Bounds.Left}x{_screens[i].Bounds.Bottom - _screens[i].Bounds.Top})");
		}
	}
}
