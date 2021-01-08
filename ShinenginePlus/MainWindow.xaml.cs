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

namespace Shinengine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        static IntPtr WindowHandle = (IntPtr)0;
        BackGroundLayer BackGround = null;

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
            }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowHandle = new WindowInteropHelper(this).Handle;

            Direct2DWindow DX = new Direct2DWindow(new System.Drawing.Size((int)this.ActualWidth, (int)this.ActualHeight), WindowHandle);
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

            this.MouseLeftButtonUp += (e, s) =>
            {
                var pos1 = s.GetPosition(this);
                new MouseSinglePoint(BackGround, DX, new System.Drawing.Point((int)pos1.X, (int)pos1.Y),this);
            };
        }

        class PerfectInfo
        {
            RenderableImage ctl = null;
            BitmapImage img = null;

            public PerfectInfo(Layer layer, Direct2DWindow vm, System.Drawing.Point pos)
            {
                img= new BitmapImage("assets\\perfect.png");
                pos.X -= img.PixelSize.Width / 2;
                pos.Y -= img.PixelSize.Height / 2;
                ctl = new RenderableImage(img);
                ctl.Position = pos;
                ctl.AddUpdateProcess(()=>
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
            int frame = 90;
            float opdivied = 0;

            public MouseSinglePoint(Layer layer, Direct2DWindow vm, System.Drawing.Point pos,UIElement w)
            {
                pos.X -= 69;
                pos.Y -= 69;
                btn = new InteractiveObject(img) { Position= pos };
                img.Proc += (s) =>
                {
                    s.BeginDraw();
                    s.Clear(new RawColor4(1, 1, 1, 0));

                    if (frame > 30)
                    {
                        double r = 132 - 100d * ((90 - frame) / 60d);
                        using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 0.2f, 1, 1)))
                            s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), (float)(r/2), (float)(r / 2)), b,6);
                    }
                    else
                    {
                        double r = 40 * (frame / 30d);
                        using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 0.2f, 1, 1)))
                            s.FillEllipse (new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), (float)(r / 2), (float)(r / 2)),b);
                    }
                    s.EndDraw();
                };
                img.Update();
                btn.AddUpdateProcess(()=> {
                    if (clicked)
                    {
                        btn.AddUpdateProcess(()=>
                        {
                            if (frame < 90)
                            {
                                frame += 4;
                                btn.Opacity -= opdivied;
                            }
                            else
                            {
                                btn.PopFrom();
                                vm.CleanTasks.Add(btn);
                                vm.CleanTasks.Add(img);
                                return false;
                            }
                            img.Update();
                            return true;
                        });
                        return false;
                    }
                    if (frame > 0)
                        frame--;
                    else
                    {
                        btn.PopFrom();
                        vm.CleanTasks.Add(btn);
                        vm.CleanTasks.Add(img);
                        return false;
                    }
                    img.Update();
                    return true;
                });
                btn.MouseDown += (e, t) =>
                {
                    if (clicked) return;
                    if (frame == 90)
                    {
                        opdivied = 1;
                    }
                    else
                        opdivied = 4 / (float)(90 - frame);
                    clicked = true;
                    if (frame > 25 && frame < 35) new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69));
                };
                btn.PushTo(layer,w);
            }
        }
    }
}
