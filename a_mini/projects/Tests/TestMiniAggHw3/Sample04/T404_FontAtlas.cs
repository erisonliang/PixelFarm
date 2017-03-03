﻿//MIT, 2014-2016,WinterDev

using System;
using Mini;
using PixelFarm.DrawingGL;
using PixelFarm.Drawing.Fonts;
namespace OpenTkEssTest
{
    [Info(OrderCode = "404")]
    [Info("T404_FontAtlas")]
    public class T404_FontAtlas : DemoBase
    {
        CanvasGL2d canvas2d;
        bool resInit;
        GLBitmap msdf_bmp;
        GLCanvasPainter painter;
        System.Drawing.Bitmap totalImg;
        SimpleFontAtlas fontAtlas;
        public override void Init()
        {
            base.Init();
        }
        public override void OnSetupDemoGLContext(CanvasGL2d canvasGL, GLCanvasPainter painter)
        {
            this.canvas2d = canvasGL;
            this.painter = painter;
        }
        protected override void OnReadyForInitGLShaderProgram()
        {
             

            //--------------------- 
            string fontfilename = "d:\\WImageTest\\a_total.xml";
            var atlasBuilder = new Typography.Rendering.SimpleFontAtlasBuilder();
            fontAtlas = atlasBuilder.LoadFontInfo(fontfilename);

            totalImg = new System.Drawing.Bitmap("d:\\WImageTest\\a_total.png");

            var bmpdata = totalImg.LockBits(new System.Drawing.Rectangle(0, 0, totalImg.Width, totalImg.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, totalImg.PixelFormat);
            var buffer = new int[totalImg.Width * totalImg.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpdata.Scan0, buffer, 0, buffer.Length);
            totalImg.UnlockBits(bmpdata);
            var glyph = new Typography.Rendering.GlyphImage(totalImg.Width, totalImg.Height);
            glyph.SetImageBuffer(buffer, false);
            fontAtlas.TotalGlyph = glyph;
        }
        protected override void DemoClosing()
        {
            canvas2d.Dispose();
        }
        protected override void OnGLRender(object sender, EventArgs args)
        {
            canvas2d.SmoothMode = CanvasSmoothMode.Smooth;
            canvas2d.StrokeColor = PixelFarm.Drawing.Color.Blue;
            canvas2d.ClearColorBuffer();
            if (!resInit)
            {
                // msdf_bmp = LoadTexture(@"..\msdf_75.png");
                //msdf_bmp = LoadTexture(@"d:\\WImageTest\\a001_x1_66.png");
                msdf_bmp = DemoHelper.LoadTexture(totalImg);
                //msdf_bmp = LoadTexture(@"d:\\WImageTest\\a001_x1.png");
                //msdf_bmp = LoadTexture(@"d:\\WImageTest\\msdf_65.png");

                resInit = true;
            }

            painter.Clear(PixelFarm.Drawing.Color.White);
            //var f = painter.CurrentFont;

            //painter.DrawString("hello!", 0, 20);
            //canvas2d.DrawImageWithSubPixelRenderingMsdf(msdf_bmp, 200, 500, 15f);

            Typography.Rendering.TextureFontGlyphData glyphData;

            byte[] codepoint = System.Text.Encoding.UTF8.GetBytes("AB");
            fontAtlas.GetRectByCodePoint(codepoint[0], out glyphData);
            PixelFarm.Drawing.Rectangle r = ConvToRect(glyphData.Rect);
            //canvas2d.DrawSubImageWithMsdf(msdf_bmp, ref r, 100, 500);
            canvas2d.DrawSubImageWithMsdf(msdf_bmp, ref r, 100, 500);

            fontAtlas.GetRectByCodePoint(codepoint[1], out glyphData);
            PixelFarm.Drawing.Rectangle r2 = ConvToRect(glyphData.Rect);
            canvas2d.DrawSubImageWithMsdf(msdf_bmp, ref r2, 100 + r.Width - 10, 500);

            //full image
            canvas2d.DrawImage(msdf_bmp, 100, 300);
            SwapBuffers();
        }
        static PixelFarm.Drawing.Rectangle ConvToRect(Typography.Rendering.Rectangle r)
        {
            return PixelFarm.Drawing.Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
        }
    }

}

