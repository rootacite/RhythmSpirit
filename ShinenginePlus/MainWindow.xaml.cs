using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ShinenginePlus.DrawableControls;

using ShinenginePlus;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using BitmapImage = ShinenginePlus.DrawableControls.BitmapImage;
using DrawingImage = ShinenginePlus.DrawableControls.DrawingImage;
using Layer = ShinenginePlus.DrawableControls.Layer;
using SolidColorBrush = SharpDX.Direct2D1.SolidColorBrush;
using RadialGradientBrush = SharpDX.Direct2D1.RadialGradientBrush;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Xml.Linq;

namespace Shinengine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public IntPtr WindowHandle = (IntPtr)0;
        public BackGroundLayer BackGround = null;
        public Direct2DWindow DX = null;
        DrawableText time_set;
        public DrawResultW DrawProc(DeviceContext dc)
        {
            dc.Clear(new RawColor4(1, 1, 1, 1));

            BackGround.Render(dc);
            BackGround.Update();


            return DrawResultW.Commit;
        }
        public MainWindow()
        {
            InitializeComponent();
            Shinengine.Media.FFmpegBinariesHelper.RegisterFFmpegBinaries();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowHandle = new WindowInteropHelper(this).Handle;

            DX = new Direct2DWindow(new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), WindowHandle);
            BackGround = new BackGroundLayer(
                new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight),
                this,
                new RawRectangleF(0, 0, (float)this.ActualWidth, (float)this.ActualHeight))
            {
                Color = new RawColor4(1, 1, 1, 0),
                Range = new RawRectangleF(0, 0, (float)this.ActualWidth, (float)this.ActualHeight)
            };

            DX.DrawProc += DrawProc;

            DX.Run();

            time_set = new DrawableText("", "黑体",18);
            time_set.Color = new RawColor4(1, 0, 0, 1);
            time_set.PushTo(BackGround);
        }
        BitmapImage img = null;
        
        public void StartStoryBoard(string path)
        {
            string bk = path + "\\bk.png";
            string music = path + "\\music.aac";
            string script = path + "\\script.xml";
            RhythmTimer sb = new RhythmTimer() {};
           


            img = new BitmapImage(bk);
            RenderableImage bk1 = new RenderableImage(img) { Position = new System.Drawing.Point(0, 0) };
            bk1.Size = new SharpDX.Size2(1280,720);
            bk1.PushTo(BackGround);

            DarkCurtain dn = new DarkCurtain(new System.Drawing.Size(1280, 720));
            dn.PushTo(BackGround);
            void KP(int x, int y, Key key, string info,double speed = 1)
            {
                speed = 60d / sb.BPM;
                new KeyboardSinglePoint(BackGround, DX, new System.Drawing.Point(x, y), this, key, info, speed, dn);
            }
            void KL(int x, int y, Key key, string info, double beat, double speed = 1)
            {
                speed= 60d / sb.BPM;
                new KeyboardLongPoint(BackGround, DX, new System.Drawing.Point(x, y), this, 60d / sb.BPM * beat, key, info, speed, dn);
            }
            void PL(int x, int y,double beat, double speed = 1)
            {
                speed = 60d / sb.BPM;
                new MouseLongPoint(BackGround, DX, new System.Drawing.Point(x, y), this, 60d / sb.BPM * beat, speed, dn);
            }
            void PP(int x, int y, double speed = 1)
            {
                speed = 60d / sb.BPM;
                new MouseSinglePoint(BackGround, DX, new System.Drawing.Point(x, y), this, speed, dn);
            }
           
            AudioFramly music_player = new AudioFramly(music);
            music_player.Decode();

            XDocument script_obj = XDocument.Load(script) ;
            var des = script_obj.Root.Nodes();
            foreach(XElement i in des)
            {
                if (i.Name == "head")
                {
                    sb.BPM = Convert.ToInt32(i.Attribute("BPM").Value.ToString(), 10);
                }
            }
            
            sb.HalfBeats += (c, b) =>
            {if (b == 0)
                    PP(new Random().Next(0,1280), (int)new Random().Next(0, 720));
                // new RippleEffect(30, 400, new System.Drawing.Point(500,500)).PushTo(BackGround);
                time_set.text = c.ToString() + ":" + b.ToString();
            };
            sb.Start();
            time_set.Top();
            new Thread(()=>
            {
                AudioFramly.waveInit(WindowHandle, music_player.Out_channels, music_player.Out_sample_rate, music_player.Bit_per_sample, music_player.Out_buffer_size);
                foreach(var i in music_player.abits)
                {
                    unsafe
                    {
                        sb.Accuracy((i?.time_base).Value);
                        AudioFramly.waveWrite((byte*)i?.data, music_player.Out_buffer_size);
                        Marshal.FreeHGlobal((i?.data).Value);
                    }
                }
                AudioFramly.waveClose();
            }) { IsBackground=true}.Start();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            StartStoryBoard(fileName);
        }
    }
    public class RhythmTimer
    {
        ManualResetEvent pn = new ManualResetEvent(false);
        bool Stopped = false;
        int wait_beat = 0;
        int skip_beat = 0;
        public delegate void BeatBack(long Beat,long Sixteenth);
        public double BPM { get; set; } = 60;
        public event BeatBack Beats;
        public event BeatBack HalfBeats;
        public event BeatBack QuarterBeats;
        public event BeatBack EighthBeats;
        public event BeatBack SixteenthBeats;

        long Timebase = 0;
        double Timerise {
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
            Debug.WriteLine(time_n.ToString() + ":" + timeoffset.ToString());
        }
        private void TimeToucher()
        {
            while (!Stopped)
            {
                pn.WaitOne();
                if (wait_beat-- > 0) Thread.Sleep((int)Timerise);
                SixteenthBeats?.DynamicInvoke(new object[2] { Timebase / 16, Timebase % 16 });
                if (Timebase % 2 == 0) EighthBeats?.DynamicInvoke(new object[2] { Timebase / 16, Timebase % 16 });
                if (Timebase % 4 == 0) QuarterBeats?.DynamicInvoke(new object[2] { Timebase / 16, Timebase % 16 });
                if (Timebase % 8 == 0) HalfBeats?.DynamicInvoke(new object[2] { Timebase / 16, Timebase % 16 });
                if (Timebase % 16 == 0) Beats?.DynamicInvoke(new object[2] { Timebase / 16, Timebase % 16 });
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

    class PerfectInfo
    {
        RenderableImage ctl = null;
        BitmapImage img = null;

        public PerfectInfo(Layer layer, Direct2DWindow vm, System.Drawing.Point pos, int info)
        {
            if (info == 0)
                img = new BitmapImage("assets\\perfect.png");
            else if (info == 1) img = new BitmapImage("assets\\great.png");
            else if (info == 2) img = new BitmapImage("assets\\bad.png");
            else img = new BitmapImage("assets\\miss.png");
            pos.X -= img.PixelSize.Width / 4;
            pos.Y -= img.PixelSize.Height / 4;
            ctl = new RenderableImage(img);
            ctl.Position = pos;
            ctl.Size = new SharpDX.Size2(ctl.Size.Width / 2, ctl.Size.Height / 2);
            ctl.AddUpdateProcess(() =>
            {
                if (ctl.Opacity >= 0)
                {
                    ctl.Opacity -= 0.02f;
                    return true;
                }
                else
                {
                    ctl.PopFrom();
                    vm.CleanTasks.Add(ctl);
                    vm.CleanTasks.Add(img);
                    return false;
                }
            });
            ctl.PushTo(layer);
        }
    }

    class MouseSinglePoint
    {
        InteractiveObject btn = null;
        DrawingImage img = new DrawingImage(new System.Drawing.Size(138, 138));
        bool clicked = false;
        double frame = 90d;
        float opdivied = 0;

        public MouseSinglePoint(Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, double speed = 1 , DarkCurtain dk=null)
        {
            speed = 1 / speed;

               var area = new SharpDX.Direct2D1.Ellipse(new RawVector2(pos.X, pos.Y), 66, 66);
            pos.X -= 69;
            pos.Y -= 69;

            btn = new InteractiveObject(img) { Position = pos };
            img.Proc += (s) =>
            {
                s.BeginDraw();
                s.Clear(new RawColor4(1, 1, 1, 0));

                if (frame > 30)
                {
                    double r = 132 - 100d * ((90d - frame) / 60d);
                    using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 0.2f, 1, 1)), c = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.35f, 0.35f, 0.35f, 0.15f)))
                    {
                        s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), (float)(r / 2), (float)(r / 2)), b, 4);
                        if (!clicked) s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), 8, 8), c);
                    }
                }
                else
                {
                    double r = 34 * (frame / 30d);
                    using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 0.2f, 1, 1)))
                        s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), (float)(r / 2), (float)(r / 2)), b);
                }
                s.EndDraw();
            };
            img.Update();
            btn.AddUpdateProcess(() =>
            {
                if (clicked)
                {
                    btn.AddUpdateProcess(() =>
                    {
                        if (frame < 90)
                        {
                            frame += 4d;
                            btn.Opacity -= opdivied;
                        }
                        else
                        {

                            btn.PopFrom();
                            vm.CleanTasks.Add(btn);
                            vm.CleanTasks.Add(img);
                            if (dk != null)
                                dk.Range.Remove(area);
                            return false;
                        }
                        img.Update();
                        return true;
                    });
                    return false;
                }
                if (frame > 0)
                    frame -= speed;
                else
                {
                    btn.PopFrom();
                    vm.CleanTasks.Add(btn);
                    vm.CleanTasks.Add(img);
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 3);
                    if (dk != null)
                        dk.Range.Remove(area);
                    return false;
                }
                img.Update();
                return true;
            });
            btn.MouseDown += (e, t) =>
            {
                if (clicked || t.Handled || frame == 0) return;
                if (frame > 89)
                {
                    opdivied = 1;
                }
                else
                    opdivied = 4 / (float)(90d - frame);
                clicked = true;
                t.Handled = true;
                if (frame > 25 && frame < 35)
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 0);

                }
                else if ((frame > 15 && frame <= 25) || (frame >= 35 && frame < 45))
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 1);
                }
                else if ((frame > 0 && frame <= 15) || (frame >= 45 && frame < 60))
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 2);
                }
                else
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 3);
                }
                new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 69, pos.Y + 69)).PushTo(layer);
            };
            btn.PushTo(layer, w);
            if(dk!=null)
                dk.Range.Add(area);
        }
    }

    class MouseLongPoint
    {
        InteractiveObject btn = null;
        DrawingImage img = new DrawingImage(new System.Drawing.Size(160, 160));
        bool downed = false;
        bool uped = false;
        double frame = 90d;
        float opdivied = 0;

        public MouseLongPoint(Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, double time, double speed = 1, DarkCurtain dk = null)
        {
            var area = new SharpDX.Direct2D1.Ellipse(new RawVector2(pos.X, pos.Y), 80, 80);
            time = time * 60d;
            pos.X -= 80;
            pos.Y -= 80;
            btn = new InteractiveObject(img) { Position = pos };
            img.Proc += (s) =>
            {

                s.BeginDraw();
                s.Clear(new RawColor4(1, 1, 1, 0));

                if (frame > 30)
                {
                    double r = 110 - 100d * ((90d - frame) / 60d);
                    using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 1f, 0.2f, 1)), c = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.35f, 0.35f, 0.35f, 0.15f)))
                    {
                        s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), (float)(r / 2), (float)(r / 2)), b, 4);
                        if (!downed) s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), 10, 10), c);
                    }
                }
                else
                {
                    double r = 18 * (frame / 30d);
                    using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 1f, 0.2f, 1)))
                        s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), (float)(r / 2), (float)(r / 2)), b);
                }
                using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.6f, 0.6f, 0.6f, 0.4f)))
                    s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), 68f, 68f), b, 1);


                s.EndDraw();
            };
            img.Update();
            btn.AddUpdateProcess(() =>
            {
                if (downed)
                {
                    double rise = (105d - frame) / time; ;

                    btn.AddUpdateProcess(() =>
                    {
                        if (frame < 115)
                        {
                            frame += rise;

                        }
                        else
                        {
                            if (dk != null)
                                dk.Range.Remove(area);
                            btn.PopFrom();
                            vm.CleanTasks.Add(btn);
                            vm.CleanTasks.Add(img);
                            if (!uped)
                                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3);
                            return false;
                        }
                        img.Update();
                        return true;
                    });
                    return false;
                }
                if (frame > 0)
                    frame -= speed;
                else
                {
                    if (dk != null)
                        dk.Range.Remove(area);
                    btn.PopFrom();
                    vm.CleanTasks.Add(btn);
                    vm.CleanTasks.Add(img);
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3);
                    return false;
                }
                img.Update();
                return true;
            });
            btn.MouseDown += (e, t) =>
            {
                if (downed || t.Handled || frame == 0) return;
                if (frame > 89)
                {
                    opdivied = 1;
                }
                else
                    opdivied = 4 / (float)(90d - frame);
                downed = true;
                t.Handled = true;
                if (frame > 25 && frame < 35)
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 0);

                }
                else if ((frame > 15 && frame <= 25) || (frame >= 35 && frame < 45))
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 1);
                }
                else if ((frame > 0 && frame <= 15) || (frame >= 45 && frame < 60))
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 2);
                }
                else
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3);
                    uped = true;
                    btn.Saturation = 0;
                }
                new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 80, pos.Y + 80)).PushTo(layer);
            };
            btn.MouseUp += (e, t) =>
            {
                if (!downed || t.Handled || frame == 115 || uped) return;
                if (frame > 114)
                {
                    opdivied = 1;
                }
                else
                    opdivied = 4 / (float)(115d - frame);
                uped = true;
                t.Handled = true;
                if (frame > 95 && frame < 105)
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 0);

                }
                else if ((frame > 85 && frame <= 95) || (frame >= 105 && frame < 115))
                {
                    btn.Saturation = 0f;
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 1);
                }
                else if (frame >= 70 && frame <= 85)
                {
                    btn.Saturation = 0f;
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 2);
                }
                else
                {
                    btn.Saturation = 0f;
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3);
                }
                new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 80, pos.Y + 80)).PushTo(layer);
            };
            btn.PushTo(layer, w);
            if (dk != null)
                dk.Range.Add(area);
        }
    }
    sealed public class RippleEffect : BRenderableObject
    {
        double _r = 0d;
        double _rele_r = 0d;
        double rise = 0d;

        double frame = 0;
        double _time;
        Point pos;
        public RippleEffect(double time, double r, System.Drawing.Point pos)
        {
            _r = r;
            rise = r / time;
            _time = time;
            AddUpdateProcess(() =>
            {
                if (frame < time)
                {
                    _rele_r += rise;
                    frame++;
                    return true;
                }
                else
                {
                    PopFrom();

                    return false;
                }
            });
            this.pos = new Point(pos.X, pos.Y);
        }
        public override void Render(DeviceContext dc)
        {
            using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(dc, new RawColor4(0.7f, 0.7f, 0.7f, 1f - ((float)frame / (float)_time))))
                dc.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2((float)pos.X, (float)pos.Y), (float)(_rele_r / 2d), (float)(_rele_r / 2d)), b, 3);
        }
    }

    class KeyboardSinglePoint
    {
        InteractiveObject btn = null;
        DrawingImage img = new DrawingImage(new System.Drawing.Size(138, 138));
        bool clicked = false;
        double frame = 90d;
        float opdivied = 0;

        public KeyboardSinglePoint(Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, Key key, string info, double speed = 1,DarkCurtain dk=null)
        {
            var area = new SharpDX.Direct2D1.Ellipse(new RawVector2(pos.X, pos.Y), 66, 66);
            pos.X -= 69;
            pos.Y -= 69;

            this.layer = layer;
            this.vm = vm;
            this.pos = pos;
            this.key = key;
            btn = new InteractiveObject(img) { Position = pos };
            img.Proc += (s) =>
            {
                s.BeginDraw();
                s.Clear(new RawColor4(1, 1, 1, 0));

                var wfactory = new SharpDX.DirectWrite.Factory();
                var wformat = new SharpDX.DirectWrite.TextFormat(wfactory, "黑体", 16);
                wformat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
                wformat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                if (frame > 30)
                {
                    double r = 132 - 100d * ((90d - frame) / 60d);
                    using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 0.2f, 1, 1)), c = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.35f, 0.35f, 0.35f, 0.15f)))
                    {
                        s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), (float)(r / 2), (float)(r / 2)), b, 4);
                        if (!clicked)
                        {
                            s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), 8, 8), c);
                            s.DrawText(info, wformat, new RawRectangleF(61, 61, 77, 77), b);
                        }
                    }
                }
                else
                {
                    double r = 34 * (frame / 30d);
                    using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 0.2f, 1, 1)))
                        s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), (float)(r / 2), (float)(r / 2)), b);
                }
                wfactory.Dispose();
                wformat.Dispose();
                s.EndDraw();
            };
            img.Update();
            btn.AddUpdateProcess(() =>
            {
                if (clicked)
                {
                    btn.AddUpdateProcess(() =>
                    {
                        if (frame < 90)
                        {
                            frame += 4d;
                            btn.Opacity -= opdivied;
                        }
                        else
                        {
                            if (dk != null)
                                dk.Range.Remove(area);
                            btn.PopFrom();
                            vm.CleanTasks.Add(btn);
                            vm.CleanTasks.Add(img);

                            w.KeyDown -= KD;
                            return false;
                        }
                        img.Update();
                        return true;
                    });
                    return false;
                }
                if (frame > 0)
                    frame -= speed;
                else
                {
                    if (dk != null)
                        dk.Range.Remove(area);
                    btn.PopFrom();
                    vm.CleanTasks.Add(btn);
                    vm.CleanTasks.Add(img);
                    w.KeyDown -= KD;
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 3);
                    return false;
                }
                img.Update();
                return true;
            });
            w.KeyDown += KD;
            btn.PushTo(layer, w);
            if (dk != null)
                dk.Range.Add(area);
        }
        private void KD(object s, KeyEventArgs t)
        {
            if (t.Key != key) return;
            if (clicked || t.Handled || frame == 0) return;
            if (frame > 89)
            {
                opdivied = 1;
            }
            else
                opdivied = 4 / (float)(90d - frame);
            clicked = true;
            t.Handled = true;
            if (frame > 25 && frame < 35)
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 0);

            }
            else if ((frame > 15 && frame <= 25) || (frame >= 35 && frame < 45))
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 1);
            }
            else if ((frame > 0 && frame <= 15) || (frame >= 45 && frame < 60))
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 2);
            }
            else
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 3);
            }
            new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 69, pos.Y + 69)).PushTo(layer);
        }
        Layer layer = null;
        Direct2DWindow vm = null;
        System.Drawing.Point pos;
        Key key;
    }

    class KeyboardLongPoint
    {
        InteractiveObject btn = null;
        DrawingImage img = new DrawingImage(new System.Drawing.Size(160, 160));
        bool downed = false;
        bool uped = false;
        double frame = 90d;
        float opdivied = 0;


        //////
        Layer layer = null;
        Direct2DWindow vm = null;
        System.Drawing.Point pos;
        Key key;
        public KeyboardLongPoint(Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, double time, Key key, string info, double speed = 1, DarkCurtain dk = null)
        {

            var area = new SharpDX.Direct2D1.Ellipse(new RawVector2(pos.X, pos.Y), 80, 80);
            time = time * 60d;
            pos.X -= 80;
            pos.Y -= 80;
            this.layer = layer;
            this.vm = vm;
            this.pos = pos;
            this.key = key;
            btn = new InteractiveObject(img) { Position = pos };
            img.Proc += (s) =>
            {

                s.BeginDraw();
                s.Clear(new RawColor4(1, 1, 1, 0));
                var wfactory = new SharpDX.DirectWrite.Factory();
                var wformat = new SharpDX.DirectWrite.TextFormat(wfactory, "幼圆", 20);
                wformat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
                wformat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                if (frame > 30)
                {
                    double r = 110 - 100d * ((90d - frame) / 60d);
                    using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 1f, 0.2f, 1)), c = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.35f, 0.35f, 0.35f, 0.15f)))
                    {
                        s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), (float)(r / 2), (float)(r / 2)), b, 4);
                        if (!downed)
                        {
                            s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), 10, 10), c);
                            //  s.FillRectangle(new RawRectangleF(70, 70, 90, 90),b);
                            s.DrawText(info, wformat, new RawRectangleF(70, 70, 90, 90), b);
                        }
                    }
                }
                else
                {
                    double r = 18 * (frame / 30d);
                    using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 1f, 0.2f, 1)))
                        s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), (float)(r / 2), (float)(r / 2)), b);
                }
                using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.6f, 0.6f, 0.6f, 0.4f)))
                    s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), 68f, 68f), b, 1);

                wfactory.Dispose();
                wformat.Dispose();
                s.EndDraw();
            };
            img.Update();
            btn.AddUpdateProcess(() =>
            {
                if (downed)
                {
                    double rise = (105d - frame) / time; ;

                    btn.AddUpdateProcess(() =>
                    {
                        if (frame < 115)
                        {
                            frame += rise;

                        }
                        else
                        {
                            if (dk != null)
                                dk.Range.Remove(area);
                            btn.PopFrom();
                            vm.CleanTasks.Add(btn);
                            vm.CleanTasks.Add(img);
                            w.KeyDown -= KD;
                            w.KeyUp -= KU;
                            if (!uped)
                                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3);
                            return false;
                        }
                        img.Update();
                        return true;
                    });
                    return false;
                }
                if (frame > 0)
                    frame -= speed;
                else
                {
                    if (dk != null)
                        dk.Range.Remove(area);
                    btn.PopFrom();
                    vm.CleanTasks.Add(btn);
                    vm.CleanTasks.Add(img);
                    w.KeyDown -= KD;
                    w.KeyUp -= KU;
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3);
                    return false;
                }
                img.Update();
                return true;
            });
            w.KeyDown += KD;
            w.KeyUp += KU;
            btn.PushTo(layer, w);
            if (dk != null)
                dk.Range.Add(area);
        }
        private void KU(object s, KeyEventArgs t)
        {

            if (t.Key != key) return;
            if (!downed || t.Handled || frame == 115 || uped) return;
            if (frame > 114)
            {
                opdivied = 1;
            }
            else
                opdivied = 4 / (float)(115d - frame);
            uped = true;
            t.Handled = true;
            if (frame > 95 && frame < 105)
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 0);

            }
            else if ((frame > 85 && frame <= 95) || (frame >= 105 && frame < 115))
            {
                btn.Saturation = 0f;
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 1);
            }
            else if (frame >= 70 && frame <= 85)
            {
                btn.Saturation = 0f;
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 2);
            }
            else
            {
                btn.Saturation = 0f;
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3);
            }
            new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 80, pos.Y + 80)).PushTo(layer);
        }
        private void KD(object s, KeyEventArgs t)
        {

            if (t.Key != key) return;
            if (downed || t.Handled || frame == 0) return;
            if (frame > 89)
            {
                opdivied = 1;
            }
            else
                opdivied = 4 / (float)(90d - frame);
            downed = true;
            t.Handled = true;
            if (frame > 25 && frame < 35)
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 0);

            }
            else if ((frame > 15 && frame <= 25) || (frame >= 35 && frame < 45))
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 1);
            }
            else if ((frame > 0 && frame <= 15) || (frame >= 45 && frame < 60))
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 2);
            }
            else
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3);
                uped = true;
                btn.Saturation = 0;
            }
            new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 80, pos.Y + 80)).PushTo(layer);
        }
    }

    class DarkCurtain : BRenderableObject, IDisposable
    {
        private readonly DrawingImage mask;
        public readonly List<SharpDX.Direct2D1.Ellipse> Range = new List<SharpDX.Direct2D1.Ellipse>();
        public DarkCurtain(System.Drawing.Size Size)
        {
            mask = new DrawingImage(Size);
            mask.Proc += (dc) =>
            {
                dc.BeginDraw();
                dc.Clear(new RawColor4(0, 0, 0, 0));
               
               
                var copy_Range = Range.ToList();
                foreach (var i in copy_Range)
                {
                    var rp = new SharpDX.Direct2D1.RadialGradientBrushProperties
                    {
                        GradientOriginOffset = new RawVector2(0, 0),
                        Center=i.Point,
                        RadiusX=i.RadiusX,
                        RadiusY=i.RadiusY
                    };
                    var gp = new SharpDX.Direct2D1.GradientStopCollection(
                        dc, 
                        new SharpDX.Direct2D1.GradientStop[] 
                        { 
                            new SharpDX.Direct2D1.GradientStop 
                            { Color = new RawColor4(0, 0, 0, 1), Position = 0f },
                            new SharpDX.Direct2D1.GradientStop 
                            { Color = new RawColor4(1, 1, 1, 0), Position = 1f } 
                        });

                    using (RadialGradientBrush b = new RadialGradientBrush(dc, rp, gp))
                        dc.FillEllipse(i, b);
                    gp.Dispose();
                }
                dc.EndDraw();
            };
        }
        public float Opacity { get; set; } = 0.7f;

        public void Dispose()
        {
            mask.Dispose();
        }

        public override void Render(DeviceContext dc)
        {
            var m_layer = new SharpDX.Direct2D1.Layer(dc, new SharpDX.Size2F(dc.Size.Width, dc.Size.Height));
            mask.Update();
            var output = mask.Output;
            var m_lock = output.Lock(SharpDX.WIC.BitmapLockFlags.Write);
            for (int i=0;i< m_lock.Size.Height; i++)
            {
                IntPtr base_line = m_lock.Data.DataPointer + m_lock.Stride * i;
                for(int q=0;q < m_lock.Size.Width; q++)
                {
                    unsafe
                    {
                        byte* pPixel = (byte*)(base_line + 4 * q);
                        *(pPixel + 3) = (byte)(255 - *(pPixel + 3));
                    }
                }

            }
            m_lock.Dispose();
            var mask_per = SharpDX.Direct2D1.Bitmap.FromWicBitmap(dc, output);
            var omask = new BitmapBrush(dc, mask_per);
            LayerParameters lp = new LayerParameters()
            {
                Opacity = 1.0f,
                ContentBounds = new RawRectangleF(0, 0, dc.Size.Width, dc.Size.Height),
                LayerOptions=LayerOptions.None,
                MaskAntialiasMode=AntialiasMode.PerPrimitive,
                OpacityBrush = omask
            };
            dc.PushLayer(ref lp, m_layer);


            using (SolidColorBrush b = new SolidColorBrush(dc, new RawColor4(0, 0, 0, Opacity)))
                dc.FillRectangle(new RawRectangleF(0, 0, dc.Size.Width, dc.Size.Height), b);
            dc.PopLayer();

            omask.Dispose();
            mask_per.Dispose();
            output.Dispose();
            m_layer.Dispose();
        }
    }
}
