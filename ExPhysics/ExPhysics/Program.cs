using ExMath.Coordinate;
using System;
using System.Collections.Generic;

namespace ExPhysics
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            float a = 1;
            float b = 2;
            Console.WriteLine($"a = {a}, b = {b}");
            Swap(ref a, ref b);
            Console.WriteLine($"a = {a}, b = {b}");
            float f = 1f / -0f;
            Console.WriteLine(f);

            Vector3 point;
            Ray ray = new Ray(new Vector3(-1, 0.5f, 0), new Vector3(1, 0.5f, 0));
            Collision sphere = new SphereCollision(Vector3.zero, 0.5f);
            if(ray.TryCast(sphere, out point))
            {
                Console.WriteLine("Sphere: " + point);
            }
            Collision cube = new CubeCollision(Vector3.zero, Vector3.one);
            if (ray.TryCast(cube, out point))
            {
                Console.WriteLine("Cube: " + point);
            }

            //List<int> list = new List<int>() { 1, 2, 3 };
            //list.Sort(delegate (int x, int y) { return x - y; });
            //for(int i = 0; i < list.Count; i++)
            //{
            //    Console.Write(string.Format("{0:000}", list[i]));
            //}

            Console.ReadLine();

        }

        static void Swap(ref float a, ref float b)
        {
            var temp = a;
            a = b;
            b = temp;
        }
    }
}
