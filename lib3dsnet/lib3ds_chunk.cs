// lib3ds_chunk.cs - Reading and writing of chunks
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

using System.Diagnostics;

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		// Reads a 3d-Studio chunk header from a little endian file stream.
		//
		// \param c  The chunk to store the data.
		// \param io The file stream.
		static void lib3ds_chunk_read(Lib3dsChunk c, Lib3dsIo io)
		{
			Debug.Assert(c!=null);
			Debug.Assert(io!=null);
			c.cur=(uint)lib3ds_io_tell(io);
			c.chunk=(Lib3dsChunks)lib3ds_io_read_word(io);
			c.size=lib3ds_io_read_dword(io);
			c.end=c.cur+c.size;
			c.cur+=6;
			if(c.size<6) lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_ERROR, "Invalid chunk header.");
		}

		static void lib3ds_chunk_read_start(Lib3dsChunk c, Lib3dsChunks chunk, Lib3dsIo io)
		{
			Debug.Assert(c!=null);
			Debug.Assert(io!=null);
			lib3ds_chunk_read(c, io);
			if((chunk!=0)&&(c.chunk!=chunk)) lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_ERROR, "Unexpected chunk found.");
			io.log_indent++;
		}

		static void lib3ds_chunk_read_tell(Lib3dsChunk c, Lib3dsIo io)
		{
			c.cur=(uint)lib3ds_io_tell(io);
		}

		static Lib3dsChunks lib3ds_chunk_read_next(Lib3dsChunk c, Lib3dsIo io)
		{
			Lib3dsChunk d=new Lib3dsChunk();

			if(c.cur>=c.end)
			{
				Debug.Assert(c.cur==c.end);
				return 0;
			}

			lib3ds_io_seek(io, (long)c.cur, Lib3dsIoSeek.LIB3DS_SEEK_SET);
			d.chunk=(Lib3dsChunks)lib3ds_io_read_word(io);
			d.size=lib3ds_io_read_dword(io);
			c.cur+=d.size;

			if(io.log_func!=null)
			{
				lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_INFO, "{0} (0x{1:X}) size={2}", lib3ds_chunk_name(d.chunk), d.chunk, d.size);
			}

			if(c.cur>c.end)
			{
				if(io.log_func!=null)
				{
					lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_WARN, "***STOPPED READING - INVALID CHUNK SIZE***");
				}
				return 0;
			}

			return d.chunk;
		}

		static void lib3ds_chunk_read_reset(Lib3dsChunk c, Lib3dsIo io)
		{
			lib3ds_io_seek(io, -6, Lib3dsIoSeek.LIB3DS_SEEK_CUR);
		}

		static void lib3ds_chunk_read_end(Lib3dsChunk c, Lib3dsIo io)
		{
			io.log_indent--;
			lib3ds_io_seek(io, c.end, Lib3dsIoSeek.LIB3DS_SEEK_SET);
		}

		// Writes a 3d-Studio chunk header into a little endian file stream.
		//
		// \param c  The chunk to be written.
		// \param io The file stream.
		static void lib3ds_chunk_write(Lib3dsChunk c, Lib3dsIo io)
		{
			Debug.Assert(c!=null);
			lib3ds_io_write_word(io, (ushort)c.chunk);
			lib3ds_io_write_dword(io, c.size);
		}

		static void lib3ds_chunk_write_start(Lib3dsChunk c, Lib3dsIo io)
		{
			Debug.Assert(c!=null);
			c.size=0;
			c.cur=(uint)lib3ds_io_tell(io);
			lib3ds_io_write_word(io, (ushort)c.chunk);
			lib3ds_io_write_dword(io, c.size);
		}

		static void lib3ds_chunk_write_end(Lib3dsChunk c, Lib3dsIo io)
		{
			Debug.Assert(c!=null);
			c.size=(uint)lib3ds_io_tell(io)-c.cur;
			lib3ds_io_seek(io, c.cur+2, Lib3dsIoSeek.LIB3DS_SEEK_SET);
			lib3ds_io_write_dword(io, c.size);
			c.cur+=c.size;
			lib3ds_io_seek(io, c.cur, Lib3dsIoSeek.LIB3DS_SEEK_SET);
		}

		static void lib3ds_chunk_unknown(Lib3dsChunks chunk, Lib3dsIo io)
		{
			if(io.log_func!=null)
			{
				lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_WARN, "Unknown Chunk: {0} (0x{1:X})", lib3ds_chunk_name(chunk), chunk);
			}
		}
	}
}
