using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using ShinenginePlus.DrawableControls;
using System.Windows;
using System.Windows.Input;
using Layer = ShinenginePlus.DrawableControls.Layer;

namespace ShinenginePlus
{
    static class KeyPics
    {
        static public SharpDX.WIC.Bitmap Left = Direct2DHelper.LoadBitmap("assets\\left.png");
        static public SharpDX.WIC.Bitmap Right = Direct2DHelper.LoadBitmap("assets\\right.png");
        static public SharpDX.WIC.Bitmap Up = Direct2DHelper.LoadBitmap("assets\\up.png");
        static public SharpDX.WIC.Bitmap Down = Direct2DHelper.LoadBitmap("assets\\down.png");
        static KeyPics()
        {

        }
    }
    class MouseSinglePoint
    {
        InteractiveObject btn = null;
        DrawingImage img = new DrawingImage(new System.Drawing.Size(138, 138));
        bool clicked = false;
        double frame = 90d;
        float opdivied = 0;

        public MouseSinglePoint(DeviceContext DC, Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, double speed = 1)
        {
            speed = 1 / speed;

            var area = new SharpDX.Direct2D1.Ellipse(new RawVector2(pos.X, pos.Y), 66, 66);
            pos.X -= 69;
            pos.Y -= 69;

            btn = new InteractiveObject(img, DC) { Position = pos };
           
            img.Update((s) =>
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
            });
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

