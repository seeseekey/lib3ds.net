// lib3ds_io.cs - File
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

using System;
using System.Diagnostics;
using System.Text;

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		static void lib3ds_io_setup(Lib3dsIo io)
		{
			Debug.Assert(io!=null);
		}

		static void lib3ds_io_cleanup(Lib3dsIo io)
		{
			Debug.Assert(io!=null);
		}

		static long lib3ds_io_seek(Lib3dsIo io, long offset, Lib3dsIoSeek origin)
		{
			Debug.Assert(io!=null);
			if(io==null||io.seek_func==null) return 0;
			return io.seek_func(io.self, offset, origin);
		}

		static long lib3ds_io_tell(Lib3dsIo io)
		{
			Debug.Assert(io!=null);
			if(io==null||io.tell_func==null) return 0;
			return io.tell_func(io.self);
		}

		static int lib3ds_io_read(Lib3dsIo io, byte[] buffer, int size)
		{
			Debug.Assert(io!=null);
			if(io==null||io.read_func==null) return 0;
			return io.read_func(io.self, buffer, size);
		}

		static int lib3ds_io_write(Lib3dsIo io, byte[] buffer, int size)
		{
			Debug.Assert(io!=null);
			if(io==null||io.write_func==null) return 0;
			return io.write_func(io.self, buffer, size);
		}

		static void lib3ds_io_log_str(Lib3dsIo io, Lib3dsLogLevel level, string str)
		{
			Debug.Assert(io!=null);
			if(io==null||io.log_func==null) return;
			io.log_func(io.self, level, io.log_indent, str);
		}

		static void lib3ds_io_log(Lib3dsIo io, Lib3dsLogLevel level, string format, params object[] args)
		{
			Debug.Assert(io!=null);
			if(io==null||io.log_func==null) return;
			lib3ds_io_log_str(io, level, string.Format(format, args));
			if(level==Lib3dsLogLevel.LIB3DS_LOG_ERROR) throw new Exception();
		}

		static void lib3ds_io_log_indent(Lib3dsIo io, int indent)
		{
			Debug.Assert(io!=null);
			if(io==null) return;
			io.log_indent+=indent;
		}

		static void lib3ds_io_read_error(Lib3dsIo io)
		{
			lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_ERROR, "Reading from input stream failed.");
		}

		static void lib3ds_io_write_error(Lib3dsIo io)
		{
			lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_ERROR, "Writing to output stream failed.");
		}

		// Read a byte from a file stream.
		static byte lib3ds_io_read_byte(Lib3dsIo io)
		{
			Debug.Assert(io!=null);
			byte[] b=new byte[1];
			lib3ds_io_read(io, b, 1);
			return b[0];
		}

		// Read a word from a file stream in little endian format.
		static ushort lib3ds_io_read_word(Lib3dsIo io)
		{
			Debug.Assert(io!=null);
			byte[] b=new byte[2];
			lib3ds_io_read(io, b, 2);
			return (ushort)((b[1]<<8)|b[0]);
		}

		// Read a dword from file a stream in little endian format.
		static uint lib3ds_io_read_dword(Lib3dsIo io)
		{
			Debug.Assert(io!=null);
			byte[] b=new byte[4];
			lib3ds_io_read(io, b, 4);
			return (uint)((b[3]<<24)|(b[2]<<16)|(b[1]<<8)|b[0]);
		}

		// Read a signed byte from a file stream.
		static sbyte lib3ds_io_read_intb(Lib3dsIo io)
		{
			Debug.Assert(io!=null);
			byte[] b=new byte[1];
			lib3ds_io_read(io, b, 1);
			return (sbyte)b[0];
		}

		// Read a signed word from a file stream in little endian format.
		static short lib3ds_io_read_intw(Lib3dsIo io)
		{
			Debug.Assert(io!=null);
			byte[] b=new byte[2];
			lib3ds_io_read(io, b, 2);
			return (short)((b[1]<<8)|b[0]);
		}

		// Read a signed dword a from file stream in little endian format.
		static int lib3ds_io_read_intd(Lib3dsIo io)
		{
			Debug.Assert(io!=null);
			byte[] b=new byte[4];
			lib3ds_io_read(io, b, 4);
			return (int)((b[3]<<24)|(b[2]<<16)|(b[1]<<8)|b[0]);
		}

		// Read a float from a file stream in little endian format.
		static float lib3ds_io_read_float(Lib3dsIo io)
		{
			Debug.Assert(io!=null);
			byte[] b=new byte[4];
			lib3ds_io_read(io, b, 4);
			return BitConverter.ToSingle(b, 0);
		}

		// Read a vector from a file stream in little endian format.
		//
		// \param io IO input handle.
		// \param v  The vector to store the data.
		static void lib3ds_io_read_vector(Lib3dsIo io, float[] v)
		{
			Debug.Assert(io!=null);
			v[0]=lib3ds_io_read_float(io);
			v[1]=lib3ds_io_read_float(io);
			v[2]=lib3ds_io_read_float(io);
		}
		
		static void lib3ds_io_read_vector(Lib3dsIo io, Lib3dsVertex v)
		{
			Debug.Assert(io!=null);
			v.x=lib3ds_io_read_float(io);
			v.y=lib3ds_io_read_float(io);
			v.z=lib3ds_io_read_float(io);
		}

		static void lib3ds_io_read_rgb(Lib3dsIo io, float[] rgb)
		{
			Debug.Assert(io!=null);
			rgb[0]=lib3ds_io_read_float(io);
			rgb[1]=lib3ds_io_read_float(io);
			rgb[2]=lib3ds_io_read_float(io);
		}

		// Read a zero-terminated string from a file stream.
		static string lib3ds_io_read_string(Lib3dsIo io, int buflen)
		{
			Debug.Assert(io!=null);
			byte[] c=new byte[1];

			string s="";

			int k=0;
			for(; ; )
			{
				if(lib3ds_io_read(io, c, 1)!=1) lib3ds_io_read_error(io);
				if(c[0]==0) break;

				s+=(char)c[0];

				k++;
				if(k>=buflen) lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_ERROR, "Invalid string in input stream.");
			}

			return s;
		}

		// Writes a byte into a file stream.
		static void lib3ds_io_write_byte(Lib3dsIo io, byte b)
		{
			Debug.Assert(io!=null);
			byte[] buffer=new byte[1];
			buffer[0]=b;
			if(lib3ds_io_write(io, buffer, 1)!=1) lib3ds_io_write_error(io);
		}

		// Writes a word into a little endian file stream.
		static void lib3ds_io_write_word(Lib3dsIo io, ushort w)
		{
			Debug.Assert(io!=null);
			byte[] b=new byte[2];
			b[1]=(byte)((w&0xFF00)>>8);
			b[0]=(byte)(w&0x00FF);
			if(lib3ds_io_write(io, b, 2)!=2) lib3ds_io_write_error(io);
		}

		// Writes a dword into a little endian file stream.
		static void lib3ds_io_write_dword(Lib3dsIo io, uint d)
		{
			Debug.Assert(io!=null);
			byte[] b=new byte[4];
			b[3]=(byte)((d&0xFF000000)>>24);
			b[2]=(byte)((d&0x00FF0000)>>16);
			b[1]=(byte)((d&0x0000FF00)>>8);
			b[0]=(byte)(d&0x000000FF);
			if(lib3ds_io_write(io, b, 4)!=4) lib3ds_io_write_error(io);
		}

		// Writes a signed byte in a file stream.
		static void lib3ds_io_write_intb(Lib3dsIo io, sbyte b)
		{
			Debug.Assert(io!=null);
			byte[] buffer=new byte[1];
			buffer[0]=(byte)b;
			if(lib3ds_io_write(io, buffer, 1)!=1) lib3ds_io_write_error(io);
		}

		// Writes a signed word into a little endian file stream.
		static void lib3ds_io_write_intw(Lib3dsIo io, short w)
		{
			Debug.Assert(io!=null);
			byte[] b=new byte[2];
			b[1]=(byte)((w&0xFF00)>>8);
			b[0]=(byte)(w&0x00FF);
			if(lib3ds_io_write(io, b, 2)!=2) lib3ds_io_write_error(io);
		}

		// Writes a signed dword into a little endian file stream.
		static void lib3ds_io_write_intd(Lib3dsIo io, int d)
		{
			Debug.Assert(io!=null);
			byte[] b=new byte[4];
			b[3]=(byte)((d&0xFF000000)>>24);
			b[2]=(byte)((d&0x00FF0000)>>16);
			b[1]=(byte)((d&0x0000FF00)>>8);
			b[0]=(byte)(d&0x000000FF);
			if(lib3ds_io_write(io, b, 4)!=4) lib3ds_io_write_error(io);
		}
		
		// Writes a float into a little endian file stream.
		static void lib3ds_io_write_float(Lib3dsIo io, float l)
		{
			Debug.Assert(io!=null);
			if(lib3ds_io_write(io, BitConverter.GetBytes(l), 4)!=4) lib3ds_io_write_error(io);
		}

		// Writes a vector into a file stream in little endian format.
		static void lib3ds_io_write_vector(Lib3dsIo io, float[] v)
		{
			for(int i=0; i<3; i++) lib3ds_io_write_float(io, v[i]);
		}

		static void lib3ds_io_write_vector(Lib3dsIo io, Lib3dsVertex v)
		{
			lib3ds_io_write_float(io, v.x);
			lib3ds_io_write_float(io, v.y);
			lib3ds_io_write_float(io, v.z);
		}

		static void lib3ds_io_write_rgb(Lib3dsIo io, float[] rgb)
		{
			for(int i=0; i<3; i++) lib3ds_io_write_float(io, rgb[i]);
		}

		// Writes a zero-terminated string into a file stream.
		static void lib3ds_io_write_string(Lib3dsIo io, string s)
		{
			Debug.Assert(io!=null&&s!=null);

			byte[] b=Encoding.ASCII.GetBytes(s);
			if(lib3ds_io_write(io, b, b.Length)!=b.Length) lib3ds_io_write_error(io);

			b=new byte[1];
			b[0]=0;
			if(lib3ds_io_write(io, b, b.Length)!=b.Length) lib3ds_io_write_error(io);
		}
	}
}
