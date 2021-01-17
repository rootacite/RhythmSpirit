using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using PInvoke;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;

using System.Xml.Linq;
using System.Threading;
using Shinengine.Media;
using ShinenginePlus;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace RhythmMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string ReciveString = "";
        WriteableBitmap BackGround = new WriteableBitmap(1280, 720, 96, 96, PixelFormats.Pbgra32, null);
        Graphics graphics;
        Preview Preview_Entity = new Preview();
        public MainWindow()
        {
            InitializeComponent();
            FFmpegBinariesHelper.RegisterFFmpegBinaries();
            Preview_Entity.Show();

            DispatcherTimer trail = new DispatcherTimer();
            trail.Interval = TimeSpan.FromSeconds(0.03);
            trail.Tick += (e, v) =>
            {
                if (Preview.StaticSelf != null)
                {
                    Preview_Entity.Left = this.Left+10;
                    Preview_Entity.Top = this.Top + 720;
                }
                if (Script != null)
                    Preview_Entity.MS.Text = Script.ToString();
                Beat.Text = Base.ToString() + ":" + Offset.ToString() + "   " + new System.Windows.Point((int)BK.Margin.Left, (int)BK.Margin.Top).ToString();
            };

            graphics = Graphics.FromImage(new Bitmap(1280, 720, BackGround.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, BackGround.BackBuffer));
            Loaded += (e, v) =>
            {
                BK.Source = BackGround;
                trail.Start();
            };

        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
          
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var handle = Kernel32.GetCurrentProcess();
            Kernel32.TerminateProcess(handle.DangerousGetHandle(), 0);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            Inet.Margin = new Thickness(e.GetPosition(this).X - 60, e.GetPosition(this).Y - 60, 0, 0);
            if (Mouse.MiddleButton==MouseButtonState.Pressed)
            {
                var new_pos = e.GetPosition(this);
                var vector = new Point(new_pos.X - RalPos.X, new_pos.Y - RalPos.Y);

                BK.Margin = new Thickness(oldPos.X+vector.X,oldPos.Y+vector.Y,0,0);
            }
        }

        private void BK_MouseDown(object sender, MouseButtonEventArgs e)
        {
            oldPos = new Point(BK.Margin.Left, BK.Margin.Top);
            RalPos = e.GetPosition(this);
        }

        Point RalPos = new Point(0, 0);
        Point oldPos = new Point();
        bool ask_stop=false;

        XDocument Script = null;
        IEnumerable<XNode> Elements = null;
        RhythmTimer Rt;

        long Base = 0;
        long Offset = 0;

        string filename = "";
        string bmp_source = "";

        private void Window_Drop(object sender, DragEventArgs e)
        {

            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

            var name_pah = path.Split('\\');
            string bk = path + "\\" + name_pah[name_pah.Length - 1] + ".png";
            string music = path + "\\" + name_pah[name_pah.Length - 1] + ".aac";
            string script = path + "\\" + name_pah[name_pah.Length - 1] + ".xml";
            filename = script.Clone().ToString();
            bmp_source= bk.Clone().ToString();
            Script = XDocument.Load(script);
            Elements = Script.Root.Nodes();

            BackGround.Lock();

            using (Bitmap sc = new Bitmap(bk))
                graphics.DrawImage(sc, new Rectangle(new System.Drawing.Point(0, 0), new System.Drawing.Size(1280, 720)), new Rectangle(new System.Drawing.Point(0, 0), sc.Size), GraphicsUnit.Pixel);

            BackGround.AddDirtyRect(new Int32Rect(0, 0, 1280, 720));
            BackGround.Unlock();

           
            Rt = new RhythmTimer();
            Rt.QuarterBeats += (b, o) =>
            {
                Base = b;
                Offset = o;
            };

            foreach (XElement i in Elements)
            {
                if (i.Name == "head")
                {
                    Rt.BPM = Convert.ToInt32(i.Attribute("BPM").Value.ToString(), 10);
                    break;
                }
            }
            var Handle = new WindowInteropHelper(this).Handle;
            new Thread(() =>
            {
                AudioFramly music_player = new AudioFramly(music);
                music_player.Decode();
                AudioFramly.waveInit(Handle, music_player.Out_channels, music_player.Out_sample_rate, music_player.Bit_per_sample, music_player.Out_buffer_size);
                Rt.Start();
                foreach (var i in music_player.abits)
                {
                    if (!ask_stop)
                    {
                        Rt.pn.WaitOne();
                        unsafe
                        {
                            try
                            {
                                Rt.Accuracy((i?.time_base).Value);
                                AudioFramly.waveWrite((byte*)i?.data, music_player.Out_buffer_size);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }

                        }
                    }
                    Marshal.FreeHGlobal((i?.data).Value);
                }
                AudioFramly.waveClose();
                Script = null;
                Rt?.Stop();
            })
            { IsBackground = true }.Start();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (Script == null) return;
            if (e.Key == Key.F1)
            {
                Rt?.pn?.Set();
            }
            else

            if (e.Key == Key.F2)
            {
                if (((XElement)Script.Root.LastNode).Name != "head")
                    Script.Root.LastNode.Remove();
            }else
            if (e.Key == Key.F5)
            {
                Script.Save(filename);
            }else
            if (e.Key == Key.F3)
            {
                Input input = new Input();
                input.BOX.Text = "";
                input.ShowDialog();

                if (!input.status) return;

                if (!IsNumberic(input.BOX.Text))
                {
                    MessageBox.Show("Error:Not a number!", "...", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                var new_ml = new XElement("MV",
                        new XAttribute("Time", Base.ToString() + ":" + Offset.ToString()),
                        new XAttribute("Rect", (-BK.Margin.Left).ToString() + ":" + (-BK.Margin.Top).ToString()+":"+ (-BK.Margin.Left + 1280).ToString() + ":" + (-BK.Margin.Top + 720).ToString()),
                        new XAttribute("Beat", Convert.ToDouble(input.BOX.Text).ToString())
                        ) ;

                Script.Root.Add(new_ml);
            }
            else
            if (e.Key == Key.W)
            {
                BackGround.Lock();


                var pos_m = Mouse.GetPosition(BK);
                pos_m.X /= 2d;
                pos_m.Y /= 2d;
                BackGround.Lock();

                graphics.DrawEllipse(Pens.Blue, new Rectangle(new System.Drawing.Point((int)(pos_m.X - 40), (int)(pos_m.Y - 40)), new System.Drawing.Size(80, 80)));

                BackGround.AddDirtyRect(new Int32Rect(0, 0, 1280, 720));
                BackGround.Unlock();
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Input input = new Input();
                    input.BOX.Text = "";
                    input.ShowDialog();

                    if (!input.status) return;

                    if (!IsNumberic(input.BOX.Text))
                    {
                        MessageBox.Show("Error:Not a number!", "...", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }


                    var new_ml = new XElement("KL",
                        new XAttribute("Time", Base.ToString() + ":" + Offset.ToString()),
                        new XAttribute("Pos", Mouse.GetPosition(BK).X + ":" + Mouse.GetPosition(BK).Y),
                        new XAttribute("Key", "W"),
                        new XAttribute("Beat", Convert.ToDouble(input.BOX.Text).ToString())
                        );

                    Script.Root.Add(new_ml);
                }
                else
                {
                    var new_ml = new XElement("KS",
                       new XAttribute("Time", Base.ToString() + ":" + Offset.ToString()),
                       new XAttribute("Pos", Mouse.GetPosition(BK).X + ":" + Mouse.GetPosition(BK).Y),
                       new XAttribute("Key", "W")
                       ); 
                    Script.Root.Add(new_ml);
                }

                if (Keyboard.IsKeyUp(Key.LeftCtrl))
                {
                    Rt?.pn?.Set();
                }
            }
            else
            if (e.Key == Key.A)
            {
                var pos_m = Mouse.GetPosition(BK);
                pos_m.X /= 2d;
                pos_m.Y /= 2d;
                BackGround.Lock();

                graphics.DrawEllipse(Pens.Blue, new Rectangle(new System.Drawing.Point((int)(pos_m.X - 40), (int)(pos_m.Y - 40)), new System.Drawing.Size(80, 80)));

                BackGround.AddDirtyRect(new Int32Rect(0, 0, 1280, 720));
                BackGround.Unlock();
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Input input = new Input();
                    input.BOX.Text = "";
                    input.ShowDialog();

                    if (!input.status) return;

                    if (!IsNumberic(input.BOX.Text))
                    {
                        MessageBox.Show("Error:Not a number!", "...", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }


                    var new_ml = new XElement("KL",
                        new XAttribute("Time", Base.ToString() + ":" + Offset.ToString()),
                        new XAttribute("Pos", Mouse.GetPosition(BK).X + ":" + Mouse.GetPosition(BK).Y),
                        new XAttribute("Key", "A"),
                        new XAttribute("Beat", Convert.ToDouble(input.BOX.Text).ToString())
                        );

                    Script.Root.Add(new_ml);
                }
                else
                {
                    var new_ml = new XElement("KS",
                       new XAttribute("Time", Base.ToString() + ":" + Offset.ToString()),
                       new XAttribute("Pos", Mouse.GetPosition(BK).X + ":" + Mouse.GetPosition(BK).Y),
                       new XAttribute("Key", "A")
                       );
                    Script.Root.Add(new_ml);
                }
                BackGround.Lock();

                if (Keyboard.IsKeyUp(Key.LeftCtrl))
                {
                    Rt?.pn?.Set();
                }
            }
            else
            if (e.Key == Key.S)
            {
                BackGround.Lock();

                var pos_m = Mouse.GetPosition(BK);
                pos_m.X /= 2d;
                pos_m.Y /= 2d;
                BackGround.Lock();

                graphics.DrawEllipse(Pens.Blue, new Rectangle(new System.Drawing.Point((int)(pos_m.X - 40), (int)(pos_m.Y - 40)), new System.Drawing.Size(80, 80)));

                BackGround.AddDirtyRect(new Int32Rect(0, 0, 1280, 720));
                BackGround.Unlock();
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Input input = new Input();
                    input.BOX.Text = "";
                    input.ShowDialog();

                    if (!input.status) return;

                    if (!IsNumberic(input.BOX.Text))
                    {
                        MessageBox.Show("Error:Not a number!", "...", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }


                    var new_ml = new XElement("KL",
                        new XAttribute("Time", Base.ToString() + ":" + Offset.ToString()),
                        new XAttribute("Pos", Mouse.GetPosition(BK).X + ":" + Mouse.GetPosition(BK).Y),
                        new XAttribute("Key", "S"),
                        new XAttribute("Beat", Convert.ToDouble(input.BOX.Text).ToString())
                        );

                    Script.Root.Add(new_ml);
                }
                else
                {
                    var new_ml = new XElement("KS",
                       new XAttribute("Time", Base.ToString() + ":" + Offset.ToString()),
                       new XAttribute("Pos", Mouse.GetPosition(BK).X + ":" + Mouse.GetPosition(BK).Y),
                       new XAttribute("Key", "S")
                       );
                    Script.Root.Add(new_ml);
                }

                if (Keyboard.IsKeyUp(Key.LeftCtrl))
                {
                    Rt?.pn?.Set();
                }
            }
            else
            if (e.Key == Key.D)
            {
                BackGround.Lock();
                var pos_m = Mouse.GetPosition(BK);
                pos_m.X /= 2d;
                pos_m.Y /= 2d;
                BackGround.Lock();

                graphics.DrawEllipse(Pens.Blue, new Rectangle(new System.Drawing.Point((int)(pos_m.X - 40), (int)(pos_m.Y - 40)), new System.Drawing.Size(80, 80)));

                BackGround.AddDirtyRect(new Int32Rect(0, 0, 1280, 720));
                BackGround.Unlock();
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Input input = new Input();
                    input.BOX.Text = "";
                    input.ShowDialog();

                    if (!input.status) return;

                    if (!IsNumberic(input.BOX.Text))
                    {
                        MessageBox.Show("Error:Not a number!", "...", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }


                    var new_ml = new XElement("KL",
                        new XAttribute("Time", Base.ToString() + ":" + Offset.ToString()),
                        new XAttribute("Pos", Mouse.GetPosition(BK).X + ":" + Mouse.GetPosition(BK).Y),
                        new XAttribute("Key", "D"),
                        new XAttribute("Beat", Convert.ToDouble(input.BOX.Text).ToString())
                        );

                    Script.Root.Add(new_ml);
                }
                else
                {
                    var new_ml = new XElement("KS",
                       new XAttribute("Time", Base.ToString() + ":" + Offset.ToString()),
                       new XAttribute("Pos", Mouse.GetPosition(BK).X + ":" + Mouse.GetPosition(BK).Y),
                       new XAttribute("Key", "D")
                       );
                    Script.Root.Add(new_ml);
                }

                if (Keyboard.IsKeyUp(Key.LeftCtrl))
                {
                    Rt?.pn?.Set();
                }


            }
            
            if (e.Key == Key.F12)
            {

                using (Bitmap sc = new Bitmap(bmp_source))
                    graphics.DrawImage(sc, new Rectangle(new System.Drawing.Point(0, 0), new System.Drawing.Size(1280, 720)), new Rectangle(new System.Drawing.Point(0, 0), sc.Size), GraphicsUnit.Pixel);

                BackGround.AddDirtyRect(new Int32Rect(0, 0, 1280, 720));
                BackGround.Unlock();

            }

        }
        private bool IsNumberic(string oText)
        {
            try
            {
                _ = Convert.ToDouble(oText);
                return true;
            }
            catch
            {
                return false;
            }
        }
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
        public ManualResetEvent pn = new ManualResetEvent(false);
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
                if (Timebase % 4 == 0) { 
                    QuarterBeats?.DynamicInvoke(new object[2] { Timebase / (int)RhythmType, Timebase % (int)RhythmType });
                    pn.Reset();
                }
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
