﻿//MIT, 2016-present, WinterDev
//MIT, 2018-present, WinterDev

using System;
using PixelFarm.CpuBlit;
using PixelFarm.Drawing;
using LayoutFarm;
using LayoutFarm.UI;

namespace Mini
{

    //This is a helper class
    class CpuBlitAppModule
    {
        //FOR DEMO PROJECT
        int _myWidth;
        int _myHeight;
        GraphicsViewRoot _surfaceViewport;
        RootGraphic _rootGfx;
        //
        DemoUI _demoUI;

        public CpuBlitAppModule()
        {

        }
        public void BindSurface(LayoutFarm.UI.GraphicsViewRoot surfaceViewport)
        {
            _myWidth = 800;
            _myHeight = 600;
            _surfaceViewport = surfaceViewport;
            _rootGfx = surfaceViewport.RootGfx;
        }
        public void InvalidateUI()
        {
            _demoUI.InvalidateGraphics();
        }

        public void LoadExample(DemoBase exBase)
        {
            _demoUI = new DemoUI(exBase, _myWidth, _myHeight);
            _rootGfx.AddChild(_demoUI.GetPrimaryRenderElement());
        }
        public void Close()
        {
            if (_surfaceViewport != null)
            {
                _surfaceViewport.Close();
                _surfaceViewport = null;
            }
            _rootGfx = null;
        }
        //This is a simple UIElement for testing only
        class DemoUI : UIElement
        {
            DemoBase _exampleBase;
            CpuBlitAggCanvasRenderElement _canvasRenderE;
            int _width;
            int _height;
            public DemoUI(DemoBase exBase, int width, int height)
            {
                _width = width;
                _height = height;
                _exampleBase = exBase;
            }

            public override RenderElement CurrentPrimaryRenderElement => _canvasRenderE;

            protected override bool HasReadyRenderElement => _canvasRenderE != null;

            public override RenderElement GetPrimaryRenderElement()
            {
                if (_canvasRenderE == null)
                {
                    _canvasRenderE = new CpuBlitAggCanvasRenderElement(_width, _height);
                    _canvasRenderE.SetController(this); //connect to event system
                    _canvasRenderE.LoadDemo(_exampleBase);
                }
                return _canvasRenderE;
            }

            public override void InvalidateGraphics() => _canvasRenderE?.InvalidateGraphics();

            //handle event
            protected override void OnMouseDown(UIMouseDownEventArgs e)
            {
                _exampleBase.MouseDown(e.X, e.Y, e.Buttons == UIMouseButtons.Right);
                base.OnMouseDown(e);
            }
            protected override void OnMouseMove(UIMouseMoveEventArgs e)
            {
                if (e.IsDragging)
                {
                    _canvasRenderE.InvalidateGraphics();
                    _exampleBase.MouseDrag(e.X, e.Y);
                    _canvasRenderE.InvalidateGraphics();
                }
                base.OnMouseMove(e);
            }
            protected override void OnMouseUp(UIMouseUpEventArgs e)
            {
                _exampleBase.MouseUp(e.X, e.Y);
                base.OnMouseUp(e);
            }
        }


        //For testing only
        //Implement simple render element***
        class CpuBlitAggCanvasRenderElement : LayoutFarm.RenderElement, IDisposable
        {
            Win32.NativeWin32MemoryDC _nativeWin32DC; //use this as gdi back buffer
            DemoBase _demo;
            MemBitmap _memBmp;
            Painter _painter;
            public CpuBlitAggCanvasRenderElement(int w, int h)
                : base(w, h)
            {

                //TODO: check if we can access raw rootGraphics buffer or not
                //1. gdi+ create backbuffer
                _nativeWin32DC = new Win32.NativeWin32MemoryDC(w, h);
                //2. create actual bitmap that share bitmap data from native _nativeWin32Dc
                _memBmp = new MemBitmap(w, h, _nativeWin32DC.PPVBits);
                //----------------------------------------------------------------
                //3. create render surface from bitmap => provide basic bitmap fill operations
                AggRenderSurface aggsx = new AggRenderSurface();
                aggsx.AttachDstBitmap(_memBmp);
                //4. painter wraps the render surface  => provide advance operations
                AggPainter aggPainter = new AggPainter(aggsx);
                aggPainter.CurrentFont = new PixelFarm.Drawing.RequestFont("Source Sans Pro", 14);
                _painter = aggPainter;
                //----------------------------------------------------------------             
            }
            public void LoadDemo(DemoBase demo)
            {
                _demo = demo;
                if (_painter != null)
                {
                    DemoBase.InvokePainterReady(_demo, _painter);
                }
            }
            protected override void RenderClientContent(DrawBoard d, UpdateArea updateArea)
            {
                //

                //TODO: review here again
                //in pure agg, we could bypass the cache/resolve process
                //and render directly to the target canvas
                //
                //if img changed then clear cache and render again
                MemBitmap.ClearCache(_memBmp);
                MemBitmap.SetCacheInnerImage(_memBmp, _nativeWin32DC, false);//** set to false because the (life-time) owner of the dc is this class (not the memBmp)
                _demo.Draw(_painter);
                //copy from actual image and paint to canvas 
                d.DrawImage(_memBmp, 0, 0);
            }

            public void Dispose()
            {
                if (_nativeWin32DC != null)
                {
                    _nativeWin32DC.Dispose();
                    _nativeWin32DC = null;
                }
            }
        }
    }


}
