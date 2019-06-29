﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic; 
using PixelFarm.Drawing;
using LayoutFarm.TextEditing.Commands; 

namespace LayoutFarm.TextEditing
{

    public class TextFlowEditSession : TextFlowSelectSession, ITextFlowEditSession
    {

        internal bool _updateJustCurrentLine = true;
        bool _enableUndoHistoryRecording = true; 
        TextMarkerLayer _textMarkerLayer;
        DocumentCommandCollection _commandHistoryList;


        internal TextFlowEditSession(TextFlowLayer textLayer) : base(textLayer)
        {

            //and record editing hx, support undo-redo
            _commandHistoryList = new DocumentCommandCollection(this);
#if DEBUG
            if (dbugEnableTextManRecorder)
            {
                _dbugActivityRecorder = new debugActivityRecorder();
                _lineEditor.dbugTextManRecorder = _dbugActivityRecorder;
                throw new NotSupportedException();
                _dbugActivityRecorder.Start(null);
            }
#endif

        }
        internal void SetMarkerLayer(TextMarkerLayer textMarkerLayer)
        {
            _textMarkerLayer = textMarkerLayer;
        }
        //
        public DocumentCommandListener DocCmdListener
        {
            get => _commandHistoryList.Listener;
            set => _commandHistoryList.Listener = value;
        }
        internal bool UndoMode { get; set; }
        //
        public bool EnableUndoHistoryRecording
        {
            get => _enableUndoHistoryRecording;
            set => _enableUndoHistoryRecording = value;
        }
        // 
        public void AddCharToCurrentLine(char c)
        {
            _updateJustCurrentLine = true;
            bool passRemoveSelectedText = false;
#if DEBUG
            if (dbugEnableTextManRecorder)
            {
                _dbugActivityRecorder.WriteInfo("TxLMan::AddCharToCurrentLine " + c);
                _dbugActivityRecorder.BeginContext();
            }
#endif
            if (SelectionRange != null)
            {
#if DEBUG
                if (dbugEnableTextManRecorder)
                {
                    _dbugActivityRecorder.WriteInfo(SelectionRange);
                }
#endif
                VisualSelectionRangeSnapShot removedRange = RemoveSelectedText();
                passRemoveSelectedText = true;
            }

            if (passRemoveSelectedText && c == ' ')
            {
            }
            else
            {
                if (!_lineEditor.CanAcceptThisChar(c))
                {
                    return;
                }
                //
                _commandHistoryList.AddDocAction(
                  new DocActionCharTyping(c, _lineEditor.LineNumber, _lineEditor.CharIndex));
            }

            _lineEditor.AddCharacter(c);
#if DEBUG
            if (dbugEnableTextManRecorder)
            {
                _dbugActivityRecorder.EndContext();
            }
#endif
        }



