using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Model.Entity
{
    public class Pos
    {
        /// <summary>
        /// The class for the position of the entities on the simulation board with given coordinates 
        /// </summary>
        public int X { get; set; }
        public int Y { get; set; }

        public Pos() : this(0, 0) { }
        public Pos(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public Pos(Pos other)
        {
            this.X = other.X;
            this.Y = other.Y;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Pos);
        }

        public bool Equals(Pos other)
        {
            return other != null &&
                   X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        ///<summary>
        ///operators of the class
        ///</summary>
        public static Pos operator +(Pos A, Pos B) => new Pos(A.X + B.X, A.Y + B.Y);
        public static Pos operator -(Pos A, Pos B) => new Pos(A.X - B.X, A.Y - B.Y);

        public static bool operator ==(Pos A, Pos B) => A.Equals(B);
        public static bool operator !=(Pos A, Pos B) => !A.Equals(B);
    }
}
