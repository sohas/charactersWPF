using NAudio.Wave;
using System.Windows;

namespace charactersWPF.Model
{
	internal class Player
	{
		private WaveOut wout;
		private Mp3FileReader reader;

		public Player(string fileName)
		{
			Uri uri = new Uri(fileName, UriKind.Relative);
			var stream = Application.GetContentStream(uri).Stream;
			reader = new Mp3FileReader(stream);
			wout = new WaveOut();
			wout.Init(reader);
			wout.PlaybackStopped += (o, e) => reader.Position = 0;
		}

		public void Play()
		{
			if (wout.PlaybackState == PlaybackState.Stopped)
			{
				try
				{
					wout.Play();
				}
				catch { }
			}
		}

		public void Stop()
		{
			wout.Stop();
			reader.Position = 0;
		}
	}
}