// lib3ds.cs - Structures, Enums and Stuff
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using lib3ds.Net;
using System.Globalization;
using System.Threading;

namespace _3ds2obj
{
	class Program
	{
		// This example shows how to convert a 3DS file to a Wavefront OBJ file.

		static void help()
		{
			Console.Error.WriteLine("Syntax: 3ds2obj 3ds-file [obj-file] [mtl-file]");
			throw new Exception();
		}

		static string input=null;
		static string obj_file=null;
		static string mtl_file=null;
		static int max_vertices=0;
		static int max_texcos=0;
		static int max_normals=0;

		static void parse_args(string[] args)
		{
			foreach(string arg in args)
			{
				if(arg.StartsWith("-"))
				{
					if(arg=="-h"||arg=="--help") help();
					else help();
				}
				else
				{
					if(input==null) input=arg;
					else if(obj_file==null) obj_file=arg;
					else if(mtl_file==null) mtl_file=arg;
					else help();
				}
			}

			if(input!=null&&input.EndsWith(".3ds"))
			{
				string filename=input.Substring(0, input.Length-4);
				if(obj_file==null) obj_file=filename+".obj";
				if(mtl_file==null) mtl_file=filename+".mtl";
			}

			if(input==null||obj_file==null) help();
		}

		static void write_mtl(StreamWriter mtl, Lib3dsFile f)
		{
			mtl.WriteLine("# Wavefront material file");
			mtl.WriteLine("# Converted by 3ds2obj");
			mtl.WriteLine("# http://www.lib3ds.org");
			mtl.WriteLine();

			bool unique=true;
			for(int i=0; i<f.materials.Count; i++)
			{
				string newname="";
				foreach(char p in f.materials[i].name)
				{
					if(!char.IsLetterOrDigit(p)&&p!='_') newname+='_';
					else newname+=p;
				}
				f.materials[i].name=newname;

				for(int j=0; j<i; j++)
				{
					if(f.materials[i].name==f.materials[j].name)
					{
						unique=false;
						break;
					}
				}
				if(!unique) break;
			}

			if(!unique)
			{
				for(int i=0; i<f.materials.Count; i++) f.materials[i].name=string.Format("mat_{0}", i);
			}

			foreach(Lib3dsMaterial m in f.materials)
			{
				mtl.WriteLine("newmtl {0}", m.name);
				mtl.WriteLine("Ka {0} {1} {2}", m.ambient[0], m.ambient[1], m.ambient[2]);
				mtl.WriteLine("Kd {0} {1} {2}", m.diffuse[0], m.diffuse[1], m.diffuse[2]);
				mtl.WriteLine("Ks {0} {1} {2}", m.specular[0], m.specular[1], m.specular[2]);
				mtl.WriteLine("illum 2");
				mtl.WriteLine("Ns {0}", Math.Pow(2, 10*m.shininess+1));
				mtl.WriteLine("d {0}", 1.0-m.transparency);
				mtl.WriteLine("map_Kd {0}", m.texture1_map.name);
				mtl.WriteLine("map_bump {0}", m.bump_map.name);
				mtl.WriteLine("map_d {0}", m.opacity_map.name);
				mtl.WriteLine("refl {0}", m.reflection_map.name);
				mtl.WriteLine("map_KS {0}", m.specular_map.name);
				mtl.WriteLine();
			}
		}

