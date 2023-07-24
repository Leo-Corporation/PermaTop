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
using PermaTop.Classes;
using PermaTop.Enums;
using PeyrSharp.Core;
using PeyrSharp.Env;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PermaTop.Pages
{
	/// <summary>
	/// Interaction logic for SettingsPage.xaml
	/// </summary>
	public partial class SettingsPage : Page
	{
		public delegate void ThemeChangedEvent(object sender, EventArgs e);
		public event ThemeChangedEvent ThemeChanged;
		readonly System.Windows.Forms.NotifyIcon notifyIcon = new();
		public SettingsPage()
		{
			InitializeComponent();
			InitUI();
		}

		private async void InitUI()
		{
			// About section
			VersionTxt.Text = Global.Version; // Update the current version label

			// Select the language
			LangComboBox.SelectedIndex = (int)Global.Settings.Language;

			// Select the default theme border
			ThemeSelectedBorder = Global.Settings.Theme switch
			{
				Themes.Light => LightBorder,
				Themes.Dark => DarkBorder,
				_ => SystemBorder
			};
			Border_MouseEnter(Global.Settings.Theme switch
			{
				Themes.Light => LightBorder,
				Themes.Dark => DarkBorder,
				_ => SystemBorder
			}, null);

			// Checkboxes
			UpdateOnStartChk.IsChecked = Global.Settings.CheckUpdateOnStart;

			if (!Global.Settings.CheckUpdateOnStart) return;
			try
			{
				if (!await Internet.IsAvailableAsync()) return;
				if (!Update.IsAvailable(Global.Version, await Update.GetLastVersionAsync(Global.LastVersionLink))) return;
			}
			catch { return; }

			// If updates are available
			// Update the UI
			CheckUpdateBtn.Content = Properties.Resources.Install;
			UpdateTxt.Text = Properties.Resources.AvailableUpdates;

			// Show notification
			notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(AppDomain.CurrentDomain.BaseDirectory + @"\PermaTop.exe");
			notifyIcon.Visible = true; // Show
			notifyIcon.ShowBalloonTip(5000, Properties.Resources.PermaTop, Properties.Resources.AvailableUpdates, System.Windows.Forms.ToolTipIcon.Info);
			notifyIcon.Visible = false; // Hide
		}

		private async void CheckUpdateBtn_Click(object sender, RoutedEventArgs e)
		{
			string lastVersion = await Update.GetLastVersionAsync(Global.LastVersionLink);
			if (Update.IsAvailable(Global.Version, lastVersion))
			{
				UpdateTxt.Text = Properties.Resources.AvailableUpdates;

				if (MessageBox.Show(Properties.Resources.InstallConfirmMsg, $"{Properties.Resources.InstallVersion} {lastVersion}", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
				{
					return;
				}

				// If the user wants to proceed.
				XmlSerializerManager.SaveToXml(Global.Settings, Global.SettingsPath);

				Sys.ExecuteAsAdmin(Directory.GetCurrentDirectory() + @"\Xalyus Updater.exe"); // Start the updater
				Application.Current.Shutdown(); // Close
			}
			else
			{
				UpdateTxt.Text = Properties.Resources.UpToDate;
			}
		}

		private void SeeLicensesBtn_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show($"{Properties.Resources.Licenses}\n\n" +
			"Fluent System Icons - MIT License - © 2020 Microsoft Corporation\n" +
			"PeyrSharp - MIT License - © 2022-2023 Léo Corporation\n" +
			"PermaTop - MIT License - © 2023 Léo Corporation", $"{Properties.Resources.PermaTop} - {Properties.Resources.Licenses}", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		Border ThemeSelectedBorder;
		private void Border_MouseEnter(object sender, MouseEventArgs e)
		{
			((Border)sender).BorderBrush = Global.GetSolidColor("Accent");
		}

		private void Border_MouseLeave(object sender, MouseEventArgs e)
		{
			if ((Border)sender == ThemeSelectedBorder) return;
			((Border)sender).BorderBrush = new SolidColorBrush { Color = Colors.Transparent };
		}

		private void ResetBorders()
		{
			LightBorder.BorderBrush = new SolidColorBrush { Color = Colors.Transparent };
			DarkBorder.BorderBrush = new SolidColorBrush { Color = Colors.Transparent };
			SystemBorder.BorderBrush = new SolidColorBrush { Color = Colors.Transparent };
		}

		private void LightBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ResetBorders();
			ThemeSelectedBorder = (Border)sender;
			((Border)sender).BorderBrush = Global.GetSolidColor("Accent");
			Global.Settings.Theme = Themes.Light;
			XmlSerializerManager.SaveToXml(Global.Settings, Global.SettingsPath);

			Global.ChangeTheme();
			ThemeChanged?.Invoke(this, new());
		}

		private void DarkBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ResetBorders();
			ThemeSelectedBorder = (Border)sender;
			((Border)sender).BorderBrush = Global.GetSolidColor("Accent");
			Global.Settings.Theme = Themes.Dark;
			XmlSerializerManager.SaveToXml(Global.Settings, Global.SettingsPath);

			Global.ChangeTheme();
			ThemeChanged?.Invoke(this, new());
		}

		private void SystemBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ResetBorders();
			ThemeSelectedBorder = (Border)sender;
			((Border)sender).BorderBrush = Global.GetSolidColor("Accent");
			Global.Settings.Theme = Themes.System;
			XmlSerializerManager.SaveToXml(Global.Settings, Global.SettingsPath);

			Global.ChangeTheme();
			ThemeChanged?.Invoke(this, new());
		}

		private void LangComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			LangApplyBtn.Visibility = Visibility.Visible; // Show apply button
		}

		private void LangApplyBtn_Click(object sender, RoutedEventArgs e)
		{
			Global.Settings.Language = (Languages)LangComboBox.SelectedIndex;
			XmlSerializerManager.SaveToXml(Global.Settings, Global.SettingsPath);
			LangApplyBtn.Visibility = Visibility.Hidden; // Hide apply button

			if (MessageBox.Show(Properties.Resources.NeedRestartToApplyChanges, Properties.Resources.Settings, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
			{
				return;
			}

			Process.Start(Directory.GetCurrentDirectory() + @"\PermaTop.exe");
			Application.Current.Shutdown();
		}

		private void ImportBtn_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new()
			{
				Filter = "XML|*.xml",
				Title = Properties.Resources.Import
			}; // Create file dialog

			if (openFileDialog.ShowDialog() ?? true)
			{
				Global.Settings = XmlSerializerManager.LoadFromXml<Settings>(openFileDialog.FileName) ?? new() { IsFirstRun = false }; // Import
				XmlSerializerManager.SaveToXml(Global.Settings, Global.SettingsPath);
				MessageBox.Show(Properties.Resources.SettingsImportedMsg, Properties.Resources.PermaTop, MessageBoxButton.OK, MessageBoxImage.Information); // Show error message

				// Restart app
				Process.Start(Directory.GetCurrentDirectory() + @"\PermaTop.exe"); // Start app
				Environment.Exit(0); // Quit
			}
		}

		private void ExportBtn_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new()
			{
				FileName = "PermaTopSettings.xml",
				Filter = "XML|*.xml",
				Title = Properties.Resources.Export
			}; // Create file dialog

			if (saveFileDialog.ShowDialog() ?? true)
			{
				XmlSerializerManager.SaveToXml(Global.Settings, saveFileDialog.FileName); // Export games
				MessageBox.Show(Properties.Resources.SettingsExportedSucessMsg, Properties.Resources.PermaTop, MessageBoxButton.OK, MessageBoxImage.Information); // Show message
			}
		}

		private void ResetSettingsLink_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show(Properties.Resources.ResetSettingsConfirmation, Properties.Resources.Settings, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
			{
				return;
			}

			Global.Settings = new() { IsFirstRun = false };
			XmlSerializerManager.SaveToXml(Global.Settings, Global.SettingsPath);


			if (MessageBox.Show(Properties.Resources.NeedRestartToApplyChanges, Properties.Resources.Settings, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
			{
				return;
			}

			Process.Start(Directory.GetCurrentDirectory() + @"\PermaTop.exe");
			Application.Current.Shutdown();
		}

		private void UpdateOnStartChk_Checked(object sender, RoutedEventArgs e)
		{
			Global.Settings.CheckUpdateOnStart = UpdateOnStartChk.IsChecked ?? true;
			XmlSerializerManager.SaveToXml(Global.Settings, Global.SettingsPath);
		}
	}
}