                            DarkCurtain.Range.RemoveUpdate(area);
                            return false;
                        }
                        img.Update((s) =>
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
                        });
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

                    DarkCurtain.Range.RemoveUpdate(area);
                    return false;
                }
                img.Update((s) =>
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
                });
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

            DarkCurtain.Range.AddUpdate(area);
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

        public MouseLongPoint(DeviceContext DC, Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, double time, double speed = 1)
        {
            speed = 1 / speed;
            var area = new SharpDX.Direct2D1.Ellipse(new RawVector2(pos.X, pos.Y), 80, 80);
            time = time * 60d;
            pos.X -= 80;
            pos.Y -= 80;
            btn = new InteractiveObject(img, DC) { Position = pos };
           
            img.Update((s) =>
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
            });
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
                            DarkCurtain.Range.RemoveUpdate(area);
                            btn.PopFrom();
                            vm.CleanTasks.Add(btn);
                            vm.CleanTasks.Add(img);
                            if (!uped)
                                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3, DC);
                            return false;
                        }
                        img.Update((s) =>
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
                        });
                        return true;
                    });
                    return false;
                }
                if (frame > 0)
                    frame -= speed;
                else
                {
                    DarkCurtain.Range.RemoveUpdate(area);
                    btn.PopFrom();
                    vm.CleanTasks.Add(btn);
                    vm.CleanTasks.Add(img);
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3, DC);
                    return false;
                }
                img.Update((s) =>
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
                });
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

            DarkCurtain.Range.AddUpdate(area);
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

        public KeyboardSinglePoint(DeviceContext DC, Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, Key key, string info, double speed = 1)
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
         
           // img.Update();
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
                            DarkCurtain.Range.RemoveUpdate(area);
                            btn.PopFrom();
                            vm.CleanTasks.Add(btn);
                            vm.CleanTasks.Add(img);

                            w.KeyDown -= KD;
                            return false;
                        }
                        img.Update((s) =>
                        {
                            s.BeginDraw();
                            s.Clear(new RawColor4(1, 1, 1, 0));

                            var wfactory = new SharpDX.DirectWrite.Factory();
                            var wformat = new SharpDX.DirectWrite.TextFormat(wfactory, "黑体", 32);
                            wformat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
                            wformat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                            if (frame > 30)
                            {
                                double r = 132 - 100d * ((90d - frame) / 60d);
                                using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(1, 0.2f, 0, 1)), c = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.35f, 0.35f, 0.35f, 0.15f)))
                                {
                                    s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), (float)(r / 2), (float)(r / 2)), b, 4);
                                    if (!clicked)
                                    {
                                        s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), 8, 8), c);
                                        if (info == "W")
                                            using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Up))
                                                s.DrawBitmap(UpBitmap, new RawRectangleF(53, 53, 85, 85), 1f, BitmapInterpolationMode.Linear);
                                        else if (info == "A")
                                            using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Left))
                                                s.DrawBitmap(UpBitmap, new RawRectangleF(53, 53, 85, 85), 1f, BitmapInterpolationMode.Linear);
                                        else if (info == "S")
                                            using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Down))
                                                s.DrawBitmap(UpBitmap, new RawRectangleF(53, 53, 85, 85), 1f, BitmapInterpolationMode.Linear);
                                        else if (info == "D")
                                            using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Right))
                                                s.DrawBitmap(UpBitmap, new RawRectangleF(53, 53, 85, 85), 1f, BitmapInterpolationMode.Linear);
                                        else
                                            using (SolidColorBrush d = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.55f, 0.55f, 0.55f, 1)))
                                                s.DrawText(info, wformat, new RawRectangleF(53, 53, 85, 85), d);
                                    }
                                }
                            }
                            else
                            {
                                double r = 34 * (frame / 30d);
                                using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(1, 0.2f, 0, 1)))
                                    s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), (float)(r / 2), (float)(r / 2)), b);
                            }
                            wfactory.Dispose();
                            wformat.Dispose();
                            s.EndDraw();
                        });
                        return true;
                    });
                    return false;
                }
                if (frame > 0)
                    frame -= speed;
                else
                {
                    DarkCurtain.Range.RemoveUpdate(area);
                    btn.PopFrom();
                    vm.CleanTasks.Add(btn);
                    vm.CleanTasks.Add(img);
                    w.KeyDown -= KD;
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 69, pos.Y + 69), 3, DC);
                    return false;
                }
                img.Update((s) =>
                {
                    s.BeginDraw();
                    s.Clear(new RawColor4(1, 1, 1, 0));

                    var wfactory = new SharpDX.DirectWrite.Factory();
                    var wformat = new SharpDX.DirectWrite.TextFormat(wfactory, "黑体", 32);
                    wformat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
                    wformat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                    if (frame > 30)
                    {
                        double r = 132 - 100d * ((90d - frame) / 60d);
                        using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(1, 0.2f, 0, 1)), c = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.35f, 0.35f, 0.35f, 0.15f)))
                        {
                            s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), (float)(r / 2), (float)(r / 2)), b, 4);
                            if (!clicked)
                            {
                                s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), 8, 8), c);
                                if (info == "W")
                                    using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Up))
                                        s.DrawBitmap(UpBitmap, new RawRectangleF(53, 53, 85, 85), 1f, BitmapInterpolationMode.Linear);
                                else if (info == "A")
                                    using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Left))
                                        s.DrawBitmap(UpBitmap, new RawRectangleF(53, 53, 85, 85), 1f, BitmapInterpolationMode.Linear);
                                else if (info == "S")
                                    using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Down))
                                        s.DrawBitmap(UpBitmap, new RawRectangleF(53, 53, 85, 85), 1f, BitmapInterpolationMode.Linear);
                                else if (info == "D")
                                    using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Right))
                                        s.DrawBitmap(UpBitmap, new RawRectangleF(53, 53, 85, 85), 1f, BitmapInterpolationMode.Linear);
                                else
                                    using (SolidColorBrush d = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.55f, 0.55f, 0.55f, 1)))
                                        s.DrawText(info, wformat, new RawRectangleF(53, 53, 85, 85), d);
                            }
                        }
                    }
                    else
                    {
                        double r = 34 * (frame / 30d);
                        using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(1, 0.2f, 0, 1)))
                            s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(69, 69), (float)(r / 2), (float)(r / 2)), b);
                    }
                    wfactory.Dispose();
                    wformat.Dispose();
                    s.EndDraw();
                });
                return true;
            });
            w.KeyDown += KD;
            btn.PushTo(layer, w);
            DarkCurtain.Range.AddUpdate(area);
        }
        private void KD(object s, KeyEventArgs t)
        {

            if (t.Key != key)
            {
                return;
            }           
            if (clicked || t.Handled || frame == 0)
            {
                return;
            }
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
        public KeyboardLongPoint(DeviceContext DC, Layer layer, Direct2DWindow vm, System.Drawing.Point pos, UIElement w, double time, Key key, string info, double speed = 1)
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
           
        //    img.Update();
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
                            DarkCurtain.Range.RemoveUpdate(area);
                            btn.PopFrom();
                            vm.CleanTasks.Add(btn);
                            vm.CleanTasks.Add(img);
                            w.KeyDown -= KD;
                            w.KeyUp -= KU;
                            if (!uped)
                                new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3, DC);
                            return false;
                        }
                        img.Update((s) =>
                        {

                            s.BeginDraw();
                            s.Clear(new RawColor4(1, 1, 1, 0));
                            var wfactory = new SharpDX.DirectWrite.Factory();
                            var wformat = new SharpDX.DirectWrite.TextFormat(wfactory, "幼圆", 32);
                            wformat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
                            wformat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                            if (frame > 30)
                            {
                                double r = 110 - 100d * ((90d - frame) / 60d);
                                using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 1f, 0.8f, 1)), c = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.35f, 0.35f, 0.35f, 0.15f)))
                                {
                                    s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), (float)(r / 2), (float)(r / 2)), b, 4);
                                    if (!downed)
                                    {
                                        s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), 10, 10), c);

                                        if (info == "W")
                                            using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Up))
                                                s.DrawBitmap(UpBitmap, new RawRectangleF(62, 62, 98, 98), 1f, BitmapInterpolationMode.Linear);
                                        else if (info == "A")
                                            using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Left))
                                                s.DrawBitmap(UpBitmap, new RawRectangleF(62, 62, 98, 98), 1f, BitmapInterpolationMode.Linear);
                                        else if (info == "S")
                                            using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Down))
                                                s.DrawBitmap(UpBitmap, new RawRectangleF(62, 62, 98, 98), 1f, BitmapInterpolationMode.Linear);
                                        else if (info == "D")
                                            using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Right))
                                                s.DrawBitmap(UpBitmap, new RawRectangleF(62, 62, 98, 98), 1f, BitmapInterpolationMode.Linear);
                                        else
                                            using (SolidColorBrush d = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.55f, 0.55f, 0.55f, 1)))
                                                s.DrawText(info, wformat, new RawRectangleF(62, 62, 98, 98), d);
                                    }
                                }
                            }
                            else
                            {
                                double r = 18 * (frame / 30d);
                                using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 1f, 0.8f, 1)))
                                    s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), (float)(r / 2), (float)(r / 2)), b);
                            }
                            using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.6f, 0.6f, 0.6f, 0.4f)))
                                s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), 68f, 68f), b, 1);

                            wfactory.Dispose();
                            wformat.Dispose();
                            s.EndDraw();
                        });
                        return true;
                    });
                    return false;
                }
                if (frame > 0)
                    frame -= speed;
                else
                {
                    DarkCurtain.Range.RemoveUpdate(area);
                    btn.PopFrom();
                    vm.CleanTasks.Add(btn);
                    vm.CleanTasks.Add(img);
                    w.KeyDown -= KD;
                    w.KeyUp -= KU;
                    new PerfectInfo(layer, vm, new System.Drawing.Point(pos.X + 80, pos.Y + 80), 3, DC);
                    return false;
                }
                img.Update((s) =>
                {

                    s.BeginDraw();
                    s.Clear(new RawColor4(1, 1, 1, 0));
                    var wfactory = new SharpDX.DirectWrite.Factory();
                    var wformat = new SharpDX.DirectWrite.TextFormat(wfactory, "幼圆", 32);
                    wformat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
                    wformat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
                    if (frame > 30)
                    {
                        double r = 110 - 100d * ((90d - frame) / 60d);
                        using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 1f, 0.8f, 1)), c = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.35f, 0.35f, 0.35f, 0.15f)))
                        {
                            s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), (float)(r / 2), (float)(r / 2)), b, 4);
                            if (!downed)
                            {
                                s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), 10, 10), c);

                                if (info == "W")
                                    using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Up))
                                        s.DrawBitmap(UpBitmap, new RawRectangleF(62, 62, 98, 98), 1f, BitmapInterpolationMode.Linear);
                                else if (info == "A")
                                    using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Left))
                                        s.DrawBitmap(UpBitmap, new RawRectangleF(62, 62, 98, 98), 1f, BitmapInterpolationMode.Linear);
                                else if (info == "S")
                                    using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Down))
                                        s.DrawBitmap(UpBitmap, new RawRectangleF(62, 62, 98, 98), 1f, BitmapInterpolationMode.Linear);
                                else if (info == "D")
                                    using (SharpDX.Direct2D1.Bitmap UpBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(s, KeyPics.Right))
                                        s.DrawBitmap(UpBitmap, new RawRectangleF(62, 62, 98, 98), 1f, BitmapInterpolationMode.Linear);
                                else
                                    using (SolidColorBrush d = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.55f, 0.55f, 0.55f, 1)))
                                        s.DrawText(info, wformat, new RawRectangleF(62, 62, 98, 98), d);
                            }
                        }
                    }
                    else
                    {
                        double r = 18 * (frame / 30d);
                        using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0, 1f, 0.8f, 1)))
                            s.FillEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), (float)(r / 2), (float)(r / 2)), b);
                    }
                    using (SolidColorBrush b = new SharpDX.Direct2D1.SolidColorBrush(s, new RawColor4(0.6f, 0.6f, 0.6f, 0.4f)))
                        s.DrawEllipse(new SharpDX.Direct2D1.Ellipse(new RawVector2(80, 80), 68f, 68f), b, 1);

                    wfactory.Dispose();
                    wformat.Dispose();
                    s.EndDraw();
                });
                return true;
            });
            w.KeyDown += KD;
            w.KeyUp += KU;
            btn.PushTo(layer, w);

            DarkCurtain.Range.AddUpdate(area);
        }
        private void KU(object s, KeyEventArgs t)
        {
            if (t.Key != key)
            {
                return;
            }
            if (!downed||uped || t.Handled || frame == 0)
            {
                return;
            }
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

            if (t.Key != key)
            {
                return;
            }
            if (downed || t.Handled || frame == 0)
            {
                return;
            }
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
}