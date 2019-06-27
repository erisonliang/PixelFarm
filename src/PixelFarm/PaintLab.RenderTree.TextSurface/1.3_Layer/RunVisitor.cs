﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using System.Text;
using PixelFarm.Drawing;
using LayoutFarm.RenderBoxes;
namespace LayoutFarm.TextEditing
{
    public class EditableRunVisitor
    {
        public EditableRunVisitor()
        {

        }
        public Point CurrentCaretPos { get; set; }
        public bool StopOnNextLine { get; set; }
        public bool SkipCurrentLineEditableRunIter { get; set; }
        public Rectangle UpdateArea { get; set; }
        public bool UseUpdateArea { get; set; }
        public bool SkipMarkerLayer { get; set; }
        public bool SkipSelectionLayer { get; set; }

        public virtual void OnBeginTextLayer() { }
        public virtual void OnEndTextLayer() { }
        public virtual void VisitNewLine(int lineTop) { }
        public virtual void VisitEditableRun(EditableRun run) { }
        //
        //
        public virtual void OnBeginSelectionBG() { }
        public virtual void OnEndSelectionBG() { }

        public virtual void OnBeginMarkerLayer() { }
        public virtual void OnEndMarkerLayer() { }
        public virtual void VisitMarker(VisualMarkerSelectionRange markerRange) { }

        public virtual void VisitSelectionRange(VisualSelectionRange selRange) { }
    }
}