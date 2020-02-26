using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibVLCSharp.Shared;

namespace Stromboli
{
  class Program
  {
    static async Task Main(string[] args)
    {
      Core.Initialize();
      var rendererItems = new HashSet<RendererItem>();

      using var libvlc = new LibVLC("--verbose=2");
      using var mediaPlayer = new MediaPlayer(libvlc);
      using var media = new Media(libvlc, "screen://", FromType.FromLocation);
      var renderer = libvlc.RendererList.FirstOrDefault();
      var discoverer = new RendererDiscoverer(libvlc, renderer.Name);
      discoverer.ItemAdded += (object sender, RendererDiscovererItemAddedEventArgs args) =>
      {
        rendererItems.Add(args.RendererItem);
      };
      discoverer.Start();


      media.AddOption(":screen-fps=24");
      media.AddOption(":sout=#transcode{vcodec=h264,vb=0,scale=0,acodec=mp4a,ab=128,channels=2,samplerate=44100}:standard{access=http,mux=ts,dst=localhost:8080}");
      media.AddOption(":sout-keep");


      await Task.Delay(TimeSpan.FromSeconds(5)); // wait for 5 seconds

      mediaPlayer.SetRenderer(rendererItems.First());

      mediaPlayer.Play(media); // start recording

      await Task.Delay(TimeSpan.FromMinutes(2.0)); // record for 2 minutes

      mediaPlayer.Stop(); // stop recording and saves the file


    }

    public static void StartCasting()
    {

    }
  }
}
