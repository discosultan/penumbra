using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics.Triangulation
{
	struct Vertex
	{
		public readonly Vector2 Position;
		public readonly int Index;

		public Vertex(Vector2 position, int index)
		{
			Position = position;
			Index = index;
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() != typeof(Vertex)) 
				return false;
			return Equals((Vertex)obj);
		}

		public bool Equals(Vertex obj)
		{
			return obj.Position.Equals(Position) && obj.Index == Index;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Position.GetHashCode() * 397) ^ Index;
			}
		}

		public override string ToString()
		{
			return $"{Position} ({Index})";
		}
	}
}