// lib3ds_atmosphere.cs - Atmosphere
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		static void fog_read(Lib3dsAtmosphere at, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_FOG, io);

			at.fog_near_plane=lib3ds_io_read_float(io);
			at.fog_near_density=lib3ds_io_read_float(io);
			at.fog_far_plane=lib3ds_io_read_float(io);
			at.fog_far_density=lib3ds_io_read_float(io);
			lib3ds_chunk_read_tell(c, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_LIN_COLOR_F:
						for(int i=0; i<3; i++) at.fog_color[i]=lib3ds_io_read_float(io);
						break;
					case Lib3dsChunks.CHK_COLOR_F: break;
					case Lib3dsChunks.CHK_FOG_BGND: at.fog_background=true; break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		static void layer_fog_read(Lib3dsAtmosphere at, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;
			bool have_lin=false;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_LAYER_FOG, io);

			at.layer_fog_near_y=lib3ds_io_read_float(io);
			at.layer_fog_far_y=lib3ds_io_read_float(io);
			at.layer_fog_density=lib3ds_io_read_float(io);
			at.layer_fog_flags=lib3ds_io_read_dword(io);
			lib3ds_chunk_read_tell(c, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_LIN_COLOR_F:
						lib3ds_io_read_rgb(io, at.layer_fog_color);
						have_lin=true;
						break;
					case Lib3dsChunks.CHK_COLOR_F:
						lib3ds_io_read_rgb(io, at.layer_fog_color);
						break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		static void distance_cue_read(Lib3dsAtmosphere at, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_DISTANCE_CUE, io);

			at.dist_cue_near_plane=lib3ds_io_read_float(io);
			at.dist_cue_near_dimming=lib3ds_io_read_float(io);
			at.dist_cue_far_plane=lib3ds_io_read_float(io);
			at.dist_cue_far_dimming=lib3ds_io_read_float(io);
			lib3ds_chunk_read_tell(c, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_DCUE_BGND: at.dist_cue_background=true; break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		public static void lib3ds_atmosphere_read(Lib3dsAtmosphere atmosphere, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();

			lib3ds_chunk_read(c, io);
			switch(c.chunk)
			{
				case Lib3dsChunks.CHK_FOG:
					lib3ds_chunk_read_reset(c, io);
					fog_read(atmosphere, io);
					break;
				case Lib3dsChunks.CHK_LAYER_FOG:
					lib3ds_chunk_read_reset(c, io);
					layer_fog_read(atmosphere, io);
					break;
				case Lib3dsChunks.CHK_DISTANCE_CUE:
					lib3ds_chunk_read_reset(c, io);
					distance_cue_read(atmosphere, io);
					break;
				case Lib3dsChunks.CHK_USE_FOG:
					atmosphere.use_fog=true;
					break;
				case Lib3dsChunks.CHK_USE_LAYER_FOG:
					atmosphere.use_layer_fog=true;
					break;
				case Lib3dsChunks.CHK_USE_DISTANCE_CUE:
					atmosphere.use_dist_cue=true;
					break;
			}
		}

		public static void lib3ds_atmosphere_write(Lib3dsAtmosphere atmosphere, Lib3dsIo io)
		{
			if(atmosphere.use_fog)
			{ // ---- LIB3DS_FOG ----
				Lib3dsChunk c_fog=new Lib3dsChunk();
				c_fog.chunk=Lib3dsChunks.CHK_FOG;
				lib3ds_chunk_write_start(c_fog, io);

				lib3ds_io_write_float(io, atmosphere.fog_near_plane);
				lib3ds_io_write_float(io, atmosphere.fog_near_density);
				lib3ds_io_write_float(io, atmosphere.fog_far_plane);
				lib3ds_io_write_float(io, atmosphere.fog_far_density);
				{
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_COLOR_F;
					c.size=18;
					lib3ds_chunk_write(c, io);
					lib3ds_io_write_rgb(io, atmosphere.fog_color);
				}
				if(atmosphere.fog_background)
				{
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_FOG_BGND;
					c.size=6;
					lib3ds_chunk_write(c, io);
				}

				lib3ds_chunk_write_end(c_fog, io);
			}

			if(atmosphere.use_layer_fog)
			{ // ---- LIB3DS_LAYER_FOG ----
				Lib3dsChunk c_layer_fog=new Lib3dsChunk();
				c_layer_fog.chunk=Lib3dsChunks.CHK_LAYER_FOG;
				c_layer_fog.size=40;
				lib3ds_chunk_write(c_layer_fog, io);
				lib3ds_io_write_float(io, atmosphere.layer_fog_near_y);
				lib3ds_io_write_float(io, atmosphere.layer_fog_far_y);
				lib3ds_io_write_float(io, atmosphere.layer_fog_near_y);
				lib3ds_io_write_dword(io, atmosphere.layer_fog_flags);
				{
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_COLOR_F;
					c.size=18;
					lib3ds_chunk_write(c, io);
					lib3ds_io_write_rgb(io, atmosphere.fog_color);
				}
			}

			if(atmosphere.use_dist_cue)
			{ // ---- LIB3DS_DISTANCE_CUE ----
				Lib3dsChunk c_distance_cue=new Lib3dsChunk();
				c_distance_cue.chunk=Lib3dsChunks.CHK_DISTANCE_CUE;
				lib3ds_chunk_write_start(c_distance_cue, io);

				lib3ds_io_write_float(io, atmosphere.dist_cue_near_plane);
				lib3ds_io_write_float(io, atmosphere.dist_cue_near_dimming);
				lib3ds_io_write_float(io, atmosphere.dist_cue_far_plane);
				lib3ds_io_write_float(io, atmosphere.dist_cue_far_dimming);
				if(atmosphere.dist_cue_background)
				{
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_DCUE_BGND;
					c.size=6;
					lib3ds_chunk_write(c, io);
				}

				lib3ds_chunk_write_end(c_distance_cue, io);
			}

			if(atmosphere.use_fog)
			{ // ---- LIB3DS_USE_FOG ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_USE_FOG;
				c.size=6;
				lib3ds_chunk_write(c, io);
			}

			if(atmosphere.use_layer_fog)
			{ // ---- LIB3DS_USE_LAYER_FOG ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_USE_LAYER_FOG;
				c.size=6;
				lib3ds_chunk_write(c, io);
			}

			if(atmosphere.use_dist_cue)
			{ // ---- LIB3DS_USE_DISTANCE_CUE ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_USE_V_GRADIENT;
				c.size=6;
				lib3ds_chunk_write(c, io);
			}
		}
	}
}
