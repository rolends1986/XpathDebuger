using System;
using Gtk;
using System.Text;
using XPathDebuger;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

public partial class MainWindow: Gtk.Window
{
	string filePath = "config";
	FileStream fileStream = null;
	BinaryFormatter formater = new BinaryFormatter ();

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		
		Build ();
		filePath = System.IO.Path.Combine (System.IO.Path.GetDirectoryName (
			Assembly.GetEntryAssembly ().Location), "config");
		Console.WriteLine (filePath);
		Init (); 
		LoadConfig ();

	}

	void Init ()
	{
		try {
			
			if (!File.Exists (filePath)) {
				fileStream = File.Open (filePath, FileMode.OpenOrCreate); 
				fileStream.Position = 0; 
				formater.Serialize (fileStream, new Config (){ XPath = "", Source = "" });
				fileStream.Flush ();
			} else {
				fileStream = File.Open (filePath, FileMode.Open); 
			}
		} catch (Exception ex) {
			
		}

	}

	void LoadConfig ()
	{
		try {
			
			fileStream.Position = 0;
			var config = (Config)formater.Deserialize (fileStream);
			txtXpath.Buffer.Text = config.XPath;
			txtSource.Buffer.Text = config.Source;
			cbbType.Active = config.Type == "HTML" ? 0 : 1;
		} catch (Exception ex) {
			
		}
	}


	void SaveConfig ()
	{
		try {
			fileStream.Position = 0;
			var config = new Config () {
				XPath = txtXpath.Buffer.Text,
				Source = txtSource.Buffer.Text,
				Type = cbbType.ActiveText
			};
			formater.Serialize (fileStream, config);
			fileStream.Flush ();
		} catch (Exception ex) {
			
		}
	}


	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnBtnExecClicked (object sender, EventArgs e)
	{
		string html =	txtSource.Buffer.Text;
		string xpath = txtXpath.Buffer.Text;
		SaveConfig ();
		if (cbbType.ActiveText.Equals ("html", StringComparison.InvariantCultureIgnoreCase)) {

			try { 
				HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument ();
				doc.LoadHtml (html);
				try {
					var nodes =	doc.DocumentNode.SelectNodes (xpath);
					if (nodes != null) {
						StringBuilder strResult = new StringBuilder ();
						foreach (var item in nodes) {
							strResult.AppendLine (item.OuterHtml);
						}
						txtResult.Buffer.Text = strResult.ToString ();
					}
				} catch (Exception ex) {
					new MessageDialog (this, DialogFlags.DestroyWithParent,
						MessageType.Error, ButtonsType.Ok, "XPATH解析错误").Show ();

				}
			} catch (Exception ex) {
				new MessageDialog (this, DialogFlags.DestroyWithParent,
					MessageType.Error, ButtonsType.Ok, "HTML解析错误").Show ();
			}
		} else {
		
			try { 
				XmlDocument doc = new XmlDocument ();
				doc.LoadXml (html);
				try {
					var nodes =	doc.SelectNodes (xpath);
					if (nodes != null) {
						StringBuilder strResult = new StringBuilder ();
						foreach (XmlNode item in nodes) {
							strResult.AppendLine (item.OuterXml);
						}
						txtResult.Buffer.Text = strResult.ToString ();
					}
				} catch (Exception ex) {
					new MessageDialog (this, DialogFlags.DestroyWithParent,
						MessageType.Error, ButtonsType.Ok, "XPATH解析错误").Show ();

				}
			} catch (Exception ex) {
				new MessageDialog (this, DialogFlags.DestroyWithParent,
					MessageType.Error, ButtonsType.Ok, "XML解析错误").Show ();
			}
		}



	}
}
