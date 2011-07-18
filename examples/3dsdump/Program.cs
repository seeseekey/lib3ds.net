// lib3ds.cs - Structures, Enums and Stuff
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.

using System;
using System.Collections.Generic;
using System.Text;
using lib3ds.Net;
using System.Diagnostics;

namespace _3dsdump
{
	class Program
	{
		// A utility to display information about the content of a 3DS file.

		static void help()
		{
			Console.Error.WriteLine("The 3D Studio File Format Library - 3dsdump");
			Console.Error.WriteLine("Copyright (C) 1996-2007 by Jan Eric Kyprianidis <www.kyprianidis.com>");
			Console.Error.WriteLine("All rights reserved.");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Syntax: 3dsdump [options] filename [options]");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Options:");
			Console.Error.WriteLine("  -h           This help");
			Console.Error.WriteLine("  -d=level     Set log level (0=ERROR, 1=WARN, 2=INFO, 3=DEBUG)");
			Console.Error.WriteLine("  -m           Dump materials");
			Console.Error.WriteLine("  -t           Dump trimeshes");
			Console.Error.WriteLine("  -i           Dump instances");
			Console.Error.WriteLine("  -c           Dump cameras");
			Console.Error.WriteLine("  -l           Dump lights");
			Console.Error.WriteLine("  -n           Dump node hierarchy");
			Console.Error.WriteLine("  -w=filename  Write new 3ds file to disk");
			Console.Error.WriteLine();
			throw new Exception();
		}

		enum Flags
		{
			LIB3DSDUMP_MATERIALS=0x0004,
			LIB3DSDUMP_TRIMESHES=0x0008,
			LIB3DSDUMP_INSTANCES=0x0010,
			LIB3DSDUMP_CAMERAS=0x0020,
			LIB3DSDUMP_LIGHTS=0x0040,
			LIB3DSDUMP_NODES=0x0080
		}

		static string filename=null;
		static string output=null;
		static Flags flags=0;
		static Lib3dsLogLevel log_level=Lib3dsLogLevel.LIB3DS_LOG_INFO;

		static void parse_args(string[] args)
		{
			for(int i=0; i<args.Length; i++)
			{
				if(args[i][0]=='-')
				{
					if(args[i]=="-h"||args[i]=="--help") help();
					else if((args[i][1]=='d')&&(args[i][2]=='=')) log_level=(Lib3dsLogLevel)int.Parse(args[i].Substring(3));
					else if(args[i][1]=='m') flags|=Flags.LIB3DSDUMP_MATERIALS;
					else if(args[i][1]=='t') flags|=Flags.LIB3DSDUMP_TRIMESHES;
					else if(args[i][1]=='i') flags|=Flags.LIB3DSDUMP_INSTANCES;
					else if(args[i][1]=='c') flags|=Flags.LIB3DSDUMP_CAMERAS;
					else if(args[i][1]=='l') flags|=Flags.LIB3DSDUMP_LIGHTS;
					else if(args[i][1]=='n') flags|=Flags.LIB3DSDUMP_NODES;
					else if(args[i][1]=='w'&&args[i][2]=='=') output=args[i].Substring(3);
					else help();
				}
				else
				{
					if(filename!=null) help(); // if filename is already given
					filename=args[i];
				}
			}

			if(filename==null) help();
		}

		static string[] level_str=new string[] { "ERROR", "WARN", "INFO", "DEBUG" };

		static void fileio_log_func(object self, Lib3dsLogLevel level, int indent, string msg)
		{
			if(log_level>=level)
			{
				Console.Write("{0,5} : ", level_str[(int)level]);
				for(int i=1; i<indent; i++) Console.Write('\t');
				Console.WriteLine(msg);
			}
		}

		static void matrix_dump(float[,] matrix)
		{
			for(int i=0; i<4; ++i)
			{
				for(int j=0; j<4; ++j) Console.Write("{0} ", matrix[j, i]);
				Console.WriteLine();
			}
		}

