// lib3ds_mesh.cs - Mesh
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		// Create and return a new empty mesh object.
		//
		// Mesh is initialized with the name and an identity matrix; all
		// other fields are zero.
		//
		// See Lib3dsFaceFlag for definitions of per-face flags.
		//
		// \param name Mesh name.  Must not be NULL.  Must be < 64 characters.
		//
		// \return mesh object or NULL on error.
		public static Lib3dsMesh lib3ds_mesh_new(string name)
		{
			Debug.Assert(name!=null);
			Debug.Assert(name.Length<64);

			try
			{
				Lib3dsMesh mesh=new Lib3dsMesh();
				mesh.name=name;
				lib3ds_matrix_identity(mesh.matrix);
				mesh.map_type=Lib3dsMapType.LIB3DS_MAP_NONE;
				return mesh;
			}
			catch
			{
				return null;
			}
		}

		// Free a mesh object and all of its resources.
		//
		// \param mesh Mesh object to be freed.
		public static void lib3ds_mesh_free(Lib3dsMesh mesh)
		{
			lib3ds_mesh_resize_vertices(mesh, 0, false, false);
			lib3ds_mesh_resize_faces(mesh, 0);
		}

		public static void lib3ds_mesh_resize_vertices(Lib3dsMesh mesh, ushort nvertices, bool use_texcos, bool use_flags)
		{
			Debug.Assert(mesh!=null);

			if(nvertices>0)
			{
				if(mesh.vertices==null) mesh.vertices=new List<Lib3dsVertex>();
				while(mesh.vertices.Count<nvertices) mesh.vertices.Add(new Lib3dsVertex());
				if(mesh.vertices.Count>nvertices) mesh.vertices.RemoveRange(nvertices, mesh.vertices.Count-nvertices);
			}
			else mesh.vertices=null;

			ushort tmp=(ushort)(use_texcos?nvertices:0);
			if(tmp>0)
			{
				if(mesh.texcos==null) mesh.texcos=new List<Lib3dsTexturecoordinate>();
				while(mesh.texcos.Count<tmp) mesh.texcos.Add(new Lib3dsTexturecoordinate());
				if(mesh.texcos.Count>tmp) mesh.texcos.RemoveRange(tmp, mesh.texcos.Count-tmp);
			}
			else mesh.texcos=null;

			tmp=(ushort)(use_flags?nvertices:0);
			if(tmp>0)
			{
				if(mesh.vflags==null) mesh.vflags=new List<ushort>();
				while(mesh.vflags.Count<tmp) mesh.vflags.Add(0);
				if(mesh.vflags.Count>tmp) mesh.vflags.RemoveRange(tmp, mesh.vflags.Count-tmp);
			}
			else mesh.vflags=null;

			mesh.nvertices=nvertices;
		}

		public static void lib3ds_mesh_resize_faces(Lib3dsMesh mesh, ushort nfaces)
		{
			Debug.Assert(mesh!=null);

			if(nfaces>0)
			{
				if(mesh.faces==null) mesh.faces=new List<Lib3dsFace>();
				while(mesh.faces.Count<nfaces)
				{
					mesh.faces.Add(new Lib3dsFace());
					mesh.faces[mesh.faces.Count-1].material=-1;
				}
				if(mesh.faces.Count>nfaces) mesh.faces.RemoveRange(nfaces, mesh.faces.Count-nfaces);
			}
			else mesh.faces=null;

			mesh.nfaces=nfaces;
		}

		// Find the bounding box of a mesh object.
		//
		// \param mesh The mesh object
		// \param bmin Returned bounding box
		// \param bmax Returned bounding box
		public static void lib3ds_mesh_bounding_box(Lib3dsMesh mesh, float[] bmin, float[] bmax)
		{
			bmin[0]=bmin[1]=bmin[2]=float.MaxValue;
			bmax[0]=bmax[1]=bmax[2]=-float.MaxValue;

			for(int i=0; i<mesh.nvertices; i++)
			{
				lib3ds_vector_min(bmin, mesh.vertices[i]);
				lib3ds_vector_max(bmax, mesh.vertices[i]);
			}
		}

		public static void lib3ds_mesh_calculate_face_normals(Lib3dsMesh mesh, float[][] face_normals)
		{
			if(mesh.nfaces==0) return;
			for(int i=0; i<mesh.nfaces; i++)
			{
				lib3ds_vector_normal(face_normals[i], mesh.vertices[mesh.faces[i].index[0]], mesh.vertices[mesh.faces[i].index[1]], mesh.vertices[mesh.faces[i].index[2]]);
			}
		}

		class Lib3dsFaces
		{
			public Lib3dsFaces next;
			public int index;
			public float[] normal=new float[3];
		}

		// Calculates the vertex normals corresponding to the smoothing group
		// settings for each face of a mesh.
		//
		// \param mesh      A pointer to the mesh to calculate the normals for.
		// \param normals   A pointer to a buffer to store the calculated
		//                  normals. The buffer must have the size:
		//                  3*3*sizeof(float)*mesh.nfaces.
		//
		// To allocate the normal buffer do for example the following:
		// \code
		//  Lib3dsVector *normals = malloc(3*3*sizeof(float)*mesh.nfaces);
		// \endcode
		//
		// To access the normal of the i-th vertex of the j-th face do the
		// following:
		// \code
		//   normals[3*j+i]
		// \endcode
		public static void lib3ds_mesh_calculate_vertex_normals(Lib3dsMesh mesh, float[][] normals)
		{
			if(mesh.nfaces==0) return;

			Lib3dsFaces[] fl=new Lib3dsFaces[mesh.nvertices];
			Lib3dsFaces[] fa=new Lib3dsFaces[3*mesh.nfaces];
			for(int i=0; i<fa.Length; i++) fa[i]=new Lib3dsFaces();

			for(int i=0; i<mesh.nfaces; i++)
			{
				for(int j=0; j<3; j++)
				{
					Lib3dsFaces l=fa[3*i+j];
					float[] p=new float[3], q=new float[3], n=new float[3];
					float len, weight;

					l.index=i;
					l.next=fl[mesh.faces[i].index[j]];
					fl[mesh.faces[i].index[j]]=l;

					lib3ds_vector_sub(p, mesh.vertices[mesh.faces[i].index[j<2?j+1:0]], mesh.vertices[mesh.faces[i].index[j]]);
					lib3ds_vector_sub(q, mesh.vertices[mesh.faces[i].index[j>0?j-1:2]], mesh.vertices[mesh.faces[i].index[j]]);
					lib3ds_vector_cross(n, p, q);
					len=lib3ds_vector_length(n);
					if(len>0)
					{
						weight=(float)Math.Atan2(len, lib3ds_vector_dot(p, q));
						lib3ds_vector_scalar_mul(l.normal, n, weight/len);
					}
					else lib3ds_vector_zero(l.normal);
				}
			}

			for(int i=0; i<mesh.nfaces; i++)
			{
				Lib3dsFace f=mesh.faces[i];
				for(int j=0; j<3; j++)
				{
					float[] n=new float[3];
					Lib3dsFaces p;
					Lib3dsFace pf;

					Debug.Assert(mesh.faces[i].index[j]<mesh.nvertices);

					if(f.smoothing_group!=0)
					{
						uint smoothing_group=f.smoothing_group;

						lib3ds_vector_zero(n);
						for(p=fl[mesh.faces[i].index[j]]; p!=null; p=p.next)
						{
							pf=mesh.faces[p.index];
							if((pf.smoothing_group&f.smoothing_group)!=0) smoothing_group|=pf.smoothing_group;
						}

						for(p=fl[mesh.faces[i].index[j]]; p!=null; p=p.next)
						{
							pf=mesh.faces[p.index];
							if((smoothing_group&pf.smoothing_group)!=0) lib3ds_vector_add(n, n, p.normal);
						}
					}
					else lib3ds_vector_copy(n, fa[3*i+j].normal);

					lib3ds_vector_normalize(n);
					lib3ds_vector_copy(normals[3*i+j], n);
				}
			}
		}

		static void face_array_read(Lib3dsFile file, Lib3dsMesh mesh, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_FACE_ARRAY, io);

			lib3ds_mesh_resize_faces(mesh, 0);
			ushort nfaces=lib3ds_io_read_word(io);
			if(nfaces!=0)
			{
				lib3ds_mesh_resize_faces(mesh, nfaces);
				for(int i=0; i<nfaces; i++)
				{
					mesh.faces[i].index[0]=lib3ds_io_read_word(io);
					mesh.faces[i].index[1]=lib3ds_io_read_word(io);
					mesh.faces[i].index[2]=lib3ds_io_read_word(io);
					mesh.faces[i].flags=lib3ds_io_read_word(io);
				}
				lib3ds_chunk_read_tell(c, io);

				while((chunk=lib3ds_chunk_read_next(c, io))!=0)
				{
					switch(chunk)
					{
						case Lib3dsChunks.CHK_MSH_MAT_GROUP:
							string name=lib3ds_io_read_string(io, 64);
							int material=lib3ds_file_material_by_name(file, name);

							ushort n=lib3ds_io_read_word(io);
							for(int i=0; i<n; i++)
							{
								ushort index=lib3ds_io_read_word(io);
								if(index<mesh.nfaces) mesh.faces[index].material=material;
								else
								{
									// TODO warning
								}
							}
							break;
						case Lib3dsChunks.CHK_SMOOTH_GROUP:
							for(int i=0; i<mesh.nfaces; i++) mesh.faces[i].smoothing_group=lib3ds_io_read_dword(io);
							break;
						case Lib3dsChunks.CHK_MSH_BOXMAP:
							mesh.box_front=lib3ds_io_read_string(io, 64);
							mesh.box_back=lib3ds_io_read_string(io, 64);
							mesh.box_left=lib3ds_io_read_string(io, 64);
							mesh.box_right=lib3ds_io_read_string(io, 64);
							mesh.box_top=lib3ds_io_read_string(io, 64);
							mesh.box_bottom=lib3ds_io_read_string(io, 64);
							break;
						default: lib3ds_chunk_unknown(chunk, io); break;
					}
				}
			}
			lib3ds_chunk_read_end(c, io);
		}

		public static void lib3ds_mesh_read(Lib3dsFile file, Lib3dsMesh mesh, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			lib3ds_chunk_read_start(c, Lib3dsChunks.CHK_N_TRI_OBJECT, io);

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_MESH_MATRIX:
						lib3ds_matrix_identity(mesh.matrix);
						for(int i=0; i<4; i++)
						{
							for(int j=0; j<3; j++) mesh.matrix[i, j]=lib3ds_io_read_float(io);
						}
						break;
					case Lib3dsChunks.CHK_MESH_COLOR:
						mesh.color=lib3ds_io_read_byte(io);
						break;
					case Lib3dsChunks.CHK_POINT_ARRAY:
						{
							ushort nvertices=lib3ds_io_read_word(io);
							lib3ds_mesh_resize_vertices(mesh, nvertices, mesh.texcos!=null, mesh.vflags!=null);
							for(int i=0; i<mesh.nvertices; i++) lib3ds_io_read_vector(io, mesh.vertices[i]);
						}
						break;
					case Lib3dsChunks.CHK_POINT_FLAG_ARRAY:
						{
							ushort nflags=lib3ds_io_read_word(io);
							ushort nvertices=(mesh.nvertices>=nflags)?mesh.nvertices:nflags;
							lib3ds_mesh_resize_vertices(mesh, nvertices, mesh.texcos!=null, true);
							for(int i=0; i<nflags; i++) mesh.vflags[i]=lib3ds_io_read_word(io);
						}
						break;
					case Lib3dsChunks.CHK_FACE_ARRAY:
						lib3ds_chunk_read_reset(c, io);
						face_array_read(file, mesh, io);
						break;
					case Lib3dsChunks.CHK_MESH_TEXTURE_INFO:
						//FIXME: mesh.map_type = lib3ds_io_read_word(io);

						for(int i=0; i<2; i++) mesh.map_tile[i]=lib3ds_io_read_float(io);
						for(int i=0; i<3; i++) mesh.map_pos[i]=lib3ds_io_read_float(io);
						mesh.map_scale=lib3ds_io_read_float(io);

						lib3ds_matrix_identity(mesh.map_matrix);
						for(int i=0; i<4; i++)
						{
							for(int j=0; j<3; j++) mesh.map_matrix[i, j]=lib3ds_io_read_float(io);
						}
						for(int i=0; i<2; i++) mesh.map_planar_size[i]=lib3ds_io_read_float(io);
						mesh.map_cylinder_height=lib3ds_io_read_float(io);
						break;
					case Lib3dsChunks.CHK_TEX_VERTS:
						{
							ushort ntexcos=lib3ds_io_read_word(io);
							ushort nvertices=(mesh.nvertices>=ntexcos)?mesh.nvertices:ntexcos;
							if(mesh.texcos==null)
							{
								lib3ds_mesh_resize_vertices(mesh, nvertices, true, mesh.vflags!=null);
							}
							for(int i=0; i<ntexcos; i++)
							{
								mesh.texcos[i].s=lib3ds_io_read_float(io);
								mesh.texcos[i].t=lib3ds_io_read_float(io);
							}
							break;
						}

					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			if(lib3ds_matrix_det(mesh.matrix)<0.0)
			{
				// Flip X coordinate of vertices if mesh matrix has negative determinant
				float[,] inv_matrix=new float[4, 4], M=new float[4, 4];
				float[] tmp=new float[3];

				lib3ds_matrix_copy(inv_matrix, mesh.matrix);
				lib3ds_matrix_inv(inv_matrix);

				lib3ds_matrix_copy(M, mesh.matrix);
				lib3ds_matrix_scale(M, -1.0f, 1.0f, 1.0f);
				lib3ds_matrix_mult(M, M, inv_matrix);

				for(int i=0; i<mesh.nvertices; i++)
				{
					lib3ds_vector_transform(tmp, M, mesh.vertices[i]);
					lib3ds_vector_copy(mesh.vertices[i], tmp);
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		static void point_array_write(Lib3dsMesh mesh, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();

			c.chunk=Lib3dsChunks.CHK_POINT_ARRAY;
			c.size=8+12u*mesh.nvertices;
			lib3ds_chunk_write(c, io);

			lib3ds_io_write_word(io, mesh.nvertices);

			if(lib3ds_matrix_det(mesh.matrix)>=0.0f)
			{
				for(int i=0; i<mesh.nvertices; i++) lib3ds_io_write_vector(io, mesh.vertices[i]);
			}
			else
			{
				// Flip X coordinate of vertices if mesh matrix has negative determinant
				float[,] inv_matrix=new float[4, 4], M=new float[4, 4];
				float[] tmp=new float[3];

				lib3ds_matrix_copy(inv_matrix, mesh.matrix);
				lib3ds_matrix_inv(inv_matrix);
				lib3ds_matrix_copy(M, mesh.matrix);
				lib3ds_matrix_scale(M, -1.0f, 1.0f, 1.0f);
				lib3ds_matrix_mult(M, M, inv_matrix);

				for(int i=0; i<mesh.nvertices; i++)
				{
					lib3ds_vector_transform(tmp, M, mesh.vertices[i]);
					lib3ds_io_write_vector(io, tmp);
				}
			}
		}

		static void flag_array_write(Lib3dsMesh mesh, Lib3dsIo io)
		{
			if(mesh.vflags==null) return;

			Lib3dsChunk c=new Lib3dsChunk();

			c.chunk=Lib3dsChunks.CHK_POINT_FLAG_ARRAY;
			c.size=8+2u*mesh.nvertices;
			lib3ds_chunk_write(c, io);

			lib3ds_io_write_word(io, mesh.nvertices);
			for(int i=0; i<mesh.nvertices; i++) lib3ds_io_write_word(io, mesh.vflags[i]);
		}

		static void face_array_write(Lib3dsFile file, Lib3dsMesh mesh, Lib3dsIo io)
		{
			if(mesh.nfaces==0) return;
			Lib3dsChunk c_face_array=new Lib3dsChunk();

			c_face_array.chunk=Lib3dsChunks.CHK_FACE_ARRAY;
			lib3ds_chunk_write_start(c_face_array, io);

			lib3ds_io_write_word(io, mesh.nfaces);
			for(int i=0; i<mesh.nfaces; i++)
			{
				lib3ds_io_write_word(io, mesh.faces[i].index[0]);
				lib3ds_io_write_word(io, mesh.faces[i].index[1]);
				lib3ds_io_write_word(io, mesh.faces[i].index[2]);
				lib3ds_io_write_word(io, mesh.faces[i].flags);
			}

			{
				// ---- MSH_CHK_MAT_GROUP ----
				Lib3dsChunk c=new Lib3dsChunk();
				ushort num;

				bool[] matf=new bool[mesh.nfaces];

				for(ushort i=0; i<mesh.nfaces; i++)
				{
					if(!matf[i]&&(mesh.faces[i].material>=0)&&(mesh.faces[i].material<file.materials.Count))
					{
						matf[i]=true;
						num=1;

						for(ushort j=(ushort)(i+1); j<mesh.nfaces; j++)
							if(mesh.faces[i].material==mesh.faces[j].material) num++;

						c.chunk=Lib3dsChunks.CHK_MSH_MAT_GROUP;
						c.size=(uint)(6+file.materials[mesh.faces[i].material].name.Length+1+2+2*num);
						lib3ds_chunk_write(c, io);
						lib3ds_io_write_string(io, file.materials[mesh.faces[i].material].name);
						lib3ds_io_write_word(io, num);
						lib3ds_io_write_word(io, i);

						for(ushort j=(ushort)(i+1); j<mesh.nfaces; j++)
						{
							if(mesh.faces[i].material==mesh.faces[j].material)
							{
								lib3ds_io_write_word(io, j);
								matf[j]=true;
							}
						}
					}
				}
			}

			{
				// ---- SMOOTH_GROUP ----
				Lib3dsChunk c=new Lib3dsChunk();

				c.chunk=Lib3dsChunks.CHK_SMOOTH_GROUP;
				c.size=6+4u*mesh.nfaces;
				lib3ds_chunk_write(c, io);

				for(int i=0; i<mesh.nfaces; i++) lib3ds_io_write_dword(io, mesh.faces[i].smoothing_group);
			}

			if(mesh.box_front.Length>0||mesh.box_back.Length>0||mesh.box_left.Length>0||mesh.box_right.Length>0||mesh.box_top.Length>0||mesh.box_bottom.Length>0)
			{ // ---- MSH_BOXMAP ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_MSH_BOXMAP;
				lib3ds_chunk_write_start(c, io);

				lib3ds_io_write_string(io, mesh.box_front);
				lib3ds_io_write_string(io, mesh.box_back);
				lib3ds_io_write_string(io, mesh.box_left);
				lib3ds_io_write_string(io, mesh.box_right);
				lib3ds_io_write_string(io, mesh.box_top);
				lib3ds_io_write_string(io, mesh.box_bottom);

				lib3ds_chunk_write_end(c, io);
			}

			lib3ds_chunk_write_end(c_face_array, io);
		}

		static void texco_array_write(Lib3dsMesh mesh, Lib3dsIo io)
		{
			if(mesh.texcos==null) return;

			Lib3dsChunk c=new Lib3dsChunk();

			c.chunk=Lib3dsChunks.CHK_TEX_VERTS;
			c.size=8+8u*mesh.nvertices;
			lib3ds_chunk_write(c, io);

			lib3ds_io_write_word(io, mesh.nvertices);
			for(int i=0; i<mesh.nvertices; i++)
			{
				lib3ds_io_write_float(io, mesh.texcos[i].s);
				lib3ds_io_write_float(io, mesh.texcos[i].t);
			}
		}

		public static void lib3ds_mesh_write(Lib3dsFile file, Lib3dsMesh mesh, Lib3dsIo io)
		{
			Lib3dsChunk c_n_tri_object=new Lib3dsChunk();

			c_n_tri_object.chunk=Lib3dsChunks.CHK_N_TRI_OBJECT;
			lib3ds_chunk_write_start(c_n_tri_object, io);

			point_array_write(mesh, io);
			texco_array_write(mesh, io);

			if(mesh.map_type!=Lib3dsMapType.LIB3DS_MAP_NONE)
			{ // ---- LIB3DS_MESH_TEXTURE_INFO ----
				Lib3dsChunk c=new Lib3dsChunk();

				c.chunk=Lib3dsChunks.CHK_MESH_TEXTURE_INFO;
				c.size=92;
				lib3ds_chunk_write(c, io);

				lib3ds_io_write_word(io, (ushort)mesh.map_type);

				for(int i=0; i<2; i++) lib3ds_io_write_float(io, mesh.map_tile[i]);
				lib3ds_io_write_vector(io, mesh.map_pos);
				lib3ds_io_write_float(io, mesh.map_scale);

				for(int i=0; i<4; i++)
				{
					for(int j=0; j<3; j++) lib3ds_io_write_float(io, mesh.map_matrix[i, j]);
				}
				for(int i=0; i<2; i++) lib3ds_io_write_float(io, mesh.map_planar_size[i]);
				lib3ds_io_write_float(io, mesh.map_cylinder_height);
			}

			flag_array_write(mesh, io);

			{
				/*---- LIB3DS_MESH_MATRIX ----*/
				Lib3dsChunk c=new Lib3dsChunk();

				c.chunk=Lib3dsChunks.CHK_MESH_MATRIX;
				c.size=54;
				lib3ds_chunk_write(c, io);
				for(int i=0; i<4; i++)
				{
					for(int j=0; j<3; j++) lib3ds_io_write_float(io, mesh.matrix[i, j]);
				}
			}

			if(mesh.color!=0)
			{ // ---- LIB3DS_MESH_COLOR ----
				Lib3dsChunk c=new Lib3dsChunk();

				c.chunk=Lib3dsChunks.CHK_MESH_COLOR;
				c.size=7;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_byte(io, mesh.color);
			}

			face_array_write(file, mesh, io);

			lib3ds_chunk_write_end(c_n_tri_object, io);
		}
	}
}
