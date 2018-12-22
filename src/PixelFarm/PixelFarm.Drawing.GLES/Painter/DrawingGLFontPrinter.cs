﻿//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
//
using PixelFarm.CpuBlit;
using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;
//
using Typography.TextLayout;
using Typography.OpenFont;


namespace PixelFarm.DrawingGL
{
    public class AggTextSpanPrinter : ITextPrinter
    {
        MemBitmap _memBmp;
        AggPainter _aggPainter;
        VxsTextPrinter _vxsTextPrinter;
        int _bmpWidth;
        int _bmpHeight;
        GLPainterContext _pcx;
        GLPainter _canvasPainter;
        LayoutFarm.OpenFontTextService _openFontTextServices;

        public AggTextSpanPrinter(GLPainter canvasPainter, int w, int h)
        {
            //this class print long text into agg canvas
            //then copy pixel buffer from aff canvas to gl-bmp
            //then draw the  gl-bmp into target gl canvas


            //TODO: review here
            _canvasPainter = canvasPainter;
            _pcx = canvasPainter.Canvas;
            _bmpWidth = w;
            _bmpHeight = h;

            _memBmp = new MemBitmap(_bmpWidth, _bmpHeight);
#if DEBUG
            _memBmp._dbugNote = "AggTextSpanPrinter.ctor";
#endif
            _aggPainter = AggPainter.Create(_memBmp);
            _aggPainter.FillColor = Color.Black;
            _aggPainter.StrokeColor = Color.Black;

            //set default1
            _aggPainter.CurrentFont = canvasPainter.CurrentFont;

            _openFontTextServices = new LayoutFarm.OpenFontTextService();
            _vxsTextPrinter = new VxsTextPrinter(_aggPainter, _openFontTextServices);
            _aggPainter.TextPrinter = _vxsTextPrinter;
        }
        public bool StartDrawOnLeftTop { get; set; }
        public Typography.Contours.HintTechnique HintTechnique
        {
            get => _vxsTextPrinter.HintTechnique;
            set => _vxsTextPrinter.HintTechnique = value;
        }
        public bool UseSubPixelRendering
        {
            get => _aggPainter.UseSubPixelLcdEffect;
            set => _aggPainter.UseSubPixelLcdEffect = value;
        }
        public void ChangeFont(RequestFont font)
        {
            _aggPainter.CurrentFont = font;
        }
        public void ChangeFillColor(Color fillColor)
        {
            //we use agg canvas to draw a font glyph
            //so we must set fill color for this
            _aggPainter.FillColor = fillColor;
        }
        public void ChangeStrokeColor(Color strokeColor)
        {
            //we use agg canvas to draw a font glyph
            //so we must set fill color for this
            _aggPainter.StrokeColor = strokeColor;
        }
        public void DrawString(char[] text, int startAt, int len, double x, double y)
        {


            if (this.UseSubPixelRendering)
            {
                //1. clear prev drawing result
                _aggPainter.Clear(Drawing.Color.FromArgb(0, 0, 0, 0));
                //aggPainter.Clear(Drawing.Color.White);
                //aggPainter.Clear(Drawing.Color.FromArgb(0, 0, 0, 0));
                //2. print text span into Agg Canvas
                _vxsTextPrinter.DrawString(text, startAt, len, 0, 0);
                //3.copy to gl bitmap
                //byte[] buffer = PixelFarm.Agg.ActualImage.GetBuffer(_actualImage);
                //------------------------------------------------------
                //TODO: review here, use reusable-bitmap instead of current new one everytime.
                GLBitmap glBmp = new GLBitmap(new PixelFarm.Drawing.MemBitmapBinder(_memBmp, false));
                glBmp.IsYFlipped = false;
                //TODO: review font height
                if (StartDrawOnLeftTop)
                {
                    y -= _vxsTextPrinter.FontLineSpacingPx;
                }
                _pcx.DrawGlyphImageWithSubPixelRenderingTechnique(glBmp, (float)x, (float)y);
                glBmp.Dispose();
            }
            else
            {

                //1. clear prev drawing result
                _aggPainter.Clear(Drawing.Color.White);
                _aggPainter.StrokeColor = Color.Black;

                //2. print text span into Agg Canvas
                _vxsTextPrinter.StartDrawOnLeftTop = false;

                float dyOffset = _vxsTextPrinter.FontDescedingPx;
                _vxsTextPrinter.DrawString(text, startAt, len, 0, -dyOffset);
                //------------------------------------------------------
                //debug save image from agg's buffer
#if DEBUG
                //actualImage.dbugSaveToPngFile("d:\\WImageTest\\aa1.png");
#endif
                //------------------------------------------------------

                //3.copy to gl bitmap
                //byte[] buffer = PixelFarm.Agg.ActualImage.GetBuffer(_actualImage);
                //------------------------------------------------------
                //debug save image from agg's buffer 

                //------------------------------------------------------
                //GLBitmap glBmp = new GLBitmap(bmpWidth, bmpHeight, buffer, true);

                //TODO: review here again ***
                //use cache buffer instead of creating the buffer every time

                GLBitmap glBmp = new GLBitmap(new PixelFarm.Drawing.MemBitmapBinder(_memBmp, false));
                glBmp.IsYFlipped = false;
                //TODO: review font height 
                //if (StartDrawOnLeftTop)
                //{
                y += _vxsTextPrinter.FontLineSpacingPx;
                //}
                _pcx.DrawGlyphImage(glBmp, (float)x, (float)y + dyOffset);
                glBmp.Dispose();
            }
        }
        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int start, int len)
        {
            throw new NotImplementedException();
        }
        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            throw new NotImplementedException();
        }
    }

    public enum GlyphTexturePrinterDrawingTechnique
    {
        Copy,
        Stencil,
        LcdSubPixelRendering,
        Msdf
    }


    public class GLBitmapGlyphTextPrinter : ITextPrinter, IDisposable
    {
        MySimpleGLBitmapFontManager _myGLBitmapFontMx;
        SimpleFontAtlas _fontAtlas;
        GLPainterContext _pcx;
        GLPainter _painter;
        GLBitmap _glBmp;
        RequestFont _font;
        LayoutFarm.OpenFontTextService _textServices;
        float _px_scale = 1;
        TextureCoordVboBuilder _vboBuilder = new TextureCoordVboBuilder();

#if DEBUG
        public static GlyphTexturePrinterDrawingTechnique s_dbugDrawTechnique = GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering;
        public static bool s_dbugUseVBO = true;
        public static bool s_dbugShowGlyphTexture = false;
        public static bool s_dbugShowMarkers = false;
#endif
        /// <summary>
        /// use vertex buffer object
        /// </summary>

        public GLBitmapGlyphTextPrinter(GLPainter painter, LayoutFarm.OpenFontTextService textServices)
        {
            //create text printer for use with canvas painter           
            _painter = painter;
            _pcx = painter.Canvas;
            _textServices = textServices;

            //_currentTextureKind = TextureKind.Msdf; 
            //_currentTextureKind = TextureKind.StencilGreyScale;

            _myGLBitmapFontMx = new MySimpleGLBitmapFontManager(TextureKind.StencilLcdEffect, textServices);


            //test textures...

            //GlyphPosPixelSnapX = GlyphPosPixelSnapKind.Integer;
            //GlyphPosPixelSnapY = GlyphPosPixelSnapKind.Integer;
            //**
            ChangeFont(painter.CurrentFont);
            //
            DrawingTechnique = GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering; //default 
            UseVBO = true;
        }

        public bool UseVBO { get; set; }
        public GlyphTexturePrinterDrawingTechnique DrawingTechnique { get; set; }
        public void ChangeFillColor(Color color)
        {
            //called by owner painter  
            _painter.FontFillColor = color;
        }
        public void ChangeStrokeColor(Color strokeColor)
        {
            //TODO: implementation here
        }
        public bool StartDrawOnLeftTop { get; set; }

        public void ChangeFont(RequestFont font)
        {
            if (_font == font)
            {
                return;
            }
            //font has been changed, 
            //resolve for the new one 
            //check if we have this texture-font atlas in our MySimpleGLBitmapFontManager 
            //if not-> request the MySimpleGLBitmapFontManager to create a newone 
            _fontAtlas = _myGLBitmapFontMx.GetFontAtlas(font, out _glBmp);
            _font = font;

            Typeface typeface = _textServices.ResolveTypeface(font);
            _px_scale = typeface.CalculateScaleToPixelFromPointSize(font.SizeInPoints);
        }
        public void Dispose()
        {
            _myGLBitmapFontMx.Clear();
            _myGLBitmapFontMx = null;

            if (_glBmp != null)
            {
                _glBmp.Dispose();
                _glBmp = null;
            }
        }

        public void DrawString(char[] buffer, int startAt, int len, double left, double top)
        {
            _vboBuilder.Clear();
            _vboBuilder.SetTextureInfo(_glBmp.Width, _glBmp.Height, _glBmp.IsYFlipped, _pcx.OriginKind);

            // 
            _pcx.FontFillColor = _painter.FontFillColor;
            _pcx.LoadTexture(_glBmp);


            //create temp buffer span that describe the part of a whole char buffer
            TextBufferSpan textBufferSpan = new TextBufferSpan(buffer, startAt, len);
            //ask text service to parse user input char buffer and create a glyph-plan-sequence (list of glyph-plan) 
            //with specific request font
            GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(ref textBufferSpan, _font);
            float px_scale = _px_scale;
            //--------------------------
            //TODO:
            //if (x,y) is left top
            //we need to adjust y again      

            float scaleFromTexture = 1;
            TextureKind textureKind = _fontAtlas.TextureKind;

            float g_left = 0;
            float g_top = 0;
            int baseLine = (int)Math.Round((float)top + _font.AscentInPixels);
            int bottom = (int)Math.Round((float)top + _font.AscentInPixels - _font.DescentInPixels);

            float acc_x = 0; //local accumulate x
            float acc_y = 0; //local accumulate y 

#if DEBUG
            if (s_dbugShowMarkers)
            {
                if (s_dbugShowGlyphTexture)
                {
                    //show original glyph texture at top 
                    _pcx.DrawImage(_glBmp, 0, 0);
                }
                //draw red-line-marker for baseLine
                _painter.StrokeColor = Color.Red;
                _painter.DrawLine(left, baseLine, left + 200, baseLine);
                //
                //draw magenta-line-marker for bottom line
                _painter.StrokeColor = Color.Magenta;
                int bottomLine = (int)Math.Round((float)top + _font.LineSpacingInPixels);
                _painter.DrawLine(left, bottomLine, left + 200, bottomLine);
                //draw blue-line-marker for top line
                _painter.StrokeColor = Color.Blue;
                _painter.DrawLine(0, top, left + 200, top);
            }

            DrawingTechnique = s_dbugDrawTechnique;//for debug only
            UseVBO = s_dbugUseVBO;//for debug only 
#endif



            int seqLen = glyphPlanSeq.Count;

            for (int i = 0; i < seqLen; ++i)
            {
                UnscaledGlyphPlan glyph = glyphPlanSeq[i];
                Typography.Rendering.TextureGlyphMapData glyphData;
                if (!_fontAtlas.TryGetGlyphMapData(glyph.glyphIndex, out glyphData))
                {
                    //if no glyph data, we should render a missing glyph ***
                    continue;
                }
                //--------------------------------------
                //TODO: review precise height in float
                //--------------------------------------  

                //paint src rect
                //temp fix, glyph texture img is not flipped
                //but the associate info is flipped => so
                //we need remap exact Y from the image 

                Rectangle srcRect =
                      new Rectangle(glyphData.Left,
                         _glBmp.Height - (glyphData.Top + glyphData.Height),
                          glyphData.Width,
                          glyphData.Height);

                //offset length from 'base-line'
                float x_offset = acc_x + (float)Math.Round(glyph.OffsetX * px_scale - glyphData.TextureXOffset);
                float y_offset = acc_y + (float)Math.Round(glyph.OffsetY * px_scale - glyphData.TextureYOffset) + srcRect.Height; //***

                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------              

                g_left = (float)(left + x_offset);
                g_top = (float)(bottom - y_offset); //***

                acc_x += (float)Math.Round(glyph.AdvanceX * px_scale);

                //g_x = (float)Math.Round(g_x); //***
                g_top = (float)Math.Floor(g_top);//adjust to integer num ***

#if DEBUG
                if (s_dbugShowMarkers)
                {

                    if (s_dbugShowGlyphTexture)
                    {
                        //draw yellow-rect-marker on original texture
                        _painter.DrawRectangle(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, Color.Yellow);
                    }

                    //draw debug-rect box at target glyph position
                    _painter.DrawRectangle(g_left, g_top, srcRect.Width, srcRect.Height, Color.Black);
                    _painter.StrokeColor = Color.Blue; //restore
                }
#endif 
                if (textureKind == TextureKind.Msdf)
                {
                    _pcx.DrawSubImageWithMsdf(_glBmp,
                        ref srcRect,
                        g_left,
                        g_top,
                        scaleFromTexture);
                }
                else
                {
                    switch (DrawingTechnique)
                    {
                        case GlyphTexturePrinterDrawingTechnique.Stencil:
                            if (UseVBO)
                            {
                                _vboBuilder.WriteVboToList(
                                     ref srcRect,
                                     g_left, g_top);
                            }
                            else
                            {
                                //stencil gray scale with fill-color
                                _pcx.DrawGlyphImageWithStecil(_glBmp,
                                    ref srcRect,
                                    g_left,
                                    g_top,
                                    scaleFromTexture);
                            }
                            break;
                        case GlyphTexturePrinterDrawingTechnique.Copy:
                            if (UseVBO)
                            {
                                _vboBuilder.WriteVboToList(
                                      ref srcRect,
                                      g_left, g_top);
                            }
                            else
                            {
                                _pcx.DrawSubImage(_glBmp,
                                    ref srcRect,
                                    g_left,
                                    g_top,
                                    1);
                            }
                            break;
                        case GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering:
                            if (UseVBO)
                            {
                                _vboBuilder.WriteVboToList(
                                      ref srcRect,
                                      g_left, g_top);
                            }
                            else
                            {
                                _pcx.DrawGlyphImageWithSubPixelRenderingTechnique2_GlyphByGlyph(
                                 ref srcRect,
                                    g_left,
                                    g_top,
                                    1);
                            }
                            break;
                    }
                }
            }
            //-------------------------------------------
            //
            if (UseVBO)
            {
                switch (DrawingTechnique)
                {
                    case GlyphTexturePrinterDrawingTechnique.Copy:
                        _pcx.DrawGlyphImageWithCopy_VBO(_vboBuilder);
                        break;
                    case GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering:
                        _pcx.DrawGlyphImageWithSubPixelRenderingTechnique3_DrawElements(_vboBuilder);
                        break;
                    case GlyphTexturePrinterDrawingTechnique.Stencil:
                        _pcx.DrawGlyphImageWithStecil_VBO(_vboBuilder);
                        break;
                }

                _vboBuilder.Clear();
            }
        }
        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            DrawString((GLRenderVxFormattedString)renderVx, x, y);
        }
        public void DrawString(GLRenderVxFormattedString renderVx, double x, double y)
        {
            _pcx.LoadTexture(_glBmp);
            _pcx.FontFillColor = _painter.FontFillColor;

            DrawingGL.GLRenderVxFormattedString renderVxString1 = (DrawingGL.GLRenderVxFormattedString)renderVx;
            DrawingGL.VertexBufferObject vbo = renderVxString1.GetVbo();
            vbo.Bind();
            _pcx.DrawGlyphImageWithSubPixelRenderingTechnique4_FromLoadedVBO(renderVxString1.IndexArrayCount, (float)x, (float)y);
            vbo.UnBind();
        }
        public void PrepareStringForRenderVx(GLRenderVxFormattedString renderVxFormattedString, char[] buffer, int startAt, int len)
        {

            int top = 0;//simulate top
            int left = 0;//simulate left

            _vboBuilder.Clear();
            _vboBuilder.SetTextureInfo(_glBmp.Width, _glBmp.Height, _glBmp.IsYFlipped, _pcx.OriginKind);

            //create temp buffer span that describe the part of a whole char buffer
            TextBufferSpan textBufferSpan = new TextBufferSpan(buffer, startAt, len);

            //ask text service to parse user input char buffer and create a glyph-plan-sequence (list of glyph-plan) 
            //with specific request font
            GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(ref textBufferSpan, _font);
            float px_scale = _px_scale;
            //-------------------------- 
            TextureKind textureKind = _fontAtlas.TextureKind;
            float g_left = 0;
            float g_top = 0;

            int baseLine = (int)Math.Round((float)top + _font.AscentInPixels);
            int bottom = (int)Math.Round((float)top + _font.AscentInPixels - _font.DescentInPixels);
            float acc_x = 0; //local accumulate x
            float acc_y = 0; //local accumulate y  

            int seqLen = glyphPlanSeq.Count;
            for (int i = 0; i < seqLen; ++i)
            {
                UnscaledGlyphPlan glyph = glyphPlanSeq[i];
                Typography.Rendering.TextureGlyphMapData glyphData;
                if (!_fontAtlas.TryGetGlyphMapData(glyph.glyphIndex, out glyphData))
                {
                    //if no glyph data, we should render a missing glyph ***
                    continue;
                }
                //--------------------------------------
                //TODO: review precise height in float
                //--------------------------------------  
                //paint src rect
                //temp fix, glyph texture img is not flipped
                //but the associate info is flipped => so
                //we need remap exact Y from the image  
                Rectangle srcRect =
                      new Rectangle(glyphData.Left,
                         _glBmp.Height - (glyphData.Top + glyphData.Height),
                          glyphData.Width,
                          glyphData.Height);

                //offset length from 'base-line'
                float x_offset = acc_x + (float)Math.Round(glyph.OffsetX * px_scale - glyphData.TextureXOffset);
                float y_offset = acc_y + (float)Math.Round(glyph.OffsetY * px_scale - glyphData.TextureYOffset) + srcRect.Height; //***

                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------              

                g_left = (float)(left + x_offset);
                g_top = (float)(bottom - y_offset); //***

                acc_x += (float)Math.Round(glyph.AdvanceX * px_scale);
                //g_x = (float)Math.Round(g_x); //***
                g_top = (float)Math.Floor(g_top);//adjust to integer num *** 
                //
                _vboBuilder.WriteVboToList(ref srcRect, g_left, g_top);
            }
            //---
            //copy vbo result and store into  renderVx 
            float[] vertexList = _vboBuilder._buffer.ToArray();
            ushort[] indexList = _vboBuilder._indexList.ToArray();
            //---

            renderVxFormattedString.IndexArrayCount = _vboBuilder._indexList.Count;
            renderVxFormattedString.IndexArray = _vboBuilder._indexList.ToArray();
            renderVxFormattedString.VertexCoords = _vboBuilder._buffer.ToArray();
            _vboBuilder.Clear();
        }
        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] buffer, int startAt, int len)
        {
            var renderVxFormattedString = renderVx as GLRenderVxFormattedString;

#if DEBUG
            if (renderVxFormattedString == null)
            {
                throw new NotSupportedException();
            }
#endif
            PrepareStringForRenderVx(renderVxFormattedString, buffer, startAt, len);

        }
    }

}