		static void viewport_dump(Lib3dsViewport vp)
		{
			Debug.Assert(vp!=null);

			Console.WriteLine("  viewport:");
			Console.WriteLine("    layout:");
			Console.WriteLine("      style:       {0}", vp.layout_style);
			Console.WriteLine("      active:      {0}", vp.layout_active);
			Console.WriteLine("      swap:        {0}", vp.layout_swap);
			Console.WriteLine("      swap_prior:  {0}", vp.layout_swap_prior);
			Console.WriteLine("      position:    {0},{1}", vp.layout_position[0], vp.layout_position[1]);
			Console.WriteLine("      size:        {0},{1}", vp.layout_size[0], vp.layout_size[1]);
			Console.WriteLine("      views:       {0}", vp.layout_views.Count);

			int i=0;
			foreach(Lib3dsView view in vp.layout_views)
			{
				Console.WriteLine("        view {0}:", i++);
				Console.WriteLine("          type:         {0}", view.type);
				Console.WriteLine("          axis_lock:    {0}", view.axis_lock);
				Console.WriteLine("          position:     ({0},{1})", view.position[0], view.position[1]);
				Console.WriteLine("          size:         ({0},{1})", view.size[0], view.size[1]);
				Console.WriteLine("          zoom:         {0}", view.zoom);
				Console.WriteLine("          center:       ({0},{1},{2})", view.center[0], view.center[1], view.center[2]);
				Console.WriteLine("          horiz_angle:  {0}", view.horiz_angle);
				Console.WriteLine("          vert_angle:   {0}", view.vert_angle);
				Console.WriteLine("          camera:       {0}", view.camera);
			}

			Console.WriteLine("    default:");
			Console.WriteLine(" type:         {0}", vp.default_type);
			Console.WriteLine(" position:     ({0},{1},{2})", vp.default_position[0], vp.default_position[1], vp.default_position[2]);
			Console.WriteLine(" width:        {0}", vp.default_width);
			Console.WriteLine(" horiz_angle:  {0}", vp.default_horiz_angle);
			Console.WriteLine(" vert_angle:   {0}", vp.default_vert_angle);
			Console.WriteLine(" roll_angle:   {0}", vp.default_roll_angle);
			Console.WriteLine(" camera:       {0}", vp.default_camera);
		}

		static void texture_dump(string maptype, Lib3dsTextureMap texture)
		{
			Debug.Assert(texture!=null);
			if(texture.name==null||texture.name.Length==0) return;

			Console.WriteLine("  {0}:", maptype);
			Console.WriteLine("    name:          {0}", texture.name);
			Console.WriteLine("    flags:         {0:X}", (ushort)texture.flags);
			Console.WriteLine("    percent:       {0}", texture.percent);
			Console.WriteLine("    blur:          {0}", texture.blur);
			Console.WriteLine("    scale:         ({0}, {1})", texture.scale[0], texture.scale[1]);
			Console.WriteLine("    offset:        ({0}, {1})", texture.offset[0], texture.offset[1]);
			Console.WriteLine("    rotation:      {0}", texture.rotation);
			Console.WriteLine("    tint_1:        ({0},{1},{2})", texture.tint_1[0], texture.tint_1[1], texture.tint_1[2]);
			Console.WriteLine("    tint_2:        ({0},{1},{2})", texture.tint_2[0], texture.tint_2[1], texture.tint_2[2]);
			Console.WriteLine("    tint_r:        ({0},{1},{2})", texture.tint_r[0], texture.tint_r[1], texture.tint_r[2]);
			Console.WriteLine("    tint_g:        ({0},{1},{2})", texture.tint_g[0], texture.tint_g[1], texture.tint_g[2]);
			Console.WriteLine("    tint_b:        ({0},{1},{2})", texture.tint_b[0], texture.tint_b[1], texture.tint_b[2]);
		}

