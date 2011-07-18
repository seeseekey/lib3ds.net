// lib3ds_light.cs - Light
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
		public static Lib3dsLight lib3ds_light_new(string name)
		{
			Debug.Assert(name!=null);
			Debug.Assert(name.Length<64);
			try
			{
				Lib3dsLight light=new Lib3dsLight();
				light.name=name;
				return light;
			}
			catch
			{
				return null;
			}
		}

		public static void lib3ds_light_free(Lib3dsLight light)
		{
		}

		static void spotlight_read(Lib3dsLight light, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_DL_SPOTLIGHT, io);

			light.spot_light=true;
			for(int i=0; i<3; i++) light.target[i]=lib3ds_io_read_float(io);
			light.hotspot=lib3ds_io_read_float(io);
			light.falloff=lib3ds_io_read_float(io);
			lib3ds_chunk_read_tell(c, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_DL_SPOT_ROLL: light.roll=lib3ds_io_read_float(io); break;
					case Lib3dsChunks.CHK_DL_SHADOWED: light.shadowed=true; break;
					case Lib3dsChunks.CHK_DL_LOCAL_SHADOW2:
						light.shadow_bias=lib3ds_io_read_float(io);
						light.shadow_filter=lib3ds_io_read_float(io);
						light.shadow_size=lib3ds_io_read_intw(io);
						break;
					case Lib3dsChunks.CHK_DL_SEE_CONE: light.see_cone=true; break;
					case Lib3dsChunks.CHK_DL_SPOT_RECTANGULAR: light.rectangular_spot=true; break;
					case Lib3dsChunks.CHK_DL_SPOT_ASPECT: light.spot_aspect=lib3ds_io_read_float(io); break;
					case Lib3dsChunks.CHK_DL_SPOT_PROJECTOR:
						light.use_projector=true;
						light.projector=lib3ds_io_read_string(io, 64);
						break;
					case Lib3dsChunks.CHK_DL_SPOT_OVERSHOOT: light.spot_overshoot=true; break;
					case Lib3dsChunks.CHK_DL_RAY_BIAS: light.ray_bias=lib3ds_io_read_float(io); break;
					case Lib3dsChunks.CHK_DL_RAYSHAD: light.ray_shadows=true; break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		public static void lib3ds_light_read(Lib3dsLight light, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_N_DIRECT_LIGHT, io);

			for(int i=0; i<3; i++) light.position[i]=lib3ds_io_read_float(io);
			lib3ds_chunk_read_tell(c, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_COLOR_F:
						for(int i=0; i<3; i++) light.color[i]=lib3ds_io_read_float(io);
						break;
					case Lib3dsChunks.CHK_DL_OFF: light.off=true; break;
					case Lib3dsChunks.CHK_DL_OUTER_RANGE: light.outer_range=lib3ds_io_read_float(io); break;
					case Lib3dsChunks.CHK_DL_INNER_RANGE: light.inner_range=lib3ds_io_read_float(io); break;
					case Lib3dsChunks.CHK_DL_MULTIPLIER: light.multiplier=lib3ds_io_read_float(io); break;
					case Lib3dsChunks.CHK_DL_EXCLUDE: lib3ds_chunk_unknown(chunk, io); break; // FIXME:
					case Lib3dsChunks.CHK_DL_ATTENUATE: light.attenuation=lib3ds_io_read_float(io); break;
					case Lib3dsChunks.CHK_DL_SPOTLIGHT: lib3ds_chunk_read_reset(c, io); spotlight_read(light, io); break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		public static void lib3ds_light_write(Lib3dsLight light, Lib3dsIo io)
		{
			Lib3dsChunk c_n_direct_light=new Lib3dsChunk();

			c_n_direct_light.chunk=Lib3dsChunks.CHK_N_DIRECT_LIGHT;
			lib3ds_chunk_write_start(c_n_direct_light, io);

			lib3ds_io_write_vector(io, light.position);
			{ // ---- LIB3DS_COLOR_F ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_COLOR_F;
				c.size=18;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_rgb(io, light.color);
			}
			if(light.off)
			{ // ---- LIB3DS_DL_OFF ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_DL_OFF;
				c.size=6;
				lib3ds_chunk_write(c, io);
			}
			{ // ---- LIB3DS_DL_OUTER_RANGE ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_DL_OUTER_RANGE;
				c.size=10;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_float(io, light.outer_range);
			}
			{ // ---- LIB3DS_DL_INNER_RANGE ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_DL_INNER_RANGE;
				c.size=10;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_float(io, light.inner_range);
			}
			{ // ---- LIB3DS_DL_MULTIPLIER ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_DL_MULTIPLIER;
				c.size=10;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_float(io, light.multiplier);
			}
			if(light.attenuation!=0)
			{ // ---- LIB3DS_DL_ATTENUATE ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_DL_ATTENUATE;
				c.size=6;
				lib3ds_chunk_write(c, io);
			}

			if(light.spot_light)
			{
				Lib3dsChunk c_dl_spotlight=new Lib3dsChunk();

				c_dl_spotlight.chunk=Lib3dsChunks.CHK_DL_SPOTLIGHT;
				lib3ds_chunk_write_start(c_dl_spotlight, io);

				lib3ds_io_write_vector(io, light.target);
				lib3ds_io_write_float(io, light.hotspot);
				lib3ds_io_write_float(io, light.falloff);

				{ // ---- LIB3DS_DL_SPOT_ROLL ----
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_DL_SPOT_ROLL;
					c.size=10;
					lib3ds_chunk_write(c, io);
					lib3ds_io_write_float(io, light.roll);
				}
				if(light.shadowed)
				{ // ---- LIB3DS_DL_SHADOWED ----
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_DL_SHADOWED;
					c.size=6;
					lib3ds_chunk_write(c, io);
				}
				if((Math.Abs(light.shadow_bias)>EPSILON)||(Math.Abs(light.shadow_filter)>EPSILON)||(light.shadow_size!=0))
				{ // ---- LIB3DS_DL_LOCAL_SHADOW2 ----
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_DL_LOCAL_SHADOW2;
					c.size=16;
					lib3ds_chunk_write(c, io);
					lib3ds_io_write_float(io, light.shadow_bias);
					lib3ds_io_write_float(io, light.shadow_filter);
					lib3ds_io_write_intw(io, (short)light.shadow_size);
				}
				if(light.see_cone)
				{ // ---- LIB3DS_DL_SEE_CONE ----
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_DL_SEE_CONE;
					c.size=6;
					lib3ds_chunk_write(c, io);
				}
				if(light.rectangular_spot)
				{ // ---- LIB3DS_DL_SPOT_RECTANGULAR ----
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_DL_SPOT_RECTANGULAR;
					c.size=6;
					lib3ds_chunk_write(c, io);
				}
				if(Math.Abs(light.spot_aspect)>EPSILON)
				{ // ---- LIB3DS_DL_SPOT_ASPECT ----
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_DL_SPOT_ASPECT;
					c.size=10;
					lib3ds_chunk_write(c, io);
					lib3ds_io_write_float(io, light.spot_aspect);
				}
				if(light.use_projector)
				{ // ---- LIB3DS_DL_SPOT_PROJECTOR ----
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_DL_SPOT_PROJECTOR;
					c.size=10;
					lib3ds_chunk_write(c, io);
					lib3ds_io_write_string(io, light.projector);
				}
				if(light.spot_overshoot)
				{ // ---- LIB3DS_DL_SPOT_OVERSHOOT ----
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_DL_SPOT_OVERSHOOT;
					c.size=6;
					lib3ds_chunk_write(c, io);
				}
				if(Math.Abs(light.ray_bias)>EPSILON)
				{ // ---- LIB3DS_DL_RAY_BIAS ----
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_DL_RAY_BIAS;
					c.size=10;
					lib3ds_chunk_write(c, io);
					lib3ds_io_write_float(io, light.ray_bias);
				}
				if(light.ray_shadows)
				{ // ---- LIB3DS_DL_RAYSHAD ----
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_DL_RAYSHAD;
					c.size=6;
					lib3ds_chunk_write(c, io);
				}
				lib3ds_chunk_write_end(c_dl_spotlight, io);
			}

			lib3ds_chunk_write_end(c_n_direct_light, io);
		}
	}
}
