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

      using var screenPlayer = new MediaPlayer(libvlc);
      using var chromecastPlayer = new MediaPlayer(libvlc);

      var renderer = libvlc.RendererList.FirstOrDefault();
      var discoverer = new RendererDiscoverer(libvlc, renderer.Name);

      discoverer.ItemAdded += (object sender, RendererDiscovererItemAddedEventArgs args) =>
      {
        rendererItems.Add(args.RendererItem);
      };
      discoverer.Start();
      await Task.Delay(TimeSpan.FromSeconds(5)); // give it a chance to find the chromecast
      chromecastPlayer.SetRenderer(rendererItems.First());

      /**
        Stream the screen to http://localhost:8080/video.mp4
       */
      using var screenMedia = new Media(libvlc, "screen://", FromType.FromLocation);
      screenMedia.AddOption(":screen-fps=24");
      screenMedia.AddOption(":sout=#transcode{vcodec=h264,vb=0,scale=0,acodec=mp3,ab=128,channels=2,samplerate=44100}:http{mux=ts,dst=:8080/video.mp4}");
      screenMedia.AddOption(":sout-keep");
      /**
        Grab the screen stream from the url above
       */
      using var screenStream = new Media(libvlc, "http://localhost:8080/video.mp4", FromType.FromLocation);

      screenPlayer.Play(screenMedia);
      // not sure if entirely necessary but give it some time to start before trying to reach
      // the screen stream in the url above
      await Task.Delay(TimeSpan.FromSeconds(2));
      chromecastPlayer.Play(screenStream);

      // make it last for a couple of minutes just to see if it works
      await Task.Delay(TimeSpan.FromMinutes(2.0));


      screenPlayer.Stop();
      chromecastPlayer.Stop();

    }
  }
}