        VisualSelectionRangeSnapShot RemoveSelectedText()
        {
#if DEBUG
            if (dbugEnableTextManRecorder)
            {
                _dbugActivityRecorder.WriteInfo("TxLMan::RemoveSelectedText");
                _dbugActivityRecorder.BeginContext();
            }
#endif

            if (_selectionRange == null)
            {
#if DEBUG
                if (dbugEnableTextManRecorder)
                {
                    _dbugActivityRecorder.WriteInfo("NO_SEL_RANGE");
                    _dbugActivityRecorder.EndContext();
                }
#endif
                return VisualSelectionRangeSnapShot.Empty;
            }
            else if (!_selectionRange.IsValid)
            {
#if DEBUG
                if (dbugEnableTextManRecorder)
                {
                    _dbugActivityRecorder.WriteInfo("!RANGE_ON_SAME_POINT");
                }
#endif
                CancelSelect();
#if DEBUG
                if (dbugEnableTextManRecorder)
                {
                    _dbugActivityRecorder.EndContext();
                }
#endif
                return VisualSelectionRangeSnapShot.Empty;
            }
            _selectionRange.SwapIfUnOrder();
            VisualSelectionRangeSnapShot selSnapshot = _selectionRange.GetSelectionRangeSnapshot();
            VisualPointInfo startPoint = _selectionRange.StartPoint;
            CurrentLineNumber = startPoint.LineId;
            int preCutIndex = startPoint.LineCharIndex;
            _lineEditor.SetCurrentCharIndex(startPoint.LineCharIndex);
            if (_selectionRange.IsOnTheSameLine)
            {
                var tobeDeleteTextRuns = new TextRangeCopy();
                _lineEditor.CopySelectedTextRuns(_selectionRange, tobeDeleteTextRuns);

                if (tobeDeleteTextRuns.HasSomeRuns)
                {
                    _commandHistoryList.AddDocAction(
                    new DocActionDeleteRange(tobeDeleteTextRuns,
                        selSnapshot.startLineNum,
                        selSnapshot.startColumnNum,
                        selSnapshot.endLineNum,
                        selSnapshot.endColumnNum));
                    _lineEditor.RemoveSelectedTextRuns(_selectionRange);
                    _updateJustCurrentLine = true;
                }
            }
            else
            {
                int startPointLindId = startPoint.LineId;
                int startPointCharIndex = startPoint.LineCharIndex;
                var tobeDeleteTextRuns = new TextRangeCopy();
                _lineEditor.CopySelectedTextRuns(_selectionRange, tobeDeleteTextRuns);
                if (tobeDeleteTextRuns != null && tobeDeleteTextRuns.HasSomeRuns)
                {
                    _commandHistoryList.AddDocAction(
                    new DocActionDeleteRange(tobeDeleteTextRuns,
                        selSnapshot.startLineNum,
                        selSnapshot.startColumnNum,
                        selSnapshot.endLineNum,
                        selSnapshot.endColumnNum));
                    _lineEditor.RemoveSelectedTextRuns(_selectionRange);
                    _updateJustCurrentLine = false;
                    _lineEditor.MoveToLine(startPointLindId);
                    _lineEditor.SetCurrentCharIndex(startPointCharIndex);
                }
            }
            CancelSelect();
            //NotifyContentSizeChanged();
#if DEBUG
            if (dbugEnableTextManRecorder)
            {
                _dbugActivityRecorder.EndContext();
            }
#endif
            return selSnapshot;
        }



        public Run LatestHitRun => _textLayer.LatestHitRun;
        void SplitSelectedText()
        {

            if (_selectionRange == null) return;
            //
            SelectionRangeInfo selRangeInfo = _lineEditor.SplitSelectedText(_selectionRange);
            //add startPointInfo and EndPoint info to current selection range
            _selectionRange.StartPoint = selRangeInfo.start;
            _selectionRange.EndPoint = selRangeInfo.end;
        }

