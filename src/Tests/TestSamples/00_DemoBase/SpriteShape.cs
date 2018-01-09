﻿
//BSD, 2014-2018, WinterDev
//MattersHackers
//AGG 2.4

using PixelFarm.Drawing;
using PixelFarm.Agg.VertexSource;
using PixelFarm.VectorMath;
namespace PixelFarm.Agg
{


    //TODO: review here again***
    //move to SVG or renderVX

    public class SpriteShape
    {
        PathWriter path = new PathWriter();
        Color[] colors = new Color[100];
        int[] pathIndexList = new int[100];
        int numPaths = 0;
        RectD boundingRect;
        Vector2 center;

        VertexStore _lionVxs;
        public SpriteShape()
        {
        }


        public VertexStore Vxs
        {
            get
            {
                return _lionVxs;

            }
        }
        public int NumPaths
        {
            get
            {
                return numPaths;
            }
        }

        public RectD Bounds
        {
            get
            {
                return boundingRect;
            }
        }

        public Color[] Colors
        {
            get
            {
                return colors;
            }
        }

        public int[] PathIndexList
        {
            get
            {
                return pathIndexList;
            }
        }

        public Vector2 Center
        {
            get
            {
                return center;
            }
        }

        public void ParseLion()
        {
            numPaths = PixelFarm.Agg.LionDataStore.LoadLionData(path, colors, pathIndexList);
            _lionVxs = path.Vxs;
            PixelFarm.Agg.BoundingRect.GetBoundingRect(_lionVxs, pathIndexList, numPaths, out boundingRect);
            center.x = (boundingRect.Right - boundingRect.Left) / 2.0;
            center.y = (boundingRect.Top - boundingRect.Bottom) / 2.0;

            ////since lion is bottom-up
            ////we invert it
            //Transform.Affine aff = Transform.Affine.NewMatix(
            //    new Transform.AffinePlan(Transform.AffineMatrixCommand.Scale, 1, -1));
            //VertexStore newvxs = new VertexStore();
            //_lionVxs = aff.TransformToVxs(_lionVxs, newvxs);



        }
        public static void UnsafeDirectSetData(SpriteShape lion,
            int numPaths,
            PathWriter pathStore,
            Color[] colors,
            int[] pathIndice)
        {
            lion.path = pathStore;
            lion.colors = colors;
            lion.pathIndexList = pathIndice;
            lion.numPaths = numPaths;
            lion.UpdateBoundingRect();
        }
        void UpdateBoundingRect()
        {
            PixelFarm.Agg.BoundingRect.GetBoundingRect(path.Vxs, pathIndexList, numPaths, out boundingRect);
            center.x = (boundingRect.Right - boundingRect.Left) / 2.0;
            center.y = (boundingRect.Top - boundingRect.Bottom) / 2.0;
        }
    }
}