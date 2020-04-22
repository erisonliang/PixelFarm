﻿//Apache2, 2014-present, WinterDev

using System.Collections.Generic;
using PixelFarm.Drawing;
using LayoutFarm.UI;
namespace LayoutFarm.CustomWidgets
{


    public enum ContentStretch
    {
        None,
        Horizontal,
        Vertical,
        Both,
    }

    public class Box : AbstractBox, IBoxContainer
    {
        UICollection _uiList;
        public Box(int w, int h)
            : base(w, h)
        {

        }
        public override void NotifyContentUpdate(UIElement childContent)
        {
            //set propersize

            //if (childContent is ImageBox)
            //{
            //    ImageBox imgBox = (ImageBox)childContent;
            //    this.SetSize(imgBox.Width, imgBox.Height); 
            //}

            this.InvalidateLayout();
            //this.ParentUI?.NotifyContentUpdate(this);
            this.ParentUI?.InvalidateLayout();
        }


        //----------------------------------------------------
        static UIElement[] s_empty = new UIElement[0];
        public IEnumerable<UIElement> GetChildIter()
        {
            if (_uiList != null)
            {
                return _uiList.GetIter();
            }
            return s_empty;
        }
        public virtual void Insert(int index, UIElement ui)
        {
            _needContentLayout = true;
            LinkedListNode<UIElement> insertAt = _uiList.GetUIElementLinkedListNode(index);

            if (this.HasReadyRenderElement)
            {
                _primElement.InsertBefore(
                        insertAt.Value.GetPrimaryRenderElement(_primElement.Root),
                        ui.GetPrimaryRenderElement(_primElement.Root));

                if (_supportViewport)
                {
                    this.InvalidateLayout();
                }
            }

            if (ui.NeedContentLayout)
            {
                ui.InvalidateLayout();
            }
        }
        public virtual void AddLast(UIElement ui) => Add(ui);
        public virtual void AddAfter(UIElement afterUI, UIElement ui)
        {
            //insert new child after existing one
            _uiList.AddAfter(afterUI, ui);
            if (this.HasReadyRenderElement)
            {
                _primElement.InsertAfter(
                    afterUI.CurrentPrimaryRenderElement,
                    ui.GetPrimaryRenderElement(_primElement.Root));

                if (_supportViewport)
                {
                    this.InvalidateLayout();
                }
            }

            if (ui.NeedContentLayout)
            {
                ui.InvalidateLayout();
            }
        }
        public virtual void AddBefore(UIElement beforeUI, UIElement ui)
        {
            _uiList.AddBefore(beforeUI, ui);

            if (this.HasReadyRenderElement)
            {
                _primElement.InsertBefore(
                    beforeUI.CurrentPrimaryRenderElement,
                    ui.GetPrimaryRenderElement(_primElement.Root));

                if (_supportViewport)
                {
                    this.InvalidateLayout();
                }
            }

            if (ui.NeedContentLayout)
            {
                ui.InvalidateLayout();
            }
        }
        public virtual void AddFirst(UIElement ui)
        {
            if (_uiList == null)
            {
                _uiList = new UICollection(this);
            }

            _needContentLayout = true;

            _uiList.AddFirst(ui);
            if (this.HasReadyRenderElement)
            {
                _primElement.AddFirst(
                    ui.GetPrimaryRenderElement(_primElement.Root));

                if (_supportViewport)
                {
                    this.InvalidateLayout();
                }
            }

            if (ui.NeedContentLayout)
            {
                ui.InvalidateLayout();
            }
        }

        public virtual void Add(UIElement ui)
        {
            if (_uiList == null)
            {
                _uiList = new UICollection(this);
            }

            _needContentLayout = true;
            _uiList.AddUI(ui);
            if (this.HasReadyRenderElement)
            {
                _primElement.AddChild(ui);

                //if (this.panelLayoutKind != BoxContentLayoutKind.Absolute)
                //{
                //    this.InvalidateLayout();
                //}
                //check if we support
                if (_supportViewport)
                {
                    this.InvalidateLayout();
                }
            }

            if (ui.NeedContentLayout)
            {
                if (!this.IsInLayoutQueue) //if this elem is in layout queue, the ui will be layout with this
                {
                    if (this.ParentUI != null)
                    {
                        //if this elem is add to the host
                        //the parent UI is not null, 

                        ui.InvalidateLayout();
                    }
                }
            }
        }
        public virtual void RemoveChild(UIElement ui)
        {
            _needContentLayout = true;
            _uiList.RemoveUI(ui);
            if (this.HasReadyRenderElement)
            {
                if (_supportViewport)
                {
                    this.InvalidateLayout();
                }
                _primElement.RemoveChild(ui.CurrentPrimaryRenderElement);
            }
        }

