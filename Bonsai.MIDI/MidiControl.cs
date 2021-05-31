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
    public class MidiControl : Source<int>
    {
        public override IObservable<int> Generate()
        {
            return Observable.Create<int>(async observer =>
            {
                var inputDevice = await Task.Run(() => InputDevice.GetById(DeviceIndex)); // TODO - Check available channels before assigning device index

                EventHandler<MidiEventReceivedEventArgs> inputReceived = (sender, e) =>
                {
                    ControlChangeEvent changeEvent = (ControlChangeEvent)e.Event;
                    int channelValue = changeEvent.Channel;
                    int controlNumber = changeEvent.ControlNumber;
                    int controlValue = changeEvent.ControlValue;
                    if (controlNumber == ControlChannel)
                    {
                        observer.OnNext(controlValue);
                    }
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

        [Description("The MIDI control channel")]
        public int ControlChannel { get; set; }
    }
}
