// $Header: $
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyBrowser {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow:Window,IDisposable {
		protected string url = String.Empty;
		protected Timer ticker;
		protected List<Process> Apps = new List<Process>();
		protected int life = 1;
		[EnvironmentPermission(SecurityAction.LinkDemand,Unrestricted = true)]
		public MainWindow() {
			InitializeComponent();
			Left=
			Top=0;
			FileInfo urn=PerseCommandLine();
			if(urn!=null) {
				//Match Ma = Regex.Match(urn.FullName,"(?<word>[a-zA-Z ]+)[[]",RegexOptions.IgnoreCase);
				Match Ma = Regex.Match(urn.FullName,"(?<word>[a-zA-Z ]+)(?<brac>[[]..[]])",RegexOptions.IgnoreCase);
				if(Ma.Success) {
					string word = Ma.Groups["word"].Value;
					string brac = Ma.Groups["brac"].Value;
					if(!(brac=="[和英]"||brac=="[国語]")) {
						//word=word.Trim().Replace(" ","-");
						ProcessStartInfo prinf = new ProcessStartInfo(String.Format("http://www.macmillandictionary.com/dictionary/american/{0}",word));
						Process browser = Process.Start(prinf);
					}
				}
				if(!String.IsNullOrEmpty(url)&&url.StartsWith("msbsj:")) {
					ProcessStartInfo prinf = new ProcessStartInfo(url);
					Process browser = Process.Start(prinf);
					this.Close();
				}
				try {
					string ie = (string)Image2_png.Tag;
#if true
					StartBrowser(ie);
#else
					ticker=new Timer((state)=> {
						StartBrowser((string)state);
						ticker.Dispose();
						ticker=null;
					},ie,life*1000,life*1000);
#endif
				}catch(Exception ex) {
					MessageBox.Show(ex.Message);
				}
			}
		}
		FileInfo PerseCommandLine() {
			string[] urls = Environment.GetCommandLineArgs();
			FileInfo urn = null;
			if(urls.Length>1) {
				url=urls[1];
				urn = new FileInfo(url);
				if(urn.Extension.Contains("url")) {
					using(StreamReader sr = new StreamReader(urn.OpenRead())) {
						while(!sr.EndOfStream) {
							string line = sr.ReadLine();
							Match Ma = Regex.Match(line,"URL=(?<path>.*)$",RegexOptions.IgnoreCase);
							if(Ma.Success) {
								url=Ma.Groups["path"].Value;
								TextBlock tb = new TextBlock();
								tb.Text=url;
								tb.FontFamily=new FontFamily("Courier New");
								URLs.Children.Add(tb);
								this.Width=Math.Max(this.Width,url.Length*8);
								break;
							}
						}
					}
				}
			}
			return urn;
		}
		private void Image1_MouseUp(object sender,MouseButtonEventArgs e) {
			Image im = sender as Image;
			StartBrowser((string)im.Tag);
			this.Close();
		}
		[EnvironmentPermission(SecurityAction.LinkDemand,Unrestricted = true)]
		protected void StartBrowser(string fileName) {
			ProcessStartInfo prinf = new ProcessStartInfo(fileName,url);
			prinf.UseShellExecute=true;
			Process browser = Process.Start(prinf);
			browser.EnableRaisingEvents=true;
			browser.Exited+=Browser_Exited;
			browser.Disposed+=Browser_Disposed;
			//browser.WaitForInputIdle();
			Apps.Add(browser);
		}
		private void Browser_Disposed(object sender,EventArgs e) {
			//This never happens.
			MessageBox.Show("Disposed!!");
		}
		private void Browser_Exited(object sender,EventArgs e) {
			//MessageBox.Show("Exited!!");
		}
		private void MacMillan_Click(object sender,RoutedEventArgs e) {
			MenuItem mi = sender as MenuItem;
			mi.IsChecked=!mi.IsChecked;
		}
		[EnvironmentPermission(SecurityAction.LinkDemand,Unrestricted = false)]
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		[EnvironmentPermission(SecurityAction.LinkDemand,Unrestricted = true)]
		protected virtual void Dispose(bool disposing) {
			if(disposing) {
				if(ticker!=null) {
					ticker.Dispose();
				}
				foreach(Process App in Apps) {
					if(!App.HasExited) {
						try {
							App.CloseMainWindow();
						} catch { }
						try {
							App.Kill();
						} catch { }
					}
				}
			}
		}
	}
}
