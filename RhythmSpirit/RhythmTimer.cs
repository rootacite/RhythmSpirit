using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShinenginePlus
{

    struct RhyStep
    {
        public long Base;
        public long Offset;

        public Type Type;
        public Point Position;

        public Key Key;
        public string Info;

        public double Time;
        public RawRectangleF Rect;
    }
    public enum RhythmType
    {
        RT_4_4 = 16,
        RT_4_3 = 12,
        RT_4_2 = 8
    }
    public class RhythmTimer
    {
        public RhythmType RhythmType { get; set; } = RhythmType.RT_4_4;
        ManualResetEvent pn = new ManualResetEvent(false);
        bool Stopped = false;
        int wait_beat = 0;
        int skip_beat = 0;
        public delegate void BeatBack(long Beat, long Sixteenth);
        public double BPM { get; set; } = 60;
        public event BeatBack Beats;
        public event BeatBack HalfBeats;
        public event BeatBack QuarterBeats;
        public event BeatBack EighthBeats;
        public event BeatBack SixteenthBeats;

        long Timebase = 0;
        double Timerise
        {
            get
            {
                return 60000 / BPM / 16d;
            }
        }
        public void Accuracy(double timeoffset)
        {
            var time_n = (Timebase * Timerise) / 1000d;
            if (timeoffset > time_n)
            {
                skip_beat = (int)((timeoffset - time_n) * 1000d / Timerise);
            }
            else
            {
                wait_beat = (int)((time_n - timeoffset) * 1000d / Timerise);
            }
            //Debug.WriteLine(time_n.ToString() + ":" + timeoffset.ToString());
        }
        private void TimeToucher()
        {
            while (!Stopped)
            {
                pn.WaitOne();
                if (wait_beat-- > 0) Thread.Sleep((int)Timerise);
                SixteenthBeats?.DynamicInvoke(new object[2] { Timebase / (int)RhythmType, Timebase % (int)RhythmType });
                if (Timebase % 2 == 0) EighthBeats?.DynamicInvoke(new object[2] { Timebase / (int)RhythmType, Timebase % (int)RhythmType });
                if (Timebase % 4 == 0) QuarterBeats?.DynamicInvoke(new object[2] { Timebase / (int)RhythmType, Timebase % (int)RhythmType });
                if (Timebase % 8 == 0) HalfBeats?.DynamicInvoke(new object[2] { Timebase / (int)RhythmType, Timebase % (int)RhythmType });
                if (Timebase % 16 == 0) Beats?.DynamicInvoke(new object[2] { Timebase / (int)RhythmType, Timebase % (int)RhythmType });
                if (skip_beat == 0) Thread.Sleep((int)Timerise);
                else { skip_beat--; }
                Timebase++;
            }
        }
        Task timer;
        public void Start()
        {
            pn.Set();
            timer = new Task(TimeToucher);
            timer.Start();
        }
        public void Pause()
        {
            pn.Reset();
        }
        public void Resume()
        {
            pn.Set();
        }
        public void Stop()
        {
            Stopped = true;
        }
        ~RhythmTimer()
        {
            timer.Dispose();
        }
    }
}
