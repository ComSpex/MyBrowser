using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChooseBrowser {
	public partial class Form1:Form {
		protected string url = String.Empty;
		public Form1() {
			InitializeComponent();
			FileInfo urn= PerseCommandLine();
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
					Close();
				}
				try {
					string ie = (string)button2.Tag;
#if true
					StartBrowser(ie);
#else
					ticker=new Timer((state)=> {
						StartBrowser((string)state);
						ticker.Dispose();
						ticker=null;
					},ie,life*1000,life*1000);
#endif
				} catch(Exception ex) {
					MessageBox.Show(ex.Message);
				}
			}
		}
		private void button_Click(object sender,EventArgs e) {
			Button bu = sender as Button;
			StartBrowser((string)bu.Tag);
			Close();
		}
		FileInfo PerseCommandLine() {
			string[] urls = Environment.GetCommandLineArgs();
			FileInfo urn = null;
			if(urls.Length>1) {
				url=urls[1];
				urn=new FileInfo(url);
				if(urn.Extension.Contains("url")) {
					using(StreamReader sr = new StreamReader(urn.OpenRead())) {
						while(!sr.EndOfStream) {
							string line = sr.ReadLine();
							Match Ma = Regex.Match(line,"URL=(?<path>.*)$",RegexOptions.IgnoreCase);
							if(Ma.Success) {
								url=Ma.Groups["path"].Value;
								label1.Text=url;
								label1.Font=new Font("Courier New",10);
								Width=Math.Max(Width,url.Length*8);
								break;
							}
						}
					}
				}
			}
			return urn;
		}
		Process browser;
		[EnvironmentPermission(SecurityAction.LinkDemand,Unrestricted = true)]
		protected void StartBrowser(string fileName) {
			ProcessStartInfo prinf = new ProcessStartInfo(fileName,url);
			prinf.UseShellExecute=false;
			browser=Process.Start(prinf);
			browser.EnableRaisingEvents=true;
			//browser.Exited+=Browser_Exited;
			browser.Disposed+=Browser_Disposed;
			browser.WaitForExit();
			//MessageBox.Show("Exited!!");
		}
		private void Browser_Disposed(object sender,EventArgs e) {
			//This never happens.
			MessageBox.Show("Disposed!!");
		}
		delegate void event_type(object sender,EventArgs e);
		private void Browser_Exited(object sender,EventArgs e) {
			//MessageBox.Show("Exited!!");
			if(InvokeRequired) {
				Invoke(new event_type(Browser_Exited),new object[] { sender,e });
			} else {
				Close();
			}
		}
	}
}
