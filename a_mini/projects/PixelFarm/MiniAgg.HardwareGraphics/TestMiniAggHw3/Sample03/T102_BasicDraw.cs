﻿
#region Using Directives

using System;
using OpenTK.Graphics.ES20;
using Mini;
#endregion

using PixelFarm.DrawingGL;
namespace OpenTkEssTest
{
    [Info(OrderCode = "102")]
    [Info("T102_BasicDraw")]
    public class T102_BasicDraw : PrebuiltGLControlDemoBase
    {
        CanvasGL2d canvas2d;
        protected override void OnInitGLProgram(object sender, EventArgs args)
        {
            //--------------------------------------------------------------------------------
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.ClearColor(1, 1, 1, 1);
            //setup viewport size
            int max = Math.Max(this.Width, this.Height);
            canvas2d = new CanvasGL2d(max, max);
            //square viewport
            GL.Viewport(0, 0, max, max);
        }
        protected override void DemoClosing()
        {
            canvas2d.Dispose();
        }
        protected override void OnGLRender(object sender, EventArgs args)
        {
            //Test1();
            Test2();
        }
        void Test1()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            canvas2d.StrokeColor = PixelFarm.Drawing.Color.Blue;
            //line
            canvas2d.DrawLine(50, 50, 200, 200);
            //--------------------------------------------
            //rect
            canvas2d.DrawRect(2.5f, 1.5f, 50, 50);
            canvas2d.FillRect(PixelFarm.Drawing.Color.Green, 50, 50, 50, 50);
            //--------------------------------------------

            //circle & ellipse
            canvas2d.DrawCircle(100, 100, 25);
            canvas2d.DrawEllipse(200, 200, 25, 50);
            canvas2d.FillCircle(PixelFarm.Drawing.Color.OrangeRed, 100, 400, 25);
            canvas2d.FillEllipse(PixelFarm.Drawing.Color.OrangeRed, 200, 400, 25, 50);
            //--------------------------------------------
            //polygon
            float[] polygon1 = new float[]{
                50,200,
                250,200,
                125,350
            };
            canvas2d.DrawPolygon(polygon1, 3);
            float[] polygon2 = new float[]{
                250,400,
                450,400,
                325,550
            };
            canvas2d.FillPolygon(PixelFarm.Drawing.Color.Green, polygon2);
            //--------------------------------------------
            miniGLControl.SwapBuffers();
        }
        void Test2()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            canvas2d.SmoothMode = CanvasSmoothMode.Smooth;
            canvas2d.StrokeColor = PixelFarm.Drawing.Color.Blue;
            //line
            canvas2d.DrawLine(50, 50, 200, 200);
            canvas2d.DrawRect(10, 10, 50, 50);
            canvas2d.FillRect(PixelFarm.Drawing.Color.Green, 100, 100, 50, 50);
            ////polygon
            float[] polygon1 = new float[]{
                50,200,
                250,200,
                125,350
            };
            canvas2d.DrawPolygon(polygon1, 3 * 2);
            float[] polygon2 = new float[]{
                250,400,
                450,400,
                325,550
            };
            canvas2d.FillPolygon(PixelFarm.Drawing.Color.Green, polygon2);
            canvas2d.StrokeColor = PixelFarm.Drawing.Color.Green;
            canvas2d.DrawPolygon(polygon2, 3 * 2);
            //--------------------------------------------
            canvas2d.DrawCircle(100, 100, 25);
            canvas2d.DrawEllipse(200, 200, 25, 50);
            //

            canvas2d.FillCircle(PixelFarm.Drawing.Color.OrangeRed, 100, 400, 25);
            canvas2d.StrokeColor = PixelFarm.Drawing.Color.OrangeRed;
            canvas2d.DrawCircle(100, 400, 25);
            //
            canvas2d.FillEllipse(PixelFarm.Drawing.Color.OrangeRed, 200, 400, 25, 50);
            canvas2d.StrokeColor = PixelFarm.Drawing.Color.OrangeRed;
            canvas2d.DrawEllipse(200, 400, 25, 50);
            miniGLControl.SwapBuffers();
        }
    }
}
