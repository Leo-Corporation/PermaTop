﻿/*
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
using PermaTop.Enums;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace PermaTop.Windows
{
	/// <summary>
	/// Interaction logic for FirstRunWindow.xaml
	/// </summary>
	public partial class FirstRunWindow : Window
	{
		public FirstRunWindow()
		{
			InitializeComponent();
			LangComboBox.SelectedIndex = (int)Global.Settings.Language;
		}

		private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void CloseBtn_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void LangComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Global.Settings.Language = (Languages)LangComboBox.SelectedIndex;
			XmlSerializerManager.SaveToXml(Global.Settings, Global.SettingsPath);
		}

		private void LaunchBtn_Click(object sender, RoutedEventArgs e)
		{
			Global.Settings.IsFirstRun = false;
			XmlSerializerManager.SaveToXml(Global.Settings, Global.SettingsPath);

			Process.Start(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\PermaTop.exe");
			Application.Current.Shutdown(); // Quit the app
		}
	}
}
