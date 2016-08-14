using UnityEngine;
using System.Collections;

public static class Utilities
{
    static public bool PointOnRect(double x, double y, double minX, double minY, double maxX, double maxY, bool check, ref double resultX, ref double resultY)
    {
        //assert minX <= maxX;
        //assert minY <= maxY;
        if (check && (minX <= x && x <= maxX) && (minY <= y && y <= maxY))
            return false;


        double midX = (minX + maxX) / 2.0;
        double midY = (minY + maxY) / 2.0;
        // if (midX - x == 0) -> m == ±Inf -> minYx/maxYx == x (because value / ±Inf = ±0)
        double m = (midY - y) / (midX - x);

        if (x <= midX)
        { // check "left" side
            double minXy = m * (minX - x) + y;

            if (minY < minXy && minXy < maxY)
            {
                resultX = minX;
                resultY = minXy;

                return true;
            }
        }

        if (x >= midX)
        { // check "right" side
            double maxXy = m * (maxX - x) + y;

            if (minY < maxXy && maxXy < maxY)
            {
                resultX = maxX;
                resultY = maxXy;

                return true;
            }
        }

        if (y <= midY)
        { // check "top" side
            double minYx = (minY - y) / m + x;

            if (minX < minYx && minYx < maxX)
            {
                resultX = minYx;
                resultY = minY;

                return true;
            }
        }

        if (y >= midY)
        { // check "bottom" side
            double maxYx = (maxY - y) / m + x;

            if (minX < maxYx && maxYx < maxX)
            {
                resultX = maxYx;
                resultY = maxY;
                return true;
            }
        }

        return false;
    }

    static public bool LineRectIntersection(Vector2 lineStartPoint, Vector2 lineEndPoint, Rect rectangle, ref double resultX, ref double resultY)
    {
        bool rectContainsLineStartPoint = rectangle.Contains(lineStartPoint);
        bool rectContainsLineEndPoint = rectangle.Contains(lineEndPoint);

        if (!(rectContainsLineStartPoint ^ rectContainsLineEndPoint))
            return false;

        if ((lineEndPoint - lineStartPoint).magnitude == 0.0F)
            return false;

        Vector2 minXLinePoint = (lineStartPoint.x <= lineEndPoint.x) ? (lineStartPoint) : (lineEndPoint);
        Vector2 maxXLinePoint = (lineStartPoint.x <= lineEndPoint.x) ? (lineEndPoint) : (lineStartPoint);
        Vector2 minYLinePoint = (lineStartPoint.y <= lineEndPoint.y) ? (lineStartPoint) : (lineEndPoint);
        Vector2 maxYLinePoint = (lineStartPoint.y <= lineEndPoint.y) ? (lineEndPoint) : (lineStartPoint);

        Vector2 rectTopLeft = new Vector2(rectangle.xMin, rectangle.yMax);
        Vector2 rectTopRight = new Vector2(rectangle.xMax, rectangle.yMax);
        Vector2 rectBottomLeft = new Vector2(rectangle.xMin, rectangle.yMin);
        Vector2 rectBottomRight = new Vector2(rectangle.xMax, rectangle.yMin);

        //double minY = Mathf.Min(lineStartPoint.x)
        double rectMaxX = rectangle.xMax;
        double rectMinX = rectangle.xMin;
        double rectMaxY = rectangle.yMax;
        double rectMinY = rectangle.yMin;

        if (minXLinePoint.x <= rectMaxX && rectMaxX <= maxXLinePoint.x)
        {
            double m = (maxXLinePoint.y - minXLinePoint.y) / (maxXLinePoint.x - minXLinePoint.x);

            double intersectionY = ((rectMaxX - ((double)minXLinePoint.x)) * m) + ((double)minXLinePoint.y);

            if(rectBottomRight.y <= intersectionY && intersectionY <= rectTopRight.y)
            {
                resultX = rectMaxX;
                resultY = intersectionY;

                return true;
            }
        }

        if (minXLinePoint.x <= rectMinX && rectMinX <= maxXLinePoint.x)
        {
            double m = (maxXLinePoint.y - minXLinePoint.y) / (maxXLinePoint.x - minXLinePoint.x);

            double intersectionY = ((rectMinX - ((double)minXLinePoint.x)) * m) + ((double)minXLinePoint.y);

            if (rectBottomLeft.y <= intersectionY && intersectionY <= rectTopLeft.y)
            {
                resultX = rectMinX;
                resultY = intersectionY;

                return true;
            }
        }

        if (minYLinePoint.y <= rectMaxY && rectMaxY <= maxYLinePoint.y)
        {
            double rm = (maxYLinePoint.x - minYLinePoint.x) / (maxYLinePoint.y - minYLinePoint.y);

            double intersectionX = ((rectMaxY - ((double)minYLinePoint.y)) * rm) + ((double)minYLinePoint.x);

            if (rectTopLeft.x <= intersectionX && intersectionX <= rectTopRight.x)
            {
                resultX = intersectionX;
                resultY = rectMaxY;

                return true;
            }
        }

        if (minYLinePoint.y <= rectMinY && rectMinY <= maxYLinePoint.y)
        {
            double rm = (maxYLinePoint.x - minYLinePoint.x) / (maxYLinePoint.y - minYLinePoint.y);

            double intersectionX = ((rectMinY - ((double)minYLinePoint.y)) * rm) + ((double)minYLinePoint.x);

            if (rectBottomLeft.x <= intersectionX && intersectionX <= rectBottomRight.x)
            {
                resultX = intersectionX;
                resultY = rectMinY;

                return true;
            }
        }

        return false;
    }
}
