using Bonsai;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;

namespace Bonsai.MIDI
{
    [Description("MIDI file reader")]
    public class MidiReader : Source<string>
    {
        private static Playback playback;

        public override IObservable<string> Generate()
        {
            return Observable.Create<string>(async observer =>
            {
                var midiFile = await Task.Run(() => MidiFile.Read(FileName));
                var outputDevice = await Task.Run(() => OutputDevice.GetByName("Microsoft GS Wavetable Synth"));

                EventHandler<NotesEventArgs> onNotesPlaybackStarted = (sender, e) =>
                {
                    observer.OnNext(e.Notes.ToString());
                };

                playback = midiFile.GetPlayback(outputDevice);
                playback.NotesPlaybackStarted += onNotesPlaybackStarted;
                playback.Start();

                return Disposable.Create(() =>
                {
                    playback.Dispose();
                });
            });
        }

        [Description("The name of the MIDI file.")]
        //[FileNameFilter("WAV Files (*.wav;*.wave)|*.wav;*.wave|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }
    }
}