		static void material_dump(Lib3dsMaterial material)
		{
			Debug.Assert(material!=null);

			Console.WriteLine("  name:            {0}", material.name);
			Console.WriteLine("  ambient:         ({0},{1},{2})", material.ambient[0], material.ambient[1], material.ambient[2]);
			Console.WriteLine("  diffuse:         ({0},{1},{2})", material.diffuse[0], material.diffuse[1], material.diffuse[2]);
			Console.WriteLine("  specular:        ({0},{1},{2})", material.specular[0], material.specular[1], material.specular[2]);
			Console.WriteLine("  shininess:       {0}", material.shininess);
			Console.WriteLine("  shin_strength:   {0}", material.shin_strength);
			Console.WriteLine("  use_blur:        {0}", material.use_blur?"yes":"no");
			Console.WriteLine("  blur:            {0}", material.blur);
			Console.WriteLine("  falloff:         {0}", material.falloff);
			Console.WriteLine("  is_additive:     {0}", material.is_additive?"yes":"no");
			Console.WriteLine("  use_falloff:     {0}", material.use_falloff?"yes":"no");
			Console.WriteLine("  self_illum_flag: {0}", material.self_illum_flag?"yes":"no");
			Console.WriteLine("  self_illum:      {0}", material.self_illum);
			Console.WriteLine("  shading:         {0}", material.shading);
			Console.WriteLine("  soften:          {0}", material.soften?"yes":"no");
			Console.WriteLine("  face_map:        {0}", material.face_map?"yes":"no");
			Console.WriteLine("  two_sided:       {0}", material.two_sided?"yes":"no");
			Console.WriteLine("  map_decal:       {0}", material.map_decal?"yes":"no");
			Console.WriteLine("  use_wire:        {0}", material.use_wire?"yes":"no");
			Console.WriteLine("  use_wire_abs:    {0}", material.use_wire_abs?"yes":"no");
			Console.WriteLine("  wire_size:       {0}", material.wire_size);
			texture_dump("texture1_map", material.texture1_map);
			texture_dump("texture1_mask", material.texture1_mask);
			texture_dump("texture2_map", material.texture2_map);
			texture_dump("texture2_mask", material.texture2_mask);
			texture_dump("opacity_map", material.opacity_map);
			texture_dump("opacity_mask", material.opacity_mask);
			texture_dump("bump_map", material.bump_map);
			texture_dump("bump_mask", material.bump_mask);
			texture_dump("specular_map", material.specular_map);
			texture_dump("specular_mask", material.specular_mask);
			texture_dump("shininess_map", material.shininess_map);
			texture_dump("shininess_mask", material.shininess_mask);
			texture_dump("self_illum_map", material.self_illum_map);
			texture_dump("self_illum_mask", material.self_illum_mask);
			texture_dump("reflection_map", material.reflection_map);
			texture_dump("reflection_mask", material.reflection_mask);
			Console.WriteLine("  autorefl_map:");
			Console.WriteLine("    flags          {0:X}", (ushort)material.autorefl_map_flags);
			Console.WriteLine("    level          {0}", material.autorefl_map_anti_alias);
			Console.WriteLine("    size           {0}", material.autorefl_map_size);
			Console.WriteLine("    frame_step     {0}", material.autorefl_map_frame_step);
			Console.WriteLine();
		}

		static void camera_dump(Lib3dsCamera camera)
		{
			Debug.Assert(camera!=null);

			Console.WriteLine("  name:       {0}", camera.name);
			Console.WriteLine("  position:   ({0},{1},{2})", camera.position[0], camera.position[1], camera.position[2]);
			Console.WriteLine("  target      ({0},{1},{2})", camera.target[0], camera.target[1], camera.target[2]);
			Console.WriteLine("  roll:       {0}", camera.roll);
			Console.WriteLine("  fov:        {0}", camera.fov);
			Console.WriteLine("  see_cone:   {0}", camera.see_cone?"yes":"no");
			Console.WriteLine("  near_range: {0}", camera.near_range);
			Console.WriteLine("  far_range:  {0}", camera.far_range);
			Console.WriteLine();
		}

		static void light_dump(Lib3dsLight light)
		{
			Debug.Assert(light!=null);

			Console.WriteLine("  name:             {0}", light.name);
			Console.WriteLine("  spot_light:       {0}", light.spot_light?"yes":"no");
			Console.WriteLine("  see_cone:         {0}", light.see_cone?"yes":"no");
			Console.WriteLine("  color:            ({0},{1},{2})", light.color[0], light.color[1], light.color[2]);
			Console.WriteLine("  position          ({0},{1},{2})", light.position[0], light.position[1], light.position[2]);
			Console.WriteLine("  target            ({0},{1},{2})", light.target[0], light.target[1], light.target[2]);
			Console.WriteLine("  roll:             {0}", light.roll);
			Console.WriteLine("  off:              {0}", light.off?"yes":"no");
			Console.WriteLine("  outer_range:      {0}", light.outer_range);
			Console.WriteLine("  inner_range:      {0}", light.inner_range);
			Console.WriteLine("  multiplier:       {0}", light.multiplier);
			Console.WriteLine("  attenuation:      {0}", light.attenuation);
			Console.WriteLine("  rectangular_spot: {0}", light.rectangular_spot?"yes":"no");
			Console.WriteLine("  shadowed:         {0}", light.shadowed?"yes":"no");
			Console.WriteLine("  shadow_bias:      {0}", light.shadow_bias);
			Console.WriteLine("  shadow_filter:    {0}", light.shadow_filter);
			Console.WriteLine("  shadow_size:      {0}", light.shadow_size);
			Console.WriteLine("  spot_aspect:      {0}", light.spot_aspect);
			Console.WriteLine("  use_projector:    {0}", light.use_projector?"yes":"no");
			Console.WriteLine("  projector:        {0}", light.projector);
			Console.WriteLine("  spot_overshoot:   {0}", light.spot_overshoot?"yes":"no");
			Console.WriteLine("  ray_shadows:      {0}", light.ray_shadows?"yes":"no");
			Console.WriteLine("  ray_bias:         {0}", light.ray_bias);
			Console.WriteLine("  hotspot:          {0}", light.hotspot);
			Console.WriteLine("  falloff:          {0}", light.falloff);
			Console.WriteLine();
		}

