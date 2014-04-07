// lib3ds_background.cs - Background
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

using System;
using System.Collections.Generic;

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		static void solid_bgnd_read(Lib3dsBackground background, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;
			//bool have_lin=false;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_SOLID_BGND, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_LIN_COLOR_F:
						lib3ds_io_read_rgb(io, background.solid_color);
						//have_lin=true;
						break;
					case Lib3dsChunks.CHK_COLOR_F:
						lib3ds_io_read_rgb(io, background.solid_color);
						break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		static void v_gradient_read(Lib3dsBackground background, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			List<float[]> color=new List<float[]>();
			List<float[]> lin_color=new List<float[]>();
			bool have_lin=false;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_V_GRADIENT, io);

			background.gradient_percent=lib3ds_io_read_float(io);
			lib3ds_chunk_read_tell(c, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_COLOR_F:
						{
							float[] col=new float[3];
							lib3ds_io_read_rgb(io, col);
							color.Add(col);
						}
						break;
					case Lib3dsChunks.CHK_LIN_COLOR_F:
						{
							float[] col=new float[3];
							lib3ds_io_read_rgb(io, col);
							lin_color.Add(col);
							have_lin=true;
						}
						break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			if(have_lin) color=lin_color;
			
			while(color.Count<3) color.Add(new float[3]);

			for(int i=0; i<3; i++)
			{
				background.gradient_top[i]=color[0][i];
				background.gradient_middle[i]=color[1][i];
				background.gradient_bottom[i]=color[2][i];
			}

			lib3ds_chunk_read_end(c, io);
		}

		public static void lib3ds_background_read(Lib3dsBackground background, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();

			lib3ds_chunk_read(c, io);
			switch(c.chunk)
			{
				case Lib3dsChunks.CHK_BIT_MAP: background.bitmap_name=lib3ds_io_read_string(io, 64); break;
				case Lib3dsChunks.CHK_SOLID_BGND:
					lib3ds_chunk_read_reset(c, io);
					solid_bgnd_read(background, io);
					break;
				case Lib3dsChunks.CHK_V_GRADIENT:
					lib3ds_chunk_read_reset(c, io);
					v_gradient_read(background, io);
					break;
				case Lib3dsChunks.CHK_USE_BIT_MAP: background.use_bitmap=true; break;
				case Lib3dsChunks.CHK_USE_SOLID_BGND: background.use_solid=true; break;
				case Lib3dsChunks.CHK_USE_V_GRADIENT: background.use_gradient=true; break;
			}
		}

		static bool colorf_defined(float[] rgb)
		{
			for(int i=0; i<3; i++)
			{
				if(Math.Abs(rgb[i])>EPSILON) return false;
			}
			return true;
		}

		public static void lib3ds_background_write(Lib3dsBackground background, Lib3dsIo io)
		{
			if(background.bitmap_name.Length>0)
			{ // ---- LIB3DS_BIT_MAP ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_BIT_MAP;
				c.size=6+1+(uint)background.bitmap_name.Length;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_string(io, background.bitmap_name);
			}

			if(colorf_defined(background.solid_color))
			{ // ---- LIB3DS_SOLID_BGND ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_SOLID_BGND;
				c.size=42;
				lib3ds_chunk_write(c, io);
				colorf_write(background.solid_color, io);
			}

			if(colorf_defined(background.gradient_top)||
				colorf_defined(background.gradient_middle)||
				colorf_defined(background.gradient_bottom))
			{ // ---- LIB3DS_V_GRADIENT ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_V_GRADIENT;
				c.size=118;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_float(io, background.gradient_percent);
				colorf_write(background.gradient_top, io);
				colorf_write(background.gradient_middle, io);
				colorf_write(background.gradient_bottom, io);
			}

			if(background.use_bitmap)
			{ // ---- LIB3DS_USE_BIT_MAP ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_USE_BIT_MAP;
				c.size=6;
				lib3ds_chunk_write(c, io);
			}

			if(background.use_solid)
			{ // ---- LIB3DS_USE_SOLID_BGND ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_USE_SOLID_BGND;
				c.size=6;
				lib3ds_chunk_write(c, io);
			}

			if(background.use_gradient)
			{ // ---- LIB3DS_USE_V_GRADIENT ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_USE_V_GRADIENT;
				c.size=6;
				lib3ds_chunk_write(c, io);
			}
		}
	}
}
