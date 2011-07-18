// lib3ds_track.cs - Track
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

using System;
using System.Diagnostics;

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		public static Lib3dsTrack lib3ds_track_new(Lib3dsTrackType type, int nkeys)
		{
			Lib3dsTrack track=new Lib3dsTrack();
			track.type=type;
			lib3ds_track_resize(track, nkeys);
			return track;
		}

		public static void lib3ds_track_free(Lib3dsTrack track)
		{
			Debug.Assert(track!=null);
			lib3ds_track_resize(track, 0);
		}

		public static void lib3ds_track_resize(Lib3dsTrack track, int nkeys)
		{
			Debug.Assert(track!=null);
			if(track.keys.Count==nkeys) return;
			while(track.keys.Count<nkeys) track.keys.Add(new Lib3dsKey());
			if(track.keys.Count>nkeys) track.keys.RemoveRange(nkeys, track.keys.Count-nkeys);
		}

		static void pos_key_setup(int n, Lib3dsKey pp, Lib3dsKey pc, Lib3dsKey pn, float[] dd, float[] ds)
		{
			Debug.Assert(pc!=null);

			float fp=1.0f, fn=1.0f;
			if(pp!=null&&pn!=null)
			{
				float dt=0.5f*(pn.frame-pp.frame);
				fp=(float)(pc.frame-pp.frame)/dt;
				fn=(float)(pn.frame-pc.frame)/dt;
				float c=(float)Math.Abs(pc.cont);
				fp=fp+c-c*fp;
				fn=fn+c-c*fn;
			}

			float cm=1.0f-pc.cont;
			float tm=0.5f*(1.0f-pc.tens);
			float cp=2.0f-cm;
			float bm=1.0f-pc.bias;
			float bp=2.0f-bm;
			float tmcm=tm*cm;
			float tmcp=tm*cp;
			float ksm=tmcm*bp*fp;
			float ksp=tmcp*bm*fp;
			float kdm=tmcp*bp*fn;
			float kdp=tmcm*bm*fn;

			float[] delm=new float[3], delp=new float[3];

			for(int i=0; i<n; i++) delm[i]=delp[i]=0;
			if(pp!=null) for(int i=0; i<n; i++) delm[i]=pc.value[i]-pp.value[i];
			if(pn!=null) for(int i=0; i<n; i++) delp[i]=pn.value[i]-pc.value[i];
			if(pp==null) for(int i=0; i<n; i++) delm[i]=delp[i];
			if(pn==null) for(int i=0; i<n; i++) delp[i]=delm[i];

			for(int i=0; i<n; i++)
			{
				ds[i]=ksm*delm[i]+ksp*delp[i];
				dd[i]=kdm*delm[i]+kdp*delp[i];
			}
		}

		static void rot_key_setup(Lib3dsKey prev, Lib3dsKey cur, Lib3dsKey next, float[] a, float[] b)
		{
			Debug.Assert(cur!=null);

			float[] qm=new float[4], qp=new float[4];
			if(prev!=null)
			{
				if(cur.value[3]>TWOPI-EPSILON)
				{
					lib3ds_quat_axis_angle(qm, cur.value, 0.0f);
					lib3ds_quat_ln(qm);
				}
				else
				{
					float[] q=new float[4];
					lib3ds_quat_copy(q, prev.value);
					if(lib3ds_quat_dot(q, cur.value)<0) lib3ds_quat_neg(q);
					lib3ds_quat_ln_dif(qm, q, cur.value);
				}
			}
			if(next!=null)
			{
				if(next.value[3]>TWOPI-EPSILON)
				{
					lib3ds_quat_axis_angle(qp, next.value, 0.0f);
					lib3ds_quat_ln(qp);
				}
				else
				{
					float[] q=new float[4];
					lib3ds_quat_copy(q, next.value);
					if(lib3ds_quat_dot(q, cur.value)<0) lib3ds_quat_neg(q);
					lib3ds_quat_ln_dif(qp, cur.value, q);
				}
			}
			if(prev==null) lib3ds_quat_copy(qm, qp);
			if(next==null) lib3ds_quat_copy(qp, qm);

			float fp=1.0f, fn=1.0f;
			float cm=1.0f-cur.cont;
			if(prev!=null&&next!=null)
			{
				float dt=0.5f*(next.frame-prev.frame);
				fp=(float)(cur.frame-prev.frame)/dt;
				fn=(float)(next.frame-cur.frame)/dt;
				float c=(float)Math.Abs(cur.cont);
				fp=fp+c-c*fp;
				fn=fn+c-c*fn;
			}

			float tm=0.5f*(1.0f-cur.tens);
			float cp=2.0f-cm;
			float bm=1.0f-cur.bias;
			float bp=2.0f-bm;
			float tmcm=tm*cm;
			float tmcp=tm*cp;
			float ksm=1.0f-tmcm*bp*fp;
			float ksp=-tmcp*bm*fp;
			float kdm=tmcp*bp*fn;
			float kdp=tmcm*bm*fn-1.0f;

			float[] qa=new float[4], qb=new float[4];
			for(int i=0; i<4; i++)
			{
				qa[i]=0.5f*(kdm*qm[i]+kdp*qp[i]);
				qb[i]=0.5f*(ksm*qm[i]+ksp*qp[i]);
			}
			lib3ds_quat_exp(qa);
			lib3ds_quat_exp(qb);

			lib3ds_quat_mul(a, cur.value, qa);
			lib3ds_quat_mul(b, cur.value, qb);
		}

		static void quat_for_index(Lib3dsTrack track, int index, float[] q)
		{
			float[] p=new float[4];
			lib3ds_quat_identity(q);
			for(int i=0; i<=index; i++)
			{
				lib3ds_quat_axis_angle(p, track.keys[i].value, track.keys[i].value[3]);
				lib3ds_quat_mul(q, p, q);
			}
		}

		static int find_index(Lib3dsTrack track, float t, out float u)
		{
			Debug.Assert(track!=null);
			Debug.Assert(track.keys.Count>0);

			u=0;
			if(track.keys.Count<=1) return -1;

			int t0=track.keys[0].frame;
			int t1=track.keys[track.keys.Count-1].frame;
			float nt;
			if((track.flags&Lib3dsTrackFlags.LIB3DS_TRACK_REPEAT)!=0) nt=(float)Math.IEEERemainder(t-t0, t1-t0)+t0;
			else nt=t;

			if(nt<=t0) return -1;
			if(nt>=t1) return track.keys.Count;

			int i=1;
			for(; i<track.keys.Count; i++)
			{
				if(nt<track.keys[i].frame) break;
			}

			u=nt-(float)track.keys[i-1].frame;
			u/=(float)(track.keys[i].frame-track.keys[i-1].frame);

			Debug.Assert((u>=0.0f)&&(u<=1.0f));
			return i;
		}

		static void setup_segment(Lib3dsTrack track, int index, ref Lib3dsKey pp, ref Lib3dsKey p0, ref Lib3dsKey p1, ref Lib3dsKey pn)
		{
			int ip=0, @in=0;

			pp.frame=pn.frame=-1;
			if(index>=2)
			{
				ip=index-2;
				pp=track.keys[index-2];
			}
			else
			{
				if((track.flags&Lib3dsTrackFlags.LIB3DS_TRACK_SMOOTH)!=0)
				{
					ip=track.keys.Count-2;
					pp=track.keys[track.keys.Count-2];
					pp.frame=track.keys[track.keys.Count-2].frame-(track.keys[track.keys.Count-1].frame-track.keys[0].frame);
				}
			}

			p0=track.keys[index-1];
			p1=track.keys[index];

			if(index<(int)track.keys.Count-1)
			{
				@in=index+1;
				pn=track.keys[index+1];
			}
			else
			{
				if((track.flags&Lib3dsTrackFlags.LIB3DS_TRACK_SMOOTH)!=0)
				{
					@in=1;
					pn=track.keys[1];
					pn.frame=track.keys[1].frame+(track.keys[track.keys.Count-1].frame-track.keys[0].frame);
				}
			}

			if(track.type==Lib3dsTrackType.LIB3DS_TRACK_QUAT)
			{
				float[] q=new float[4];
				if(pp.frame>=0) quat_for_index(track, ip, pp.value);
				else lib3ds_quat_identity(pp.value);

				quat_for_index(track, index-1, p0.value);
				lib3ds_quat_axis_angle(q, track.keys[index].value, track.keys[index].value[3]);
				lib3ds_quat_mul(p1.value, q, p0.value);

				if(pn.frame>=0)
				{
					lib3ds_quat_axis_angle(q, track.keys[@in].value, track.keys[@in].value[3]);
					lib3ds_quat_mul(pn.value, q, p1.value);
				}
				else lib3ds_quat_identity(pn.value);
			}
		}

		public static void lib3ds_track_eval_bool(Lib3dsTrack track, out bool b, float t)
		{
			b=false;
			if(track==null) return;

			Debug.Assert(track.type==Lib3dsTrackType.LIB3DS_TRACK_BOOL);

			if(track.keys.Count==0) return;

			float u;
			int index=find_index(track, t, out u);
			if(index<0) return;
			if(index>=track.keys.Count) b=(track.keys.Count&1)==0;
			else b=(index&1)==0;
		}

		static void track_eval_linear(Lib3dsTrack track, float[] value, float t)
		{
			Debug.Assert(track!=null);

			if(track.keys.Count==0)
			{
				for(int i=0; i<(int)track.type; i++) value[i]=0.0f;
				return;
			}

			float u;
			int index=find_index(track, t, out u);

			if(index<0)
			{
				for(int i=0; i<(int)track.type; i++) value[i]=track.keys[0].value[i];
				return;
			}
			if(index>=track.keys.Count)
			{
				for(int i=0; i<(int)track.type; i++) value[i]=track.keys[track.keys.Count-1].value[i];
				return;
			}

			Lib3dsKey pp=new Lib3dsKey(), p0=new Lib3dsKey(), p1=new Lib3dsKey(), pn=new Lib3dsKey();
			float[] dsp=new float[3], ddp=new float[3], dsn=new float[3], ddn=new float[3];

			setup_segment(track, index, ref pp, ref p0, ref p1, ref pn);

			pos_key_setup((int)track.type, pp.frame>=0?pp:null, p0, p1, ddp, dsp);
			pos_key_setup((int)track.type, p0, p1, pn.frame>=0?pn:null, ddn, dsn);

			lib3ds_math_cubic_interp(value, p0.value, ddp, dsn, p1.value, (int)track.type, u);
		}

		public static void lib3ds_track_eval_float(Lib3dsTrack track, out float f, float t)
		{
			f=0;
			if(track==null) return;

			Debug.Assert(track.type==Lib3dsTrackType.LIB3DS_TRACK_FLOAT);
			float[] tmp=new float[1];

			track_eval_linear(track, tmp, t);
			f=tmp[0];
		}

		public static void lib3ds_track_eval_vector(Lib3dsTrack track, float[] v, float t)
		{
			lib3ds_vector_zero(v);
			if(track==null) return;

			Debug.Assert(track.type==Lib3dsTrackType.LIB3DS_TRACK_VECTOR);

			track_eval_linear(track, v, t);
		}

		public static void lib3ds_track_eval_quat(Lib3dsTrack track, float[] q, float t)
		{
			lib3ds_quat_identity(q);
			if(track==null) return;

			Debug.Assert(track.type==Lib3dsTrackType.LIB3DS_TRACK_QUAT);
			if(track.keys.Count==0) return;

			float u;
			int index=find_index(track, t, out u);
			if(index<0)
			{
				lib3ds_quat_axis_angle(q, track.keys[0].value, track.keys[0].value[3]);
				return;
			}
			if(index>=track.keys.Count)
			{
				quat_for_index(track, track.keys.Count-1, q);
				return;
			}

			Lib3dsKey pp=new Lib3dsKey(), p0=new Lib3dsKey(), p1=new Lib3dsKey(), pn=new Lib3dsKey();
			setup_segment(track, index, ref pp, ref p0, ref p1, ref pn);

			float[] ap=new float[4], bp=new float[4], an=new float[4], bn=new float[4];
			rot_key_setup(pp.frame>=0?pp:null, p0, p1, ap, bp);
			rot_key_setup(p0, p1, pn.frame>=0?pn:null, an, bn);

			lib3ds_quat_squad(q, p0.value, ap, bn, p1.value, u);
		}

		static void tcb_read(Lib3dsKey key, Lib3dsIo io)
		{
			key.flags=(Lib3dsKeyFlags)lib3ds_io_read_word(io);
			if((key.flags&Lib3dsKeyFlags.LIB3DS_KEY_USE_TENS)!=0) key.tens=lib3ds_io_read_float(io);
			if((key.flags&Lib3dsKeyFlags.LIB3DS_KEY_USE_CONT)!=0) key.cont=lib3ds_io_read_float(io);
			if((key.flags&Lib3dsKeyFlags.LIB3DS_KEY_USE_BIAS)!=0) key.bias=lib3ds_io_read_float(io);
			if((key.flags&Lib3dsKeyFlags.LIB3DS_KEY_USE_EASE_TO)!=0) key.ease_to=lib3ds_io_read_float(io);
			if((key.flags&Lib3dsKeyFlags.LIB3DS_KEY_USE_EASE_FROM)!=0) key.ease_from=lib3ds_io_read_float(io);
		}

		public static void lib3ds_track_read(Lib3dsTrack track, Lib3dsIo io)
		{
			track.flags=(Lib3dsTrackFlags)lib3ds_io_read_word(io);
			lib3ds_io_read_dword(io);
			lib3ds_io_read_dword(io);
			int nkeys=lib3ds_io_read_intd(io);
			lib3ds_track_resize(track, nkeys);

			switch(track.type)
			{
				case Lib3dsTrackType.LIB3DS_TRACK_BOOL:
					for(int i=0; i<nkeys; i++)
					{
						track.keys[i].frame=lib3ds_io_read_intd(io);
						tcb_read(track.keys[i], io);
					}
					break;
				case Lib3dsTrackType.LIB3DS_TRACK_FLOAT:
					for(int i=0; i<nkeys; i++)
					{
						track.keys[i].frame=lib3ds_io_read_intd(io);
						tcb_read(track.keys[i], io);
						track.keys[i].value[0]=lib3ds_io_read_float(io);
					}
					break;
				case Lib3dsTrackType.LIB3DS_TRACK_VECTOR:
					for(int i=0; i<nkeys; i++)
					{
						track.keys[i].frame=lib3ds_io_read_intd(io);
						tcb_read(track.keys[i], io);
						lib3ds_io_read_vector(io, track.keys[i].value);
					}
					break;
				case Lib3dsTrackType.LIB3DS_TRACK_QUAT:
					for(int i=0; i<nkeys; i++)
					{
						track.keys[i].frame=lib3ds_io_read_intd(io);
						tcb_read(track.keys[i], io);
						track.keys[i].value[3]=lib3ds_io_read_float(io);
						lib3ds_io_read_vector(io, track.keys[i].value);
					}
					break;
				//case Lib3dsTrackType.LIB3DS_TRACK_MORPH:
				//    for(int i=0; i<nkeys; i++)
				//    {
				//        track.keys[i].frame = lib3ds_io_read_intd(io);
				//        tcb_read(track.keys[i].tcb, io);
				//        lib3ds_io_read_string(io, track.keys[i].data.m.name, 64);
				//    }
				//    break;
			}
		}

		public static void tcb_write(Lib3dsKey key, Lib3dsIo io)
		{
			lib3ds_io_write_word(io, (ushort)key.flags);
			if((key.flags&Lib3dsKeyFlags.LIB3DS_KEY_USE_TENS)!=0) lib3ds_io_write_float(io, key.tens);
			if((key.flags&Lib3dsKeyFlags.LIB3DS_KEY_USE_CONT)!=0) lib3ds_io_write_float(io, key.cont);
			if((key.flags&Lib3dsKeyFlags.LIB3DS_KEY_USE_BIAS)!=0) lib3ds_io_write_float(io, key.bias);
			if((key.flags&Lib3dsKeyFlags.LIB3DS_KEY_USE_EASE_TO)!=0) lib3ds_io_write_float(io, key.ease_to);
			if((key.flags&Lib3dsKeyFlags.LIB3DS_KEY_USE_EASE_FROM)!=0) lib3ds_io_write_float(io, key.ease_from);
		}

		public static void lib3ds_track_write(Lib3dsTrack track, Lib3dsIo io)
		{
			lib3ds_io_write_word(io, (ushort)track.flags);
			lib3ds_io_write_dword(io, 0);
			lib3ds_io_write_dword(io, 0);
			lib3ds_io_write_intd(io, track.keys.Count);

			switch(track.type)
			{
				case Lib3dsTrackType.LIB3DS_TRACK_BOOL:
					for(int i=0; i<track.keys.Count; i++)
					{
						lib3ds_io_write_intd(io, track.keys[i].frame);
						tcb_write(track.keys[i], io);
					}
					break;
				case Lib3dsTrackType.LIB3DS_TRACK_FLOAT:
					for(int i=0; i<track.keys.Count; i++)
					{
						lib3ds_io_write_intd(io, track.keys[i].frame);
						tcb_write(track.keys[i], io);
						lib3ds_io_write_float(io, track.keys[i].value[0]);
					}
					break;
				case Lib3dsTrackType.LIB3DS_TRACK_VECTOR:
					for(int i=0; i<track.keys.Count; i++)
					{
						lib3ds_io_write_intd(io, track.keys[i].frame);
						tcb_write(track.keys[i], io);
						lib3ds_io_write_vector(io, track.keys[i].value);
					}
					break;
				case Lib3dsTrackType.LIB3DS_TRACK_QUAT:
					for(int i=0; i<track.keys.Count; i++)
					{
						lib3ds_io_write_intd(io, track.keys[i].frame);
						tcb_write(track.keys[i], io);
						lib3ds_io_write_float(io, track.keys[i].value[3]);
						lib3ds_io_write_vector(io, track.keys[i].value);
					}
					break;
				//case Lib3dsTrackType.LIB3DS_TRACK_MORPH:
				//    for(int i=0; i<track.keys.Count; i++)
				//    {
				//        lib3ds_io_write_intd(io, track.keys[i].frame);
				//        tcb_write(track.keys[i].tcb, io);
				//        lib3ds_io_write_string(io, track.keys[i].data.m.name);
				//    }
				//    break;
			}
		}
	}
}
