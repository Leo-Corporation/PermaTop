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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PermaTop.UserControls;
/// <summary>
/// Interaction logic for FavoriteItem.xaml
/// </summary>
public partial class FavoriteItem : UserControl
{
	Favorite Favorite { get; set; }
	StackPanel ParentPanel { get; set; }
	public FavoriteItem(Favorite favorite, StackPanel stackPanel, bool noPin)
	{
		InitializeComponent();
		Favorite = favorite;
		ParentPanel = stackPanel;
		InitUI(noPin);
	}

	private void InitUI(bool noPin)
	{
		TitleTxt.Text = Favorite.WindowTitle.Length > 50 ? Favorite.WindowTitle[0..50] + "..." : Favorite.WindowTitle;
		TitleToolTip.Content = Favorite.WindowTitle;

		var hWnd = Global.FindWindow(Favorite.ClassName, Favorite.WindowTitle);
		string? file = null;
		try
		{
			Global.GetWindowThreadProcessId(hWnd, out uint processId);
			Process process = Process.GetProcessById((int)processId);
			file = process.MainModule.FileName;
		}
		catch { }

		if (file != null && file == Favorite.ProcessFileName)
		{
			if (!noPin)	Global.SetWindowTopMost(hWnd, true);
			StatusTxt.Text = "\uF299";
			return;
		}

		StatusTxt.Text = "\uF36E";
	}

	private void DelBtn_Click(object sender, RoutedEventArgs e)
	{
		Global.Favorites.Remove(Favorite);
		XmlSerializerManager.SaveToXml(Global.Favorites, $@"{FileSys.AppDataPath}\Léo Corporation\PermaTop\Favs.xml");
		ParentPanel.Children.Remove(this);
	}
}
