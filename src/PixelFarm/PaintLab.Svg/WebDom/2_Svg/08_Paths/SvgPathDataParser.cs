﻿//Apache2, 2014-2017, WinterDev

using System;
using System.Collections.Generic;
namespace LayoutFarm.Svg.Pathing
{
    public class SvgPathDataParser
    {

#if DEBUG
        static int dbugCounter = 0;
#endif
        protected virtual void OnMoveTo(float x, float y, bool relative)
        {
        }
        protected virtual void OnLineTo(float x, float y, bool relative)
        {
        }
        protected virtual void OnHLineTo(float x, bool relative)
        {
        }
        protected virtual void OnVLineTo(float y, bool relative)
        {
        }
        protected virtual void OnCloseFigure()
        {
        }
        protected virtual void OnArc(float r1, float r2,
            float xAxisRotation,
            int largeArcFlag,
            int sweepFlags, float x, float y, bool isRelative)
        {

        }
        protected virtual void OnCurveToCubic(
            float x1, float y1,
            float x2, float y2, 
            float x, float y, bool isRelative)
        {

        }
        protected virtual void OnCurveToCubicSmooth(
            float x2, float y2,
            float x, float y, bool isRelative)
        {

        }

        protected virtual void OnCurveToQuadratic(
            float x1, float y1, 
            float x, float y, bool isRelative)
        {

        }
        protected virtual void OnCurveToQuadraticSmooth(
            float x, float y, bool isRelative)
        {

        }


