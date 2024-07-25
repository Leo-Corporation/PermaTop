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
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PermaTop;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
		InitUI();
		if (Global.Settings.LaunchAtStart ?? false)
		{
			HideMenu_Click(this, null);
		}
	}

	private void InitUI()
	{
		HelloTxt.Text = Global.GetHiSentence;
		PageContent.Navigate(Global.PinWindowsPage);
		CheckButton(PinPageBtn);
		Global.SettingsPage.ThemeChanged += (o, e) =>
		{
			CheckButton(SettingsPageBtn);
		};

		WindowsMenu.Loaded += (o, e) => { GenerateMenu(); };		
	}

	private void CheckButton(Button btn)
	{
		btn.Background = Global.GetSolidColor("Background1");
	}

	private void UnCheckAllButton()
	{
		// Background
		PinPageBtn.Background = new SolidColorBrush(Colors.Transparent);
		FavoritePageBtn.Background = new SolidColorBrush(Colors.Transparent);
		SettingsPageBtn.Background = new SolidColorBrush(Colors.Transparent);
	}

	private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	private void CloseBtn_Click(object sender, RoutedEventArgs e)
	{
		if (Global.Settings.LaunchAtStart ?? false)
		{
			HideMenu_Click(sender, e);
			return;
		}
		Application.Current.Shutdown();
	}

	private void PinPageBtn_Click(object sender, RoutedEventArgs e)
	{
		Global.PinWindowsPage?.InitUI();
		PageContent.Navigate(Global.PinWindowsPage);

		UnCheckAllButton();
		CheckButton(PinPageBtn);
	}

	private void FavoritePageBtn_Click(object sender, RoutedEventArgs e)
	{
		Global.FavoritePage?.InitUI(true);
		PageContent.Navigate(Global.FavoritePage);

		UnCheckAllButton();
		CheckButton(FavoritePageBtn);
	}

	private void SettingsPageBtn_Click(object sender, RoutedEventArgs e)
	{
		PageContent.Navigate(Global.SettingsPage);

		UnCheckAllButton();
		CheckButton(SettingsPageBtn);
	}

	bool isHidden = false;
	private void HideMenu_Click(object sender, RoutedEventArgs e)
	{
		isHidden = !isHidden;
		if (isHidden)
		{
			Hide();
			HideMenu.Header = Properties.Resources.Show;

			return;
		}
		Show();
		HideMenu.Header = Properties.Resources.Hide;
	}

	private void QuitMenu_Click(object sender, RoutedEventArgs e)
	{
		Application.Current.Shutdown();
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
	private void GenerateMenu()
	{
		WindowsMenu.Items.Clear();
		var windows = Global.GetWindows();
		foreach (var window in windows)
		{
			var menu = new MenuItem() { Header = window.Title, Style = (Style)Application.Current.Resources["MenuStyle"] };

			var closeMenu = new MenuItem() { Header = Properties.Resources.Close, Style = (Style)Application.Current.Resources["MenuStyle"] };
			closeMenu.Click += (o, e) =>
			{
				MessageBox.Show(window.Title);
				try
				{
					Global.SendMessage(window.Hwnd, WM_SYSCOMMAND, (IntPtr)SC_CLOSE, IntPtr.Zero);
				}
				catch { }
			};

			var maxMenu = new MenuItem() { Header = Properties.Resources.Maximize, Style = (Style)Application.Current.Resources["MenuStyle"] };
			maxMenu.Click += (o, e) =>
			{
				try
				{
					Global.SendMessage(window.Hwnd, WM_SYSCOMMAND, (IntPtr)SC_MAXIMIZE, IntPtr.Zero);
				}
				catch { }
			};

			var restoreMenu = new MenuItem() { Header = Properties.Resources.Restore, Style = (Style)Application.Current.Resources["MenuStyle"] };
			restoreMenu.Click += (o, e) =>
			{
				try
				{
					Global.SendMessage(window.Hwnd, WM_SYSCOMMAND, (IntPtr)SC_RESTORE, IntPtr.Zero);
				}
				catch  { }
			};

			var minMenu = new MenuItem() { Header = Properties.Resources.Minimize, Style = (Style)Application.Current.Resources["MenuStyle"] };
			minMenu.Click += (o, e) =>
			{
				try
				{
					Global.SendMessage(window.Hwnd, WM_SYSCOMMAND, (IntPtr)SC_MINIMIZE, IntPtr.Zero);
				}
				catch { }
			};

			var pinMenu = new MenuItem() { Header = Properties.Resources.Pin, Style = (Style)Application.Current.Resources["MenuStyle"] };
			pinMenu.Click += (o, e) =>
			{
				try
				{
					Global.SetWindowTopMost(window.Hwnd, true);
				}
				catch { }
			};

			var unpinMenu = new MenuItem() { Header = Properties.Resources.UnPin, Style = (Style)Application.Current.Resources["MenuStyle"] };
			unpinMenu.Click += (o, e) =>
			{
				try
				{
					Global.SetWindowTopMost(window.Hwnd, false);
				}
				catch { }
			};

			menu.Items.Add(closeMenu);
			menu.Items.Add(maxMenu);
			menu.Items.Add(restoreMenu);
			menu.Items.Add(minMenu);
			menu.Items.Add(pinMenu);
			menu.Items.Add(unpinMenu);
			WindowsMenu.Items.Add(menu);
		}
	}
}
