// lib3ds_vector.cs - Vector
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

using System;

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		public static void lib3ds_vector_make(float[] c, float x, float y, float z)
		{
			c[0]=x;
			c[1]=y;
			c[2]=z;
		}

		public static void lib3ds_vector_make(Lib3dsVertex c, float x, float y, float z)
		{
			c.x=x;
			c.y=y;
			c.z=z;
		}
		
		public static void lib3ds_vector_zero(float[] c)
		{
			for(int i=0; i<3; i++) c[i]=0.0f;
		}

		public static void lib3ds_vector_copy(float[] dst, float[] src)
		{
			for(int i=0; i<3; i++) dst[i]=src[i];
		}

		public static void lib3ds_vector_copy(Lib3dsVertex dst, float[] src)
		{
			dst.x=src[0];
			dst.y=src[1];
			dst.z=src[2];
		}

		public static void lib3ds_vector_copy(Lib3dsVertex dst, Lib3dsVertex src)
		{
			dst.x=src.x;
			dst.y=src.y;
			dst.z=src.z;
		}

		// Add two vectors.
		//
		// \param c Result.
		// \param a First addend.
		// \param b Second addend.
		public static void lib3ds_vector_add(float[] c, float[] a, float[] b)
		{
			for(int i=0; i<3; i++) c[i]=a[i]+b[i];
		}

		// Subtract two vectors.
		//
		// \param c Result.
		// \param a Addend.
		// \param b Minuend.
		public static void lib3ds_vector_sub(float[] c, float[] a, float[] b)
		{
			for(int i=0; i<3; i++) c[i]=a[i]-b[i];
		}

		public static void lib3ds_vector_sub(float[] c, Lib3dsVertex a, Lib3dsVertex b)
		{
			lib3ds_vector_sub(c, a.ToArray(), b.ToArray());
		}
		
		// Multiply a vector by a scalar.
		//
		// \param c Vector to be multiplied.
		// \param k Scalar.
		public static void lib3ds_vector_scalar_mul(float[] c, float[] a, float k)
		{
			for(int i=0; i<3; i++) c[i]=a[i]*k;
		}

		// Compute cross product.
		//
		// \param c Result.
		// \param a First vector.
		// \param b Second vector.
		public static void lib3ds_vector_cross(float[] c, float[] a, float[] b)
		{
			c[0]=a[1]*b[2]-a[2]*b[1];
			c[1]=a[2]*b[0]-a[0]*b[2];
			c[2]=a[0]*b[1]-a[1]*b[0];
		}

		// Compute dot product.
		//
		// \param a First vector.
		// \param b Second vector.
		//
		// \return Dot product.
		public static float lib3ds_vector_dot(float[] a, float[] b)
		{
			return (a[0]*b[0]+a[1]*b[1]+a[2]*b[2]);
		}

		// Compute length of vector.
		//
		// Computes |c| = sqrt(x*x + y*y + z*z)
		//
		// \param c Vector to compute.
		//
		// \return Length of vector.
		public static float lib3ds_vector_length(float[] c)
		{
			return ((float)Math.Sqrt(c[0]*c[0]+c[1]*c[1]+c[2]*c[2]));
		}

		// Normalize a vector.
		//
		// Scales a vector so that its length is 1.0.
		//
		// \param c Vector to normalize.
		public static void lib3ds_vector_normalize(float[] c)
		{
			float l=(float)Math.Sqrt(c[0]*c[0]+c[1]*c[1]+c[2]*c[2]);
			if(Math.Abs(l)<EPSILON)
			{
				if((c[0]>=c[1])&&(c[0]>=c[2]))
				{
					c[0]=1.0f;
					c[1]=c[2]=0.0f;
				}
				else
					if(c[1]>=c[2])
					{
						c[1]=1.0f;
						c[0]=c[2]=0.0f;
					}
					else
					{
						c[2]=1.0f;
						c[0]=c[1]=0.0f;
					}
			}
			else
			{
				float m=1.0f/l;
				c[0]*=m;
				c[1]*=m;
				c[2]*=m;
			}
		}

		// Compute a vector normal to two line segments.
		//
		// Computes the normal vector to the lines b-a and b-c.
		//
		// \param n Returned normal vector.
		// \param a Endpoint of first line.
		// \param b Base point of both lines.
		// \param c Endpoint of second line.
		public static void lib3ds_vector_normal(float[] n, float[] a, float[] b, float[] c)
		{
			float[] p=new float[3], q=new float[3];
			lib3ds_vector_sub(p, c, b);
			lib3ds_vector_sub(q, a, b);
			lib3ds_vector_cross(n, p, q);
			lib3ds_vector_normalize(n);
		}

		public static void lib3ds_vector_normal(float[] n, Lib3dsVertex a, Lib3dsVertex b, Lib3dsVertex c)
		{
			lib3ds_vector_normal(n, a.ToArray(), b.ToArray(), c.ToArray());
		}

		// Multiply a point by a transformation matrix.
		//
		// Applies the given transformation matrix to the given point.  With some
		// transformation matrices, a vector may also be transformed.
		//
		// \param c Result.
		// \param m Transformation matrix.
		// \param a Input point.
		public static void lib3ds_vector_transform(float[] c, float[,] m, float[] a)
		{
			c[0]=m[0, 0]*a[0]+m[1, 0]*a[1]+m[2, 0]*a[2]+m[3, 0];
			c[1]=m[0, 1]*a[0]+m[1, 1]*a[1]+m[2, 1]*a[2]+m[3, 1];
			c[2]=m[0, 2]*a[0]+m[1, 2]*a[1]+m[2, 2]*a[2]+m[3, 2];
		}

		// Multiply a point by a transformation matrix.
		//
		// Applies the given transformation matrix to the given point.  With some
		// transformation matrices, a vector may also be transformed.
		//
		// \param c Result.
		// \param m Transformation matrix.
		// \param a Input point.
		public static void lib3ds_vector_transform(float[] c, float[,] m, Lib3dsVertex a)
		{
			c[0]=m[0, 0]*a.x+m[1, 0]*a.y+m[2, 0]*a.z+m[3, 0];
			c[1]=m[0, 1]*a.x+m[1, 1]*a.y+m[2, 1]*a.z+m[3, 1];
			c[2]=m[0, 2]*a.x+m[1, 2]*a.y+m[2, 2]*a.z+m[3, 2];
		}

		// c[i] = min(c[i], a[i]);
		//
		// Computes minimum values of x,y,z independently.
		public static void lib3ds_vector_min(float[] c, float[] a)
		{
			for(int i=0; i<3; i++)
			{
				if(a[i]<c[i]) c[i]=a[i];
			}
		}

		public static void lib3ds_vector_min(float[] c, Lib3dsVertex a)
		{
			if(a.x<c[0]) c[0]=a.x;
			if(a.y<c[1]) c[1]=a.y;
			if(a.z<c[2]) c[2]=a.z;
		}

		// c[i] = max(c[i], a[i]);
		//
		// Computes maximum values of x,y,z independently.
		public static void lib3ds_vector_max(float[] c, float[] a)
		{
			for(int i=0; i<3; i++)
			{
				if(a[i]>c[i]) c[i]=a[i];
			}
		}

		public static void lib3ds_vector_max(float[] c, Lib3dsVertex a)
		{
			if(a.x>c[0]) c[0]=a.x;
			if(a.y>c[1]) c[1]=a.y;
			if(a.z>c[2]) c[2]=a.z;
		}

		public static void lib3ds_vector_dump(float[] c)
		{
			Console.Error.WriteLine("{0} {2} {2}", c[0], c[1], c[2]);
		}
	}
}
