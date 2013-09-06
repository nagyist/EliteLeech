using System;
using Gtk;
using YoutubeExtractor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

public partial class MainWindow: Gtk.Window
{	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	bool vid = true;
	protected void OnButton1Clicked (object sender, EventArgs e)
	{
		if (!String.IsNullOrEmpty (entry1.Text)) {
			// Our test youtube link
			string link = entry1.Text;

			IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls (link);
			if (vid == true) {
				VideoInfo video = videoInfos
                .First (info => info.VideoType == VideoType.Mp4 && info.Resolution == 720);
				WebClient wc = new WebClient ();

				wc.DownloadProgressChanged += wc_DownloadProgressChanged;
				wc.DownloadFileAsync (new Uri (video.DownloadUrl), Environment.GetFolderPath (Environment.SpecialFolder.Desktop) + @"/" + video.Title + video.VideoExtension);

				label1.Text = dlspeed;
			}
			else {
				VideoInfo video = videoInfos
    .Where(info => info.CanExtractAudio)
    .OrderByDescending(info => info.AudioBitrate)
    .First();
				entry1.Text = video.DownloadUrl;
				/*WebClient wc = new WebClient ();

				wc.DownloadProgressChanged += wc_DownloadProgressChanged;
				wc.DownloadFileAsync (new Uri (video.DownloadUrl), Environment.GetFolderPath (Environment.SpecialFolder.Desktop) + @"/" + video.Title + video.AudioExtension);
			*/}
		}
	}
	string dlspeed = "";
	void wc_DownloadProgressChanged (object sender, DownloadProgressChangedEventArgs e)
	{
		try {
			Application.Invoke (delegate {
				progressbar1.Fraction = e.ProgressPercentage / 100.0;
				downloadSpeed (e.BytesReceived);
			}
			);
		} catch (Exception ex) {
			errorLog (true, ex.Message, true, "Downloading Video/Audio");
		}
	}
	protected void OnRadiobutton1Clicked (object sender, EventArgs e)
	{
		vid = true;
	}	
	protected void OnRadiobutton2Clicked (object sender, EventArgs e)
	{
		vid = false;
	}
	private void errorLog (bool isFatal, string addtolog, bool uselocation, string location)
	{
		/*  errorLog is a function I wrote a long time ago to allow error logging in developer mode
		 *  I ported it to Mono as I found it to be a VERY handy tool
		 *  Usage:
		 *  errorLog(true, "error message", true, "module at fault"); - this says its a fatal system error, and tells you where the error happened
		 *  errorLog(true, "Error Message", false, ""); - this is still a fatal system error, however the fault module is not listed
		 *  errorLog(false, "Error Message", true, "Module At Fault"); - this says its not a fatal error and lists the fault module
		 *  errorLog(false, "Error Message", false, ""); - this says its not a fatal error nor does it list the module at fault
		 */
		TextIter mIterr = textview2.Buffer.EndIter;
		string fatal = "";
		if (isFatal) {
			fatal = "Fatal - ";
		}
		if (uselocation == false) {
			textview2.Buffer.Insert (ref mIterr, fatal + addtolog + Environment.NewLine);
		} else {
			textview2.Buffer.Insert (ref mIterr, fatal + addtolog + " Location: " + location + Environment.NewLine);
		}
	}
	public DateTime lastUpdate;
	public long lastBytes;
	private void downloadSpeed (long bytes)
	{
		try {
			if (lastBytes == 0) {
				lastUpdate = DateTime.Now;
				lastBytes = bytes;
			} else {

				var now = DateTime.Now;
				var bytesChange = bytes - lastBytes;
				double seconds = (now - lastUpdate).TotalSeconds;
				var bytesPerSecond = bytesChange / seconds;	
				dlspeed = Math.Truncate(bytesPerSecond /1024).ToString() + "kb/s";
				lastBytes = bytes;
				lastUpdate = now;
			}
		} catch (Exception ex) {
			errorLog (true, ex.Message, true, "DownloadSpeed Void");
		}
    }
}