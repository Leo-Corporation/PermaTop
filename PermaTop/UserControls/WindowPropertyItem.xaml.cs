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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

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

	private void InitUI()
	{
		TitleTxt.Text = WindowInfo.Title.Length > 50 ? WindowInfo.Title[0..50] + "..." : WindowInfo.Title;
		TitleToolTip.Content = WindowInfo.Title;

		PinBtn.Content = WindowInfo.IsPinned ? "\uF604" : "\uF602";

		Favorite item = new(WindowInfo.ClassName, WindowInfo.Title, WindowInfo.ProcessName);
		FavBtn.Content = Global.Favorites.Contains(item) ? "\uF71B" : "\uF710";

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


	[DllImport("user32.dll")]
	private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
	private void CloseBtn_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			SendMessage(WindowInfo.Hwnd, WM_SYSCOMMAND, (IntPtr)SC_CLOSE, IntPtr.Zero);
			ParentElement.Children.Remove(this);
		}
		catch { }
	}

	private void MaxRestoreBtn_Click(object sender, RoutedEventArgs e)
	{
		if (!Global.IsWindowMaximized(WindowInfo.Hwnd))
		{
			SendMessage(WindowInfo.Hwnd, WM_SYSCOMMAND, (IntPtr)SC_MAXIMIZE, IntPtr.Zero);

			return;
		}
	}
}
