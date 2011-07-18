// lib3ds_file.cs - Parse file
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		internal static long fileio_seek_func(Stream file, long offset, Lib3dsIoSeek origin)
		{
			SeekOrigin o=SeekOrigin.Begin;
			switch(origin)
			{
				case Lib3dsIoSeek.LIB3DS_SEEK_SET: o=SeekOrigin.Begin; break;
				case Lib3dsIoSeek.LIB3DS_SEEK_CUR: o=SeekOrigin.Current; break;
				case Lib3dsIoSeek.LIB3DS_SEEK_END: o=SeekOrigin.End; break;
				default: Debug.Assert(false); return 0;
			}
			return file.Seek(offset, o);
		}

		internal static long fileio_tell_func(Stream file)
		{
			return file.Position;
		}

		internal static int fileio_read_func(Stream file, byte[] buffer, int size)
		{
			return file.Read(buffer, 0, size);
		}

		internal static int fileio_write_func(Stream file, byte[] buffer, int size)
		{
			file.Write(buffer, 0, size);
			return size;
		}

		// Loads a .3DS file from disk into memory.
		//
		// \param filename	The filename of the .3DS file
		//
		// \return	A pointer to the Lib3dsFile structure containing the
		//			data of the .3DS file.
		//			If the .3DS file can not be loaded NULL is returned.
		//
		// \note	To free the returned structure use lib3ds_free.
		//
		// \see lib3ds_file_save
		// \see lib3ds_file_new
		// \see lib3ds_file_free
		public static Lib3dsFile lib3ds_file_open(string filename)
		{
			return lib3ds_file_open(filename, null);
		}

		public static Lib3dsFile lib3ds_file_open(string filename, log_func log_func)
		{
			try
			{
				FileStream f=File.Open(filename, FileMode.Open, FileAccess.Read);

				try
				{
					Lib3dsIo io=new Lib3dsIo();
					io.self=f;
					io.seek_func=fileio_seek_func;
					io.tell_func=fileio_tell_func;
					io.read_func=fileio_read_func;
					io.write_func=fileio_write_func;
					io.log_func=log_func;

					Lib3dsFile file=lib3ds_file_new();
					if(file==null) return null;

					if(!lib3ds_file_read(file, io)) return null;

					return file;
				}
				finally
				{
					f.Close();
				}
			}
			catch
			{
				return null;
			}
		}

		// Saves a .3DS file from memory to disk.
		//
		// \param file		A pointer to a Lib3dsFile structure containing the
		//					the data that should be stored.
		// \param filename	The filename of the .3DS file to store the data in.
		//
		// \return			true on success, false otherwise.
		//
		// \see lib3ds_file_open
		public static bool lib3ds_file_save(Lib3dsFile file, string filename)
		{
			try
			{
				FileStream f=File.Create(filename);

				try
				{
					Lib3dsIo io=new Lib3dsIo();
					io.self=f;
					io.seek_func=fileio_seek_func;
					io.tell_func=fileio_tell_func;
					io.read_func=fileio_read_func;
					io.write_func=fileio_write_func;
					io.log_func=null;

					return lib3ds_file_write(file, io);
				}
				finally
				{
					f.Close();
				}
			}
			catch
			{
				return false;
			}
		}

		// Creates and returns a new, empty Lib3dsFile object.
		//
		// \return A pointer to the Lib3dsFile structure.
		//  If the structure cannot be allocated, NULL is returned.
		public static Lib3dsFile lib3ds_file_new()
		{
			try
			{
				Lib3dsFile file=new Lib3dsFile();

				file.mesh_version=3;
				file.master_scale=1.0f;
				file.keyf_revision=5;
				file.name="LIB3DS";

				file.frames=100;
				file.segment_from=0;
				file.segment_to=100;
				file.current_frame=0;

				return file;
			}
			catch
			{
				return null;
			}
		}

		// Free a Lib3dsFile object and all of its resources.
		//
		// \param file The Lib3dsFile object to be freed.
		public static void lib3ds_file_free(Lib3dsFile file)
		{
			Debug.Assert(file!=null);

			lib3ds_file_reserve_materials(file, 0, true);
			lib3ds_file_reserve_cameras(file, 0, true);
			lib3ds_file_reserve_lights(file, 0, true);
			lib3ds_file_reserve_meshes(file, 0, true);

			foreach(Lib3dsNode p in file.nodes) lib3ds_node_free(p);
			file.nodes.Clear();
		}

		// Evaluate all of the nodes in this Lib3dsFile object.
		//
		// \param file The Lib3dsFile object to be evaluated.
		// \param t time value, between 0. and file->frames
		//
		// \see lib3ds_node_eval
		public static void lib3ds_file_eval(Lib3dsFile file, float t)
		{
			foreach(Lib3dsNode p in file.nodes) lib3ds_node_eval(p, t);
		}

		static void named_object_read(Lib3dsFile file, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			string name;
			Lib3dsChunks chunk;

			Lib3dsMesh mesh=null;
			Lib3dsCamera camera=null;
			Lib3dsLight light=null;
			Lib3dsObjectFlags object_flags=0;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_NAMED_OBJECT, io);

			name=lib3ds_io_read_string(io, 64);
			lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_INFO, "NAME={0}", name);
			lib3ds_chunk_read_tell(c, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_N_TRI_OBJECT:
						mesh=lib3ds_mesh_new(name);
						lib3ds_file_insert_mesh(file, mesh, -1);
						lib3ds_chunk_read_reset(c, io);
						lib3ds_mesh_read(file, mesh, io);
						break;
					case Lib3dsChunks.CHK_N_CAMERA:
						camera=lib3ds_camera_new(name);
						lib3ds_file_insert_camera(file, camera, -1);
						lib3ds_chunk_read_reset(c, io);
						lib3ds_camera_read(camera, io);
						break;
					case Lib3dsChunks.CHK_N_DIRECT_LIGHT:
						light=lib3ds_light_new(name);
						lib3ds_file_insert_light(file, light, -1);
						lib3ds_chunk_read_reset(c, io);
						lib3ds_light_read(light, io);
						break;
					case Lib3dsChunks.CHK_OBJ_HIDDEN: object_flags|=Lib3dsObjectFlags.LIB3DS_OBJECT_HIDDEN; break;
					case Lib3dsChunks.CHK_OBJ_DOESNT_CAST: object_flags|=Lib3dsObjectFlags.LIB3DS_OBJECT_DOESNT_CAST; break;
					case Lib3dsChunks.CHK_OBJ_VIS_LOFTER: object_flags|=Lib3dsObjectFlags.LIB3DS_OBJECT_VIS_LOFTER; break;
					case Lib3dsChunks.CHK_OBJ_MATTE: object_flags|=Lib3dsObjectFlags.LIB3DS_OBJECT_MATTE; break;
					case Lib3dsChunks.CHK_OBJ_DONT_RCVSHADOW: object_flags|=Lib3dsObjectFlags.LIB3DS_OBJECT_DONT_RCVSHADOW; break;
					case Lib3dsChunks.CHK_OBJ_FAST: object_flags|=Lib3dsObjectFlags.LIB3DS_OBJECT_FAST; break;
					case Lib3dsChunks.CHK_OBJ_FROZEN: object_flags|=Lib3dsObjectFlags.LIB3DS_OBJECT_FROZEN; break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			if(mesh!=null) mesh.object_flags=object_flags;
			if(camera!=null) camera.object_flags=object_flags;
			if(light!=null) light.object_flags=object_flags;

			lib3ds_chunk_read_end(c, io);
		}

		static void ambient_read(Lib3dsFile file, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;
			bool have_lin=false;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_AMBIENT_LIGHT, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_LIN_COLOR_F:
						for(int i=0; i<3; i++) file.ambient[i]=lib3ds_io_read_float(io);
						have_lin=true;
						break;
					case Lib3dsChunks.CHK_COLOR_F:
						// gamma corrected color chunk
						// replaced in 3ds R3 by LIN_COLOR_24
						if(!have_lin)
						{
							for(int i=0; i<3; i++) file.ambient[i]=lib3ds_io_read_float(io);
						}
						break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		static void mdata_read(Lib3dsFile file, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_MDATA, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_MESH_VERSION: file.mesh_version=lib3ds_io_read_dword(io); break;
					case Lib3dsChunks.CHK_MASTER_SCALE: file.master_scale=lib3ds_io_read_float(io); break;
					case Lib3dsChunks.CHK_SHADOW_MAP_SIZE:
					case Lib3dsChunks.CHK_LO_SHADOW_BIAS:
					case Lib3dsChunks.CHK_HI_SHADOW_BIAS:
					case Lib3dsChunks.CHK_SHADOW_SAMPLES:
					case Lib3dsChunks.CHK_SHADOW_RANGE:
					case Lib3dsChunks.CHK_SHADOW_FILTER:
					case Lib3dsChunks.CHK_RAY_BIAS:
						lib3ds_chunk_read_reset(c, io);
						lib3ds_shadow_read(file.shadow, io);
						break;
					case Lib3dsChunks.CHK_VIEWPORT_LAYOUT:
					case Lib3dsChunks.CHK_DEFAULT_VIEW:
						lib3ds_chunk_read_reset(c, io);
						lib3ds_viewport_read(file.viewport, io);
						break;
					case Lib3dsChunks.CHK_O_CONSTS:
						for(int i=0; i<3; i++) file.construction_plane[i]=lib3ds_io_read_float(io);
						break;
					case Lib3dsChunks.CHK_AMBIENT_LIGHT:
						lib3ds_chunk_read_reset(c, io);
						ambient_read(file, io);
						break;
					case Lib3dsChunks.CHK_BIT_MAP:
					case Lib3dsChunks.CHK_SOLID_BGND:
					case Lib3dsChunks.CHK_V_GRADIENT:
					case Lib3dsChunks.CHK_USE_BIT_MAP:
					case Lib3dsChunks.CHK_USE_SOLID_BGND:
					case Lib3dsChunks.CHK_USE_V_GRADIENT:
						lib3ds_chunk_read_reset(c, io);
						lib3ds_background_read(file.background, io);
						break;
					case Lib3dsChunks.CHK_FOG:
					case Lib3dsChunks.CHK_LAYER_FOG:
					case Lib3dsChunks.CHK_DISTANCE_CUE:
					case Lib3dsChunks.CHK_USE_FOG:
					case Lib3dsChunks.CHK_USE_LAYER_FOG:
					case Lib3dsChunks.CHK_USE_DISTANCE_CUE:
						lib3ds_chunk_read_reset(c, io);
						lib3ds_atmosphere_read(file.atmosphere, io);
						break;
					case Lib3dsChunks.CHK_MAT_ENTRY:
						Lib3dsMaterial material=lib3ds_material_new(null);
						lib3ds_file_insert_material(file, material, -1);
						lib3ds_chunk_read_reset(c, io);
						lib3ds_material_read(material, io);
						break;
					case Lib3dsChunks.CHK_NAMED_OBJECT:
						lib3ds_chunk_read_reset(c, io);
						named_object_read(file, io);
						break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		static int compare_node_id(Lib3dsNode a, Lib3dsNode b)
		{
			return a.node_id-b.node_id;
		}

		static int compare_node_id2(ushort a, Lib3dsNode b)
		{
			return a-b.node_id;
		}

		static void kfdata_read(Lib3dsFile file, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_KFDATA, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_KFHDR:
						file.keyf_revision=lib3ds_io_read_word(io);
						file.name=lib3ds_io_read_string(io, 12+1);
						file.frames=lib3ds_io_read_intd(io);
						break;
					case Lib3dsChunks.CHK_KFSEG:
						file.segment_from=lib3ds_io_read_intd(io);
						file.segment_to=lib3ds_io_read_intd(io);
						break;
					case Lib3dsChunks.CHK_KFCURTIME:
						file.current_frame=lib3ds_io_read_intd(io);
						break;
					case Lib3dsChunks.CHK_VIEWPORT_LAYOUT:
					case Lib3dsChunks.CHK_DEFAULT_VIEW:
						lib3ds_chunk_read_reset(c, io);
						lib3ds_viewport_read(file.viewport_keyf, io);
						break;
					case Lib3dsChunks.CHK_AMBIENT_NODE_TAG:
					case Lib3dsChunks.CHK_OBJECT_NODE_TAG:
					case Lib3dsChunks.CHK_CAMERA_NODE_TAG:
					case Lib3dsChunks.CHK_TARGET_NODE_TAG:
					case Lib3dsChunks.CHK_LIGHT_NODE_TAG:
					case Lib3dsChunks.CHK_SPOTLIGHT_NODE_TAG:
					case Lib3dsChunks.CHK_L_TARGET_NODE_TAG:
						Lib3dsNodeType type;
						switch(chunk)
						{
							case Lib3dsChunks.CHK_AMBIENT_NODE_TAG: type=Lib3dsNodeType.LIB3DS_NODE_AMBIENT_COLOR; break;
							case Lib3dsChunks.CHK_OBJECT_NODE_TAG: type=Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE; break;
							case Lib3dsChunks.CHK_CAMERA_NODE_TAG: type=Lib3dsNodeType.LIB3DS_NODE_CAMERA; break;
							case Lib3dsChunks.CHK_TARGET_NODE_TAG: type=Lib3dsNodeType.LIB3DS_NODE_CAMERA_TARGET; break;
							case Lib3dsChunks.CHK_LIGHT_NODE_TAG: type=Lib3dsNodeType.LIB3DS_NODE_OMNILIGHT; break;
							case Lib3dsChunks.CHK_SPOTLIGHT_NODE_TAG: type=Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT; break;
							case Lib3dsChunks.CHK_L_TARGET_NODE_TAG: type=Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT_TARGET; break;
							default: throw new Exception("Unknown chunk type.");
						}

						Lib3dsNode node=lib3ds_node_new(type);
						file.nodes.Add(node);
						lib3ds_chunk_read_reset(c, io);
						lib3ds_node_read(node, io);
						break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			// fehlende Node IDs vergeben
			List<Lib3dsNode> missingIDs=new List<Lib3dsNode>();
			Dictionary<ushort, Lib3dsNode> hasIDs=new Dictionary<ushort, Lib3dsNode>();

			foreach(Lib3dsNode node in file.nodes)
			{
				if(!node.hasNodeID) missingIDs.Add(node);
				else if(!hasIDs.ContainsKey(node.node_id)) hasIDs.Add(node.node_id, node);
			}

			ushort num_nodes=0;
			foreach(Lib3dsNode node in missingIDs)
			{
				while(hasIDs.ContainsKey(num_nodes))
				{
					num_nodes++;
					if(num_nodes==65535) throw new Exception("Out of IDs.");
				}

				node.node_id=num_nodes;
				node.hasNodeID=true;
				hasIDs.Add(num_nodes, node);

				num_nodes++;
				if(num_nodes==65535) throw new Exception("Out of IDs.");
			}

			missingIDs.Clear();

			foreach(Lib3dsNode node in file.nodes)
			{
				if(node.parent_id!=65535&&hasIDs.ContainsKey(node.parent_id))
				{
					Lib3dsNode parent=hasIDs[node.parent_id];
					parent.childs.Add(node);
					node.parent=parent;
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		// Read 3ds file data into a Lib3dsFile object.
		//
		// \param file The Lib3dsFile object to be filled.
		// \param io A Lib3dsIo object previously set up by the caller.
		//
		// \return true on success, false on failure.
		public static bool lib3ds_file_read(Lib3dsFile file, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			lib3ds_io_setup(io);

			try
			{
				lib3ds_chunk_read_start(c, 0, io);

				switch(c.chunk)
				{
					case Lib3dsChunks.CHK_MDATA:
						lib3ds_chunk_read_reset(c, io);
						mdata_read(file, io);
						break;
					case Lib3dsChunks.CHK_M3DMAGIC:
					case Lib3dsChunks.CHK_MLIBMAGIC:
					case Lib3dsChunks.CHK_CMAGIC:
						while((chunk=lib3ds_chunk_read_next(c, io))!=0)
						{
							switch(chunk)
							{
								case Lib3dsChunks.CHK_M3D_VERSION:
									file.mesh_version=lib3ds_io_read_dword(io);
									break;
								case Lib3dsChunks.CHK_MDATA:
									lib3ds_chunk_read_reset(c, io);
									mdata_read(file, io);
									break;
								case Lib3dsChunks.CHK_KFDATA:
									lib3ds_chunk_read_reset(c, io);
									kfdata_read(file, io);
									break;
								default: lib3ds_chunk_unknown(chunk, io); break;
							}
						}
						break;
					default:
						lib3ds_chunk_unknown(c.chunk, io);
						return false;
				}

				lib3ds_chunk_read_end(c, io);
				lib3ds_io_cleanup(io);
				return true;
			}
			catch
			{
				lib3ds_io_cleanup(io);
				return false;
			}
		}

		static void colorf_write(float[] rgb, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();

			c.chunk=Lib3dsChunks.CHK_COLOR_F;
			c.size=18;
			lib3ds_chunk_write(c, io);
			lib3ds_io_write_rgb(io, rgb);

			c.chunk=Lib3dsChunks.CHK_LIN_COLOR_F;
			c.size=18;
			lib3ds_chunk_write(c, io);
			lib3ds_io_write_rgb(io, rgb);
		}

		static void object_flags_write(Lib3dsObjectFlags flags, Lib3dsIo io)
		{
			if(flags==0) return;

			Lib3dsChunk c=new Lib3dsChunk();
			c.size=6;

			if((flags&Lib3dsObjectFlags.LIB3DS_OBJECT_HIDDEN)!=0)
			{
				c.chunk=Lib3dsChunks.CHK_OBJ_HIDDEN;
				lib3ds_chunk_write(c, io);
			}
			if((flags&Lib3dsObjectFlags.LIB3DS_OBJECT_VIS_LOFTER)!=0)
			{
				c.chunk=Lib3dsChunks.CHK_OBJ_VIS_LOFTER;
				lib3ds_chunk_write(c, io);
			}
			if((flags&Lib3dsObjectFlags.LIB3DS_OBJECT_DOESNT_CAST)!=0)
			{
				c.chunk=Lib3dsChunks.CHK_OBJ_DOESNT_CAST;
				lib3ds_chunk_write(c, io);
			}
			if((flags&Lib3dsObjectFlags.LIB3DS_OBJECT_MATTE)!=0)
			{
				c.chunk=Lib3dsChunks.CHK_OBJ_MATTE;
				lib3ds_chunk_write(c, io);
			}
			if((flags&Lib3dsObjectFlags.LIB3DS_OBJECT_DONT_RCVSHADOW)!=0)
			{
				c.chunk=Lib3dsChunks.CHK_OBJ_DOESNT_CAST;
				lib3ds_chunk_write(c, io);
			}
			if((flags&Lib3dsObjectFlags.LIB3DS_OBJECT_FAST)!=0)
			{
				c.chunk=Lib3dsChunks.CHK_OBJ_FAST;
				lib3ds_chunk_write(c, io);
			}
			if((flags&Lib3dsObjectFlags.LIB3DS_OBJECT_FROZEN)!=0)
			{
				c.chunk=Lib3dsChunks.CHK_OBJ_FROZEN;
				lib3ds_chunk_write(c, io);
			}
		}

		static void mdata_write(Lib3dsFile file, Lib3dsIo io)
		{
			Lib3dsChunk c_mdata=new Lib3dsChunk();

			c_mdata.chunk=Lib3dsChunks.CHK_MDATA;
			lib3ds_chunk_write_start(c_mdata, io);

			{ // ---- LIB3DS_MESH_VERSION ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_MESH_VERSION;
				c.size=10;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_intd(io, (int)file.mesh_version);
			}
			{ // ---- LIB3DS_MASTER_SCALE ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_MASTER_SCALE;
				c.size=10;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_float(io, file.master_scale);
			}
			{ // ---- LIB3DS_O_CONSTS ----
				int i;
				for(i=0; i<3; i++) if(Math.Abs(file.construction_plane[i])>EPSILON) break;
				if(i<3)
				{
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_O_CONSTS;
					c.size=18;
					lib3ds_chunk_write(c, io);
					lib3ds_io_write_vector(io, file.construction_plane);
				}
			}
			{ // ---- LIB3DS_AMBIENT_LIGHT ----
				int i;
				for(i=0; i<3; i++) if(Math.Abs(file.ambient[i])>EPSILON) break;
				if(i<3)
				{
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_AMBIENT_LIGHT;
					c.size=42;
					lib3ds_chunk_write(c, io);
					colorf_write(file.ambient, io);
				}
			}
			lib3ds_background_write(file.background, io);
			lib3ds_atmosphere_write(file.atmosphere, io);
			lib3ds_shadow_write(file.shadow, io);
			lib3ds_viewport_write(file.viewport, io);
			{
				foreach(Lib3dsMaterial material in file.materials) lib3ds_material_write(material, io);
			}
			{
				Lib3dsChunk c=new Lib3dsChunk();
				foreach(Lib3dsCamera camera in file.cameras)
				{
					c.chunk=Lib3dsChunks.CHK_NAMED_OBJECT;
					lib3ds_chunk_write_start(c, io);
					lib3ds_io_write_string(io, camera.name);
					lib3ds_camera_write(camera, io);
					object_flags_write(camera.object_flags, io);
					lib3ds_chunk_write_end(c, io);
				}
			}
			{
				Lib3dsChunk c=new Lib3dsChunk();
				foreach(Lib3dsLight light in file.lights)
				{
					c.chunk=Lib3dsChunks.CHK_NAMED_OBJECT;
					lib3ds_chunk_write_start(c, io);
					lib3ds_io_write_string(io, light.name);
					lib3ds_light_write(light, io);
					object_flags_write(light.object_flags, io);
					lib3ds_chunk_write_end(c, io);
				}
			}
			{
				Lib3dsChunk c=new Lib3dsChunk();
				foreach(Lib3dsMesh mesh in file.meshes)
				{
					c.chunk=Lib3dsChunks.CHK_NAMED_OBJECT;
					lib3ds_chunk_write_start(c, io);
					lib3ds_io_write_string(io, mesh.name);
					lib3ds_mesh_write(file, mesh, io);
					object_flags_write(mesh.object_flags, io);
					lib3ds_chunk_write_end(c, io);
				}
			}

			lib3ds_chunk_write_end(c_mdata, io);
		}

		static void nodes_write(List<Lib3dsNode> nodes, ref ushort default_id, ushort parent_id, Lib3dsIo io)
		{
			foreach(Lib3dsNode p in nodes)
			{
				ushort node_id;
				if((p.type==Lib3dsNodeType.LIB3DS_NODE_AMBIENT_COLOR)||(p.node_id!=65535)) node_id=p.node_id;
				else node_id=default_id;
				default_id++;
				lib3ds_node_write(p, node_id, parent_id, io);

				nodes_write(p.childs, ref default_id, node_id, io);
			}
		}

		static void kfdata_write(Lib3dsFile file, Lib3dsIo io)
		{
			if(file.nodes.Count==0) return;

			Lib3dsChunk c_kfdata=new Lib3dsChunk();
			c_kfdata.chunk=Lib3dsChunks.CHK_KFDATA;
			lib3ds_chunk_write_start(c_kfdata, io);

			{ // ---- LIB3DS_KFHDR ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_KFHDR;
				c.size=6+2+(uint)file.name.Length+1+4;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_intw(io, (short)file.keyf_revision);
				lib3ds_io_write_string(io, file.name);
				lib3ds_io_write_intd(io, file.frames);
			}
			{ // ---- LIB3DS_KFSEG ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_KFSEG;
				c.size=14;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_intd(io, file.segment_from);
				lib3ds_io_write_intd(io, file.segment_to);
			}
			{ // ---- LIB3DS_KFCURTIME ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_KFCURTIME;
				c.size=10;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_intd(io, file.current_frame);
			}
			lib3ds_viewport_write(file.viewport_keyf, io);

			ushort default_id=0;
			nodes_write(file.nodes, ref default_id, 65535, io);

			lib3ds_chunk_write_end(c_kfdata, io);
		}

		// Write 3ds file data from a Lib3dsFile object to a file.
		//
		// \param file The Lib3dsFile object to be written.
		// \param io A Lib3dsIo object previously set up by the caller.
		//
		// \return true on success, false on failure.
		public static bool lib3ds_file_write(Lib3dsFile file, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();

			lib3ds_io_setup(io);

			try
			{
				c.chunk=Lib3dsChunks.CHK_M3DMAGIC;
				lib3ds_chunk_write_start(c, io);

				// ---- LIB3DS_M3D_VERSION ----
				Lib3dsChunk c_version=new Lib3dsChunk();
				c_version.chunk=Lib3dsChunks.CHK_M3D_VERSION;
				c_version.size=10;
				lib3ds_chunk_write(c_version, io);
				lib3ds_io_write_dword(io, file.mesh_version);

				mdata_write(file, io);
				kfdata_write(file, io);

				lib3ds_chunk_write_end(c, io);

				lib3ds_io_cleanup(io);
				return true;
			}
			catch
			{
				lib3ds_io_cleanup(io);
				return false;
			}
		}

		public static void lib3ds_file_reserve_materials(Lib3dsFile file, int size, bool force)
		{
			Debug.Assert(file!=null);
			// nix mehr
		}

		public static void lib3ds_file_insert_material(Lib3dsFile file, Lib3dsMaterial material, int index)
		{
			Debug.Assert(file!=null);
			if(index<0) file.materials.Add(material);
			else file.materials.Insert(index, material);
		}

		public static void lib3ds_file_remove_material(Lib3dsFile file, int index)
		{
			Debug.Assert(file!=null);
			file.materials.RemoveAt(index);
		}

		public static int lib3ds_file_material_by_name(Lib3dsFile file, string name)
		{
			Debug.Assert(file!=null);
			for(int i=0; i<file.materials.Count; i++)
				if(file.materials[i].name==name) return i;
			return -1;
		}

		public static void lib3ds_file_reserve_cameras(Lib3dsFile file, int size, bool force)
		{
			Debug.Assert(file!=null);
			// nix
		}

		public static void lib3ds_file_insert_camera(Lib3dsFile file, Lib3dsCamera camera, int index)
		{
			Debug.Assert(file!=null);
			if(index<0) file.cameras.Add(camera);
			else file.cameras.Insert(index, camera);
		}

		public static void lib3ds_file_remove_camera(Lib3dsFile file, int index)
		{
			Debug.Assert(file!=null);
			file.cameras.RemoveAt(index);
		}

		public static int lib3ds_file_camera_by_name(Lib3dsFile file, string name)
		{
			Debug.Assert(file!=null);
			for(int i=0; i<file.cameras.Count; i++)
				if(file.cameras[i].name==name) return i;
			return -1;
		}

		public static void lib3ds_file_reserve_lights(Lib3dsFile file, int size, bool force)
		{
			Debug.Assert(file!=null);
			// nix
		}

		public static void lib3ds_file_insert_light(Lib3dsFile file, Lib3dsLight light, int index)
		{
			Debug.Assert(file!=null);
			if(index<0) file.lights.Add(light);
			else file.lights.Insert(index, light);
		}

		public static void lib3ds_file_remove_light(Lib3dsFile file, int index)
		{
			Debug.Assert(file!=null);
			file.lights.RemoveAt(index);
		}

		public static int lib3ds_file_light_by_name(Lib3dsFile file, string name)
		{
			Debug.Assert(file!=null);
			for(int i=0; i<file.lights.Count; i++)
				if(file.lights[i].name==name) return i;
			return -1;
		}

		public static void lib3ds_file_reserve_meshes(Lib3dsFile file, int size, bool force)
		{
			Debug.Assert(file!=null);
			// nix
		}

		public static void lib3ds_file_insert_mesh(Lib3dsFile file, Lib3dsMesh mesh, int index)
		{
			Debug.Assert(file!=null);
			if(index<0) file.meshes.Add(mesh);
			else file.meshes.Insert(index, mesh);
		}

		public static void lib3ds_file_remove_mesh(Lib3dsFile file, int index)
		{
			Debug.Assert(file!=null);
			file.meshes.RemoveAt(index);
		}

		public static int lib3ds_file_mesh_by_name(Lib3dsFile file, string name)
		{
			Debug.Assert(file!=null);
			for(int i=0; i<file.meshes.Count; i++)
				if(file.meshes[i].name==name) return i;
			return -1;
		}

		public static Lib3dsMesh lib3ds_file_mesh_for_node(Lib3dsFile file, Lib3dsNode node)
		{
			if(node.type!=Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE) return null;
			Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
			int index=lib3ds_file_mesh_by_name(file, node.name);
			return (index>=0)?file.meshes[index]:null;
		}

		// Return a node object by name and type.
		//
		// This function performs a recursive search for the specified node.
		// Both name and type must match.
		//
		// \param file The Lib3dsFile to be searched.
		// \param name The target node name.
		// \param type The target node type
		//
		// \return A pointer to the first matching node, or NULL if not found.
		//
		// \see lib3ds_node_by_name
		public static Lib3dsNode lib3ds_file_node_by_name(Lib3dsFile file, string name, Lib3dsNodeType type)
		{
			Debug.Assert(file!=null);
			foreach(Lib3dsNode p in file.nodes)
			{
				if(p.type==type&&p.name==name) return p;
				Lib3dsNode q=lib3ds_node_by_name(p, name, type);
				if(q!=null) return q;
			}
			return null;
		}

		// Return a node object by id.
		//
		// This function performs a recursive search for the specified node.
		//
		// \param file The Lib3dsFile to be searched.
		// \param node_id The target node id.
		//
		// \return A pointer to the first matching node, or NULL if not found.
		//
		// \see lib3ds_node_by_id
		public static Lib3dsNode lib3ds_file_node_by_id(Lib3dsFile file, ushort node_id)
		{
			Debug.Assert(file!=null);
			foreach(Lib3dsNode p in file.nodes)
			{
				if(p.node_id==node_id) return p;
				Lib3dsNode q=lib3ds_node_by_id(p, node_id);
				if(q!=null) return q;
			}
			return null;
		}

		public static void lib3ds_file_append_node(Lib3dsFile file, Lib3dsNode node, Lib3dsNode parent)
		{
			Debug.Assert(file!=null);
			Debug.Assert(node!=null);

			List<Lib3dsNode> list=parent!=null?parent.childs:file.nodes;
			list.Add(node);
			node.parent=parent;
		}

		public static void lib3ds_file_insert_node(Lib3dsFile file, Lib3dsNode node, Lib3dsNode before)
		{
			Debug.Assert(file!=null);
			Debug.Assert(node!=null);

			if(before!=null)
			{
				List<Lib3dsNode> list=before.parent!=null?before.parent.childs:file.nodes;
				Debug.Assert(list.Count!=0);
				int index=list.IndexOf(before);

				if(index>=0) list.Insert(index, node);
				else list.Add(node);
				node.parent=before.parent;
			}
			else
			{
				file.nodes.Insert(0, node);
				node.parent=null;
			}
		}

		// Remove a node from the a Lib3dsFile object.
		//
		// \param file The Lib3dsFile object to be modified.
		// \param node The Lib3dsNode object to be removed from file
		public static void lib3ds_file_remove_node(Lib3dsFile file, Lib3dsNode node)
		{
			if(node.parent!=null) node.parent.childs.Remove(node);
			else file.nodes.Remove(node);
		}

		static void file_minmax_node_id_impl(Lib3dsFile file, Lib3dsNode node, ref ushort min_id, ref ushort max_id)
		{
			if(min_id>node.node_id) min_id=node.node_id;
			if(max_id<node.node_id) max_id=node.node_id;

			foreach(Lib3dsNode p in node.childs)
				file_minmax_node_id_impl(file, p, ref min_id, ref max_id);
		}

		public static void lib3ds_file_minmax_node_id(Lib3dsFile file, ref ushort min_id, ref ushort max_id)
		{
			min_id=65535;
			max_id=0;

			foreach(Lib3dsNode p in file.nodes)
				file_minmax_node_id_impl(file, p, ref min_id, ref max_id);
		}

		public static void lib3ds_file_bounding_box_of_objects(Lib3dsFile file, bool include_meshes, bool include_cameras, bool include_lights, float[] bmin, float[] bmax)
		{
			bmin[0]=bmin[1]=bmin[2]=float.MaxValue;
			bmax[0]=bmax[1]=bmax[2]=-float.MaxValue;

			if(include_meshes)
			{
				float[] lmin=new float[3], lmax=new float[3];
				foreach(Lib3dsMesh mesh in file.meshes)
				{
					lib3ds_mesh_bounding_box(mesh, lmin, lmax);
					lib3ds_vector_min(bmin, lmin);
					lib3ds_vector_max(bmax, lmax);
				}
			}
			if(include_cameras)
			{
				foreach(Lib3dsCamera camera in file.cameras)
				{
					lib3ds_vector_min(bmin, camera.position);
					lib3ds_vector_max(bmax, camera.position);
					lib3ds_vector_min(bmin, camera.target);
					lib3ds_vector_max(bmax, camera.target);
				}
			}
			if(include_lights)
			{
				foreach(Lib3dsLight light in file.lights)
				{
					lib3ds_vector_min(bmin, light.position);
					lib3ds_vector_max(bmax, light.position);
					if(light.spot_light)
					{
						lib3ds_vector_min(bmin, light.target);
						lib3ds_vector_max(bmax, light.target);
					}
				}
			}
		}

		static void file_bounding_box_of_nodes_impl(Lib3dsNode node, Lib3dsFile file, bool include_meshes, bool include_cameras, bool include_lights, float[] bmin, float[] bmax, float[,] matrix)
		{
			switch(node.type)
			{
				case Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE:
					if(include_meshes)
					{
						Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;

						int index=lib3ds_file_mesh_by_name(file, n.instance_name);
						if(index<0) index=lib3ds_file_mesh_by_name(file, node.name);
						if(index>=0)
						{
							float[,] inv_matrix=new float[4, 4], M=new float[4, 4];
							float[] v=new float[3];

							Lib3dsMesh mesh=file.meshes[index];
							lib3ds_matrix_copy(inv_matrix, mesh.matrix);
							lib3ds_matrix_inv(inv_matrix);
							lib3ds_matrix_mult(M, matrix, node.matrixNode);
							lib3ds_matrix_translate(M, -n.pivot[0], -n.pivot[1], -n.pivot[2]);
							lib3ds_matrix_mult(M, M, inv_matrix);

							foreach(Lib3dsVertex vertex in mesh.vertices)
							{
								lib3ds_vector_transform(v, M, vertex);
								lib3ds_vector_min(bmin, v);
								lib3ds_vector_max(bmax, v);
							}
						}
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_CAMERA:
				case Lib3dsNodeType.LIB3DS_NODE_CAMERA_TARGET:
					if(include_cameras)
					{
						float[] z=new float[3], v=new float[3];
						float[,] M=new float[4, 4];
						lib3ds_matrix_mult(M, matrix, node.matrixNode);
						lib3ds_vector_zero(z);
						lib3ds_vector_transform(v, M, z);
						lib3ds_vector_min(bmin, v);
						lib3ds_vector_max(bmax, v);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_OMNILIGHT:
				case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT:
				case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT_TARGET:
					if(include_lights)
					{
						float[] z=new float[3], v=new float[3];
						float[,] M=new float[4, 4];
						lib3ds_matrix_mult(M, matrix, node.matrixNode);
						lib3ds_vector_zero(z);
						lib3ds_vector_transform(v, M, z);
						lib3ds_vector_min(bmin, v);
						lib3ds_vector_max(bmax, v);
					}
					break;
			}

			foreach(Lib3dsNode p in node.childs)
				file_bounding_box_of_nodes_impl(p, file, include_meshes, include_cameras, include_lights, bmin, bmax, matrix);
		}

		public static void lib3ds_file_bounding_box_of_nodes(Lib3dsFile file, bool include_meshes, bool include_cameras, bool include_lights, float[] bmin, float[] bmax, float[,] matrix)
		{
			float[,] M=new float[4, 4];

			if(matrix!=null) lib3ds_matrix_copy(M, matrix);
			else lib3ds_matrix_identity(M);

			bmin[0]=bmin[1]=bmin[2]=float.MaxValue;
			bmax[0]=bmax[1]=bmax[2]=-float.MaxValue;
			foreach(Lib3dsNode p in file.nodes)
				file_bounding_box_of_nodes_impl(p, file, include_meshes, include_cameras, include_lights, bmin, bmax, M);
		}

		public static void lib3ds_file_create_nodes_for_meshes(Lib3dsFile file)
		{
			foreach(Lib3dsMesh mesh in file.meshes)
			{
				Lib3dsNode p=lib3ds_node_new(Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE);
				p.name=mesh.name;
				lib3ds_file_insert_node(file, p, null);
			}
		}
	}
}
