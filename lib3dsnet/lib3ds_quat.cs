// lib3ds_quat.cs - Quaternion
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

using System;

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		// Set a quaternion to Identity
		public static void lib3ds_quat_identity(float[] c)
		{
			c[0]=c[1]=c[2]=0.0f;
			c[3]=1.0f;
		}

		// Copy a quaternion.
		public static void lib3ds_quat_copy(float[] dest, float[] src)
		{
			for(int i=0; i<4; i++) dest[i]=src[i];
		}

		// Compute a quaternion from axis and angle.
		//
		// \param c Computed quaternion
		// \param axis Rotation axis
		// \param angle Angle of rotation, radians.
		public static void lib3ds_quat_axis_angle(float[] c, float[] axis, float angle)
		{
			double l=Math.Sqrt(axis[0]*axis[0]+axis[1]*axis[1]+axis[2]*axis[2]);
			if(l<EPSILON)
			{
				c[0]=c[1]=c[2]=0.0f;
				c[3]=1.0f;
			}
			else
			{
				double omega=-0.5*angle;
				double s=Math.Sin(omega)/l;
				c[0]=(float)s*axis[0];
				c[1]=(float)s*axis[1];
				c[2]=(float)s*axis[2];
				c[3]=(float)Math.Cos(omega);
			}
		}

		// Negate a quaternion
		public static void lib3ds_quat_neg(float[] c)
		{
			for(int i=0; i<4; i++) c[i]=-c[i];
		}

		// Compute the conjugate of a quaternion
		public static void lib3ds_quat_cnj(float[] c)
		{
			for(int i=0; i<3; i++) c[i]=-c[i];
		}

		// Multiply two quaternions.
		//
		// \param c Result
		// \param a,b Inputs
		public static void lib3ds_quat_mul(float[] c, float[] a, float[] b)
		{
			float[] qa=new float[4], qb=new float[4];
			lib3ds_quat_copy(qa, a);
			lib3ds_quat_copy(qb, b);
			c[0]=qa[3]*qb[0]+qa[0]*qb[3]+qa[1]*qb[2]-qa[2]*qb[1];
			c[1]=qa[3]*qb[1]+qa[1]*qb[3]+qa[2]*qb[0]-qa[0]*qb[2];
			c[2]=qa[3]*qb[2]+qa[2]*qb[3]+qa[0]*qb[1]-qa[1]*qb[0];
			c[3]=qa[3]*qb[3]-qa[0]*qb[0]-qa[1]*qb[1]-qa[2]*qb[2];
		}

		// Multiply a quaternion by a scalar.
		public static void lib3ds_quat_scalar(float[] c, float k)
		{
			for(int i=0; i<4; i++) c[i]*=k;
		}

		// Normalize a quaternion.
		public static void lib3ds_quat_normalize(float[] c)
		{
			double l=Math.Sqrt(c[0]*c[0]+c[1]*c[1]+c[2]*c[2]+c[3]*c[3]);
			if(Math.Abs(l)<EPSILON)
			{
				c[0]=c[1]=c[2]=0.0f;
				c[3]=1.0f;
			}
			else
			{
				double m=1.0f/l;
				for(int i=0; i<4; i++) c[i]=(float)(c[i]*m);
			}
		}

		// Compute the inverse of a quaternion.
		public static void lib3ds_quat_inv(float[] c)
		{
			double l=Math.Sqrt(c[0]*c[0]+c[1]*c[1]+c[2]*c[2]+c[3]*c[3]);
			if(Math.Abs(l)<EPSILON)
			{
				c[0]=c[1]=c[2]=0.0f;
				c[3]=1.0f;
			}
			else
			{
				double m=1.0f/l;
				c[0]=(float)(-c[0]*m);
				c[1]=(float)(-c[1]*m);
				c[2]=(float)(-c[2]*m);
				c[3]=(float)(c[3]*m);
			}
		}
		
		// Compute the dot-product of a quaternion.
		public static float lib3ds_quat_dot(float[] a, float[] b)
		{
			return (a[0]*b[0]+a[1]*b[1]+a[2]*b[2]+a[3]*b[3]);
		}

		public static float lib3ds_quat_norm(float[] c)
		{
			return (c[0]*c[0]+c[1]*c[1]+c[2]*c[2]+c[3]*c[3]);
		}

		public static void lib3ds_quat_ln(float[] c)
		{
			double s=Math.Sqrt(c[0]*c[0]+c[1]*c[1]+c[2]*c[2]);
			double t=0;
			if(Math.Abs(s)>=EPSILON) t=Math.Atan2(s, c[3])/s;

			for(int i=0; i<3; i++) c[i]=(float)(c[i]*t);
			c[3]=0.0f;
		}

		public static void lib3ds_quat_ln_dif(float[] c, float[] a, float[] b)
		{
			float[] invp=new float[4];

			lib3ds_quat_copy(invp, a);
			lib3ds_quat_inv(invp);
			lib3ds_quat_mul(c, invp, b);
			lib3ds_quat_ln(c);
		}

		public static void lib3ds_quat_exp(float[] c)
		{
			double om=Math.Sqrt(c[0]*c[0]+c[1]*c[1]+c[2]*c[2]);
			double sinom=1;
			if(Math.Abs(om)>=EPSILON) sinom=Math.Sin(om)/om;

			for(int i=0; i<3; i++) c[i]=(float)(c[i]*sinom);
			c[3]=(float)Math.Cos(om);
		}

		public static void lib3ds_quat_slerp(float[] c, float[] a, float[] b, float t)
		{
			double l=a[0]*b[0]+a[1]*b[1]+a[2]*b[2]+a[3]*b[3];
			float flip=1.0f;
			if(l<0)
			{
				flip=-1.0f;
				l=-l;
			}

			double om=Math.Acos(l);
			double sinom=Math.Sin(om);
			double sp, sq;
			if(Math.Abs(sinom)>EPSILON)
			{
				sp=Math.Sin((1.0f-t)*om)/sinom;
				sq=Math.Sin(t*om)/sinom;
			}
			else
			{
				sp=1.0f-t;
				sq=t;
			}

			sq*=flip;
			for(int i=0; i<4; i++) c[i]=(float)(sp*a[i]+sq*b[i]);
		}

		public static void lib3ds_quat_squad(float[] c, float[] a, float[] p, float[] q, float[] b, float t)
		{
			float[] ab=new float[4];
			float[] pq=new float[4];

			lib3ds_quat_slerp(ab, a, b, t);
			lib3ds_quat_slerp(pq, p, q, t);
			lib3ds_quat_slerp(c, ab, pq, 2*t*(1-t));
		}

		public static void lib3ds_quat_tangent(float[] c, float[] p, float[] q, float[] n)
		{
			float[] dn=new float[4], dp=new float[4], x=new float[4];

			lib3ds_quat_ln_dif(dn, q, n);
			lib3ds_quat_ln_dif(dp, q, p);

			for(int i=0; i<4; i++) x[i]=-1.0f/4.0f*(dn[i]+dp[i]);

			lib3ds_quat_exp(x);
			lib3ds_quat_mul(c, q, x);
		}

		public static void lib3ds_quat_dump(float[] q)
		{
			Console.WriteLine("{0} {1} {2} {3}", q[0], q[1], q[2], q[3]);
		}
	}
}