        public virtual void ClearChildren()
        {
            _needContentLayout = true;
            if (_uiList != null)
            {
                _uiList.Clear();
            }
            if (this.HasReadyRenderElement)
            {
                _primElement.ClearAllChildren();
                if (Visible)
                {
                    if (_supportViewport)
                    {
                        this.InvalidateLayout();
                    }
                }
            }
        }

        public int ChildCount => (_uiList != null) ? _uiList.Count : 0;
        protected override void OnAcceptVisitor(UIVisitor visitor)
        {
            if (_uiList != null)
            {
                UICollection.AcceptVisitor(_uiList, visitor);
            }
        }
        public override void PerformContentLayout()
        {
            //****
            //this.InvalidateGraphics();
            //temp : arrange as vertical stack***
            Rectangle preBounds = this.Bounds;
            switch (this.ContentLayoutKind)
            {
                case BoxContentLayoutKind.VerticalStack:
                    {

                        int maxRight = 0;

                        int xpos = this.PaddingLeft; //start X at paddingLeft
                        int ypos = this.PaddingTop; //start Y at padding top

                        if (ChildCount > 0)
                        {
                            foreach (UIElement ui in GetChildIter())
                            {
                                if (ui is AbstractRectUI element)
                                {
                                    element.PerformContentLayout();
                                    element.SetLocationAndSize(xpos + element.MarginLeft, ypos + element.MarginTop, element.Width, element.Height);
                                    ypos += element.Height + element.MarginTopBottom;

                                    int tmp_right = element.Right;
                                    if (tmp_right > maxRight)
                                    {
                                        maxRight = tmp_right;
                                    }
                                }
                            }
                        }

                        this.SetInnerContentSize(maxRight, ypos);
                    }
                    break;
                case BoxContentLayoutKind.HorizontalStack:
                    {

                        int maxBottom = 0;

                        //experiment
                        bool allowAutoContentExpand = this.AllowAutoContentExpand;

                        int xpos = this.PaddingLeft; //start X at paddingLeft
                        int ypos = this.PaddingTop; //start Y at padding top
                        if (ChildCount > 0)
                        {
                            List<AbstractRectUI> alignToEnds = null;
                            var alignToEndsContext = MayBeEmptyTempContext<List<AbstractRectUI>>.Empty;

                            List<AbstractRectUI> notHaveSpecificWidthElems = null;
                            var notHaveSpecificWidthElemsContext = MayBeEmptyTempContext<List<AbstractRectUI>>.Empty;

                            int left_to_right_max_x = 0;

                            foreach (UIElement ui in GetChildIter())
                            {
                                if (ui is AbstractRectUI element)
                                {
                                    element.PerformContentLayout();

                                    //TODO: review Middle again
                                    if (element.Alignment == RectUIAlignment.End)
                                    {
                                        //skip this
                                        if (alignToEnds == null)
                                        {
                                            alignToEndsContext = LayoutTools.BorrowList(out alignToEnds);
                                        }
                                        alignToEnds.Add(element);
                                    }
                                    else
                                    {
                                        if (allowAutoContentExpand && !element.HasSpecificWidth)
                                        {
                                            if (notHaveSpecificWidthElems == null)
                                            {
                                                notHaveSpecificWidthElemsContext = LayoutTools.BorrowList(out notHaveSpecificWidthElems);
                                            }
                                            notHaveSpecificWidthElems.Add(element);
                                        }

                                        element.SetLocationAndSize(xpos, ypos + element.MarginTop, element.Width, element.Height); //
                                        xpos += element.Width + element.MarginLeftRight;
                                        int tmp_bottom = element.Bottom;
                                        if (tmp_bottom > maxBottom)
                                        {
                                            maxBottom = tmp_bottom;
                                        }
                                    }
                                }
                            }

                            left_to_right_max_x = xpos;

                            //--------
                            //arrange alignToEnd again
                            if (alignToEnds != null)
                            {
                                //var node = alignToEnds.Last; //start from last node
                                int n = alignToEnds.Count;
                                xpos = this.Width - PaddingRight;
                                while (n > 0)
                                {
                                    --n;
                                    AbstractRectUI rectUI = alignToEnds[n];
                                    xpos -= rectUI.Width + rectUI.MarginLeft;
                                    rectUI.SetLocationAndSize(xpos, ypos + rectUI.MarginTop, rectUI.Width, rectUI.Height); //

                                    //
                                    int tmp_bottom = rectUI.Bottom;
                                    if (tmp_bottom > maxBottom)
                                    {
                                        maxBottom = tmp_bottom;
                                    }
                                }

                                //release back to pool
                                alignToEndsContext.Dispose();
                            }
                            //--------

                            if (notHaveSpecificWidthElems != null && (xpos > left_to_right_max_x))
                            {
                                //this mean this allow content expand
                                float avaliable_w = xpos - left_to_right_max_x;
                                //distribute this 
                                float avg_w = avaliable_w / notHaveSpecificWidthElems.Count;

                                for (int m = notHaveSpecificWidthElems.Count - 1; m >= 0; --m)
                                {
                                    AbstractRectUI ui = notHaveSpecificWidthElems[m];
                                    ui.SetWidth((int)(ui.Width + avg_w));
                                }

                                //arrange location again
                                xpos = this.PaddingLeft; //start X at paddingLeft
                                foreach (UIElement ui in GetChildIter())
                                {
                                    if (ui is AbstractRectUI element && element.Alignment != RectUIAlignment.End)
                                    {
                                        //TODO: review here again
                                        element.SetLocation(xpos, ypos + element.MarginTop);
                                        xpos += element.Width + element.MarginLeftRight;
                                    }
                                }

                            }
                            notHaveSpecificWidthElemsContext.Dispose();
                            //--------
                        }

                        this.SetInnerContentSize(xpos, maxBottom);
                    }
                    break;
                default:
                    {

                        //this case : no action about paddings, margins, borders...

                        int count = this.ChildCount;
                        int maxRight = 0;
                        int maxBottom = 0;
                        if (ChildCount > 0)
                        {
                            foreach (UIElement ui in GetChildIter())
                            {
                                if (ui is AbstractRectUI element)
                                {
                                    element.PerformContentLayout();
                                    int tmp_right = element.Right;// element.InnerWidth + element.Left;
                                    if (tmp_right > maxRight)
                                    {
                                        maxRight = tmp_right;
                                    }
                                    int tmp_bottom = element.Bottom;// element.InnerHeight + element.Top;
                                    if (tmp_bottom > maxBottom)
                                    {
                                        maxBottom = tmp_bottom;
                                    }
                                }
                            }
                        }

                        if (!this.HasSpecificWidth)
                        {
                            this.SetInnerContentSize(maxRight, this.InnerHeight);
                        }
                        if (!this.HasSpecificHeight)
                        {
                            this.SetInnerContentSize(this.InnerWidth, maxBottom);
                        }
                    }
                    break;
            }

#if DEBUG
            Rectangle postBounds = this.Bounds;
            if (preBounds != postBounds)
            {

            }
#endif
            //------------------------------------------------
            base.RaiseLayoutFinished();

#if DEBUG
            if (HasReadyRenderElement)
            {
                // this.InvalidateGraphics();
            }
#endif
        }
        protected override void BuildChildrenRenderElement(RenderElement parent)
        {
            GlobalRootGraphic.BlockGraphicsUpdate();
            parent.HasSpecificHeight = this.HasSpecificHeight;
            parent.HasSpecificWidth = this.HasSpecificWidth;
            parent.SetController(this);
            parent.SetVisible(this.Visible);
            parent.SetLocation(this.Left, this.Top);
            parent.HasSpecificWidthAndHeight = true; //?
            parent.SetViewport(this.ViewportLeft, this.ViewportTop);

            if (ChildCount > 0)
            {

                foreach (UIElement ui in GetChildIter())
                {
                    parent.AddChild(ui);
                }
            }

            GlobalRootGraphic.ReleaseGraphicsUpdate();
            parent.InvalidateGraphics();
        }
        public override void UpdateLayout()
        {
            base.UpdateLayout();
            foreach (var chlid in GetChildIter())
            {
                if (chlid != null)
                {
                    chlid.UpdateLayout();
                }
            }
        }
        public override BoxContentLayoutKind ContentLayoutKind
        {
            get => base.ContentLayoutKind;
            set
            {
                base.ContentLayoutKind = value; //invalidate layout after change this


                if (_uiList != null && _uiList.Count > 0)
                {
                    this.InvalidateLayout();
                }
            }
        }
    }


}