        public void DoTabOverSelectedRange()
        {
            //eg. user press 'Tab' key over selected range
            VisualSelectionRange selRange = SelectionRange;
            if (selRange == null) return;
            //

            EditableVisualPointInfo startPoint = selRange.StartPoint;
            EditableVisualPointInfo endPoint = selRange.EndPoint;
            //
            if (!selRange.IsOnTheSameLine)
            {
                TextLine line = startPoint.Line;
                TextLine end_line = endPoint.Line;

                RunStyle runstyle = _lineEditor.CurrentSpanStyle;

                while (line.LineNumber <= end_line.LineNumber)
                {
                    //TODO, review here...
                    var whitespace = new TextRun(runstyle, "    ".ToCharArray());
                    line.AddFirst(whitespace);
                    line.TextLineReCalculateActualLineSize();
                    line.RefreshInlineArrange();
                    line = line.Next;//move to next line
                }

                return;//finish here
            }

        }
        public void SplitCurrentLineIntoNewLine()
        {
            VisualSelectionRangeSnapShot removedRange = RemoveSelectedText();
            _commandHistoryList.AddDocAction(
                 new DocActionSplitToNewLine(_lineEditor.LineNumber, _lineEditor.CharIndex));
            _lineEditor.SplitToNewLine();
            CurrentLineNumber++;
            _updateJustCurrentLine = false;
            //
            NotifyContentSizeChanged();
        }
        public void AddTextLine(PlainTextLine textline)
        {

            //TODO: replace 1 tab with 4 blank spaces? 
            _updateJustCurrentLine = true;
            VisualSelectionRangeSnapShot removedRange = RemoveSelectedText();
            int startLineNum = _lineEditor.LineNumber;
            int startCharIndex = _lineEditor.CharIndex;
            bool isRecordingHx = EnableUndoHistoryRecording;
            EnableUndoHistoryRecording = false;

            //---------------------
            //TODO: review here again, use pool
            System.Text.StringBuilder stbuilder = new System.Text.StringBuilder();
            textline.CopyText(stbuilder);
            char[] textbuffer = stbuilder.ToString().ToCharArray();
            _lineEditor.AddTextSpan(textbuffer);
            //---------------------


            CopyRun copyRun = new CopyRun(textbuffer);
            EnableUndoHistoryRecording = isRecordingHx;
            _commandHistoryList.AddDocAction(
                new DocActionInsertRuns(copyRun, startLineNum, startCharIndex,
                    _lineEditor.LineNumber, _lineEditor.CharIndex));
            _updateJustCurrentLine = false;
            //
            NotifyContentSizeChanged();
        }
        public TextSpanStyle GetFirstTextStyleInSelectedRange()
        {
            //TODO: review here again
            throw new NotSupportedException();
            //VisualSelectionRange selRange = SelectionRange;
            //if (selRange != null)
            //{
            //    if (_selectionRange.StartPoint.Run != null)
            //    {
            //        return _selectionRange.StartPoint.Run.SpanStyle;
            //    }
            //    else
            //    {
            //        return TextSpanStyle.Empty;
            //    }
            //}
            //else
            //{
            //    return TextSpanStyle.Empty;
            //}
        }
        public void DoFormatSelection(TextSpanStyle textStyle)
        {
            //int startLineNum = _textLineWriter.LineNumber;
            //int startCharIndex = _textLineWriter.CharIndex;
            SplitSelectedText();
            VisualSelectionRange selRange = SelectionRange;
            if (selRange != null)
            {
                RunStyle runstyle = new RunStyle(_textLayer.TextServices)
                {
                    ReqFont = textStyle.ReqFont,
                    FontColor = textStyle.FontColor,
                    ContentHAlign = textStyle.ContentHAlign
                };

                foreach (Run r in selRange.GetPrintableTextRunIter())
                {
                    r.SetStyle(runstyle);
                }

                _updateJustCurrentLine = _selectionRange.IsOnTheSameLine;
                CancelSelect();
                //?
                //CharIndex++;
                //CharIndex--;
            }
        }
        public void DoFormatSelection(TextSpanStyle textStyle, FontStyle toggleFontStyle)
        {
            ////int startLineNum = _textLineWriter.LineNumber;
            ////int startCharIndex = _textLineWriter.CharIndex;
            //SplitSelectedText();
            //VisualSelectionRange selRange = SelectionRange;
            //if (selRange != null)
            //{
            //    foreach (EditableRun r in selRange.GetPrintableTextRunIter())
            //    {
            //        RunStyle existingStyle = r.SpanStyle;
            //        switch (toggleFontStyle)
            //        {
            //            case FontStyle.Bold:
            //                if ((existingStyle.ReqFont.Style & FontStyle.Bold) != 0)
            //                {
            //                    //change to normal
            //                    RequestFont existingFont = existingStyle.ReqFont;
            //                    RequestFont newReqFont = new RequestFont(
            //                        existingFont.Name, existingFont.SizeInPoints,
            //                        existingStyle.ReqFont.Style & ~FontStyle.Bold); //clear bold

            //                    RunStyle textStyle2 = new RunStyle();
            //                    textStyle2.ReqFont = newReqFont;
            //                    textStyle2.ContentHAlign = textStyle.ContentHAlign;
            //                    textStyle2.FontColor = textStyle.FontColor;
            //                    r.SetStyle(textStyle2);
            //                    continue;//go next***
            //                }
            //                break;
            //            case FontStyle.Italic:
            //                if ((existingStyle.ReqFont.Style & FontStyle.Italic) != 0)
            //                {
            //                    //change to normal
            //                    RequestFont existingFont = existingStyle.ReqFont;
            //                    RequestFont newReqFont = new RequestFont(
            //                        existingFont.Name, existingFont.SizeInPoints,
            //                        existingStyle.ReqFont.Style & ~FontStyle.Italic); //clear italic

            //                    TextSpanStyle textStyle2 = new TextSpanStyle();
            //                    textStyle2.ReqFont = newReqFont;
            //                    textStyle2.ContentHAlign = textStyle.ContentHAlign;
            //                    textStyle2.FontColor = textStyle.FontColor;
            //                    r.SetStyle(textStyle2);
            //                    continue;//go next***
            //                }
            //                break;
            //        }
            //        r.SetStyle(textStyle);
            //    }
            //    _updateJustCurrentLine = _selectionRange.IsOnTheSameLine;
            //    CancelSelect();
            //    //?
            //    //CharIndex++;
            //    //CharIndex--;
            //}
        }

