using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace charactersWPF.Model
{
      internal class Player
      {
            private WaveOut wout;
            private bool isPlaying = false;
            private Mp3FileReader reader;

            public Player(string fileName, int deviceNumber) 
            {
                  reader = new Mp3FileReader("..\\..\\..\\Notes\\" + fileName); 
                  wout = new WaveOut();

                  //var _audioFile = new AudioFileReader("..\\..\\..\\Notes\\" + fileName) { Volume = 1f };
                  //var mono = new StereoToMonoSampleProvider(_audioFile) { LeftVolume = 1f, RightVolume = 1f };
                  //var panner = new PanningSampleProvider(mono);


                  wout.Init(reader);
                  wout.DeviceNumber = 1;

                  //var lo = new List<WaveOutCapabilities>();

                  //for (int n = -1; n < WaveOut.DeviceCount; n++)
                  //{
                  //      lo.Add(WaveOut.GetCapabilities(n));
                  //}

                  wout.PlaybackStopped += (o, e) =>
                  {
                        reader.Position = 0;
                        //_audioFile.Position = 0;
                  };

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
            }
      }
}