		static void mesh_dump(Lib3dsMesh mesh)
		{
			Debug.Assert(mesh!=null);

			Console.WriteLine("  {0} vertices={1} faces={2}", mesh.name, mesh.nvertices, mesh.nfaces);
			Console.WriteLine("  matrix:");
			matrix_dump(mesh.matrix);

			Lib3dsVertex p=new Lib3dsVertex();
			Console.WriteLine("  vertices (x, y, z, u, v):");
			for(int i=0; i<mesh.nvertices; i++)
			{
				LIB3DS.lib3ds_vector_copy(p, mesh.vertices[i]);
				Console.Write("    {0,10:F5} {1,10:F5} {2,10:F5}", p.x, p.y, p.z);
				if(mesh.texcos!=null) Console.Write(" {0,10:F5} {1,10:F5}", mesh.texcos[i].s, mesh.texcos[i].t);
				Console.WriteLine();
			}

			Console.WriteLine("  facelist:");
			for(int i=0; i<mesh.nfaces; i++)
				Console.WriteLine("    {0,4} {1,4} {2,4}  flags:{3:X}  smoothing:{4:X}  material:\"{5}\"\n", mesh.faces[i].index[0], mesh.faces[i].index[1], mesh.faces[i].index[2], mesh.faces[i].flags, mesh.faces[i].smoothing_group, mesh.faces[i].material);
		}

		static void dump_instances(Lib3dsNode node, string parent)
		{
			if(node.type==Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE)
			{
				string name=parent+"."+node.name;

				Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
				Console.WriteLine("  {0} : {1}", name, n.instance_name);
			}

			foreach(Lib3dsNode p in node.childs) dump_instances(p, parent);
		}

		static string[] node_names_table=new string[] { "Ambient", "Mesh", "Camera", "Camera Target", "Omnilight", "Spotlight", "Spotlight Target" };

		static void node_dump(Lib3dsNode node, int level)
		{
			Debug.Assert(node!=null);
			string l="";
			l=l.PadRight(2*level);

			if(node.type==Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE)
			{
				Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
				Console.WriteLine("{0}{1} [{2}] ({3})", l, node.name, n.instance_name, node_names_table[(int)node.type]);
			}
			else Console.WriteLine("{0}{1} ({2})", l, node.name, node_names_table[(int)node.type]);

			foreach(Lib3dsNode p in node.childs) node_dump(p, level+1);
		}

		static int Main(string[] args)
		{
			try
			{
				parse_args(args);
			}
			catch
			{
				return 1;
			}

			Lib3dsFile f=LIB3DS.lib3ds_file_open(filename, fileio_log_func);
			if(f==null)
			{
				Console.Error.WriteLine("***ERROR***");
				Console.Error.WriteLine("Loading file failed: {0}", filename);
				return 1;
			}

			if((flags&Flags.LIB3DSDUMP_MATERIALS)!=0)
			{
				Console.WriteLine("Dumping materials:");
				foreach(Lib3dsMaterial material in f.materials) material_dump(material);
				Console.WriteLine();
			}

			if((flags&Flags.LIB3DSDUMP_TRIMESHES)!=0)
			{
				Console.WriteLine("Dumping meshes:");
				foreach(Lib3dsMesh mesh in f.meshes) mesh_dump(mesh);
				Console.WriteLine();
			}

			if((flags&Flags.LIB3DSDUMP_INSTANCES)!=0)
			{
				Console.WriteLine("Dumping instances:");
				foreach(Lib3dsNode p in f.nodes) dump_instances(p, "");
				Console.WriteLine();
			}

			if((flags&Flags.LIB3DSDUMP_CAMERAS)!=0)
			{
				Console.WriteLine("Dumping cameras:");
				foreach(Lib3dsCamera camera in f.cameras) camera_dump(camera);
				Console.WriteLine();
			}

			if((flags&Flags.LIB3DSDUMP_LIGHTS)!=0)
			{
				Console.WriteLine("Dumping lights:\n");
				foreach(Lib3dsLight light in f.lights) light_dump(light);
				Console.WriteLine();
			}

			if((flags&Flags.LIB3DSDUMP_NODES)!=0)
			{
				Console.WriteLine("Dumping node hierarchy:\n");
				foreach(Lib3dsNode p in f.nodes) node_dump(p, 1);
				Console.WriteLine();
			}

			if(output!=null)
			{
				if(!LIB3DS.lib3ds_file_save(f, output))
				{
					Console.WriteLine("***ERROR**** Writing {0}", output);
				}
			}

			LIB3DS.lib3ds_file_free(f);
			return 0;
		}
	}
}
