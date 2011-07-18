// lib3ds_node.cs - Node
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
		// Create and return a new node object.
		//
		// The node is returned with an identity matrix. All other fields
		// are zero.
		//
		// \return Lib3dsNode
		public static Lib3dsNode lib3ds_node_new(Lib3dsNodeType type)
		{
			Lib3dsNode node=null;
			switch(type)
			{
				case Lib3dsNodeType.LIB3DS_NODE_AMBIENT_COLOR:
					{
						Lib3dsAmbientColorNode n=new Lib3dsAmbientColorNode();
						node=n;
						node.name="$AMBIENT$";
						n.color_track.type=Lib3dsTrackType.LIB3DS_TRACK_VECTOR;
						break;
					}
				case Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE:
					{
						Lib3dsMeshInstanceNode n=new Lib3dsMeshInstanceNode();
						node=n;
						node.name="$$$DUMMY";
						n.pos_track.type=Lib3dsTrackType.LIB3DS_TRACK_VECTOR;
						n.scl_track.type=Lib3dsTrackType.LIB3DS_TRACK_VECTOR;
						n.rot_track.type=Lib3dsTrackType.LIB3DS_TRACK_QUAT;
						n.hide_track.type=Lib3dsTrackType.LIB3DS_TRACK_BOOL;
						break;
					}
				case Lib3dsNodeType.LIB3DS_NODE_CAMERA:
					{
						Lib3dsCameraNode n=new Lib3dsCameraNode();
						node=n;
						n.pos_track.type=Lib3dsTrackType.LIB3DS_TRACK_VECTOR;
						n.fov_track.type=Lib3dsTrackType.LIB3DS_TRACK_FLOAT;
						n.roll_track.type=Lib3dsTrackType.LIB3DS_TRACK_FLOAT;
						break;
					}
				case Lib3dsNodeType.LIB3DS_NODE_CAMERA_TARGET:
					{
						Lib3dsTargetNode n=new Lib3dsTargetNode();
						node=n;
						n.pos_track.type=Lib3dsTrackType.LIB3DS_TRACK_VECTOR;
						break;
					}
				case Lib3dsNodeType.LIB3DS_NODE_OMNILIGHT:
					{
						Lib3dsOmnilightNode n=new Lib3dsOmnilightNode();
						node=n;
						n.pos_track.type=Lib3dsTrackType.LIB3DS_TRACK_VECTOR;
						n.color_track.type=Lib3dsTrackType.LIB3DS_TRACK_VECTOR;
						break;
					}
				case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT:
					{
						Lib3dsSpotlightNode n=new Lib3dsSpotlightNode();
						node=n;
						n.pos_track.type=Lib3dsTrackType.LIB3DS_TRACK_VECTOR;
						n.color_track.type=Lib3dsTrackType.LIB3DS_TRACK_VECTOR;
						n.hotspot_track.type=Lib3dsTrackType.LIB3DS_TRACK_FLOAT;
						n.falloff_track.type=Lib3dsTrackType.LIB3DS_TRACK_FLOAT;
						n.roll_track.type=Lib3dsTrackType.LIB3DS_TRACK_FLOAT;
						break;
					}
				case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT_TARGET:
					{
						Lib3dsTargetNode n=new Lib3dsTargetNode();
						node=n;
						n.pos_track.type=Lib3dsTrackType.LIB3DS_TRACK_VECTOR;
						break;
					}
				default: Debug.Assert(false); return null;
			}

			node.type=type;
			node.node_id=65535;
			node.parent_id=65535;
			lib3ds_matrix_identity(node.matrixNode);
			return node;
		}

		public static Lib3dsAmbientColorNode lib3ds_node_new_ambient_color(float[] color0)
		{
			Lib3dsNode node=lib3ds_node_new(Lib3dsNodeType.LIB3DS_NODE_AMBIENT_COLOR);

			Lib3dsAmbientColorNode n=(Lib3dsAmbientColorNode)node;
			lib3ds_track_resize(n.color_track, 1);
			if(color0!=null) lib3ds_vector_copy(n.color_track.keys[0].value, color0);
			else lib3ds_vector_zero(n.color_track.keys[0].value);

			return n;
		}

		public static Lib3dsMeshInstanceNode lib3ds_node_new_mesh_instance(Lib3dsMesh mesh, string instance_name, float[] pos0, float[] scl0, float[] rot0)
		{
			Lib3dsNode node=lib3ds_node_new(Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE);
			if(mesh!=null) node.name=mesh.name;
			else node.name="$$$DUMMY";

			Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
			if(instance_name!=null) n.instance_name=instance_name;
			else n.instance_name="";

			lib3ds_track_resize(n.pos_track, 1);
			if(pos0!=null) lib3ds_vector_copy(n.pos_track.keys[0].value, pos0);

			lib3ds_track_resize(n.scl_track, 1);
			if(scl0!=null) lib3ds_vector_copy(n.scl_track.keys[0].value, scl0);
			else lib3ds_vector_make(n.scl_track.keys[0].value, 1, 1, 1);

			lib3ds_track_resize(n.rot_track, 1);
			if(rot0!=null) for(int i=0; i<4; i++) n.rot_track.keys[0].value[i]=rot0[i];
			else for(int i=0; i<4; i++) n.rot_track.keys[0].value[i]=0;

			return n;
		}

		public static Lib3dsCameraNode lib3ds_node_new_camera(Lib3dsCamera camera)
		{
			Debug.Assert(camera!=null);
			Lib3dsNode node=lib3ds_node_new(Lib3dsNodeType.LIB3DS_NODE_CAMERA);
			node.name=camera.name;

			Lib3dsCameraNode n=(Lib3dsCameraNode)node;
			lib3ds_track_resize(n.pos_track, 1);
			lib3ds_vector_copy(n.pos_track.keys[0].value, camera.position);

			lib3ds_track_resize(n.fov_track, 1);
			n.fov_track.keys[0].value[0]=camera.fov;

			lib3ds_track_resize(n.roll_track, 1);
			n.roll_track.keys[0].value[0]=camera.roll;

			return n;
		}

		public static Lib3dsTargetNode lib3ds_node_new_camera_target(Lib3dsCamera camera)
		{
			Debug.Assert(camera!=null);
			Lib3dsNode node=lib3ds_node_new(Lib3dsNodeType.LIB3DS_NODE_CAMERA_TARGET);
			node.name=camera.name;

			Lib3dsTargetNode n=(Lib3dsTargetNode)node;
			lib3ds_track_resize(n.pos_track, 1);
			lib3ds_vector_copy(n.pos_track.keys[0].value, camera.target);

			return n;
		}

		public static Lib3dsOmnilightNode lib3ds_node_new_omnilight(Lib3dsLight light)
		{
			Debug.Assert(light!=null);
			Lib3dsNode node=lib3ds_node_new(Lib3dsNodeType.LIB3DS_NODE_OMNILIGHT);
			node.name=light.name;

			Lib3dsOmnilightNode n=(Lib3dsOmnilightNode)node;
			lib3ds_track_resize(n.pos_track, 1);
			lib3ds_vector_copy(n.pos_track.keys[0].value, light.position);

			lib3ds_track_resize(n.color_track, 1);
			lib3ds_vector_copy(n.color_track.keys[0].value, light.color);

			return n;
		}

		public static Lib3dsSpotlightNode lib3ds_node_new_spotlight(Lib3dsLight light)
		{
			Debug.Assert(light!=null);
			Lib3dsNode node=lib3ds_node_new(Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT);
			node.name=light.name;

			Lib3dsSpotlightNode n=(Lib3dsSpotlightNode)node;
			lib3ds_track_resize(n.pos_track, 1);
			lib3ds_vector_copy(n.pos_track.keys[0].value, light.position);

			lib3ds_track_resize(n.color_track, 1);
			lib3ds_vector_copy(n.color_track.keys[0].value, light.color);

			lib3ds_track_resize(n.hotspot_track, 1);
			n.hotspot_track.keys[0].value[0]=light.hotspot;

			lib3ds_track_resize(n.falloff_track, 1);
			n.falloff_track.keys[0].value[0]=light.falloff;

			lib3ds_track_resize(n.roll_track, 1);
			n.roll_track.keys[0].value[0]=light.roll;

			return n;
		}

		public static Lib3dsTargetNode lib3ds_node_new_spotligf_target(Lib3dsLight light)
		{
			Debug.Assert(light!=null);
			Lib3dsNode node=lib3ds_node_new(Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT_TARGET);
			node.name=light.name;

			Lib3dsTargetNode n=(Lib3dsTargetNode)node;
			lib3ds_track_resize(n.pos_track, 1);
			lib3ds_vector_copy(n.pos_track.keys[0].value, light.target);

			return n;
		}

		static void free_node_and_childs(Lib3dsNode node)
		{
			Debug.Assert(node!=null);

			switch(node.type)
			{
				case Lib3dsNodeType.LIB3DS_NODE_AMBIENT_COLOR:
					{
						Lib3dsAmbientColorNode n=(Lib3dsAmbientColorNode)node;
						lib3ds_track_resize(n.color_track, 0);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE:
					{
						Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
						lib3ds_track_resize(n.pos_track, 0);
						lib3ds_track_resize(n.rot_track, 0);
						lib3ds_track_resize(n.scl_track, 0);
						lib3ds_track_resize(n.hide_track, 0);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_CAMERA:
					{
						Lib3dsCameraNode n=(Lib3dsCameraNode)node;
						lib3ds_track_resize(n.pos_track, 0);
						lib3ds_track_resize(n.fov_track, 0);
						lib3ds_track_resize(n.roll_track, 0);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_CAMERA_TARGET:
					{
						Lib3dsTargetNode n=(Lib3dsTargetNode)node;
						lib3ds_track_resize(n.pos_track, 0);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_OMNILIGHT:
					{
						Lib3dsOmnilightNode n=(Lib3dsOmnilightNode)node;
						lib3ds_track_resize(n.pos_track, 0);
						lib3ds_track_resize(n.color_track, 0);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT:
					{
						Lib3dsSpotlightNode n=(Lib3dsSpotlightNode)node;
						lib3ds_track_resize(n.pos_track, 0);
						lib3ds_track_resize(n.color_track, 0);
						lib3ds_track_resize(n.hotspot_track, 0);
						lib3ds_track_resize(n.falloff_track, 0);
						lib3ds_track_resize(n.roll_track, 0);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT_TARGET:
					{
						Lib3dsTargetNode n=(Lib3dsTargetNode)node;
						lib3ds_track_resize(n.pos_track, 0);
					}
					break;
			}

			foreach(Lib3dsNode p in node.childs) free_node_and_childs(p);
			node.childs.Clear();
		}

		// Free a node and all of its resources.
		//
		// \param node Lib3dsNode object to be freed.
		public static void lib3ds_node_free(Lib3dsNode node)
		{
			Debug.Assert(node!=null);
			free_node_and_childs(node);
		}

		// Evaluate an animation node.
		//
		// Recursively sets node and its children to their appropriate values
		// for this point in the animation.
		//
		// \param node Node to be evaluated.
		// \param t time value, between 0. and file.frames
		public static void lib3ds_node_eval(Lib3dsNode node, float t)
		{
			Debug.Assert(node!=null);
			switch(node.type)
			{
				case Lib3dsNodeType.LIB3DS_NODE_AMBIENT_COLOR:
					{
						Lib3dsAmbientColorNode n=(Lib3dsAmbientColorNode)node;
						if(node.parent!=null) lib3ds_matrix_copy(node.matrixNode, node.parent.matrixNode);
						else lib3ds_matrix_identity(node.matrixNode);
						lib3ds_track_eval_vector(n.color_track, n.color, t);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE:
					{
						float[,] M=new float[4, 4];
						Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;

						lib3ds_track_eval_vector(n.pos_track, n.pos, t);
						lib3ds_track_eval_quat(n.rot_track, n.rot, t);
						if(n.scl_track.keys.Count!=0) lib3ds_track_eval_vector(n.scl_track, n.scl, t);
						else n.scl[0]=n.scl[1]=n.scl[2]=1.0f;
						lib3ds_track_eval_bool(n.hide_track, out n.hide, t);

						lib3ds_matrix_identity(M);
						lib3ds_matrix_translate(M, n.pos[0], n.pos[1], n.pos[2]);
						lib3ds_matrix_rotate_quat(M, n.rot);
						lib3ds_matrix_scale(M, n.scl[0], n.scl[1], n.scl[2]);

						if(node.parent!=null) lib3ds_matrix_mult(node.matrixNode, node.parent.matrixNode, M);
						else lib3ds_matrix_copy(node.matrixNode, M);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_CAMERA:
					{
						Lib3dsCameraNode n=(Lib3dsCameraNode)node;
						lib3ds_track_eval_vector(n.pos_track, n.pos, t);
						lib3ds_track_eval_float(n.fov_track, out n.fov, t);
						lib3ds_track_eval_float(n.roll_track, out n.roll, t);
						if(node.parent!=null) lib3ds_matrix_copy(node.matrixNode, node.parent.matrixNode);
						else lib3ds_matrix_identity(node.matrixNode);
						lib3ds_matrix_translate(node.matrixNode, n.pos[0], n.pos[1], n.pos[2]);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_CAMERA_TARGET:
					{
						Lib3dsTargetNode n=(Lib3dsTargetNode)node;
						lib3ds_track_eval_vector(n.pos_track, n.pos, t);
						if(node.parent!=null) lib3ds_matrix_copy(node.matrixNode, node.parent.matrixNode);
						else lib3ds_matrix_identity(node.matrixNode);
						lib3ds_matrix_translate(node.matrixNode, n.pos[0], n.pos[1], n.pos[2]);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_OMNILIGHT:
					{
						Lib3dsOmnilightNode n=(Lib3dsOmnilightNode)node;
						lib3ds_track_eval_vector(n.pos_track, n.pos, t);
						lib3ds_track_eval_vector(n.color_track, n.color, t);
						if(node.parent!=null) lib3ds_matrix_copy(node.matrixNode, node.parent.matrixNode);
						else lib3ds_matrix_identity(node.matrixNode);
						lib3ds_matrix_translate(node.matrixNode, n.pos[0], n.pos[1], n.pos[2]);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT:
					{
						Lib3dsSpotlightNode n=(Lib3dsSpotlightNode)node;
						lib3ds_track_eval_vector(n.pos_track, n.pos, t);
						lib3ds_track_eval_vector(n.color_track, n.color, t);
						lib3ds_track_eval_float(n.hotspot_track, out n.hotspot, t);
						lib3ds_track_eval_float(n.falloff_track, out n.falloff, t);
						lib3ds_track_eval_float(n.roll_track, out n.roll, t);
						if(node.parent!=null) lib3ds_matrix_copy(node.matrixNode, node.parent.matrixNode);
						else lib3ds_matrix_identity(node.matrixNode);
						lib3ds_matrix_translate(node.matrixNode, n.pos[0], n.pos[1], n.pos[2]);
					}
					break;
				case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT_TARGET:
					{
						Lib3dsTargetNode n=(Lib3dsTargetNode)node;
						lib3ds_track_eval_vector(n.pos_track, n.pos, t);
						if(node.parent!=null) lib3ds_matrix_copy(node.matrixNode, node.parent.matrixNode);
						else lib3ds_matrix_identity(node.matrixNode);
						lib3ds_matrix_translate(node.matrixNode, n.pos[0], n.pos[1], n.pos[2]);
					}
					break;
			}

			foreach(Lib3dsNode p in node.childs) lib3ds_node_eval(p, t);
		}

		// Return a node object by name and type.
		//
		// This function performs a recursive search for the specified node.
		// Both name and type must match.
		//
		// \param node The parent node for the search
		// \param name The target node name.
		// \param type The target node type
		//
		// \return A pointer to the first matching node, or NULL if not found.
		public static Lib3dsNode lib3ds_node_by_name(Lib3dsNode node, string name, Lib3dsNodeType type)
		{
			foreach(Lib3dsNode p in node.childs)
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
		// \param node The parent node for the search
		// \param node_id The target node id.
		//
		// \return A pointer to the first matching node, or NULL if not found.
		public static Lib3dsNode lib3ds_node_by_id(Lib3dsNode node, ushort node_id)
		{
			foreach(Lib3dsNode p in node.childs)
			{
				if(p.node_id==node_id) return p;
				Lib3dsNode q=lib3ds_node_by_id(p, node_id);
				if(q!=null) return q;
			}
			return null;
		}

		public static void lib3ds_node_read(Lib3dsNode node, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			Debug.Assert(node!=null);
			lib3ds_chunk_read_start(c, 0, io);

			switch(c.chunk)
			{
				case Lib3dsChunks.CHK_AMBIENT_NODE_TAG:
				case Lib3dsChunks.CHK_OBJECT_NODE_TAG:
				case Lib3dsChunks.CHK_CAMERA_NODE_TAG:
				case Lib3dsChunks.CHK_TARGET_NODE_TAG:
				case Lib3dsChunks.CHK_LIGHT_NODE_TAG:
				case Lib3dsChunks.CHK_SPOTLIGHT_NODE_TAG:
				case Lib3dsChunks.CHK_L_TARGET_NODE_TAG: break;
				default: return;
			}

			while((chunk=lib3ds_chunk_read_next(c, io))!=0)
			{
				switch(chunk)
				{
					case Lib3dsChunks.CHK_NODE_ID:
						node.node_id=lib3ds_io_read_word(io);
						node.hasNodeID=true;
						lib3ds_io_log_indent(io, 1);
						lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_INFO, "ID={0}", node.node_id);
						lib3ds_io_log_indent(io, -1);
						break;
					case Lib3dsChunks.CHK_NODE_HDR:
						node.name=lib3ds_io_read_string(io, 64);
						node.flags=lib3ds_io_read_dword(io);
						node.parent_id=lib3ds_io_read_word(io);

						lib3ds_io_log_indent(io, 1);
						lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_INFO, "NAME={0}", node.name);
						lib3ds_io_log(io, Lib3dsLogLevel.LIB3DS_LOG_INFO, "PARENT={0}", (short)node.parent_id);
						lib3ds_io_log_indent(io, -1);
						break;
					case Lib3dsChunks.CHK_PIVOT:
						if(node.type==Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE)
						{
							Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
							lib3ds_io_read_vector(io, n.pivot);
						}
						else lib3ds_chunk_unknown(chunk, io);
						break;
					case Lib3dsChunks.CHK_INSTANCE_NAME:
						if(node.type==Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE)
						{
							Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
							n.instance_name=lib3ds_io_read_string(io, 64);
						}
						else lib3ds_chunk_unknown(chunk, io);
						break;
					case Lib3dsChunks.CHK_BOUNDBOX:
						if(node.type==Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE)
						{
							Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
							lib3ds_io_read_vector(io, n.bbox_min);
							lib3ds_io_read_vector(io, n.bbox_max);
						}
						else lib3ds_chunk_unknown(chunk, io);
						break;
					case Lib3dsChunks.CHK_COL_TRACK_TAG:
						{
							Lib3dsTrack track=null;
							switch(node.type)
							{
								case Lib3dsNodeType.LIB3DS_NODE_AMBIENT_COLOR:
									{
										Lib3dsAmbientColorNode n=(Lib3dsAmbientColorNode)node;
										track=n.color_track;
									}
									break;
								case Lib3dsNodeType.LIB3DS_NODE_OMNILIGHT:
									{
										Lib3dsOmnilightNode n=(Lib3dsOmnilightNode)node;
										track=n.color_track;
									}
									break;
								case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT:
									{
										Lib3dsSpotlightNode n=(Lib3dsSpotlightNode)node;
										track=n.color_track;
									}
									break;
								default: lib3ds_chunk_unknown(chunk, io); break;
							}
							if(track!=null) lib3ds_track_read(track, io);
						}
						break;
					case Lib3dsChunks.CHK_POS_TRACK_TAG:
						{
							Lib3dsTrack track=null;
							switch(node.type)
							{
								case Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE:
									{
										Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
										track=n.pos_track;
									}
									break;
								case Lib3dsNodeType.LIB3DS_NODE_CAMERA:
									{
										Lib3dsCameraNode n=(Lib3dsCameraNode)node;
										track=n.pos_track;
									}
									break;
								case Lib3dsNodeType.LIB3DS_NODE_CAMERA_TARGET:
									{
										Lib3dsTargetNode n=(Lib3dsTargetNode)node;
										track=n.pos_track;
									}
									break;
								case Lib3dsNodeType.LIB3DS_NODE_OMNILIGHT:
									{
										Lib3dsOmnilightNode n=(Lib3dsOmnilightNode)node;
										track=n.pos_track;
									}
									break;
								case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT:
									{
										Lib3dsSpotlightNode n=(Lib3dsSpotlightNode)node;
										track=n.pos_track;
									}
									break;
								case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT_TARGET:
									{
										Lib3dsTargetNode n=(Lib3dsTargetNode)node;
										track=n.pos_track;
									}
									break;
								default: lib3ds_chunk_unknown(chunk, io); break;
							}
							if(track!=null) lib3ds_track_read(track, io);
						}
						break;
					case Lib3dsChunks.CHK_ROT_TRACK_TAG:
						if(node.type==Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE)
						{
							Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
							n.rot_track.type=Lib3dsTrackType.LIB3DS_TRACK_QUAT;
							lib3ds_track_read(n.rot_track, io);
						}
						else lib3ds_chunk_unknown(chunk, io);
						break;
					case Lib3dsChunks.CHK_SCL_TRACK_TAG:
						if(node.type==Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE)
						{
							Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
							n.scl_track.type=Lib3dsTrackType.LIB3DS_TRACK_VECTOR;
							lib3ds_track_read(n.scl_track, io);
						}
						else lib3ds_chunk_unknown(chunk, io);
						break;
					case Lib3dsChunks.CHK_FOV_TRACK_TAG:
						if(node.type==Lib3dsNodeType.LIB3DS_NODE_CAMERA)
						{
							Lib3dsCameraNode n=(Lib3dsCameraNode)node;
							n.fov_track.type=Lib3dsTrackType.LIB3DS_TRACK_FLOAT;
							lib3ds_track_read(n.fov_track, io);
						}
						else lib3ds_chunk_unknown(chunk, io);
						break;
					case Lib3dsChunks.CHK_HOT_TRACK_TAG:
						if(node.type==Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT)
						{
							Lib3dsSpotlightNode n=(Lib3dsSpotlightNode)node;
							n.hotspot_track.type=Lib3dsTrackType.LIB3DS_TRACK_FLOAT;
							lib3ds_track_read(n.hotspot_track, io);
						}
						else lib3ds_chunk_unknown(chunk, io);
						break;
					case Lib3dsChunks.CHK_FALL_TRACK_TAG:
						if(node.type==Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT)
						{
							Lib3dsSpotlightNode n=(Lib3dsSpotlightNode)node;
							n.falloff_track.type=Lib3dsTrackType.LIB3DS_TRACK_FLOAT;
							lib3ds_track_read(n.falloff_track, io);
						}
						else lib3ds_chunk_unknown(chunk, io);
						break;
					case Lib3dsChunks.CHK_ROLL_TRACK_TAG:
						switch(node.type)
						{
							case Lib3dsNodeType.LIB3DS_NODE_CAMERA:
								{
									Lib3dsCameraNode n=(Lib3dsCameraNode)node;
									n.roll_track.type=Lib3dsTrackType.LIB3DS_TRACK_FLOAT;
									lib3ds_track_read(n.roll_track, io);
								}
								break;
							case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT:
								{
									Lib3dsSpotlightNode n=(Lib3dsSpotlightNode)node;
									n.roll_track.type=Lib3dsTrackType.LIB3DS_TRACK_FLOAT;
									lib3ds_track_read(n.roll_track, io);
								}
								break;
							default: lib3ds_chunk_unknown(chunk, io); break;
						}
						break;
					case Lib3dsChunks.CHK_HIDE_TRACK_TAG:
						if(node.type==Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE)
						{
							Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
							n.hide_track.type=Lib3dsTrackType.LIB3DS_TRACK_BOOL;
							lib3ds_track_read(n.hide_track, io);
						}
						else lib3ds_chunk_unknown(chunk, io);
						break;
					case Lib3dsChunks.CHK_MORPH_SMOOTH:
						if(node.type==Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE)
						{
							Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
							n.morph_smooth=lib3ds_io_read_float(io);
						}
						else lib3ds_chunk_unknown(chunk, io);
						break;
					//case Lib3dsChunks.LIB3DS_MORPH_TRACK_TAG:
					//    if(node.type==Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE)
					//    {
					//        Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
					//        n.morph_track=lib3ds_track_new(node, LIB3DS_TRACK_MORPH, 0);
					//        lib3ds_track_read(n.morph_track, io);
					//    }
					//    else lib3ds_chunk_unknown(chunk, io);
					//    break;
					default: lib3ds_chunk_unknown(chunk, io); break;
				}
			}

			lib3ds_chunk_read_end(c, io);
		}

		public static void lib3ds_node_write(Lib3dsNode node, ushort node_id, ushort parent_id, Lib3dsIo io)
		{
			Lib3dsChunk c_node=new Lib3dsChunk();

			switch(node.type)
			{
				case Lib3dsNodeType.LIB3DS_NODE_AMBIENT_COLOR: c_node.chunk=Lib3dsChunks.CHK_AMBIENT_NODE_TAG; break;
				case Lib3dsNodeType.LIB3DS_NODE_MESH_INSTANCE: c_node.chunk=Lib3dsChunks.CHK_OBJECT_NODE_TAG; break;
				case Lib3dsNodeType.LIB3DS_NODE_CAMERA: c_node.chunk=Lib3dsChunks.CHK_CAMERA_NODE_TAG; break;
				case Lib3dsNodeType.LIB3DS_NODE_CAMERA_TARGET: c_node.chunk=Lib3dsChunks.CHK_TARGET_NODE_TAG; break;
				case Lib3dsNodeType.LIB3DS_NODE_OMNILIGHT: c_node.chunk=Lib3dsChunks.CHK_LIGHT_NODE_TAG; break;
				case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT: c_node.chunk=Lib3dsChunks.CHK_SPOTLIGHT_NODE_TAG; break;
				case Lib3dsNodeType.LIB3DS_NODE_SPOTLIGHT_TARGET: c_node.chunk=Lib3dsChunks.CHK_L_TARGET_NODE_TAG; break;
				default: Debug.Assert(false); return;
			}

			lib3ds_chunk_write_start(c_node, io);

			{ // ---- CHK_NODE_ID ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_NODE_ID;
				c.size=8;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_word(io, node_id);
			}

			{ // ---- CHK_NODE_HDR ----
				Lib3dsChunk c=new Lib3dsChunk();
				c.chunk=Lib3dsChunks.CHK_NODE_HDR;
				c.size=6+1+(uint)node.name.Length+2+2+2;
				lib3ds_chunk_write(c, io);
				lib3ds_io_write_string(io, node.name);
				lib3ds_io_write_dword(io, node.flags);
				lib3ds_io_write_word(io, parent_id);
			}

			switch(c_node.chunk)
			{
				case Lib3dsChunks.CHK_AMBIENT_NODE_TAG:
					{
						Lib3dsAmbientColorNode n=(Lib3dsAmbientColorNode)node;
						if(n.color_track.keys.Count!=0)
						{ // ---- CHK_COL_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_COL_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.color_track, io);
							lib3ds_chunk_write_end(c, io);
						}
					}
					break;
				case Lib3dsChunks.CHK_OBJECT_NODE_TAG:
					{
						Lib3dsMeshInstanceNode n=(Lib3dsMeshInstanceNode)node;
						{ // ---- CHK_PIVOT ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_PIVOT;
							c.size=18;
							lib3ds_chunk_write(c, io);
							lib3ds_io_write_vector(io, n.pivot);
						}

						if(n.instance_name.Length!=0)
						{ // ---- CHK_INSTANCE_NAME ----
							string name=n.instance_name;
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_INSTANCE_NAME;
							c.size=6+1+(uint)name.Length;
							lib3ds_chunk_write(c, io);
							lib3ds_io_write_string(io, name);
						}

						{
							int i=0;
							for(i=0; i<3; i++)
							{
								if(Math.Abs(n.bbox_min[i])>EPSILON||Math.Abs(n.bbox_max[i])>EPSILON) break;
							}

							if(i<3)
							{ // ---- CHK_BOUNDBOX ----
								Lib3dsChunk c=new Lib3dsChunk();
								c.chunk=Lib3dsChunks.CHK_BOUNDBOX;
								c.size=30;
								lib3ds_chunk_write(c, io);
								lib3ds_io_write_vector(io, n.bbox_min);
								lib3ds_io_write_vector(io, n.bbox_max);
							}
						}

						if(n.pos_track.keys.Count!=0)
						{ // ---- CHK_POS_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_POS_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.pos_track, io);
							lib3ds_chunk_write_end(c, io);
						}
						if(n.rot_track.keys.Count!=0)
						{ // ---- CHK_ROT_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_ROT_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.rot_track, io);
							lib3ds_chunk_write_end(c, io);
						}
						if(n.scl_track.keys.Count!=0)
						{ // ---- LIB3DS_SCL_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_SCL_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.scl_track, io);
							lib3ds_chunk_write_end(c, io);
						}
						if(n.hide_track.keys.Count!=0)
						{ // ---- CHK_HIDE_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_HIDE_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.hide_track, io);
							lib3ds_chunk_write_end(c, io);
						}
						if(Math.Abs(n.morph_smooth)>EPSILON)
						{ // ---- CHK_MORPH_SMOOTH ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_MORPH_SMOOTH;
							c.size=10;
							lib3ds_chunk_write(c, io);
							lib3ds_io_write_float(io, n.morph_smooth);
						}
					}
					break;
				case Lib3dsChunks.CHK_CAMERA_NODE_TAG:
					{
						Lib3dsCameraNode n=(Lib3dsCameraNode)node;
						if(n.pos_track.keys.Count!=0)
						{ // ---- CHK_POS_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_POS_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.pos_track, io);
							lib3ds_chunk_write_end(c, io);
						}
						if(n.fov_track.keys.Count!=0)
						{ // ---- CHK_FOV_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_FOV_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.fov_track, io);
							lib3ds_chunk_write_end(c, io);
						}
						if(n.roll_track.keys.Count!=0)
						{ // ---- CHK_ROLL_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_ROLL_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.roll_track, io);
							lib3ds_chunk_write_end(c, io);
						}
					}
					break;
				case Lib3dsChunks.CHK_TARGET_NODE_TAG:
					{
						Lib3dsTargetNode n=(Lib3dsTargetNode)node;
						if(n.pos_track.keys.Count!=0)
						{ // ---- CHK_POS_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_POS_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.pos_track, io);
							lib3ds_chunk_write_end(c, io);
						}
					}
					break;
				case Lib3dsChunks.CHK_LIGHT_NODE_TAG:
					{
						Lib3dsOmnilightNode n=(Lib3dsOmnilightNode)node;
						if(n.pos_track.keys.Count!=0)
						{ // ---- CHK_POS_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_POS_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.pos_track, io);
							lib3ds_chunk_write_end(c, io);
						}
						if(n.color_track.keys.Count!=0)
						{ // ---- CHK_COL_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_COL_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.color_track, io);
							lib3ds_chunk_write_end(c, io);
						}
					}
					break;
				case Lib3dsChunks.CHK_SPOTLIGHT_NODE_TAG:
					{
						Lib3dsSpotlightNode n=(Lib3dsSpotlightNode)node;
						if(n.pos_track.keys.Count!=0)
						{ // ---- CHK_POS_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_POS_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.pos_track, io);
							lib3ds_chunk_write_end(c, io);
						}
						if(n.color_track.keys.Count!=0)
						{ // ---- CHK_COL_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_COL_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.color_track, io);
							lib3ds_chunk_write_end(c, io);
						}
						if(n.hotspot_track.keys.Count!=0)
						{ // ---- CHK_HOT_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_HOT_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.hotspot_track, io);
							lib3ds_chunk_write_end(c, io);
						}
						if(n.falloff_track.keys.Count!=0)
						{ // ---- CHK_FALL_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_FALL_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.falloff_track, io);
							lib3ds_chunk_write_end(c, io);
						}
						if(n.roll_track.keys.Count!=0)
						{ // ---- CHK_ROLL_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_ROLL_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.roll_track, io);
							lib3ds_chunk_write_end(c, io);
						}
					}
					break;
				case Lib3dsChunks.CHK_L_TARGET_NODE_TAG:
					{
						Lib3dsTargetNode n=(Lib3dsTargetNode)node;
						if(n.pos_track.keys.Count!=0)
						{ // ---- CHK_POS_TRACK_TAG ----
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_POS_TRACK_TAG;
							lib3ds_chunk_write_start(c, io);
							lib3ds_track_write(n.pos_track, io);
							lib3ds_chunk_write_end(c, io);
						}
					}
					break;
			}

			lib3ds_chunk_write_end(c_node, io);
		}
	}
}
