using System.IO;

namespace Yuizumi.Icfpc2018
{
    public static class ModelFile
    {
        public static Matrix Load(string filename)
        {
            using (Stream stream = File.OpenRead(filename))
                return Load(stream);
        }

        public static Matrix Load(Stream stream)
        {
            int r = stream.StrictReadByte();
            Matrix matrix = Matrix.Empty(r);

            var bytes = new byte[(r * r * r + 7) / 8];
            stream.Read(bytes, 0, bytes.Length);

            for (int x = 0; x < r; x++)
            for (int y = 0; y < r; y++)
            for (int z = 0; z < r; z++) {
                int i = x * r * r + y * r + z;
                matrix[x, y, z] = (Voxel) ((bytes[i / 8] >> (i % 8)) & 1);
            }

            return matrix;
        }
    }
}
