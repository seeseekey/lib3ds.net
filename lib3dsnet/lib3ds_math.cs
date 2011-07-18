// lib3ds_math.cs - Math
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		public static float lib3ds_math_ease(float fp, float fc, float fn, float ease_from, float ease_to)
		{
			double s, step;
			double tofrom;
			double a;

			s=step=(float)(fc-fp)/(fn-fp);
			tofrom=ease_to+ease_from;
			if(tofrom!=0.0)
			{
				if(tofrom>1.0)
				{
					ease_to=(float)(ease_to/tofrom);
					ease_from=(float)(ease_from/tofrom);
				}
				a=1.0/(2.0-(ease_to+ease_from));

				if(step<ease_from) s=a/ease_from*step*step;
				else
				{
					if((1.0-ease_to)<=step)
					{
						step=1.0-step;
						s=1.0-a/ease_to*step*step;
					}
					else
					{
						s=((2.0*step)-ease_from)*a;
					}
				}
			}
			return (float)s;
		}

		public static void lib3ds_math_cubic_interp(float[] v, float[] a, float[] p, float[] q, float[] b, int n, float t)
		{
			float x=2*t*t*t-3*t*t+1;
			float y=-2*t*t*t+3*t*t;
			float z=t*t*t-2*t*t+t;
			float w=t*t*t-t*t;

			for(int i=0; i<n; i++) v[i]=x*a[i]+y*b[i]+z*p[i]+w*q[i];
		}
	}
}
