﻿//2014 BSD,WinterDev   
using System;
using System.Collections.Generic; 
using System.Text;
using PixelFarm.VectorMath;

namespace PixelFarm.Agg
{
    public struct RectangleDouble
    {
        public double Left, Bottom, Right, Top;

        public static readonly RectangleDouble ZeroIntersection = new RectangleDouble(double.MaxValue, double.MaxValue, double.MinValue, double.MinValue);

        public RectangleDouble(double left, double bottom, double right, double top)
        {
            this.Left = left;
            this.Bottom = bottom;
            this.Right = right;
            this.Top = top;
        }

        public RectangleDouble(RectangleInt intRect)
        {
            Left = intRect.Left;
            Bottom = intRect.Bottom;
            Right = intRect.Right;
            Top = intRect.Top;
        }

        public void SetRect(double left, double bottom, double right, double top)
        {
            init(left, bottom, right, top);
        }

        public static bool operator ==(RectangleDouble a, RectangleDouble b)
        {
            if (a.Left == b.Left && a.Bottom == b.Bottom && a.Right == b.Right && a.Top == b.Top)
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(RectangleDouble a, RectangleDouble b)
        {
            if (a.Left != b.Left || a.Bottom != b.Bottom || a.Right != b.Right || a.Top != b.Top)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return new { x1 = Left, x2 = Right, y1 = Bottom, y2 = Top }.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(RectangleDouble))
            {
                return this == (RectangleDouble)obj;
            }
            return false;
        }

        public bool Equals(RectangleDouble other, double epsilon)
        {
            return Math.Abs(Left - other.Left) <= epsilon
                && Math.Abs(Bottom - other.Bottom) <= epsilon
                && Math.Abs(Right - other.Right) <= epsilon
                && Math.Abs(Top - other.Top) <= epsilon;
        }

        public void init(double left, double bottom, double right, double top)
        {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }

        // This function assumes the rect is normalized
        public double Width
        {
            get
            {
                return Right - Left;
            }
        }

        // This function assumes the rect is normalized
        public double Height
        {
            get
            {
                return Top - Bottom;
            }
        }

        public RectangleDouble normalize()
        {
            double t;
            if (Left > Right) { t = Left; Left = Right; Right = t; }
            if (Bottom > Top) { t = Bottom; Bottom = Top; Top = t; }
            return this;
        }

        public bool clip(RectangleDouble r)
        {
            if (Right > r.Right) Right = r.Right;
            if (Top > r.Top) Top = r.Top;
            if (Left < r.Left) Left = r.Left;
            if (Bottom < r.Bottom) Bottom = r.Bottom;
            return Left <= Right && Bottom <= Top;
        }

        public bool is_valid()
        {
            return Left <= Right && Bottom <= Top;
        }

        public bool Contains(double x, double y)
        {
            return (x >= Left && x <= Right && y >= Bottom && y <= Top);
        }

        public bool Contains(RectangleDouble innerRect)
        {
            if (Contains(innerRect.Left, innerRect.Bottom) && Contains(innerRect.Right, innerRect.Top))
            {
                return true;
            }

            return false;
        }

        public bool Contains(Vector2 position)
        {
            return Contains(position.x, position.y);
        }

        public bool IntersectRectangles(RectangleDouble rectToCopy, RectangleDouble rectToIntersectWith)
        {
            Left = rectToCopy.Left;
            Bottom = rectToCopy.Bottom;
            Right = rectToCopy.Right;
            Top = rectToCopy.Top;

            if (Left < rectToIntersectWith.Left) Left = rectToIntersectWith.Left;
            if (Bottom < rectToIntersectWith.Bottom) Bottom = rectToIntersectWith.Bottom;
            if (Right > rectToIntersectWith.Right) Right = rectToIntersectWith.Right;
            if (Top > rectToIntersectWith.Top) Top = rectToIntersectWith.Top;

            if (Left < Right && Bottom < Top)
            {
                return true;
            }

            return false;
        }

        public bool IntersectWithRectangle(RectangleDouble rectToIntersectWith)
        {
            if (Left < rectToIntersectWith.Left) Left = rectToIntersectWith.Left;
            if (Bottom < rectToIntersectWith.Bottom) Bottom = rectToIntersectWith.Bottom;
            if (Right > rectToIntersectWith.Right) Right = rectToIntersectWith.Right;
            if (Top > rectToIntersectWith.Top) Top = rectToIntersectWith.Top;

            if (Left < Right && Bottom < Top)
            {
                return true;
            }

            return false;
        }

        public void unite_rectangles(RectangleDouble r1, RectangleDouble r2)
        {
            Left = r1.Left;
            Bottom = r1.Bottom;
            Right = r1.Right;
            Right = r1.Top;
            if (Right < r2.Right) Right = r2.Right;
            if (Top < r2.Top) Top = r2.Top;
            if (Left > r2.Left) Left = r2.Left;
            if (Bottom > r2.Bottom) Bottom = r2.Bottom;
        }

        public void ExpandToInclude(RectangleDouble rectToInclude)
        {
            if (Right < rectToInclude.Right) Right = rectToInclude.Right;
            if (Top < rectToInclude.Top) Top = rectToInclude.Top;
            if (Left > rectToInclude.Left) Left = rectToInclude.Left;
            if (Bottom > rectToInclude.Bottom) Bottom = rectToInclude.Bottom;
        }

        public void ExpandToInclude(double x, double y)
        {
            if (Right < x) Right = x;
            if (Top < y) Top = y;
            if (Left > x) Left = x;
            if (Bottom > y) Bottom = y;
        }

        public void Inflate(double inflateSize)
        {
            Left = Left - inflateSize;
            Bottom = Bottom - inflateSize;
            Right = Right + inflateSize;
            Top = Top + inflateSize;
        }

        public void Offset(Vector2 offset)
        {
            Offset(offset.x, offset.y);
        }

        public void Offset(double x, double y)
        {
            Left = Left + x;
            Bottom = Bottom + y;
            Right = Right + x;
            Top = Top + y;
        }

        static public RectangleDouble operator *(RectangleDouble a, double b)
        {
            return new RectangleDouble(a.Left * b, a.Bottom * b, a.Right * b, a.Top * b);
        }

        static public RectangleDouble operator *(double b, RectangleDouble a)
        {
            return new RectangleDouble(a.Left * b, a.Bottom * b, a.Right * b, a.Top * b);
        }

        public double XCenter
        {
            get { return (Right - Left) / 2; }
        }

        

        public override string ToString()
        {
            return string.Format("L:{0}, B:{1}, R:{2}, T:{3}", Left, Bottom, Right, Top);
        }
    }
}