		static void write_mesh(StreamWriter o, Lib3dsFile f, Lib3dsMeshInstanceNode node)
		{
			Lib3dsMesh mesh=LIB3DS.lib3ds_file_mesh_for_node(f, node);
			if(mesh==null||mesh.vertices==null||mesh.vertices.Count==0) return;

			o.WriteLine("# object {0}", node.name);
			o.WriteLine("g {0}", node.instance_name!=null&&node.instance_name.Length!=0?node.instance_name:node.name);

			List<Lib3dsVertex> orig_vertices=new List<Lib3dsVertex>();
			foreach(Lib3dsVertex v in mesh.vertices) orig_vertices.Add(new Lib3dsVertex(v));

			float[,] inv_matrix=new float[4, 4], M=new float[4, 4];
			float[] tmp=new float[3];

			LIB3DS.lib3ds_matrix_copy(M, node.matrixNode);
			LIB3DS.lib3ds_matrix_translate(M, -node.pivot[0], -node.pivot[1], -node.pivot[2]);
			LIB3DS.lib3ds_matrix_copy(inv_matrix, mesh.matrix);
			LIB3DS.lib3ds_matrix_inv(inv_matrix);
			LIB3DS.lib3ds_matrix_mult(M, M, inv_matrix);

			for(int i=0; i<mesh.nvertices; i++)
			{
				LIB3DS.lib3ds_vector_transform(tmp, M, mesh.vertices[i]);
				LIB3DS.lib3ds_vector_copy(mesh.vertices[i], tmp);
			}

			bool export_texcos=mesh.texcos!=null;
			bool export_normals=mesh.faces!=null;

			foreach(Lib3dsVertex v in mesh.vertices) o.WriteLine("v {0} {1} {2}", v.x, v.y, v.z);
			o.WriteLine("# {0} vertices", mesh.vertices.Count);

			if(export_texcos)
			{
				foreach(Lib3dsTexturecoordinate vt in mesh.texcos) o.WriteLine("vt {0} {1}", vt.s, vt.t);
				o.WriteLine("# {0} texture vertices", mesh.vertices.Count);
			}

			if(export_normals)
			{
				float[][] normals=new float[3*mesh.nfaces][];
				for(int i=0; i<3*mesh.nfaces; i++) normals[i]=new float[3];
				LIB3DS.lib3ds_mesh_calculate_vertex_normals(mesh, normals);
				for(int i=0; i<3*mesh.nfaces; i++) o.WriteLine("vn {0} {1} {2}", normals[i][0], normals[i][1], normals[i][2]);
				o.WriteLine("# {0} normals", 3*mesh.nfaces);
			}

			int mat_index=-1;
			for(int i=0; i<mesh.nfaces; i++)
			{
				if(mat_index!=mesh.faces[i].material)
				{
					mat_index=mesh.faces[i].material;
					if(mat_index!=-1) o.WriteLine("usemtl {0}", f.materials[mat_index].name);
				}

				o.Write("f ");
				for(int j=0; j<3; ++j)
				{
					o.Write("{0}", mesh.faces[i].index[j]+max_vertices+1);
					if(export_texcos) o.Write("/{0}", mesh.faces[i].index[j]+max_texcos+1);
					else if(export_normals) o.Write("/");
					if(export_normals) o.Write("/{0}", 3*i+j+max_normals+1);
					if(j<3) o.Write(" ");
				}
				o.WriteLine();
			}

			max_vertices+=mesh.nvertices;
			if(export_texcos) max_texcos+=mesh.nvertices;
			if(export_normals) max_normals+=3*mesh.nfaces;

			mesh.vertices=orig_vertices;
		}

		static void write_nodes(StreamWriter o, Lib3dsFile f, List<Lib3dsNode> nodes)
		{
			foreach(Lib3dsNode p in nodes)
			{
				if(p.type==Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE)
				{
					write_mesh(o, f, (Lib3dsMeshInstanceNode)p);
					write_nodes(o, f, p.childs);
				}
			}
		}

		static int Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture=new CultureInfo("");

			try
			{
				parse_args(args);
			}
			catch
			{
				return 1;
			}

			Lib3dsFile f=LIB3DS.lib3ds_file_open(input);
			if(f==null)
			{
				Console.Error.WriteLine("***ERROR***");
				Console.Error.WriteLine("Loading file failed: {0}", input);
				return 1;
			}

			if(mtl_file!=null)
			{
				try
				{
					using(StreamWriter mtl=new StreamWriter(File.Create(mtl_file), Encoding.ASCII))
					{
						write_mtl(mtl, f);
						mtl.Close();
					}
				}
				catch
				{
					Console.Error.WriteLine("***ERROR***");
					Console.Error.WriteLine("Creating output file failed: {0}", mtl_file);
					return 1;
				}
			}

			try
			{
				using(StreamWriter obj=new StreamWriter(File.Create(obj_file), Encoding.ASCII))
				{
					if(f.nodes==null||f.nodes.Count==0) LIB3DS.lib3ds_file_create_nodes_for_meshes(f);
					LIB3DS.lib3ds_file_eval(f, 0);

					obj.WriteLine("# Wavefront OBJ file");
					obj.WriteLine("# Converted by 3ds2obj");
					obj.WriteLine("# http://www.lib3ds.org");
					obj.WriteLine();
					if(mtl_file!=null) obj.WriteLine("mtllib {0}", mtl_file);

					write_nodes(obj, f, f.nodes);
					obj.Close();
				}
			}
			catch
			{
				Console.Error.WriteLine("***ERROR***");
				Console.Error.WriteLine("Creating output file failed: {0}", obj_file);
				return 1;
			}

			LIB3DS.lib3ds_file_free(f);
			return 0;
		}
	}
}
