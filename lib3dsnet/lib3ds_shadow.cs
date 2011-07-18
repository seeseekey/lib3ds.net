// lib3ds_shadow.cs - Shadow
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

using System;

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		public static void lib3ds_shadow_read(Lib3dsShadow shadow, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();

			lib3ds_chunk_read(c, io);
			switch(c.chunk)
			{
				case Lib3dsChunks.CHK_SHADOW_MAP_SIZE: shadow.map_size=lib3ds_io_read_intw(io); break;
				case Lib3dsChunks.CHK_LO_SHADOW_BIAS: shadow.low_bias=lib3ds_io_read_float(io); break;
				case Lib3dsChunks.CHK_HI_SHADOW_BIAS: shadow.hi_bias=lib3ds_io_read_float(io); break;
				case Lib3dsChunks.CHK_SHADOW_FILTER: shadow.filter=lib3ds_io_read_float(io); break;
				case Lib3dsChunks.CHK_RAY_BIAS: shadow.ray_bias=lib3ds_io_read_float(io); break;
			}
		}

		public static void lib3ds_shadow_write(Lib3dsShadow shadow, Lib3dsIo io)
		{
			if(Math.Abs(shadow.low_bias)>EPSILON)
			{ // ---- CHK_LO_SHADOW_BIAS ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_LO_SHADOW_BIAS;
				c.size=10;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_float(io, shadow.low_bias);
			}

			if(Math.Abs(shadow.hi_bias)>EPSILON)
			{ // ---- CHK_HI_SHADOW_BIAS ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_HI_SHADOW_BIAS;
				c.size=10;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_float(io, shadow.hi_bias);
			}

			if(shadow.map_size!=0)
			{ // ---- CHK_SHADOW_MAP_SIZE ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_SHADOW_MAP_SIZE;
				c.size=8;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_intw(io, shadow.map_size);
			}

			if(Math.Abs(shadow.filter)>EPSILON)
			{ // ---- CHK_SHADOW_FILTER ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_SHADOW_FILTER;
				c.size=10;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_float(io, shadow.filter);
			}
			if(Math.Abs(shadow.ray_bias)>EPSILON)
			{ // ---- CHK_RAY_BIAS ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_RAY_BIAS;
				c.size=10;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_float(io, shadow.ray_bias);
			}
		}
	}
}
