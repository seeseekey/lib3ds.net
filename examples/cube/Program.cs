// lib3ds.cs - Structures, Enums and Stuff
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.

using System;
using System.Collections.Generic;
using System.Text;
using lib3ds.Net;

namespace cube
{
	class Program
	{
// This examples demonstrates how to export a textured cube using lib3ds.

		static Lib3dsVertex[] g_vertices=new Lib3dsVertex[8]
		{
			new Lib3dsVertex(-10.0, -10.0,  15.0),
			new Lib3dsVertex( 10.0, -10.0,  15.0),
			new Lib3dsVertex( 10.0,  10.0,  15.0),
			new Lib3dsVertex(-10.0,  10.0,  15.0),
			new Lib3dsVertex(-10.0, -10.0, -15.0),
			new Lib3dsVertex( 10.0, -10.0, -15.0),
			new Lib3dsVertex( 10.0,  10.0, -15.0),
			new Lib3dsVertex(-10.0,  10.0, -15.0)
		};

		// Texture coodinate origin (0,0) is in bottom-left corner!
		static Lib3dsTexturecoordinate[] g_texcoords=new Lib3dsTexturecoordinate[8]
		{
			new Lib3dsTexturecoordinate(0.00, 1.0),
			new Lib3dsTexturecoordinate(0.25, 1.0),
			new Lib3dsTexturecoordinate(0.50, 1.0),
			new Lib3dsTexturecoordinate(0.75, 1.0),
			new Lib3dsTexturecoordinate(0.00, 0.0),
			new Lib3dsTexturecoordinate(0.25, 0.0),
			new Lib3dsTexturecoordinate(0.50, 0.0),
			new Lib3dsTexturecoordinate(0.75, 0.0)
		};

		// CCW
		static ushort[,] g_indices=new ushort[,]
		{
			{0, 5, 1},
			{0, 4, 5},
			{1, 6, 2},
			{1, 5, 6},
			{2, 6, 7},
			{2, 7, 3},
			{0, 3, 7},
			{0, 7, 4},
			{0, 1, 2},
			{0, 2, 3},
			{4, 7, 6},
			{4, 6, 5}
		};

		public static void Main(string[] args)
		{
			Lib3dsFile file=LIB3DS.lib3ds_file_new();
			file.frames=360;

			Lib3dsMaterial mat=LIB3DS.lib3ds_material_new("c_tex");
			LIB3DS.lib3ds_file_insert_material(file, mat, -1);
			mat.texture1_map.name="cube.tga";
			mat.texture1_map.percent=1.0f;

			mat=LIB3DS.lib3ds_material_new("c_red");
			LIB3DS.lib3ds_file_insert_material(file, mat, -1);
			mat.diffuse[0]=1.0f;
			mat.diffuse[1]=0.0f;
			mat.diffuse[2]=0.0f;

			mat=LIB3DS.lib3ds_material_new("c_blue");
			LIB3DS.lib3ds_file_insert_material(file, mat, -1);
			mat.diffuse[0]=0.0f;
			mat.diffuse[1]=0.0f;
			mat.diffuse[2]=1.0f;

			Lib3dsMesh mesh=LIB3DS.lib3ds_mesh_new("cube");
			Lib3dsMeshInstanceNode inst;
			LIB3DS.lib3ds_file_insert_mesh(file, mesh, -1);

			LIB3DS.lib3ds_mesh_resize_vertices(mesh, 8, true, false);
			for(int i=0; i<8; i++)
			{
				LIB3DS.lib3ds_vector_copy(mesh.vertices[i], g_vertices[i]);
				mesh.texcos[i]=g_texcoords[i];
			}

			LIB3DS.lib3ds_mesh_resize_faces(mesh, 12);
			for(int i=0; i<12; i++)
			{
				for(int j=0; j<3; j++)
				{
					mesh.faces[i].index[j]=g_indices[i, j];
				}
			}

			for(int i=0; i<8; i++) mesh.faces[i].material=0;
			for(int i=0; i<2; i++) mesh.faces[8+i].material=1;
			for(int i=0; i<2; i++) mesh.faces[10+i].material=2;

			inst=LIB3DS.lib3ds_node_new_mesh_instance(mesh, "01", null, null, null);
			LIB3DS.lib3ds_file_append_node(file, inst, null);

			Lib3dsCamera camera=LIB3DS.lib3ds_camera_new("camera01");
			LIB3DS.lib3ds_file_insert_camera(file, camera, -1);
			LIB3DS.lib3ds_vector_make(camera.position, 0.0f, -100f, 0.0f);
			LIB3DS.lib3ds_vector_make(camera.target, 0.0f, 0.0f, 0.0f);

			Lib3dsCameraNode n=LIB3DS.lib3ds_node_new_camera(camera);
			Lib3dsTargetNode t=LIB3DS.lib3ds_node_new_camera_target(camera);
			LIB3DS.lib3ds_file_append_node(file, n, null);
			LIB3DS.lib3ds_file_append_node(file, t, null);

			LIB3DS.lib3ds_track_resize(n.pos_track, 37);
			for(int i=0; i<=36; i++)
			{
				n.pos_track.keys[i].frame=10*i;
				LIB3DS.lib3ds_vector_make(n.pos_track.keys[i].value, (float)(100.0*Math.Cos(2*Math.PI*i/36.0)), (float)(100.0*Math.Sin(2*Math.PI*i/36.0)), 50.0f);
			}

			if(!LIB3DS.lib3ds_file_save(file, "C:\\cube.3ds"))
				Console.Error.WriteLine("ERROR: Saving 3ds file failed!");

			LIB3DS.lib3ds_file_free(file);
		}
	}
}