        public void AddMarkerSpan(VisualMarkerSelectionRange markerRange)
        {
            markerRange.BindToTextLayer(_textLayer);
            _textMarkerLayer.AddMarker(markerRange);
        }

        /// <summary>
        /// clear all marker
        /// </summary>
        public void ClearMarkers() => _textMarkerLayer?.Clear();
        public void RemoveMarkers(VisualMarkerSelectionRange marker)
        {
            _textMarkerLayer?.Remove(marker);
        }

        void JoinWithNextLine()
        {
            _lineEditor.JoinWithNextLine();
            //
            NotifyContentSizeChanged();
        }
        public void UndoLastAction() => _commandHistoryList.UndoLastAction();

        public void ReverseLastUndoAction() => _commandHistoryList.ReverseLastUndoAction();

#if DEBUG
        int dbug_BackSpaceCount = 0;
#endif
        public VisualSelectionRangeSnapShot DoDelete()
        {
            //recursive
#if DEBUG
            if (dbugEnableTextManRecorder)
            {
                _dbugActivityRecorder.WriteInfo("TxLMan::DoDelete");
                _dbugActivityRecorder.BeginContext();
            }
#endif

            VisualSelectionRangeSnapShot removedRange = this.RemoveSelectedText();
            if (removedRange.IsEmpty())
            {
                _updateJustCurrentLine = true;

                char deletedChar = _lineEditor.DoDeleteOneChar();
                if (deletedChar == '\0')
                {
                    //end of this line
                    _commandHistoryList.AddDocAction(
                        new DocActionJoinWithNextLine(
                            _lineEditor.LineNumber, _lineEditor.CharIndex));

                    JoinWithNextLine();

                    _updateJustCurrentLine = false;
                }
                else
                {
                    _commandHistoryList.AddDocAction(
                        new DocActionDeleteChar(
                            deletedChar, _lineEditor.LineNumber, _lineEditor.CharIndex));

                    char nextChar = _lineEditor.NextChar;

                    if (nextChar != '\0')
                    {
                        if (!CanCaretStopOnThisChar(nextChar))
                        {
                            //TODO: review return range here again
                            return DoDelete();
                        }
                    }
                }
            }
#if DEBUG
            if (dbugEnableTextManRecorder) _dbugActivityRecorder.EndContext();
#endif
            NotifyContentSizeChanged();
            return removedRange;
        }
        public bool DoBackspace()
        {
#if DEBUG

            if (dbugEnableTextManRecorder)
            {
                dbug_BackSpaceCount++;
                _dbugActivityRecorder.WriteInfo("TxLMan::DoBackSpace");
                _dbugActivityRecorder.BeginContext();
            }
#endif

            VisualSelectionRangeSnapShot removeSelRange = this.RemoveSelectedText();
            if (!removeSelRange.IsEmpty())
            {
                CancelSelect();
                NotifyContentSizeChanged();
#if DEBUG
                if (dbugEnableTextManRecorder) _dbugActivityRecorder.EndContext();
#endif
                return true;
            }
            else
            {
                _updateJustCurrentLine = true;

                char deletedChar = _lineEditor.DoBackspaceOneChar();
                if (deletedChar == '\0')
                {
                    //end of current line 
                    if (!IsOnFirstLine)
                    {
                        CurrentLineNumber--;
                        DoEnd();
                        _commandHistoryList.AddDocAction(
                            new DocActionJoinWithNextLine(
                                _lineEditor.LineNumber, _lineEditor.CharIndex));
                        JoinWithNextLine();
                    }
                    NotifyContentSizeChanged();
#if DEBUG
                    if (dbugEnableTextManRecorder) _dbugActivityRecorder.EndContext();
#endif
                    return false;
                }
                else
                {
                    _commandHistoryList.AddDocAction(
                            new DocActionDeleteChar(
                                deletedChar, _lineEditor.LineNumber, _lineEditor.CharIndex));
                    NotifyContentSizeChanged();
#if DEBUG
                    if (dbugEnableTextManRecorder) _dbugActivityRecorder.EndContext();
#endif
                    return true;
                }
            }
        }

