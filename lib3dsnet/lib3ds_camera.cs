// lib3ds_camera.cs - Camera
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
		// Return a new Lib3dsCamera object.
		//
		// Object is initialized with the given name and fov=45.  All other
		// values are 0.
		//
		// \param name Name of this camera.  Must not be NULL.  Must be < 64 characters.
		//
		// \return Lib3dsCamera object or NULL on failure.
		public static Lib3dsCamera lib3ds_camera_new(string name)
		{
			Debug.Assert(name!=null);
			Debug.Assert(name.Length<64);
			try
			{
				Lib3dsCamera camera=new Lib3dsCamera();
				camera.name=name;
				camera.fov=45.0f;
				return camera;
			}
			catch
			{
				return null;
			}
		}

		// Free a Lib3dsCamera object and all of its resources.
		//
		// \param camera Lib3dsCamera object to be freed.
		public static void lib3ds_camera_free(Lib3dsCamera camera)
		{
		}

		// Read a camera definition from a file.
		//
		// This function is called by lib3ds_file_read(), and you probably
		// don't want to call it directly.
		//
		// \param camera A Lib3dsCamera to be filled in.
		// \param io A Lib3dsIo object previously set up by the caller.
		//
		// \see lib3ds_file_read
		public static void lib3ds_camera_read(Lib3dsCamera camera, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_N_CAMERA, io);

			for(int i=0; i<3; i++) camera.position[i]=lib3ds_io_read_float(io);
			for(int i=0; i<3; i++) camera.target[i]=lib3ds_io_read_float(io);
			camera.roll=lib3ds_io_read_float(io);

			float s=lib3ds_io_read_float(io);
			if(Math.Abs(s)<EPSILON) camera.fov=45.0f;
			else camera.fov=2400.0f/s;

			lib3ds_chunk_read_tell(c, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_CAM_SEE_CONE: camera.see_cone=true; break;
					case Lib3dsChunks.CHK_CAM_RANGES:
						camera.near_range=lib3ds_io_read_float(io);
						camera.far_range=lib3ds_io_read_float(io);
						break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		// Write a camera definition to a file.
		//
		// This function is called by lib3ds_file_write(), and you probably
		// don't want to call it directly.
		//
		// \param camera A Lib3dsCamera to be written.
		// \param io A Lib3dsIo object previously set up by the caller.
		//
		// \see lib3ds_file_write
		public static void lib3ds_camera_write(Lib3dsCamera camera, Lib3dsIo io)
		{
			Lib3dsChunk c_n_camera=new Lib3dsChunk();

			c_n_camera.chunk=Lib3dsChunks.CHK_N_CAMERA;
			lib3ds_chunk_write_start(c_n_camera, io);

			lib3ds_io_write_vector(io, camera.position);
			lib3ds_io_write_vector(io, camera.target);
			lib3ds_io_write_float(io, camera.roll);
			if(Math.Abs(camera.fov)<EPSILON) lib3ds_io_write_float(io, 2400.0f/45.0f);
			else lib3ds_io_write_float(io, 2400.0f/camera.fov);

			if(camera.see_cone)
			{
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_CAM_SEE_CONE;
				c.size=6;
				lib3ds_chunk_write(c, io);
			}

			Lib3dsChunk c_cam_ranges=new Lib3dsChunk();
			c_cam_ranges.chunk=Lib3dsChunks.CHK_CAM_RANGES;
			c_cam_ranges.size=14;
			lib3ds_chunk_write(c_cam_ranges, io);
			lib3ds_io_write_float(io, camera.near_range);
			lib3ds_io_write_float(io, camera.far_range);

			lib3ds_chunk_write_end(c_n_camera, io);
		}
	}
}
