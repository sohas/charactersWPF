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
			var rr = Application.GetContentStream(uri);
			var stream = Application.GetContentStream(uri).Stream;
			reader = new Mp3FileReader(stream);
			wout = new WaveOut();

			wout.Init(reader);
			wout.DeviceNumber = 1;
			wout.PlaybackStopped += (o, e) => reader.Position = 0;
		}

		public void Play(double volume = 1)
		{
			if (wout.PlaybackState == PlaybackState.Stopped)
			{
				wout.Volume = (float)volume / 5f;
				wout.Play();
			}
		}

		public void Stop()
		{
			wout.Stop();
			reader.Position = 0;
		}
	}
}