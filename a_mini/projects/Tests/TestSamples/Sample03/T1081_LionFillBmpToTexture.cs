﻿//MIT, 2014-2016,WinterDev

using System;
using PixelFarm.Drawing;
using Mini;
using PixelFarm.DrawingGL;
using PixelFarm.Agg;
namespace OpenTkEssTest
{
    [Info(OrderCode = "108.1")]
    [Info("T1081_LionFillBmpToTexture")]
    public class T1081_LionFillBmpToTexture : DemoBase
    {
        //***
        //software-based bitmap cache
        //this example:
        //we render the lion with Agg (software-based)
        //then copy pixel buffer to gl texture
        //and render the texture the bg surface 
        //***

        //---------------------------

        ActualImage aggImage;
        ImageGraphics2D imgGfx2d;
        AggCanvasPainter aggPainter;

        //---------------------------
        RenderSurface canvas2d;
        SpriteShape lionShape;
        GLCanvasPainter painter;

        GLBitmap glBmp;
        protected override void OnGLContextReady(RenderSurface canvasGL, GLCanvasPainter painter)
        {
            this.canvas2d = canvasGL;
            this.painter = painter;
        }
        protected override void OnReadyForInitGLShaderProgram()
        {
            lionShape = new SpriteShape();
            lionShape.ParseLion();
            RectD lionBounds = lionShape.Bounds;
            //-------------
            aggImage = new ActualImage((int)lionBounds.Width, (int)lionBounds.Height, PixelFarm.Agg.PixelFormat.ARGB32);
            imgGfx2d = new ImageGraphics2D(aggImage);
            aggPainter = new AggCanvasPainter(imgGfx2d);


            DrawLion(aggPainter, lionShape, lionShape.Path.Vxs);
            //convert affImage to texture 
            glBmp = DemoHelper.LoadTexture(aggImage);
        }
        protected override void DemoClosing()
        {
            canvas2d.Dispose();
        }
        static void DrawLion(CanvasPainter p, SpriteShape shape, VertexStore myvxs)
        {
            int j = shape.NumPaths;
            int[] pathList = shape.PathIndexList;
            Color[] colors = shape.Colors;
            for (int i = 0; i < j; ++i)
            {
                p.FillColor = colors[i];
                p.Fill(new VertexStoreSnap(myvxs, pathList[i]));
            }
        }
        protected override void OnGLRender(object sender, EventArgs args)
        {
            canvas2d.SmoothMode = CanvasSmoothMode.Smooth;
            canvas2d.StrokeColor = PixelFarm.Drawing.Color.Blue;
            canvas2d.ClearColorBuffer();
            //-------------------------------
            canvas2d.DrawImage(glBmp, 0, 600);
            SwapBuffers();
        }
    }
}

