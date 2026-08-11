using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Disk
{
    public static class FileUtilities
    {
        public static int[] ToArrayFromRectangle(Rectangle rect)
        {
            return [rect.X, rect.Y, rect.Width, rect.Height];
        }

        public static Rectangle ToRectangleFromArray(int[] array)
        {
            return new Rectangle(array[0], array[1], array[2], array[3]);
        }

        public static Size ToSizeFromArray(int[] array)
        {
            return new Size(array[0], array[1]);
        }

        public static int[] ToArrayFromSize(Size size)
        {
            return [size.Width, size.Height];
        }

    }
}
