using System.Windows;
using System.Windows.Controls;
using WHLClasses;
public partial class DistinguishChoices
{
	public void FillWindow(SkuCollection skuColl)
	{
		DistinguishChoicesHolder.Children.Clear();
		skuColl.Sort(( x, y) => x.PackSize.CompareTo(y.PackSize));
		foreach (var sku in skuColl)
        {
		    if (sku.PackSize <= 0) continue;
		    var newButton = new Button
		    {
		        FontFamily = TemplateButton.FontFamily,
		        Background = TemplateButton.Background,
		        Foreground = TemplateButton.Foreground,
		        FontSize = TemplateButton.FontSize,
		        FontWeight = TemplateButton.FontWeight,
		        Margin = new Thickness(4),
		        Width = TemplateButton.Width,
		        Height = TemplateButton.Height,
		        Content = sku.Title.Distinguish.ToUpper() + " " + sku.PackSize.ToString() + " pack",
		        Name = "ch" + sku.SKU,
		        Tag = sku
		    };
            if (sku.Title.Distinguish.Trim().Length == 0)
                newButton.Content = sku.PackSize.ToString() + " pack " + sku.Title.Label;
		    //Formatting done, we can FINALLY start doing useful datas.
		    newButton.Click += ButtonClicker;
		    DistinguishChoicesHolder.Children.Add(newButton);
		}
		//Mouse Murder
		System.Windows.Forms.Cursor.Position = new System.Drawing.Point(0, 0);
		//Reset the height of the distinguish. Form is 65 w/o buttons, plus 70 per button.

		
	}

	public void ButtonClicker(object Sender, RoutedEventArgs E)
	{
	    var button = Sender as Button;
	    if (button != null) Returnable = button.Tag as WhlSKU;
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

internal static class Distinguish
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
		DistinguishWindow.CancelButton.Visibility = SupportsCancellation ? Visibility.Visible : Visibility.Collapsed;
		DistinguishWindow.FillWindow(DistinguishOptions);
		DistinguishWindow.ShowDialog();
		return DistinguishWindow.Returnable;
	}
}
