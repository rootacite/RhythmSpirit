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

using WICBitmap = SharpDX.WIC.Bitmap;
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
using ImageSource = ShinenginePlus.DrawableControls.ImageSource;
using Image = SharpDX.Direct2D1.Image;
using SharpDX.DirectWrite;
using TextAlignment = SharpDX.DirectWrite.TextAlignment;
using System.IO;
using LinearGradientBrush = SharpDX.Direct2D1.LinearGradientBrush;

namespace ShinenginePlus
{

    public static class DTExtension
    {
        static public bool isPaused = false;
        public static void Change(this DrawableText Text, int Raise)
        {
            int n_combo = Convert.ToInt32(Text.text.Split('\n')[0]);

            n_combo += Raise;

            Text.text = n_combo.ToString() + "\n" + "Combo";

            var old = Text.Color;
            old.A = 1f;
            Text.Color = old;
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class MaskedRenderableImage : RenderableImage
    {
      


        public MaskedRenderableImage(ImageSource im, DeviceContext dc) : base(im, dc)
        {
          
        }

        public override void Render()
        {
            if (Source.Updated)
            {
                _Pelete?.Dispose();
                _Pelete = Source.Output;
            }
            


            using (SharpDX.Direct2D1.Layer m_layer = new SharpDX.Direct2D1.Layer(HostDC, new SharpDX.Size2F(HostDC.Size.Width, HostDC.Size.Height)))  
            using (D2DBitmap loadBp = new D2DBitmap(HostDC, Size,
                 new BitmapProperties1(HostDC.PixelFormat, HostDC.DotsPerInch.Width, HostDC.DotsPerInch.Height, BitmapOptions.Target | BitmapOptions.CannotDraw)),
                 mask = new D2DBitmap(HostDC, Size,
              new BitmapProperties1(HostDC.PixelFormat, HostDC.DotsPerInch.Width, HostDC.DotsPerInch.Height, BitmapOptions.None)))
            {
                HostDC.EndDraw();
                var old_target = HostDC.Target;
                HostDC.Target = loadBp;

                HostDC.BeginDraw();


                HostDC.Clear(new RawColor4(1,1,1,0.25f));
                var RCM = DarkCurtain.Range.ToArray();
                foreach (var i in RCM)
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
                            { Color = new RawColor4(1, 1, 1, 1), Position = 0f },
                            new SharpDX.Direct2D1.GradientStop
                            { Color = new RawColor4(1, 1, 1, 0f), Position = 1f }
                        });

                    using (RadialGradientBrush b = new RadialGradientBrush(HostDC, rp, gp))
                        HostDC.FillEllipse(i, b);

                }
                HostDC.EndDraw();

                HostDC.Target = old_target;
                HostDC.BeginDraw();

                if (old_target as D2DBitmap == null)
                    old_target.Dispose();

                mask.CopyFromBitmap(loadBp);

                using (BitmapBrush br = new BitmapBrush(HostDC, mask))
                {

                    LayerParameters lp = new LayerParameters()
                    {
                        Opacity = 1f,
                        ContentBounds = new RawRectangleF(0, 0, HostDC.Size.Width, HostDC.Size.Height),
                        LayerOptions = LayerOptions.None,
                        MaskAntialiasMode = AntialiasMode.PerPrimitive,
                        OpacityBrush = br
                    };
                    HostDC.PushLayer(ref lp, m_layer);
                    if(_Pelete?.IsDisposed==false)
                    using (Image PrepairedImage = Output(HostDC))
                        HostDC.DrawImage(PrepairedImage, new RawVector2(Position.X, Position.Y), null, SharpDX.Direct2D1.InterpolationMode.Linear, CompositeMode.SourceOver);

                    HostDC.PopLayer();
                }
            }

        }
    }
    public partial class MainWindow : Window
    {
        string title_set_old = "";
        bool onDeath = false;
        int uo = 0;
        List<Key> DownedKeys = new List<Key>();
        string[] pargs = Environment.GetCommandLineArgs();
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (DownedKeys.Contains(e.Key)) 
                e.Handled = true;
            else
                DownedKeys.Add(e.Key);
            base.OnKeyDown(e);
            uo++;
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            var bs = DownedKeys.ToArray();
            for (int i = 0; i < bs.Length; i++)
            {
                if (bs[i] == e.Key)
                {
                    DownedKeys.RemoveAt(i);
                }
            }
        }
        static int update_time = 0;
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
        GroupLayer TitleBar;
        GroupLayer OpearArea;
        DrawableText title_set;
        RenderableImage Dify;
        InteractiveObject pause_button;
        static public BloodBar bdbar;
        DrawableText RankBar;

        ProcessBar PB;
        static public DrawableText combo;
        static public DrawableText mark;
        public DrawResultW DrawProc(DeviceContext dc)
        {
            try
            {
              
                    dc.Clear(new RawColor4(1, 1, 1, 1));

                    BackGround.Render();
                
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            return DrawResultW.Commit;
        }
        public MainWindow()
        {
            InitializeComponent();
            ShinenginePlus.Media.FFmpegBinariesHelper.RegisterFFmpegBinaries();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            pn.Set();
            pv.Set();
            WindowHandle = new WindowInteropHelper(this).Handle;



            DX = new Direct2DWindow(new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), WindowHandle, false) { AskedFrames = SharedSetting.AskedFrame };
            BackGround = new BackGroundLayer(
                new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight),
                this,
                new RawRectangleF(0, 0, (float)this.ActualWidth, (float)this.ActualHeight),DX.DC)
            {
                
                Color = new RawColor4(1, 1, 1, 0),
                Range = new RawRectangleF(0, 0, (float)this.ActualWidth, (float)this.ActualHeight)
            };

             void Proc_Update()
            {

                while (true)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    try
                    {
                        BackGround.Update();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }

                    sw.Stop();


                    decimal time = sw.ElapsedTicks / (decimal)Stopwatch.Frequency * 1000;
                    decimal wait_time = 1000.0M / 60M - time;

                    if (wait_time < 0)
                    {
                        wait_time = 0;
                    }

                    Thread.Sleep((int)wait_time);
                }


            }
            Thread updateTimer = new Thread(Proc_Update)
            { IsBackground = true };



            DX.DrawProc += DrawProc;


            OpearArea = new GroupLayer(BackGround, new SharpDX.Size2(2560, 1440), DX.DC) { Range = new RawRectangleF(0, 0, 2560, 1440) };
            OpearArea.OutPutRange = new RawRectangleF(0, 0, 1280, 720);
            OpearArea.Color = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 1);

            //  img = new DrawingImage("assets\\title.png");
            img = new BitmapImage("assets\\title.png");
           
            bk1 = new MaskedRenderableImage(img,DX.DC) { Position = new System.Drawing.Point(0, 0) };
            bk1.Size = OpearArea.Size;
            bk1.PushTo(OpearArea);

            TitleBar = new GroupLayer(BackGround, new SharpDX.Size2(1280, 33), DX.DC);
            TitleBar.OutPutRange = new RawRectangleF(0, 0, 0, 30);
            TitleBar.Range = new RawRectangleF(0, 0, 0, 33);
            TitleBar.Color = new SharpDX.Mathematics.Interop.RawColor4(1, 1, 1, 0f);

            TitleBar.AddUpdateProcess(()=> 
            {
                var o_old = TitleBar.OutPutRange;
                var r_old = TitleBar.Range;
                var c_old = TitleBar.Color;

                o_old.Right += 16;
                r_old.Right += 16;
                c_old.A += 0.00625f;

                TitleBar.OutPutRange = o_old;
                TitleBar.Range = r_old;
                TitleBar.Color = c_old;
                if (TitleBar.Range.Right == 1280)
                    return false;
                return true;
            });

            pause_button = new InteractiveObject(new BitmapImage("assets\\pause.png"), DX.DC);
            pause_button.Size = new SharpDX.Size2(22,22);
            pause_button.Position = new System.Drawing.Point(1230, 2);
            pause_button.PushTo(TitleBar, this);
            pause_button.MouseUp += (e, v) => 
            {
                OpearArea.Freezing = !OpearArea.Freezing;
                if (OpearArea.Freezing) { 
                    sb.Pause();
                    title_set_old = title_set.text;
                    title_set.text = "Pause";
                    OpearArea.SetEffect((i, d) =>
                    {
                        SharpDX.Direct2D1.Effects.GaussianBlur Gaussian = new SharpDX.Direct2D1.Effects.GaussianBlur(d);

                        Gaussian.SetInput(0, i, new RawBool());
                        Gaussian.StandardDeviation = 15.0f;

                        return Gaussian;
                    });
                }
                else {
                    title_set.text = title_set_old;
                    sb.Resume();
                    OpearArea.SetEffect(null);
                }
            };
            pause_button.MouseEnter += (e, b) =>
            {
                (e as InteractiveObject).AddUpdateProcess(()=> 
                {
                    if((e as InteractiveObject).isInArea&& (e as InteractiveObject).Opacity > 0.3f)
                    {
                        (e as InteractiveObject).Opacity -= 0.05f;
                        return true;
                    }
                    return false;
                });
            };
            pause_button.MouseLeft += (e, b) =>
            {
                (e as InteractiveObject).AddUpdateProcess(() =>
                {
                    if (!(e as InteractiveObject).isInArea && (e as InteractiveObject).Opacity < 1f)
                    {
                        (e as InteractiveObject).Opacity += 0.05f;
                        return true;
                    }
                    return false;
                });
            };

            bdbar = new BloodBar(DX.DC);

            RankBar = new DrawableText(SharedSetting.GetRank(), SharedSetting.Font, 24, DX.DC); ;
            RankBar.Color = new RawColor4(0.85f, 0.85f, 0.85f, 0.75f);
            RankBar.Range = new RawRectangleF(240, 40, 400, 60);
            RankBar.ParagraphAlignment = ParagraphAlignment.Center;
            RankBar.TextAlignment = TextAlignment.Center;

            title_set = new DrawableText("", SharedSetting.Font, 22, DX.DC);
            title_set.Color = new RawColor4(0.1f, 0.1f, 0.1f, 1);
            title_set.Range = new RawRectangleF(0, 0, 1280, 30);
            title_set.ParagraphAlignment = ParagraphAlignment.Center;
            title_set.TextAlignment = TextAlignment.Center;


            title_set.PushTo(TitleBar);

            mark = new DrawableText("", SharedSetting.Font, 22, DX.DC);
            mark.Color = new RawColor4(1f, 0.1f, 0.1f, 1);
            mark.Range = new RawRectangleF(20, 0, 1250, 30);

            mark.PushTo(TitleBar);

            combo = new DrawableText("0\nCombo", SharedSetting.Font, 35, DX.DC);
            combo.Color = new RawColor4(0.85f, 0.85f, 0.85f, 0);
            combo.Range = new RawRectangleF(1050, 260, 1250, 360);
            combo.ParagraphAlignment = ParagraphAlignment.Center;
            combo.TextAlignment = TextAlignment.Center;
            combo.AddUpdateProcess(()=> 
            {
                var old = combo.Color;
                if (old.A > 0)
                    old.A -= 0.02f;
                combo.Color = old;
                return true;
            });


            PB = new ProcessBar(DX.DC);
            PB.Position = new System.Drawing.Point(0, 30);
            PB.PixelLong = 1280;
            PB.Process = 0f;
            PB.PushTo(TitleBar);

            OpearArea.PushTo(BackGround);
            TitleBar.PushTo(BackGround); 
            combo.PushTo(BackGround);
            bdbar.PushTo(BackGround);
            RankBar.PushTo(BackGround);

            DX.Run();
            updateTimer.Start();

            if (pargs.Length > 1)
            {
                StartStoryBoard(pargs[1]);
            }
        }
        MaskedRenderableImage bk1;
        static public ImageSource img = null;
        RhythmTimer sb;
        ManualResetEvent pn = new ManualResetEvent(false);
        ManualResetEvent pv = new ManualResetEvent(false);
        bool ask_stop = false;
        async public void StartStoryBoard(string path)
        {
            if (path[path.Length - 1] == '\\')
            {
                path = path.Substring(0, path.Length - 1);
            }
            sb?.Stop();
            ask_stop = true;
            pv.WaitOne();
            ask_stop = false;
            var name_pah = path.Split('\\');

            string bk = path + "\\" + name_pah[name_pah.Length - 1] + ".png";
            string music = path + "\\" + name_pah[name_pah.Length - 1] + ".aac";
            string script = path + "\\" + name_pah[name_pah.Length - 1] + ".xml";
            string bkv = path + "\\" + name_pah[name_pah.Length - 1] + ".mpg";
            sb = new RhythmTimer() { };

            if (File.Exists(bk))
                (img as BitmapImage).ReLoad(bk);
            else
            {
                var imgv = new VideoSource(bkv);
                bk1.Source = imgv;

                (img as BitmapImage).Dispose();
                img = imgv;
                await imgv.DecodeAsync();

                new Thread(async () =>
                {
                    while (!ask_stop)
                    {
                        while (OpearArea.Freezing) Thread.Sleep(1);
                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        if ((img as VideoSource).Position - time_save > 0.1d)
                        {
                            goto endDecode;
                        }

                        if ((img as VideoSource).Position - time_save < -0.1d)
                        {
                            await (imgv as VideoSource).DecodeAsync();
                        }

                        await (imgv as VideoSource).DecodeAsync();
                        endDecode:
                        sw.Stop();


                        decimal time = sw.ElapsedTicks / (decimal)Stopwatch.Frequency * 1000;
                        decimal wait_time = 1000.0M / 30M - time;

                        if (wait_time < 0)
                        {
                            wait_time = 0;
                        }

                        Thread.Sleep((int)wait_time);
                    }
                })
                { IsBackground = true }.Start();


            }


            title_set.text = name_pah[name_pah.Length - 1];
            mark.text = "0";

            #region Keys
            void KP(int x, int y, Key key, string info, double speed = 1)
            {
                speed = 60d / sb.BPM;
                new KeyboardSinglePoint(DX.DC, OpearArea, DX, new System.Drawing.Point(x, y), this, key, info, speed);
            }
            void KL(int x, int y, Key key, string info, double beat, double speed = 1)
            {
                speed = 60d / sb.BPM;
                new KeyboardLongPoint(DX.DC, OpearArea, DX, new System.Drawing.Point(x, y), this, 60d / sb.BPM * beat, key, info, speed);
            }
            void PL(int x, int y, double beat, double speed = 1)
            {
                speed = 60d / sb.BPM;
                new MouseLongPoint(DX.DC, OpearArea, DX, new System.Drawing.Point(x, y), this, 60d / sb.BPM * beat, speed);
            }
            void PP(int x, int y, double speed = 1)
            {
                speed = 60d / sb.BPM;
                new MouseSinglePoint(DX.DC, OpearArea, DX, new System.Drawing.Point(x, y), this, speed);
            }
            #endregion
            AudioFramly music_player = new AudioFramly(music);
            music_player.Decode();

            XDocument script_obj = XDocument.Load(script);
            var des = script_obj.Root.Nodes();
            List<RhyStep> rs = new List<RhyStep>();

            foreach (XElement i in des)
            {
                if (i.Name == "head")
                {
                    sb.BPM = Convert.ToInt32(i.Attribute("BPM").Value.ToString(), 10);
                    string Dy = i.Attribute("Difficulty").Value.ToString();
                    BitmapImage bim;
                    if (Dy == "Easy") { SharedSetting.Difficulty = Difficulty.Easy; bim = new BitmapImage("assets\\easy.png"); }
                    else if (Dy == "Normal") { SharedSetting.Difficulty = Difficulty.Normal; bim = new BitmapImage("assets\\normal.png"); }
                    else { SharedSetting.Difficulty = Difficulty.Hard; bim = new BitmapImage("assets\\hard.png"); }

                   
                    Dify = new RenderableImage(bim,DX.DC);
                    Dify.Position = new System.Drawing.Point(611, 35);
                    Dify.PushTo(BackGround);
                    continue;
                }
                int b = Convert.ToInt32(i.Attribute("Time").Value.ToString().Split(":")[0], 10);
                int o = Convert.ToInt32(i.Attribute("Time").Value.ToString().Split(":")[1], 10);
                if (i.Name == "MV")
                {
                    var reccct_array = i.Attribute("Rect").Value.ToString().Split(":");

                    double t = Convert.ToDouble(i.Attribute("Beat").Value.ToString());

                    rs.Add(new RhyStep() { Type = typeof(Point), Base = b, Offset = o, Time = t, Rect = new RawRectangleF(Convert.ToInt32(reccct_array[0]), Convert.ToInt32(reccct_array[1]), Convert.ToInt32(reccct_array[2]), Convert.ToInt32(reccct_array[3])) });
                    continue;
                }
                int x = Convert.ToInt32(i.Attribute("Pos").Value.ToString().Split(":")[0], 10);
                int y = Convert.ToInt32(i.Attribute("Pos").Value.ToString().Split(":")[1], 10);

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

                if (i.Name == "FL")
                {

                    rs.Add(new RhyStep() { Type = typeof(RippleEffect), Position = new Point(x, y), Base = b, Offset = o });
                }
            }

            sb.QuarterBeats += (c, b) =>
            {
                try
                {
                    RankBar.text = SharedSetting.GetRank();
                    if ((img as VideoSource)?.FrameRate != null ? (int)((img as VideoSource)?.FrameRate) == 29 : false)
                    {
                        if (Math.Abs((img as VideoSource).Position - time_save) > 0.1d)
                        {
                            // (img as VideoSource).Position = time_save;
                            Debug.WriteLine(((img as VideoSource).Position - time_save).ToString());
                        }
                    }
                    if (bdbar.Per <= 0f&&!onDeath)
                    {
                        onDeath = true;
                        sb.Pause();
                        OpearArea.Freezing = true;
                        float ff = 1f;
                        BackGround.Color = new RawColor4(0,0,0,1);
                        BackGround.SetEffect(
                            (i, e) =>
                            {
                                if (ff > 0)
                                {
                                    var blEf = new SharpDX.Direct2D1.Effect(e, SharpDX.Direct2D1.Effect.Opacity);

                                    blEf.SetInput(0, i, new RawBool());
                                    blEf.SetValue(0, ff -= 0.02f);

                                    return blEf;
                                }
                                else
                                {
                                    var blEf = new SharpDX.Direct2D1.Effect(e, SharpDX.Direct2D1.Effect.Opacity);

                                    blEf.SetInput(0, i, new RawBool());
                                    blEf.SetValue(0, ff);

                                   OpearArea.Pop();
                                    TitleBar.Pop();
                                    bdbar.PopFrom();
                                    RankBar.PopFrom();
                                    Dify?.PopFrom();

                                    var yad = new DrawableText("", SharedSetting.Font, 85, DX.DC);
                                    yad.Color = new RawColor4(1f, 1f, 1f, 1);
                                    yad.Range = new RawRectangleF(340, 150, 940, 450);
                                    yad.ParagraphAlignment = ParagraphAlignment.Center;
                                    yad.TextAlignment = TextAlignment.Center;

                                    yad.PushTo(BackGround);
                                    yad.PushString("You are Dead.");
                                    int width = 115, height = 45;
                                    GDIBitmap gDIBitmap = new GDIBitmap(new System.Drawing.Size(width, height));
                                    gDIBitmap.Graphics.Clear(System.Drawing.Color.FromArgb(55, 255, 255, 255));
                                    gDIBitmap.Graphics.DrawRectangle(new System.Drawing.Pen(new System.Drawing.SolidBrush(System.Drawing.Color.White), 3.5f), new System.Drawing.Rectangle(0, 0, width, height));
                                    gDIBitmap.Graphics.DrawString("Exit", new System.Drawing.Font(SharedSetting.Font,25), new System.Drawing.SolidBrush(System.Drawing.Color.White),new System.Drawing.RectangleF(0,5f,115,35f),new System.Drawing.StringFormat() { LineAlignment=System.Drawing.StringAlignment.Center,Alignment= System.Drawing.StringAlignment.Center });


                                    var exit_button = new InteractiveObject(gDIBitmap, DX.DC);
                                    exit_button.Size = new SharpDX.Size2(width, height);
                                    exit_button.Position = new System.Drawing.Point((int)(640 - width / 2d), (int)(560 - height / 2d));
                                    exit_button.PushTo(BackGround, this);
                                    exit_button.MouseUp += (e, v) =>
                                    {
                                        this.Dispatcher.Invoke(()=> { this.Close(); });
                                    };
                                    exit_button.MouseEnter += (e, b) =>
                                    {
                                        (e as InteractiveObject).AddUpdateProcess(() =>
                                        {
                                            if ((e as InteractiveObject).isInArea && (e as InteractiveObject).Opacity > 0.3f)
                                            {
                                                (e as InteractiveObject).Opacity -= 0.05f;
                                                return true;
                                            }
                                            return false;
                                        });
                                    };
                                    exit_button.MouseLeft += (e, b) =>
                                    {
                                        (e as InteractiveObject).AddUpdateProcess(() =>
                                        {
                                            if (!(e as InteractiveObject).isInArea && (e as InteractiveObject).Opacity < 1f)
                                            {
                                                (e as InteractiveObject).Opacity += 0.05f;
                                                return true;
                                            }
                                            return false;
                                        });
                                    };

                                    BackGround.SetEffect((i,e)=> {
                                        var blEf = new SharpDX.Direct2D1.Effect(e, SharpDX.Direct2D1.Effect.Opacity);

                                        blEf.SetInput(0, i, new RawBool());
                                        blEf.SetValue(0, ff <= 1f ? ff += 0.02f : 1);

                                        return blEf;
                                    });
                                    return blEf;
                                }
                                
                            });

                        pause_button.PopFrom();
                    }

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
                        {
                            RhyStep mc = rs[0];
                            new Thread(() =>
                            {
                                Thread.Sleep((int)((60d / sb.BPM) * 1000d));
                                new RippleEffect(30, 400, new System.Drawing.Point((int)mc.Position.X, (int)mc.Position.Y), DX.DC).PushTo(OpearArea);
                            }).Start();
                        }
                        if (rs[0].Type == typeof(Point))
                        {
                            RhyStep mc = rs[0];
                            new Thread(() =>
                            {
                                Thread.Sleep((int)((60d / sb.BPM) * 1000d));
                                int time_frame = (int)((60d / sb.BPM * mc.Time) * 60d);
                                double left_change = (mc.Rect.Left - OpearArea.Range.Left) / time_frame;
                                double top_change = (mc.Rect.Top - OpearArea.Range.Top) / time_frame;
                                double right_change = (mc.Rect.Right - OpearArea.Range.Right) / time_frame;
                                double bottom_change = (mc.Rect.Bottom - OpearArea.Range.Bottom) / time_frame;

                                OpearArea.AddUpdateProcess(() =>
                                {
                                    var new_rect = OpearArea.Range;
                                    new_rect.Left += (float)left_change;
                                    new_rect.Top += (float)top_change;
                                    new_rect.Right += (float)right_change;
                                    new_rect.Bottom += (float)bottom_change;
                                    OpearArea.Range = new_rect;
                                    if (--time_frame > 0) return true;
                                    else return false;

                                });
                            }).Start();
                        }

                        rs.RemoveAt(0);
                        goto frame_start;
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }


            };
            sb.Start();
            TitleBar.Top();
            time_save = 0;
            double time_max = 0;
            IntPtr mem_pos;
            unsafe
            {
                byte* mem = (byte*)Marshal.AllocHGlobal(music_player.Out_buffer_size);
                for (int x = 0; x < music_player.Out_buffer_size; x++)
                {
                    *(mem + x) = 0;
                }

                mem_pos = (IntPtr)mem;
            }
            new Thread(() =>
            {
                pv.Reset();
                AudioFramly.waveInit(WindowHandle, music_player.Out_channels, music_player.Out_sample_rate, music_player.Bit_per_sample, music_player.Out_buffer_size);
                time_max = (music_player.abits[music_player.abits.Count - 1]?.time_base).Value;
                foreach (var i in music_player.abits)
                {
                   
                    if (!ask_stop)
                    {
                        unsafe
                        {
                            try
                            {
                                time_save = (i?.time_base).Value;

                                //  sb.Accuracy((i?.time_base).Value);
                                while (OpearArea.Freezing) {
                                    if (onDeath) goto endf;
                                    AudioFramly.waveWrite((byte*)mem_pos, music_player.Out_buffer_size);
                                }
                                AudioFramly.waveWrite((byte*)i?.data, music_player.Out_buffer_size);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }

                        }
                    }
                    endf:
                    Marshal.FreeHGlobal((i?.data).Value);
                }
                AudioFramly.waveClose();
                if (onDeath) return;
                ask_stop = true;
                sb?.Stop();
                pv.Set();
                this.Dispatcher.Invoke(()=> { this.Close(); });
               
            })
            { IsBackground = true }.Start();
            new Thread(() =>
            {
                while (!ask_stop)
                {
                    while (OpearArea.Freezing) Thread.Sleep(1);
                    sb.Accuracy(time_save);
                    PB.Process = time_save / time_max;
                    Thread.Sleep(100);
                }
            })
            { IsBackground = true }.Start();


        }
        double time_save = 0;
        private void Window_Drop(object sender, DragEventArgs e)
        {
            string fileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            StartStoryBoard(fileName);
        }
    }


    class PerfectInfo
    {
        RenderableImage ctl = null;
        BitmapImage img = null;

        public PerfectInfo(Layer layer, Direct2DWindow vm, System.Drawing.Point pos, int info,DeviceContext DC)
        {
            if (info == 0)
            { SharedSetting.PrefectCount += 1; img = new BitmapImage("assets\\perfect.png"); MainWindow.combo.Change(1); if (MainWindow.bdbar.Per+0.04d < 1) MainWindow.bdbar.Per += 0.02d; }
            else if (info == 1) { SharedSetting.GreatCount += 1; img = new BitmapImage("assets\\great.png"); MainWindow.combo.Change(1); if (MainWindow.bdbar.Per + 0.01d < 1) MainWindow.bdbar.Per += 0.01d; }
            else if (info == 2) { SharedSetting.BadCount += 1; img = new BitmapImage("assets\\bad.png"); MainWindow.combo.Change(0); }
            else { SharedSetting.MissCount += 1; img = new BitmapImage("assets\\miss.png"); MainWindow.combo.text = "0\nCombo";if(MainWindow.bdbar.Per >0) MainWindow.bdbar.Per -= 0.1d; }

            int mark_n = 0;
            try
            {
                mark_n = Convert.ToInt32(MainWindow.mark.text);
            }
            catch (Exception)
            {
                mark_n = 0;
            }
            if (info == 0)
                mark_n += 20;
            else if (info == 1) mark_n += 10;
            else if (info == 2) mark_n += 5;
            else mark_n -= 5;

            MainWindow.mark.text = mark_n.ToString();

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
    static class DarkCurtain
    {
        static DrawingImage Mask = new DrawingImage(new System.Drawing.Size(2560, 1440));

        static DarkCurtain()
        { 
        }

        static public List<SharpDX.Direct2D1.Ellipse> Range = new List<SharpDX.Direct2D1.Ellipse>();
        public static void AddUpdate(this List<SharpDX.Direct2D1.Ellipse> list, SharpDX.Direct2D1.Ellipse e)
        {
            list.Add(e);
        }

        public static void RemoveUpdate(this List<SharpDX.Direct2D1.Ellipse> list, SharpDX.Direct2D1.Ellipse e)
        {
            list.Remove(e);
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
        public RippleEffect(double time, double r, System.Drawing.Point pos, DeviceContext DC) : base(DC)
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

    sealed public class BloodBar : RenderableObject
    {
        public double Per { get; set; } = 1.0d;
        
        public BloodBar(DeviceContext DC) : base(DC) 
        {

        }

        public override void Render()
        {
            using (SolidColorBrush cb = new SolidColorBrush(HostDC, new RawColor4(1, 0, 0, 1)))
                HostDC.DrawRectangle(new RawRectangleF(10, 40, 220, 60), cb,2.5f);

            var gp = new SharpDX.Direct2D1.GradientStopCollection(
                       HostDC,
                       new SharpDX.Direct2D1.GradientStop[]
                       {
                            new SharpDX.Direct2D1.GradientStop
                            { Color = new RawColor4(1, 0, 0, 1), Position = 0f },
                            new SharpDX.Direct2D1.GradientStop
                            { Color = new RawColor4(0, 1, 0, 1), Position = 1f }
                       });

            if (Per >= 0)
                using (LinearGradientBrush lb = new LinearGradientBrush(HostDC, new LinearGradientBrushProperties() { StartPoint = new RawVector2(10, 40), EndPoint = new RawVector2(220, 60) }, gp))
            {
                HostDC.FillRectangle(new RawRectangleF(10, 40, (float)(10 +209* Per), 59), lb);
            }
            gp.Dispose();
        }
    }
}
