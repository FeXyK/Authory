using System;

namespace AuthoryServer
{
    public struct Vector3
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }
        public static Vector3 operator /(Vector3 lhs, float rhs) => new Vector3(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);
        public static Vector3 operator *(Vector3 lhs, float rhs) => new Vector3(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
        public static Vector3 operator +(Vector3 lhs, float rhs) => new Vector3(lhs.X + rhs, lhs.Y + rhs, lhs.Z + rhs);
        public static Vector3 operator -(Vector3 lhs, float rhs) => new Vector3(lhs.X - rhs, lhs.Y - rhs, lhs.Z - rhs);
        public static Vector3 operator /(Vector3 lhs, Vector3 rhs) => new Vector3(lhs.X / rhs.X, lhs.Y / rhs.Y, lhs.Z / rhs.Z);
        public static Vector3 operator *(Vector3 lhs, Vector3 rhs) => new Vector3(lhs.X * rhs.X, lhs.Y * rhs.Y, lhs.Z * rhs.Z);
        public static Vector3 operator +(Vector3 lhs, Vector3 rhs) => new Vector3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        public static Vector3 operator -(Vector3 lhs, Vector3 rhs) => new Vector3(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);

        private static Random rand = new Random();


        public Vector3(float x = 0, float y = 0, float z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Set(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Set(Vector3 pos)
        {
            this.X = pos.X;
            this.Y = pos.Y;
            this.Z = pos.Z;
        }

        public static float Dot(Vector3 p1, Vector3 p2)
        {
            return p1.X * p1.Z + p2.X * p2.Z;
        }

        public static float SqrDistance(Vector3 p1, Vector3 p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Z - p2.Z) * (p1.Z - p2.Z);
        }

        public static float Distance(Vector3 p1, Vector3 p2)
        {
            return (float)Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Z - p2.Z) * (p1.Z - p2.Z));
        }

        public static Vector3 RandomRangeSphere(int min, int max)
        {
            return new Vector3(new Random().Next(min, max), new Random().Next(min, max), new Random().Next(min, max));
        }

        public static Vector3 f_RandomRangeCircle()
        {
            int angle = rand.Next(0, 360);
            return new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));
        }

        public static Vector3 RandomRangeCircle(int min, int max)
        {
            //return new Vector3(0, 0, 0);
            return new Vector3(new Random().Next(min, max), 0, new Random().Next(min, max));
        }

        public static Vector3 RandomRangeSquare(int min, int max)
        {
            return new Vector3(new Random().Next(min, max), 0, new Random().Next(min, max));
        }

        public override string ToString()
        {
            return string.Format("X:{0,5}, Y:{1,5}, Z:{2,5}", X, Y, Z);
        }

        public Vector3 Normalize()
        {
            Y = 0;
            float dist = (float)Math.Sqrt(X * X + Z * Z) + 1;
            X /= dist;
            Z /= dist;
            return this;
        }

        public float SqrMagnitude()
        {
            return X * X + Y * Y + Z * Z;
        }
    }
}