        public void ReplaceCurrentLineTextRun(IEnumerable<Run> runs)
        {
            _lineEditor.ReplaceCurrentLine(runs);
        }

        public void ReplaceLocalContent(int nBackSpace, string content)
        {
            if (content != null)
            {
                for (int i = 0; i < nBackSpace; i++)
                {
                    DoBackspace();
                }
                //------------------
                int j = content.Length;
                if (j > 0)
                {
                    for (int i = 0; i < j; i++)
                    {
                        char c = content[i];
                        _lineEditor.AddCharacter(c);
                        _commandHistoryList.AddDocAction(
                            new DocActionCharTyping(c, _lineEditor.LineNumber, _lineEditor.CharIndex));
                    }
                }
            }
        }
        public void AddTextToCurrentLine(PlainTextDocument doc)
        {
            int lineCount = 0;
            foreach (PlainTextLine line in doc.GetLineIter())
            {
                if (lineCount > 0)
                {
                    SplitCurrentLineIntoNewLine();
                }
                AddTextLine(line);
                lineCount++;
            }
        }
        public void AddTextRunsToCurrentLine(TextRangeCopy copyRange)
        {
            VisualSelectionRangeSnapShot removedRange = RemoveSelectedText();
            int startLineNum = _lineEditor.LineNumber;
            int startCharIndex = _lineEditor.CharIndex;
            bool isRecordingHx = EnableUndoHistoryRecording;
            EnableUndoHistoryRecording = false;

            if (copyRange.HasSomeRuns)
            {
                bool hasFirstLine = false;
                foreach (string line in copyRange.GetLineIter())
                {
                    if (hasFirstLine)
                    {
                        _lineEditor.SplitToNewLine();
                        CurrentLineNumber++;
                    }
                    _lineEditor.AddTextSpan(line);
                    hasFirstLine = true;
                }
            }

            EnableUndoHistoryRecording = isRecordingHx;
            _commandHistoryList.AddDocAction(
                new DocActionInsertRuns(copyRange, startLineNum, startCharIndex,
                    _lineEditor.LineNumber, _lineEditor.CharIndex));
            _updateJustCurrentLine = false;
            //
            NotifyContentSizeChanged();
        }
        public void AddTextRunToCurrentLine(CopyRun copyRun)
        {
            AddTextRunToCurrentLine(copyRun.RawContent);
        }
        public void AddTextRunToCurrentLine(char[] textbuffer)
        {
            _updateJustCurrentLine = true;
            VisualSelectionRangeSnapShot removedRange = RemoveSelectedText();
            int startLineNum = _lineEditor.LineNumber;
            int startCharIndex = _lineEditor.CharIndex;
            bool isRecordingHx = EnableUndoHistoryRecording;
            EnableUndoHistoryRecording = false;
            _lineEditor.AddTextSpan(textbuffer);

            CopyRun copyRun = new CopyRun(textbuffer);
            EnableUndoHistoryRecording = isRecordingHx;
            _commandHistoryList.AddDocAction(
                new DocActionInsertRuns(copyRun, startLineNum, startCharIndex,
                    _lineEditor.LineNumber, _lineEditor.CharIndex));
            _updateJustCurrentLine = false;
            //
            NotifyContentSizeChanged();
        }
        public void AddTextRunToCurrentLine(Run run)
        {
            _updateJustCurrentLine = true;
            VisualSelectionRangeSnapShot removedRange = RemoveSelectedText();
            int startLineNum = _lineEditor.LineNumber;
            int startCharIndex = _lineEditor.CharIndex;
            bool isRecordingHx = EnableUndoHistoryRecording;
            EnableUndoHistoryRecording = false;
            _lineEditor.AddTextSpan(run);


            EnableUndoHistoryRecording = isRecordingHx;
            _commandHistoryList.AddDocAction(
                new DocActionInsertRuns(run.CreateCopy(), startLineNum, startCharIndex,
                    _lineEditor.LineNumber, _lineEditor.CharIndex));
            _updateJustCurrentLine = false;
            //
            NotifyContentSizeChanged();
        }
        public void Clear()
        {
            //1.
            CancelSelect();
            _textLayer.Clear();
            _lineEditor.Clear();
            //
            NotifyContentSizeChanged();
        }
    }
}