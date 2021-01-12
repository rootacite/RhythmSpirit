﻿using System;
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

using D2DBitmap = SharpDX.Direct2D1.Bitmap1;
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
using System.Windows.Threading;

namespace Shinengine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        static Key GetKey(char c)
        {
            if (c >= 'A' && c <= 'Z')
                return Key.A + (c - 'A');

            if (c == '!')
                return Key.Up;
            else if (c == '@')
                return Key.Down;
            else if (c == '#')
                return Key.Left;
            else if (c == '%')
                return Key.Right;
            else
                return Key.Space;
        }
        public IntPtr WindowHandle = (IntPtr)0;
        public BackGroundLayer BackGround = null;
        public Direct2DWindow DX = null;
        GroupLayer Title;
        DrawableText time_set;
        public DrawResultW DrawProc(DeviceContext dc)
        {
            dc.Clear(new RawColor4(1, 1, 1, 1));

            BackGround.Render();

            return DrawResultW.Commit;
        }
        public MainWindow()
        {
            InitializeComponent();
            Shinengine.Media.FFmpegBinariesHelper.RegisterFFmpegBinaries();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            pn.Set();
            pv.Set();
            WindowHandle = new WindowInteropHelper(this).Handle;

            
              

            DX = new Direct2DWindow(new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), WindowHandle) { AskedFrames=60};
            BackGround = new BackGroundLayer(
                new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight),
                this,
                new RawRectangleF(0, 0, (float)this.ActualWidth, (float)this.ActualHeight),DX.DC)
            {
                
                Color = new RawColor4(1, 1, 1, 0),
                Range = new RawRectangleF(0, 0, (float)this.ActualWidth, (float)this.ActualHeight)
            };
            DispatcherTimer updateTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1d / 60d) };
            updateTimer.Tick += (s, e) => { BackGround.Update(); };
            

            DX.DrawProc += DrawProc;
            img = new BitmapImage("assets\\title.png");
            bk1 = new RenderableImage(img,DX.DC) { Position = new System.Drawing.Point(0, 0) };
            bk1.Size = new SharpDX.Size2(1280, 720);
            bk1.PushTo(BackGround);
            dn = new DarkCurtain(DX.DC);
            dn.PushTo(BackGround);

            Title = new GroupLayer(BackGround, new SharpDX.Size2(1280, 30), DX.DC);
            Title.OutPutRange = new RawRectangleF(0, 0, 1280, 30);
            Title.Color = new SharpDX.Mathematics.Interop.RawColor4(1, 1, 1, 0.35f);

            time_set = new DrawableText("Hello World", "黑体",22, DX.DC);
            time_set.Color = new RawColor4(1, 0, 0, 1);
            time_set.PushTo(Title);

            DX.Run();
            updateTimer.Start();
        }
        RenderableImage bk1;
        BitmapImage img = null;
        DarkCurtain dn;
        RhythmTimer sb;
        ManualResetEvent pn = new ManualResetEvent(false);
        ManualResetEvent pv = new ManualResetEvent(false);
        bool ask_stop = false;
        public void StartStoryBoard(string path)
        {
            sb?.Stop();
            ask_stop = true;
            pv.WaitOne();
            ask_stop = false;
            string bk = path + "\\bk.png";
            string music = path + "\\music.aac";
            string script = path + "\\script.xml";
            sb = new RhythmTimer() {};
            img.ReLoad(bk);
            void KP(int x, int y, Key key, string info,double speed = 1)
            {
                speed = 60d / sb.BPM;
                new KeyboardSinglePoint(DX.DC,BackGround, DX, new System.Drawing.Point(x, y), this, key, info, speed, dn);
            }
            void KL(int x, int y, Key key, string info, double beat, double speed = 1)
            {
                speed= 60d / sb.BPM;
                new KeyboardLongPoint(DX.DC, BackGround, DX, new System.Drawing.Point(x, y), this, 60d / sb.BPM * beat, key, info, speed, dn);
            }
            void PL(int x, int y,double beat, double speed = 1)
            {
                speed = 60d / sb.BPM;
                new MouseLongPoint(DX.DC, BackGround, DX, new System.Drawing.Point(x, y), this, 60d / sb.BPM * beat, speed, dn);
            }
            void PP(int x, int y, double speed = 1)
            {
                speed = 60d / sb.BPM;
                new MouseSinglePoint(DX.DC, BackGround, DX, new System.Drawing.Point(x, y), this, speed, dn);
            }
           
            AudioFramly music_player = new AudioFramly(music);
            music_player.Decode();

            XDocument script_obj = XDocument.Load(script) ;
            var des = script_obj.Root.Nodes();
            List<RhyStep> rs = new List<RhyStep>();

            foreach (XElement i in des)
            {
                if (i.Name == "head")
                {
                    sb.BPM = Convert.ToInt32(i.Attribute("BPM").Value.ToString(), 10);
                    continue;
                }

                int x = Convert.ToInt32(i.Attribute("Pos").Value.ToString().Split(":")[0], 10);
                int y = Convert.ToInt32(i.Attribute("Pos").Value.ToString().Split(":")[1], 10);

                int b = Convert.ToInt32(i.Attribute("Time").Value.ToString().Split(":")[0], 10);
                int o = Convert.ToInt32(i.Attribute("Time").Value.ToString().Split(":")[1], 10);
                if (i.Name == "MS")
                {
                    rs.Add(new RhyStep() { Type = typeof(MouseSinglePoint), Position = new Point(x, y), Base = b, Offset = o });
                }
                if (i.Name == "ML")
                {
                    double t = Convert.ToDouble(i.Attribute("Beat").Value.ToString());
                    rs.Add(new RhyStep() { Type = typeof(MouseLongPoint), Position = new Point(x, y), Base = b, Offset = o, Time = t });
                }
                if (i.Name == "KS")
                {
                    char key_c = i.Attribute("Key").Value.ToString()[0];


                    rs.Add(new RhyStep() { Type = typeof(KeyboardSinglePoint), Position = new Point(x, y), Base = b, Offset = o, Key = GetKey(key_c), Info = key_c.ToString() });
                }

                if (i.Name == "KL")
                { 

                    double t = Convert.ToDouble(i.Attribute("Beat").Value.ToString());
                    char key_c = i.Attribute("Key").Value.ToString()[0];

                    rs.Add(new RhyStep() { Type = typeof(KeyboardLongPoint), Position = new Point(x, y), Base = b, Offset = o, Time = t, Key = GetKey(key_c), Info = key_c.ToString() });
                }
            }

            sb.QuarterBeats += (c, b) =>
            {
                time_set.text = c.ToString() + ":" + b.ToString() + "  FPS:" + DX.FrameRate.ToString(); ;

                frame_start:
                if (rs.Count == 0)
                    return;
                if (c == rs[0].Base - 1 && b == rs[0].Offset)
                {
                    if (rs[0].Type == typeof(MouseSinglePoint))
                        PP((int)rs[0].Position.X, (int)rs[0].Position.Y);
                    if (rs[0].Type == typeof(MouseLongPoint))
                        PL((int)rs[0].Position.X, (int)rs[0].Position.Y, rs[0].Time);
                    if (rs[0].Type == typeof(KeyboardSinglePoint))
                        KP((int)rs[0].Position.X, (int)rs[0].Position.Y, rs[0].Key, rs[0].Info);
                    if (rs[0].Type == typeof(KeyboardLongPoint))
                        KL((int)rs[0].Position.X, (int)rs[0].Position.Y, rs[0].Key, rs[0].Info, rs[0].Time);
                    if (rs[0].Type == typeof(RippleEffect))
                        new RippleEffect(30, 400, new System.Drawing.Point((int)rs[0].Position.X, (int)rs[0].Position.Y), DX.DC).PushTo(BackGround);

                    rs.RemoveAt(0);
                    goto frame_start;
                }
                else
                {
                    return;
                }

            };
            sb.Start();
            Title.Top();
            double time_save = 0;
            new Thread(() =>
            {
            pv.Reset();
            AudioFramly.waveInit(WindowHandle, music_player.Out_channels, music_player.Out_sample_rate, music_player.Bit_per_sample, music_player.Out_buffer_size);
            foreach (var i in music_player.abits)
            {
                if (!ask_stop)
                {
                    unsafe
                    {
                            time_save = (i?.time_base).Value;
                          //  sb.Accuracy((i?.time_base).Value);
                            AudioFramly.waveWrite((byte*)i?.data, music_player.Out_buffer_size);
                        }
                    }
                    Marshal.FreeHGlobal((i?.data).Value);
                }
                AudioFramly.waveClose();
                pv.Set();
            }) { IsBackground=true}.Start(); 
            new Thread(() =>
            {
                while (!ask_stop)
                {
                    sb.Accuracy(time_save);
                    Thread.Sleep(100);
                }
            })
            { IsBackground = true }.Start();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            StartStoryBoard(fileName);
        }
    }

    struct RhyStep
    {
        public long Base;
        public long Offset;

        public Type Type;
        public Point Position;

        public Key Key;
        public string Info;

        public double Time;
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

        public PerfectInfo(Layer layer, Direct2DWindow vm, System.Drawing.Point pos, int info,DeviceContext DC)
        {
            if (info == 0)
                img = new BitmapImage("assets\\perfect.png");
            else if (info == 1) img = new BitmapImage("assets\\great.png");
            else if (info == 2) img = new BitmapImage("assets\\bad.png");
            else img = new BitmapImage("assets\\miss.png");
            pos.X -= img.PixelSize.Width / 4;
            pos.Y -= img.PixelSize.Height / 4;
            ctl = new RenderableImage(img, DC);
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

        public MouseSinglePoint(DeviceContext DC,Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, double speed = 1 , DarkCurtain dk=null)
        {
            speed = 1 / speed;

               var area = new SharpDX.Direct2D1.Ellipse(new RawVector2(pos.X, pos.Y), 66, 66);
            pos.X -= 69;
            pos.Y -= 69;

            btn = new InteractiveObject(img, DC) { Position = pos };
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
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 3, DC);
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
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 0, DC);

                }
                else if ((frame > 15 && frame <= 25) || (frame >= 35 && frame < 45))
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 1, DC);
                }
                else if ((frame > 0 && frame <= 15) || (frame >= 45 && frame < 60))
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 2, DC);
                }
                else
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 3, DC);
                }
                new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 69, pos.Y + 69), DC).PushTo(layer);
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

        public MouseLongPoint(DeviceContext DC, Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, double time, double speed = 1, DarkCurtain dk = null)
        {
            speed = 1 / speed;
            var area = new SharpDX.Direct2D1.Ellipse(new RawVector2(pos.X, pos.Y), 80, 80);
            time = time * 60d;
            pos.X -= 80;
            pos.Y -= 80;
            btn = new InteractiveObject(img, DC) { Position = pos };
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
                                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3,DC);
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
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3, DC);
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
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 0, DC);

                }
                else if ((frame > 15 && frame <= 25) || (frame >= 35 && frame < 45))
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 1, DC);
                }
                else if ((frame > 0 && frame <= 15) || (frame >= 45 && frame < 60))
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 2, DC);
                }
                else
                {
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3, DC);
                    uped = true;
                    btn.Saturation = 0;
                }
                new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 80, pos.Y + 80), DC).PushTo(layer);
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
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 0, DC);

                }
                else if ((frame > 85 && frame <= 95) || (frame >= 105 && frame < 115))
                {
                    btn.Saturation = 0f;
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 1, DC);
                }
                else if (frame >= 70 && frame <= 85)
                {
                    btn.Saturation = 0f;
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 2, DC);
                }
                else
                {
                    btn.Saturation = 0f;
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3, DC);
                }
                new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 80, pos.Y + 80), DC).PushTo(layer);
            };
            btn.PushTo(layer, w);
            if (dk != null)
                dk.Range.Add(area);
        }
    }
    sealed public class RippleEffect : RenderableObject
    {
        double _r = 0d;
        double _rele_r = 0d;
        double rise = 0d;

        double frame = 0;
        double _time;
        Point pos;
        public RippleEffect(double time, double r, System.Drawing.Point pos,DeviceContext DC):base(DC)
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
        public override void Render()
        {
            using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(HostDC, new RawColor4(0.7f, 0.7f, 0.7f, 1f - ((float)frame / (float)_time))))
                HostDC.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2((float)pos.X, (float)pos.Y), (float)(_rele_r / 2d), (float)(_rele_r / 2d)), b, 3);
        }
    }

    class KeyboardSinglePoint
    {
        InteractiveObject btn = null;
        DrawingImage img = new DrawingImage(new System.Drawing.Size(138, 138));
        bool clicked = false;
        double frame = 90d;
        float opdivied = 0;

        DeviceContext DC;

        public KeyboardSinglePoint(DeviceContext DC, Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, Key key, string info, double speed = 1,DarkCurtain dk=null)
        {
            this.DC = DC;
            speed = 1 / speed;
            var area = new SharpDX.Direct2D1.Ellipse(new RawVector2(pos.X, pos.Y), 66, 66);
            pos.X -= 69;
            pos.Y -= 69;

            this.layer = layer;
            this.vm = vm;
            this.pos = pos;
            this.key = key;
            btn = new InteractiveObject(img, DC) { Position = pos };
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
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 3, DC);
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
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 0, DC);

            }
            else if ((frame > 15 && frame <= 25) || (frame >= 35 && frame < 45))
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 1, DC);
            }
            else if ((frame > 0 && frame <= 15) || (frame >= 45 && frame < 60))
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 2, DC);
            }
            else
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 3, DC);
            }
            new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 69, pos.Y + 69), DC).PushTo(layer);
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

        DeviceContext DC;
        //////
        Layer layer = null;
        Direct2DWindow vm = null;
        System.Drawing.Point pos;
        Key key;
        public KeyboardLongPoint(DeviceContext DC, Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, double time, Key key, string info, double speed = 1, DarkCurtain dk = null)
        {
            this.DC = DC;
            speed = 1 / speed;
            var area = new SharpDX.Direct2D1.Ellipse(new RawVector2(pos.X, pos.Y), 80, 80);
            time = time * 60d;
            pos.X -= 80;
            pos.Y -= 80;
            this.layer = layer;
            this.vm = vm;
            this.pos = pos;
            this.key = key;
            btn = new InteractiveObject(img, DC) { Position = pos };
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
                                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3, DC);
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
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3, DC);
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
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 0, DC);

            }
            else if ((frame > 85 && frame <= 95) || (frame >= 105 && frame < 115))
            {
                btn.Saturation = 0f;
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 1, DC);
            }
            else if (frame >= 70 && frame <= 85)
            {
                btn.Saturation = 0f;
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 2, DC);
            }
            else
            {
                btn.Saturation = 0f;
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3, DC);
            }
            new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 80, pos.Y + 80), DC).PushTo(layer);
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
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 0, DC);

            }
            else if ((frame > 15 && frame <= 25) || (frame >= 35 && frame < 45))
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 1, DC);
            }
            else if ((frame > 0 && frame <= 15) || (frame >= 45 && frame < 60))
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 2, DC);
            }
            else
            {
                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3, DC);
                uped = true;
                btn.Saturation = 0;
            }
            new RippleEffect(30, 400, new System.Drawing.Point(pos.X + 80, pos.Y + 80), DC).PushTo(layer);
        }
    }

    class DarkCurtain : RenderableObject, IDisposable
    {
        private BitmapBrush o_mask = null;
        public readonly List<SharpDX.Direct2D1.Ellipse> Range = new List<SharpDX.Direct2D1.Ellipse>();
        public DarkCurtain(DeviceContext DC):base(DC)
        {
             
        }
        public float Opacity { get; set; } = 0.7f;

        public void Dispose()
        {
            o_mask?.Dispose();
        }

        public override void Render()
        {
            o_mask?.Dispose();
            HostDC.EndDraw();
            using (D2DBitmap loadBp = new D2DBitmap(HostDC, new SharpDX.Size2((int)HostDC.Size.Width, (int)HostDC.Size.Height),
                new BitmapProperties1(HostDC.PixelFormat, HostDC.DotsPerInch.Width, HostDC.DotsPerInch.Height, BitmapOptions.Target | BitmapOptions.CannotDraw)),
                loadBp2 = new D2DBitmap(HostDC, new SharpDX.Size2((int)HostDC.Size.Width, (int)HostDC.Size.Height),
             new BitmapProperties1(HostDC.PixelFormat, HostDC.DotsPerInch.Width, HostDC.DotsPerInch.Height, BitmapOptions.None)),
             loadBp3 = new D2DBitmap(HostDC, new SharpDX.Size2((int)HostDC.Size.Width, (int)HostDC.Size.Height),
             new BitmapProperties1(HostDC.PixelFormat, HostDC.DotsPerInch.Width, HostDC.DotsPerInch.Height, BitmapOptions.None)))
            {
                var old_tg = HostDC.Target;
                HostDC.Target = loadBp;

                HostDC.BeginDraw();
                HostDC.Clear(new RawColor4(0, 0, 0, 1));
                HostDC.EndDraw();

                loadBp2.CopyFromBitmap(loadBp);

                HostDC.BeginDraw();
                HostDC.Clear(new RawColor4(0, 0, 0, 0));
                var CRG = Range.ToArray();
                foreach (var i in CRG)
                {
                    var rp = new SharpDX.Direct2D1.RadialGradientBrushProperties
                    {
                        GradientOriginOffset = new RawVector2(0, 0),
                        Center = i.Point,
                        RadiusX = i.RadiusX,
                        RadiusY = i.RadiusY
                    };
                    var gp = new SharpDX.Direct2D1.GradientStopCollection(
                        HostDC,
                        new SharpDX.Direct2D1.GradientStop[]
                        {
                            new SharpDX.Direct2D1.GradientStop
                            { Color = new RawColor4(0, 0, 0, 1), Position = 0f },
                            new SharpDX.Direct2D1.GradientStop
                            { Color = new RawColor4(1, 1, 1, 0), Position = 1f }
                        });
                    using (RadialGradientBrush b = new RadialGradientBrush(HostDC, rp, gp))
                        HostDC.FillEllipse(i, b);
                    gp.Dispose();
                }
                HostDC.EndDraw();

                loadBp3.CopyFromBitmap(loadBp);

                var m_bn_effect = new SharpDX.Direct2D1.Effects.Blend(HostDC)
                {
                    Mode = BlendMode.HardLight
                };

                m_bn_effect.SetInput(0, loadBp2, new RawBool());
                m_bn_effect.SetInput(1, loadBp3, new RawBool());
                HostDC.BeginDraw();
                HostDC.DrawImage(m_bn_effect);
                HostDC.EndDraw();

                loadBp2.CopyFromBitmap(loadBp);
                m_bn_effect.Dispose();

                HostDC.Target = old_tg;
                old_tg.Dispose();


                o_mask = new BitmapBrush(HostDC, loadBp2);

            }

            HostDC.BeginDraw();
                var m_layer = new SharpDX.Direct2D1.Layer(HostDC, new SharpDX.Size2F(HostDC.Size.Width, HostDC.Size.Height));
         
            LayerParameters lp = new LayerParameters()
            {
                Opacity = 1.0f,
                ContentBounds = new RawRectangleF(0, 0, HostDC.Size.Width, HostDC.Size.Height),
                LayerOptions=LayerOptions.None,
                MaskAntialiasMode=AntialiasMode.PerPrimitive,
                OpacityBrush = o_mask
            };
            HostDC.PushLayer(ref lp, m_layer);


            using (SolidColorBrush b = new SolidColorBrush(HostDC, new RawColor4(0, 0, 0, Opacity)))
                HostDC.FillRectangle(new RawRectangleF(0, 0, HostDC.Size.Width, HostDC.Size.Height), b);
            HostDC.PopLayer();

            m_layer.Dispose();
        }
    }
}
