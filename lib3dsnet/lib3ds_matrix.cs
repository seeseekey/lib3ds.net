// lib3ds_matrix.cs - Matrix
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

using System;

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		// Clear a matrix to all zeros.
		//
		// \param m Matrix to be cleared.
		public static void lib3ds_matrix_zero(float[,] m)
		{
			for(int i=0; i<4; i++)
			{
				for(int j=0; j<4; j++) m[i, j]=0.0f;
			}
		}

		// Set a matrix to identity.
		//
		// \param m Matrix to be set.
		public static void lib3ds_matrix_identity(float[,] m)
		{
			for(int i=0; i<4; i++)
			{
				for(int j=0; j<4; j++) m[i, j]=0.0f;
			}
			for(int i=0; i<4; i++) m[i, i]=1.0f;
		}

		// Copy a matrix.
		public static void lib3ds_matrix_copy(float[,] dest, float[,] src)
		{
			for(int i=0; i<4; i++)
			{
				for(int j=0; j<4; j++) dest[i, j]=src[i, j];
			}
		}

		// Negate a matrix -- all elements negated.
		public static void lib3ds_matrix_neg(float[,] m)
		{
			for(int j=0; j<4; j++)
			{
				for(int i=0; i<4; i++)
				{
					m[j, i]=-m[j, i];
				}
			}
		}

		// Transpose a matrix in place.
		public static void lib3ds_matrix_transpose(float[,] m)
		{
			for(int j=0; j<4; j++)
			{
				for(int i=j+1; i<4; i++)
				{
					float swp=m[j, i];
					m[j, i]=m[i, j];
					m[i, j]=swp;
				}
			}
		}

		// Add two matrices.
		public static void lib3ds_matrix_add(float[,] m, float[,] a, float[,] b)
		{
			for(int j=0; j<4; j++)
			{
				for(int i=0; i<4; i++)
				{
					m[j, i]=a[j, i]+b[j, i];
				}
			}
		}

		// Subtract two matrices.
		//
		// \param m Result.
		// \param a Addend.
		// \param b Minuend.
		public static void lib3ds_matrix_sub(float[,] m, float[,] a, float[,] b)
		{
			for(int j=0; j<4; j++)
			{
				for(int i=0; i<4; i++)
				{
					m[j, i]=a[j, i]-b[j, i];
				}
			}
		}

		// Multiplies a matrix by a second one (m = m * n).
		public static void lib3ds_matrix_mult(float[,] m, float[,] a, float[,] b)
		{
			float[,] tmp=new float[4, 4];
			lib3ds_matrix_copy(tmp, a); // m=m*n => m=tmp*n
			for(int j=0; j<4; j++)
			{
				for(int i=0; i<4; i++)
				{
					float ab=0.0f;
					for(int k=0; k<4; k++) ab+=tmp[k, i]*b[j, k];
					m[j, i]=ab;
				}
			}
		}

		// Multiply a matrix by a scalar.
		//
		// \param m Matrix to be set.
		// \param k Scalar.
		public static void lib3ds_matrix_scalar(float[,] m, float k)
		{
			for(int j=0; j<4; j++)
			{
				for(int i=0; i<4; i++)
				{
					m[j, i]*=k;
				}
			}
		}

		static float det2x2(float a, float b, float c, float d)
		{
			return ((a)*(d)-(b)*(c));
		}

		static float det3x3(float a1, float a2, float a3, float b1, float b2, float b3, float c1, float c2, float c3)
		{
			return a1*det2x2(b2, b3, c2, c3)-b1*det2x2(a2, a3, c2, c3)+c1*det2x2(a2, a3, b2, b3);
		}

		// Find determinant of a matrix.
		public static float lib3ds_matrix_det(float[,] m)
		{
			float a1=m[0, 0];
			float b1=m[1, 0];
			float c1=m[2, 0];
			float d1=m[3, 0];
			float a2=m[0, 1];
			float b2=m[1, 1];
			float c2=m[2, 1];
			float d2=m[3, 1];
			float a3=m[0, 2];
			float b3=m[1, 2];
			float c3=m[2, 2];
			float d3=m[3, 2];
			float a4=m[0, 3];
			float b4=m[1, 3];
			float c4=m[2, 3];
			float d4=m[3, 3];

			return
				a1*det3x3(b2, b3, b4, c2, c3, c4, d2, d3, d4)-
				b1*det3x3(a2, a3, a4, c2, c3, c4, d2, d3, d4)+
				c1*det3x3(a2, a3, a4, b2, b3, b4, d2, d3, d4)-
				d1*det3x3(a2, a3, a4, b2, b3, b4, c2, c3, c4);
		}

		// Invert a matrix in place.
		//
		// \param m Matrix to invert.
		//
		// \return true on success, false on failure.
		//
		// GGemsII, K.Wu, Fast Matrix Inversion
		public static bool lib3ds_matrix_inv(float[,] m)
		{
			int i, j, k;
			int[] pvt_i=new int[4], pvt_j=new int[4];	// Locations of pivot elements
			float pvt_val;								// Value of current pivot element
			float hold;									// Temporary storage
			float determinat;

			determinat=1.0f;
			for(k=0; k<4; k++)
			{
				// Locate k'th pivot element
				pvt_val=m[k, k]; // Initialize for search
				pvt_i[k]=k;
				pvt_j[k]=k;
				for(i=k; i<4; i++)
				{
					for(j=k; j<4; j++)
					{
						if(Math.Abs(m[i, j])>Math.Abs(pvt_val))
						{
							pvt_i[k]=i;
							pvt_j[k]=j;
							pvt_val=m[i, j];
						}
					}
				}

				// Product of pivots, gives determinant when finished
				determinat*=pvt_val;
				if(Math.Abs(determinat)<EPSILON) return false; // Matrix is singular (zero determinant)

				// "Interchange" rows (with sign change stuff)
				i=pvt_i[k];
				if(i!=k)
				{ // If rows are different
					for(j=0; j<4; j++)
					{
						hold=-m[k, j];
						m[k, j]=m[i, j];
						m[i, j]=hold;
					}
				}

				// "Interchange" columns
				j=pvt_j[k];
				if(j!=k)
				{ // If columns are different
					for(i=0; i<4; i++)
					{
						hold=-m[i, k];
						m[i, k]=m[i, j];
						m[i, j]=hold;
					}
				}

				// Divide column by minus pivot value
				for(i=0; i<4; i++)
				{
					if(i!=k) m[i, k]/=(-pvt_val);
				}

				// Reduce the matrix
				for(i=0; i<4; i++)
				{
					hold=m[i, k];
					for(j=0; j<4; j++)
					{
						if(i!=k&&j!=k) m[i, j]+=hold*m[k, j];
					}
				}

				// Divide row by pivot
				for(j=0; j<4; j++)
				{
					if(j!=k) m[k, j]/=pvt_val;
				}

				// Replace pivot by reciprocal (at last we can touch it).
				m[k, k]=1.0f/pvt_val;
			}

			// That was most of the work, one final pass of row/column interchange
			// to finish
			for(k=4-2; k>=0; k--)
			{ // Don't need to work with 1 by 1 corner
				i=pvt_j[k]; // Rows to swap correspond to pivot COLUMN
				if(i!=k)
				{ // If rows are different
					for(j=0; j<4; j++)
					{
						hold=m[k, j];
						m[k, j]=-m[i, j];
						m[i, j]=hold;
					}
				}

				j=pvt_i[k]; // Columns to swap correspond to pivot ROW */
				if(j!=k)
				{ // If columns are different
					for(i=0; i<4; i++)
					{
						hold=m[i, k];
						m[i, k]=-m[i, j];
						m[i, j]=hold;
					}
				}
			}
			return true;
		}

		// Apply a translation to a matrix.
		public static void lib3ds_matrix_translate(float[,] m, float x, float y, float z)
		{
			for(int i=0; i<3; i++)
			{
				m[3, i]+=m[0, i]*x+m[1, i]*y+m[2, i]*z;
			}
		}

		// Apply scale factors to a matrix.
		public static void lib3ds_matrix_scale(float[,] m, float x, float y, float z)
		{
			for(int i=0; i<4; i++)
			{
				m[0, i]*=x;
				m[1, i]*=y;
				m[2, i]*=z;
			}
		}

		// Apply a rotation about an arbitrary axis to a matrix.
		public static void lib3ds_matrix_rotate_quat(float[,] m, float[] q)
		{
			float l=q[0]*q[0]+q[1]*q[1]+q[2]*q[2]+q[3]*q[3];
			float s;
			if(Math.Abs(l)<EPSILON) s=1.0f;
			else s=2.0f/l;

			float xs=q[0]*s;
			float ys=q[1]*s;
			float zs=q[2]*s;
			float wx=q[3]*xs;
			float wy=q[3]*ys;
			float wz=q[3]*zs;
			float xx=q[0]*xs;
			float xy=q[0]*ys;
			float xz=q[0]*zs;
			float yy=q[1]*ys;
			float yz=q[1]*zs;
			float zz=q[2]*zs;

			float[,] R=new float[4, 4];
			R[0, 0]=1.0f-(yy+zz);
			R[1, 0]=xy-wz;
			R[2, 0]=xz+wy;
			R[0, 1]=xy+wz;
			R[1, 1]=1.0f-(xx+zz);
			R[2, 1]=yz-wx;
			R[0, 2]=xz-wy;
			R[1, 2]=yz+wx;
			R[2, 2]=1.0f-(xx+yy);
			R[3, 0]=R[3, 1]=R[3, 2]=R[0, 3]=R[1, 3]=R[2, 3]=0.0f;
			R[3, 3]=1.0f;

			lib3ds_matrix_mult(m, m, R);
		}

		// Apply a rotation about an arbitrary axis to a matrix.
		public static void lib3ds_matrix_rotate(float[,] m, float angle, float ax, float ay, float az)
		{
			float[] q=new float[4];
			float[] axis=new float[3];

			lib3ds_vector_make(axis, ax, ay, az);
			lib3ds_quat_axis_angle(q, axis, angle);
			lib3ds_matrix_rotate_quat(m, q);
		}

		// Compute a camera matrix based on position, target and roll.
		//
		// Generates a translate/rotate matrix that maps world coordinates
		// to camera coordinates. Resulting matrix does not include perspective
		// transform.
		//
		// \param matrix Destination matrix.
		// \param pos Camera position
		// \param tgt Camera target
		// \param roll Roll angle
		public static void lib3ds_matrix_camera(float[,] matrix, float[] pos, float[] tgt, float roll)
		{
			float[,] M=new float[4, 4];
			float[] x=new float[3], y=new float[3], z=new float[3];

			lib3ds_vector_sub(y, tgt, pos);
			lib3ds_vector_normalize(y);

			if(y[0]!=0.0||y[1]!=0)
			{
				z[0]=0;
				z[1]=0;
				z[2]=1.0f;
			}
			else
			{ // Special case: looking straight up or down z axis
				z[0]=-1.0f;
				z[1]=0;
				z[2]=0;
			}

			lib3ds_vector_cross(x, y, z);
			lib3ds_vector_cross(z, x, y);
			lib3ds_vector_normalize(x);
			lib3ds_vector_normalize(z);

			lib3ds_matrix_identity(M);
			M[0, 0]=x[0];
			M[1, 0]=x[1];
			M[2, 0]=x[2];
			M[0, 1]=y[0];
			M[1, 1]=y[1];
			M[2, 1]=y[2];
			M[0, 2]=z[0];
			M[1, 2]=z[1];
			M[2, 2]=z[2];

			lib3ds_matrix_identity(matrix);
			lib3ds_matrix_rotate(matrix, roll, 0, 1, 0);
			lib3ds_matrix_mult(matrix, matrix, M);
			lib3ds_matrix_translate(matrix, -pos[0], -pos[1], -pos[2]);
		}
	}
}
