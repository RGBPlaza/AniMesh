using CanvasDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Constellations
{
    public class Node
    {
        // Mechanics
        public Vector2 Dir;
        public Point Position;
        public double Velocity;

        // Visuals
        static Random rand = new Random();
        public double Diameter;

        public Node()
        {
            Position = new Point(rand.NextDouble() * MainPage.canvasWidth, rand.NextDouble() * MainPage.canvasHeight);
            Dir = new Vector2(rand.Next(-32, 33), rand.Next(-32, 33));
            Velocity = (rand.NextDouble() * 2) + 1;
            Diameter = (rand.NextDouble() * 2) + 4;
        }

        public void ResetMechanics()
        {
            int sideToStart = rand.Next(0, 4);
            if (sideToStart == 0)
            {
                Position = new Point(-Diameter, rand.NextDouble() * (MainPage.canvasHeight - Diameter));
                Dir = new Vector2(rand.Next(0, 33), rand.Next(-32, 33));
            }
            else if (sideToStart == 1)
            {
                Position = new Point(rand.NextDouble() * (MainPage.canvasWidth - Diameter), -Diameter);
                Dir = new Vector2(rand.Next(-32, 33), rand.Next(0, 33));
            }
            else if (sideToStart == 2)
            {
                Position = new Point(MainPage.canvasWidth, rand.NextDouble() * (MainPage.canvasHeight - Diameter));
                Dir = new Vector2(rand.Next(-32, 0), rand.Next(-32, 33));
            }
            else
            {
                Position = new Point(rand.NextDouble() * (MainPage.canvasWidth - Diameter), MainPage.canvasHeight);
                Dir = new Vector2(rand.Next(-32, 33), rand.Next(-32, 0));
            }

            Velocity = (rand.NextDouble() * 2) + 1;
            Diameter = (rand.NextDouble() * 2) + 4;
        }

        public double GetDistanceFrom(Node node)
        {
            double dx = node.Position.X - Position.X;
            double dy = node.Position.Y - Position.Y;

            return Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
        }

        public void Update(double deltaTime)
        {
            Vector2 moveBy = Vector2.Multiply((float)(deltaTime * Velocity), (Vector2.Normalize(Dir)));
            Position.X += moveBy.X;
            Position.Y += moveBy.Y;

            if (Position.X < -Diameter || Position.X > MainPage.canvasWidth || Position.Y < -Diameter || Position.Y > MainPage.canvasHeight)
                ResetMechanics();
        }

    }
}
