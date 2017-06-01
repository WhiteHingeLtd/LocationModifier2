using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using LocationModifier2.Dialogs;
using LocationModifier2.UserControls;
using WHLClasses;
public partial class DistinguishChoices : Window
{
	public void FillWindow(SkuCollection SkUColl)
	{
		DistinguishChoicesHolder.Children.Clear();
		SkUColl.Sort((WhlSKU X, WhlSKU Y) => X.PackSize.CompareTo(Y.PackSize));
		foreach (WhlSKU Sku in SkUColl) {
			if (Sku.PackSize > 0) {
				Button NewButton = new Button();
				NewButton.FontFamily = TemplateButton.FontFamily;
				NewButton.Background = TemplateButton.Background;
				NewButton.Foreground = TemplateButton.Foreground;
				NewButton.FontSize = TemplateButton.FontSize;
				NewButton.FontWeight = TemplateButton.FontWeight;
				NewButton.Margin = new Thickness(4);
				NewButton.Width = TemplateButton.Width;
				NewButton.Height = TemplateButton.Height;
				//Formatting done, we can FINALLY start doing useful datas.
				NewButton.Content = Sku.Title.Distinguish.ToUpper() + " " + Sku.PackSize.ToString() + " pack";
				NewButton.Name = "ch" + Sku.SKU;
				NewButton.Tag = Sku;
				NewButton.Click += ButtonClicker;
				DistinguishChoicesHolder.Children.Add(NewButton);
			}
		}
		//Mouse Murder
		System.Windows.Forms.Cursor.Position = new System.Drawing.Point(0, 0);
		//Reset the height of the distinguish. Form is 65 w/o buttons, plus 70 per button.

		if (CancelButton.Visibility == Visibility.Visible) {
			Height = 123 + (DistinguishChoicesHolder.Children.Count * 70);
		} else {
			Height = 47 + (DistinguishChoicesHolder.Children.Count * 70);
		}

	}

	public void ButtonClicker(object Sender, RoutedEventArgs E)
	{
		Returnable = (Sender as Button).Tag as WhlSKU;
		//Me.DialogResult = True
		Hide();
	}


	public WhlSKU Returnable;
	private void Window_SizeChanged(object Sender, SizeChangedEventArgs E)
	{
		Top = (SystemParameters.WorkArea.Height - Height) / 2;
		Left = (SystemParameters.WorkArea.Width - Width) / 2;
	}

	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		Returnable = null;
		this.Hide();
	}

    public DistinguishChoices()
    {
        InitializeComponent();
    }
}

static internal class Distinguish
{


	static DistinguishChoices DistinguishWindow = new DistinguishChoices();
	/// <summary>
	/// 
	/// </summary>
	/// <param name="DistinguishOptions"></param>
	/// <param name="SupportsCancellation">Supplying this parameter will return NOTHING/NULL if the user cancels.</param>
	/// <returns></returns>
	public static WhlSKU DistinguishSku(SkuCollection DistinguishOptions, bool SupportsCancellation = false)
	{
		if (SupportsCancellation) {
			DistinguishWindow.CancelButton.Visibility = Visibility.Visible;
		} else {
			DistinguishWindow.CancelButton.Visibility = Visibility.Collapsed;
		}
		DistinguishWindow.FillWindow(DistinguishOptions);
		DistinguishWindow.ShowDialog();
		return DistinguishWindow.Returnable;
	}
}