        List<float> _reusable_nums = new List<float>();
        public void Parse(char[] pathDataBuffer)
        {
            //parse pathdata to pathsegments
            //List<SvgPathSeg> pathSegments = new List<SvgPathSeg>();

            int j = pathDataBuffer.Length;
            int currentState = 0;
            for (int i = 0; i < j;)
            {
                //lex and parse
                char c = pathDataBuffer[i];
                switch (currentState)
                {
                    case 0:
                        {
                            //init state
                            switch (c)
                            {
                                case 'M':
                                case 'm':
                                    {
                                        //move to 
                                        ParseNumberList(pathDataBuffer, i + 1, out i, _reusable_nums);

                                        int numCount = _reusable_nums.Count;
                                        if ((numCount % 2) == 0)
                                        {
                                            bool isRelative = c == 'm';
                                            for (int m1 = 0; m1 < numCount;)
                                            {
                                                //var moveTo = new SvgPathSegMoveTo(
                                                // numbers[m1],
                                                // numbers[m1 + 1]); 
                                                //moveTo.IsRelative = isRelative; 
                                                //pathSegments.Add(moveTo);
                                                OnMoveTo(_reusable_nums[m1], _reusable_nums[m1 + 1], isRelative);
                                                m1 += 2;
                                            }
                                        }
                                        else
                                        {
                                            //error 
                                            throw new NotSupportedException();
                                        }

                                        _reusable_nums.Clear();//reset
                                    }
                                    break;
                                case 'L':
                                case 'l':
                                    {
                                        //line to
                                        ParseNumberList(pathDataBuffer, i + 1, out i, _reusable_nums);

                                        int numCount = _reusable_nums.Count;
                                        if ((numCount % 2) == 0)
                                        {
                                            bool isRelative = c == 'l';
                                            for (int m1 = 0; m1 < numCount;)
                                            {
                                                OnLineTo(_reusable_nums[m1], _reusable_nums[m1 + 1], isRelative);
                                                //var lineTo = new SvgPathSegLineTo(
                                                // numbers[m1], numbers[m1 + 1]);
                                                //lineTo.IsRelative = isRelative;
                                                //pathSegments.Add(lineTo);
                                                m1 += 2;
                                            }
                                        }
                                        else
                                        {
                                            //error 
                                            throw new NotSupportedException();
                                        }

                                        _reusable_nums.Clear();//reset
                                    }
                                    break;
                                case 'H':
                                case 'h':
                                    {
                                        ParseNumberList(pathDataBuffer, i + 1, out i, _reusable_nums);

                                        int numCount = _reusable_nums.Count;
                                        if (numCount > 0)
                                        {
                                            bool isRelative = c == 'h';
                                            for (int m1 = 0; m1 < numCount;)
                                            {
                                                OnHLineTo(_reusable_nums[m1], isRelative);

                                                //var v = new SvgPathSegLineToHorizontal(
                                                //numbers[m1]);
                                                //v.IsRelative = isRelative;
                                                //pathSegments.Add(v);
                                                m1++;
                                            }

                                        }
                                        else
                                        {  //error 
                                            throw new NotSupportedException();
                                        }
                                        _reusable_nums.Clear();//reset
                                    }
                                    break;
                                case 'V':
                                case 'v':
                                    {
                                        ParseNumberList(pathDataBuffer, i + 1, out i, _reusable_nums);
                                        int numCount = _reusable_nums.Count;
                                        if (numCount > 0)
                                        {
                                            bool isRelative = c == 'v';
                                            for (int m1 = 0; m1 < numCount;)
                                            {
                                                OnVLineTo(_reusable_nums[m1], isRelative);
                                                //var v = new SvgPathSegLineToVertical(
                                                //numbers[m1]);
                                                //v.IsRelative = isRelative;
                                                //pathSegments.Add(v);
                                                m1++;
                                            }

                                        }
                                        else
                                        {  //error 
                                            throw new NotSupportedException();
                                        }
                                        _reusable_nums.Clear();//reset
                                    }
                                    break;
                                case 'Z':
                                case 'z':
                                    {
                                        OnCloseFigure();
                                        //pathSegments.Add(new SvgPathSegClosePath());
                                        i++;
                                    }
                                    break;
                                case 'A':
                                case 'a':
                                    {
                                        ParseNumberList(pathDataBuffer, i + 1, out i, _reusable_nums);
                                        int numCount = _reusable_nums.Count;
                                        if ((numCount % 7) == 0)
                                        {
                                            bool isRelative = c == 'a';
                                            for (int m1 = 0; m1 < numCount;)
                                            {
                                                OnArc(_reusable_nums[m1], _reusable_nums[m1 + 1],
                                                   _reusable_nums[m1 + 2], (int)_reusable_nums[m1 + 3], (int)_reusable_nums[m1 + 4],
                                                   _reusable_nums[m1 + 5], _reusable_nums[m1 + 6], isRelative);

                                                //var arc = new SvgPathSegArc(
                                                //   numbers[m1], numbers[m1 + 1],
                                                //   numbers[m1 + 2], (int)numbers[m1 + 3], (int)numbers[m1 + 4],
                                                //   numbers[m1 + 5], numbers[m1 + 6]);
                                                //arc.IsRelative = isRelative;
                                                //pathSegments.Add(arc);

                                                m1 += 7;
                                            }

                                        }
                                        else
                                        {
                                            throw new NotSupportedException();
                                        }


                                        _reusable_nums.Clear();
                                    }
                                    break;
                                case 'C':
                                case 'c':
                                    {
#if DEBUG
                                        dbugCounter++;
#endif

                                        ParseNumberList(pathDataBuffer, i + 1, out i, _reusable_nums);
                                        int numCount = _reusable_nums.Count;
                                        if ((numCount % 6) == 0)
                                        {

                                            bool isRelative = c == 'c';
                                            for (int m1 = 0; m1 < numCount;)
                                            {
                                                OnCurveToCubic(_reusable_nums[m1], _reusable_nums[m1 + 1],
                                                  _reusable_nums[m1 + 2], _reusable_nums[m1 + 3],
                                                  _reusable_nums[m1 + 4], _reusable_nums[m1 + 5], isRelative);
                                                //var squadCurve = new SvgPathSegCurveToCubic(
                                                //  numbers[m1], numbers[m1 + 1],
                                                //  numbers[m1 + 2], numbers[m1 + 3],
                                                //  numbers[m1 + 4], numbers[m1 + 5]);
                                                //squadCurve.IsRelative = isRelative;
                                                //pathSegments.Add(squadCurve);
                                                m1 += 6;
                                            }

                                        }
                                        else
                                        {
                                            throw new NotSupportedException();
                                        }
                                        _reusable_nums.Clear();
                                    }
                                    break;
                                case 'Q':
                                case 'q':
                                    {
                                        ParseNumberList(pathDataBuffer, i + 1, out i, _reusable_nums);
                                        int numCount = _reusable_nums.Count;
                                        if ((numCount % 4) == 0)
                                        {
                                            bool isRelative = c == 'q';

                                            for (int m1 = 0; m1 < numCount;)
                                            {
                                                //var quadCurve = new SvgPathSegCurveToQuadratic(
                                                // numbers[m1], numbers[m1 + 1],
                                                // numbers[m1 + 2], numbers[m1 + 3]);
                                                //quadCurve.IsRelative = isRelative;
                                                //pathSegments.Add(quadCurve);

                                                OnCurveToQuadratic(_reusable_nums[m1], _reusable_nums[m1 + 1],
                                                 _reusable_nums[m1 + 2], _reusable_nums[m1 + 3], isRelative);

                                                m1 += 4;

                                            }

                                        }
                                        else
                                        {
                                            throw new NotSupportedException();
                                        }
                                        _reusable_nums.Clear();
                                    }
                                    break;
                                case 'S':
                                case 's':
                                    {
#if DEBUG
                                        dbugCounter++;
#endif

                                        ParseNumberList(pathDataBuffer, i + 1, out i, _reusable_nums);
                                        int numCount = _reusable_nums.Count;
                                        if ((numCount % 4) == 0)
                                        {
                                            bool isRelative = c == 's';
                                            for (int m1 = 0; m1 < numCount;)
                                            {
                                                //var scubicCurve = new SvgPathSegCurveToCubicSmooth(
                                                //   numbers[m1], numbers[m1 + 1],
                                                //   numbers[m1 + 2], numbers[m1 + 3]);
                                                //scubicCurve.IsRelative = isRelative;
                                                //pathSegments.Add(scubicCurve);

                                                OnCurveToCubicSmooth(_reusable_nums[m1], _reusable_nums[m1 + 1],
                                                   _reusable_nums[m1 + 2], _reusable_nums[m1 + 3], isRelative);


                                                m1 += 4;
                                            }
                                        }
                                        else
                                        {
                                            throw new NotSupportedException();
                                        }
                                        _reusable_nums.Clear();

                                    }
                                    break;
                                case 'T':
                                case 't':
                                    {
                                        ParseNumberList(pathDataBuffer, i + 1, out i, _reusable_nums);
                                        int numCount = _reusable_nums.Count;
                                        if ((numCount % 2) == 0)
                                        {
                                            bool isRelative = c == 't';
                                            for (int m1 = 0; m1 < numCount;)
                                            {
                                                OnCurveToQuadraticSmooth(_reusable_nums[m1], _reusable_nums[m1 + 1], isRelative);

                                                //var squadCurve = new SvgPathSegCurveToQuadraticSmooth(
                                                //     numbers[m1], numbers[m1 + 1]);
                                                //squadCurve.IsRelative = isRelative;
                                                //pathSegments.Add(squadCurve);

                                                m1 += 2;
                                            }
                                        }
                                        else
                                        {
                                            throw new NotSupportedException();
                                        }

                                        _reusable_nums.Clear();
                                    }
                                    break;
                                default:
                                    {
                                    }
                                    break;
                            }
                        }
                        break;
                    default:
                        {
                        }
                        break;
                }
            }

        }
        static void ParseNumberList(char[] pathDataBuffer, int startIndex, out int latestIndex, List<float> numbers)
        {
            latestIndex = startIndex;
            //parse coordinate
            int j = pathDataBuffer.Length;
            int currentState = 0;
            int startCollectNumber = -1;
            for (; latestIndex < j; ++latestIndex)
            {
                //lex and parse
                char c = pathDataBuffer[latestIndex];
                if (c == ',' || char.IsWhiteSpace(c))
                {
                    if (startCollectNumber >= 0)
                    {
                        //collect latest number
                        string str = new string(pathDataBuffer, startCollectNumber, latestIndex - startCollectNumber);
                        float number;
                        float.TryParse(str, out number);
                        numbers.Add(number);
                        startCollectNumber = -1;
                        currentState = 0;//reset
                    }
                    continue;
                }

                switch (currentState)
                {
                    case 0:
                        {
                            //--------------------------
                            if (c == '-')
                            {
                                currentState = 1;//negative
                                startCollectNumber = latestIndex;
                            }
                            else if (char.IsNumber(c))
                            {
                                currentState = 2;//number found
                                startCollectNumber = latestIndex;
                            }
                            else
                            {
                                if (startCollectNumber >= 0)
                                {
                                    //collect latest number
                                    string str = new string(pathDataBuffer, startCollectNumber, latestIndex - startCollectNumber);
                                    float number;
                                    float.TryParse(str, out number);
                                    numbers.Add(number);
                                    startCollectNumber = -1;
                                    currentState = 0;//reset
                                }
                                return;
                            }
                        }
                        break;
                    case 1:
                        {
                            //after negative expect first number
                            if (char.IsNumber(c))
                            {
                                //ok collect next
                                currentState = 2;
                            }
                            else
                            {
                                //must found number
                                if (startCollectNumber >= 0)
                                {
                                    //collect latest number
                                    string str = new string(pathDataBuffer, startCollectNumber, latestIndex - startCollectNumber);
                                    float number;
                                    float.TryParse(str, out number);
                                    numbers.Add(number);
                                    startCollectNumber = -1;
                                    currentState = 0;//reset
                                }
                                return;
                            }
                        }
                        break;
                    case 2:
                        {
                            //number state
                            if (char.IsNumber(c))
                            {
                                //ok collect next
                            }
                            else if (c == '.')
                            {
                                //collect next
                                currentState = 3;
                            }
                            else if (c == '-')
                            {
                                if (startCollectNumber >= 0)
                                {
                                    //collect latest number
                                    string str = new string(pathDataBuffer, startCollectNumber, latestIndex - startCollectNumber);
                                    float number;
                                    float.TryParse(str, out number);
                                    numbers.Add(number);

                                    currentState = 1;//negative
                                    startCollectNumber = latestIndex;

                                }
                            }
                            else
                            {
                                //must found number
                                if (startCollectNumber >= 0)
                                {
                                    //collect latest number
                                    string str = new string(pathDataBuffer, startCollectNumber, latestIndex - startCollectNumber);
                                    float number;
                                    float.TryParse(str, out number);
                                    numbers.Add(number);
                                    startCollectNumber = -1;
                                    currentState = 0;//reset
                                }
                                return;
                            }
                        }
                        break;
                    case 3:
                        {
                            //after .
                            if (char.IsNumber(c))
                            {
                                //ok collect next
                            }
                            else if (c == '-')
                            {
                                if (startCollectNumber >= 0)
                                {
                                    //collect latest number
                                    string str = new string(pathDataBuffer, startCollectNumber, latestIndex - startCollectNumber);
                                    float number;
                                    float.TryParse(str, out number);
                                    numbers.Add(number);

                                    currentState = 1;//negative
                                    startCollectNumber = latestIndex;

                                }
                            }
                            else
                            {
                                if (startCollectNumber >= 0)
                                {
                                    //collect latest number
                                    string str = new string(pathDataBuffer, startCollectNumber, latestIndex - startCollectNumber);
                                    float number;
                                    float.TryParse(str, out number);
                                    numbers.Add(number);
                                    startCollectNumber = -1;
                                    currentState = 0;//reset
                                }
                                return;
                                //break hear
                            }
                        }
                        break;
                }
            }
            //-------------------
            if (startCollectNumber >= 0)
            {
                //collect latest number
                string str = new string(pathDataBuffer, startCollectNumber, latestIndex - startCollectNumber);
                float number;
                float.TryParse(str, out number);
                numbers.Add(number);
                startCollectNumber = -1;
                currentState = 0;//reset
            }
        }
    }
}