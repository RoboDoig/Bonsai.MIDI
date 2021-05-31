using Bonsai;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using System.Threading.Tasks;
using System.Reactive.Disposables;

namespace Bonsai.MIDI
{
    [Description("MIDI controller input")]
    public class MidiControl : Source<string>
    {
        public override IObservable<string> Generate()
        {
            return Observable.Create<string>(async observer =>
            {
                var inputDevice = await Task.Run(() => InputDevice.GetById(0));

                EventHandler<MidiEventReceivedEventArgs> inputReceived = (sender, e) =>
                {
                    ControlChangeEvent changeEvent = (ControlChangeEvent)e.Event;
                    observer.OnNext(changeEvent.ControlValue.ToString());
                };
                inputDevice.EventReceived += inputReceived;
                inputDevice.StartEventsListening();

                return Disposable.Create(() =>
                {
                    inputDevice.EventReceived -= inputReceived;
                    inputDevice.Dispose();
                });
            });
        }

        [Description("The MIDI controller index")]
        public int DeviceIndex { get; set; }
    }
}
