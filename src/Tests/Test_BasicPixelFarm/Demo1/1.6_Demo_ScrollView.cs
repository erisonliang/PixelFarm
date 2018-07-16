﻿//Apache2, 2014-present, WinterDev

using PixelFarm.Drawing;
using LayoutFarm.UI;

namespace LayoutFarm
{
    [DemoNote("1.6 ScrollView")]
    class Demo_ScrollView : App
    {

        protected override void OnStart(AppHost host)
        {
            //AddScrollView1(host, 0, 0);
            AddScrollView2(host, 10, 0);
        }

        void AddScrollView1(AppHost host, int x, int y)
        {
            var panel = new LayoutFarm.CustomWidgets.Box(200, 175);
            panel.NeedClipArea = true;
            panel.SetLocation(x + 30, y + 30);
            panel.BackColor = Color.LightGray;
            host.AddChild(panel);
            //-------------------------  
            {
                //vertical scrollbar
                var vscbar = new LayoutFarm.CustomWidgets.ScrollBar(15, 200);
                vscbar.SetLocation(x + 10, y + 10);
                vscbar.MinValue = 0;
                vscbar.MaxValue = 170;
                vscbar.SmallChange = 20;
                host.AddChild(vscbar);
                //add relation between viewpanel and scroll bar 
                var scRelation = new LayoutFarm.CustomWidgets.ScrollingRelation(vscbar.SliderBox, panel);
            }
            //-------------------------  
            {
                //horizontal scrollbar
                var hscbar = new LayoutFarm.CustomWidgets.ScrollBar(200, 15);
                hscbar.ScrollBarType = CustomWidgets.ScrollBarType.Horizontal;
                hscbar.SetLocation(x + 30, y + 10);
                hscbar.MinValue = 0;
                hscbar.MaxValue = 170;
                hscbar.SmallChange = 20;
                host.AddChild(hscbar);
                //add relation between viewpanel and scroll bar 
                var scRelation = new LayoutFarm.CustomWidgets.ScrollingRelation(hscbar.SliderBox, panel);
            }

            //add content to panel
            for (int i = 0; i < 10; ++i)
            {
                var box1 = new LayoutFarm.CustomWidgets.Box(400, 30);
                box1.HasSpecificWidth = true;
                box1.HasSpecificHeight = true;
                box1.BackColor = Color.OrangeRed;
                box1.SetLocation(i * 20, i * 40);
                panel.AddChild(box1);
            }
            //--------------------------   
            //panel.PerformContentLayout();

        }
        void AddScrollView2(AppHost viewport, int x, int y)
        {
            var panel = new LayoutFarm.CustomWidgets.Box(800, 1000);
            panel.HasSpecificSize = true;
            panel.NeedClipArea = true;
            panel.SetLocation(x + 10, y + 30);
            panel.BackColor = Color.LightGray;
            panel.ContentLayoutKind = CustomWidgets.BoxContentLayoutKind.VerticalStack;
            viewport.AddChild(panel);
            //-------------------------  
            //load images...

            //check folder before load
            string[] fileNames = new string[0];

            if (System.IO.Directory.Exists("../../Data/imgs"))
            {
                fileNames = System.IO.Directory.GetFiles("../../Data/imgs", "0*.jpg");
            }
            //select only
            int lastY = 0;


            int imgNo = 0;

            for (int i = 0; i < fileNames.Length * 4; ++i) //5 imgs
            {
                var imgbox = new LayoutFarm.CustomWidgets.ImageBox(36, 400);
                imgbox.ImageBinder = viewport.GetImageBinder(fileNames[imgNo]);
                imgbox.BackColor = Color.OrangeRed;
                imgbox.SetLocation(0, lastY);
                imgbox.MouseUp += (s, e) =>
                {
                    if (e.Button == UIMouseButtons.Right)
                    {
                        //test remove this imgbox on right mouse click
                        panel.RemoveChild(imgbox);
                    }
                };
                lastY += imgbox.Height + 5;
                panel.AddChild(imgbox);

                imgNo++;
                if (imgNo == fileNames.Length - 1) //last img
                {
                    imgNo = 0;//reset
                }

            }
            //--------------------------
            //panel may need more 
            panel.SetViewport(0, 0);
            //-------------------------  
            {
                //vertical scrollbar
                var vscbar = new LayoutFarm.CustomWidgets.ScrollBar(15, 200);
                vscbar.SetLocation(x + 10, y + 10);
                vscbar.MinValue = 0;
                vscbar.MaxValue = lastY;
                vscbar.SmallChange = 20;
                viewport.AddChild(vscbar);
                //add relation between viewpanel and scroll bar 
                var scRelation = new LayoutFarm.CustomWidgets.ScrollingRelation(vscbar.SliderBox, panel);
            }
            //-------------------------  
            {
                //horizontal scrollbar
                var hscbar = new LayoutFarm.CustomWidgets.ScrollBar(150, 15);
                hscbar.ScrollBarType = CustomWidgets.ScrollBarType.Horizontal;
                hscbar.SetLocation(x + 30, y + 10);
                hscbar.MinValue = 0;
                hscbar.MaxValue = panel.Width;//just init
                hscbar.SmallChange = 20;
                viewport.AddChild(hscbar);
                //add relation between viewpanel and scroll bar 
                var scRelation = new LayoutFarm.CustomWidgets.ScrollingRelation(hscbar.SliderBox, panel);
            }
            //panel.PerformContentLayout();
        }

    }
}