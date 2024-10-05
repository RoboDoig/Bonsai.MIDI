using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.MIDI
{
    public class MidiDevice : Source<ControlChangeEvent>
    {
        [TypeConverter(typeof(MidiDeviceNameConverter))]
        public string DeviceName { get; set; }

        public override IObservable<ControlChangeEvent> Generate()
        {
            return Observable.Create<ControlChangeEvent>(async observer =>
            {
                var device = await Task.Run(() => InputDevice.GetByName(DeviceName));

                EventHandler<MidiEventReceivedEventArgs> inputReceived = (sender, e) =>
                {
                    ControlChangeEvent changeEvent = (ControlChangeEvent)e.Event;

                    observer.OnNext(changeEvent);
                };

                device.EventReceived += inputReceived;
                device.StartEventsListening();

                return Disposable.Create(() =>
                {
                    device.StopEventsListening();
                    device.EventReceived -= inputReceived;
                    device.Dispose();
                });
            });
        }
    }
}
