// lib3ds.cs - Structures, Enums and Stuff
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

using System;
using System.Collections.Generic;
using System.IO;

namespace lib3ds.Net
{
	public enum Lib3dsIoSeek
	{
		LIB3DS_SEEK_SET=0,
		LIB3DS_SEEK_CUR=1,
		LIB3DS_SEEK_END=2
	}

	public enum Lib3dsLogLevel
	{
		LIB3DS_LOG_ERROR=0,
		LIB3DS_LOG_WARN=1,
		LIB3DS_LOG_INFO=2,
		LIB3DS_LOG_DEBUG=3
	}

	public delegate long seek_func(Stream file, long offset, Lib3dsIoSeek origin);
	public delegate long tell_func(Stream file);
	public delegate int read_func(Stream file, byte[] buffer, int size);
	public delegate int write_func(Stream file, byte[] buffer, int size);
	public delegate void log_func(object self, Lib3dsLogLevel level, int indent, string msg);

	public class Lib3dsIo
	{
		internal int log_indent;
		public FileStream self;
		public seek_func seek_func;
		public tell_func tell_func;
		public read_func read_func;
		public write_func write_func;
		public log_func log_func;
	}

	// Atmosphere settings
	public class Lib3dsAtmosphere
	{
		public bool use_fog;
		public float[] fog_color=new float[3];
		public bool fog_background;
		public float fog_near_plane;
		public float fog_near_density;
		public float fog_far_plane;
		public float fog_far_density;
		public bool use_layer_fog;
		public uint layer_fog_flags;
		public float[] layer_fog_color=new float[3];
		public float layer_fog_near_y;
		public float layer_fog_far_y;
		public float layer_fog_density;
		public bool use_dist_cue;
		public bool dist_cue_background;
		public float dist_cue_near_plane;
		public float dist_cue_near_dimming;
		public float dist_cue_far_plane;
		public float dist_cue_far_dimming;
	}

	// Background settings
	public class Lib3dsBackground
	{
		public bool use_bitmap;
		public string bitmap_name="";
		public bool use_solid;
		public float[] solid_color=new float[3];
		public bool use_gradient;
		public float gradient_percent;
		public float[] gradient_top=new float[3];
		public float[] gradient_middle=new float[3];
		public float[] gradient_bottom=new float[3];
	}

	// Shadow settings
	public class Lib3dsShadow
	{
		public short map_size;	// Global shadow map size that ranges from 10 to 4096
		public float low_bias;	// Global shadow low bias
		public float hi_bias;	// Global shadow hi bias
		public float filter;	// Global shadow filter that ranges from 1 (lowest) to 10 (highest)
		public float ray_bias;	// Global raytraced shadow bias
	}

	// Layout view types
	public enum Lib3dsViewType
	{
		LIB3DS_VIEW_NOT_USED=0,
		LIB3DS_VIEW_TOP=1,
		LIB3DS_VIEW_BOTTOM=2,
		LIB3DS_VIEW_LEFT=3,
		LIB3DS_VIEW_RIGHT=4,
		LIB3DS_VIEW_FRONT=5,
		LIB3DS_VIEW_BACK=6,
		LIB3DS_VIEW_USER=7,
		LIB3DS_VIEW_SPOTLIGHT=18,
		LIB3DS_VIEW_CAMERA=65535
	}

	// Layout styles
	public enum Lib3dsLayoutStyle
	{
		LIB3DS_LAYOUT_SINGLE=0,
		LIB3DS_LAYOUT_TWO_PANE_VERT_SPLIT=1,
		LIB3DS_LAYOUT_TWO_PANE_HORIZ_SPLIT=2,
		LIB3DS_LAYOUT_FOUR_PANE=3,
		LIB3DS_LAYOUT_THREE_PANE_LEFT_SPLIT=4,
		LIB3DS_LAYOUT_THREE_PANE_BOTTOM_SPLIT=5,
		LIB3DS_LAYOUT_THREE_PANE_RIGHT_SPLIT=6,
		LIB3DS_LAYOUT_THREE_PANE_TOP_SPLIT=7,
		LIB3DS_LAYOUT_THREE_PANE_VERT_SPLIT=8,
		LIB3DS_LAYOUT_THREE_PANE_HORIZ_SPLIT=9,
		LIB3DS_LAYOUT_FOUR_PANE_LEFT_SPLIT=10,
		LIB3DS_LAYOUT_FOUR_PANE_RIGHT_SPLIT=11
	}

	// Layout view settings
	public class Lib3dsView
	{
		public Lib3dsViewType type;
		public ushort axis_lock;
		public short[] position=new short[2];
		public short[] size=new short[2];
		public float zoom;
		public float[] center=new float[3];
		public float horiz_angle;
		public float vert_angle;
		public byte[] camera=new byte[11];
	}

	// Viewport and default view settings
	public class Lib3dsViewport
	{
		public Lib3dsLayoutStyle layout_style;
		public short layout_active;
		public short layout_swap;
		public short layout_swap_prior;
		public short layout_swap_view;
		public ushort[] layout_position=new ushort[2];
		public ushort[] layout_size=new ushort[2];
		public List<Lib3dsView> layout_views=new List<Lib3dsView>();
		public Lib3dsViewType default_type;
		public float[] default_position=new float[3];
		public float default_width;
		public float default_horiz_angle;
		public float default_vert_angle;
		public float default_roll_angle;
		public byte[] default_camera=new byte[11];

		public Lib3dsViewport()
		{
			Clear();
		}

		public void Clear()
		{
			layout_style=Lib3dsLayoutStyle.LIB3DS_LAYOUT_SINGLE;
			layout_active=layout_swap=layout_swap_prior=layout_swap_view=0;
			layout_position=new ushort[2];
			layout_size=new ushort[2];
			layout_views.Clear();
			default_type=0;
			default_position=new float[3];
			default_width=default_horiz_angle=default_vert_angle=default_roll_angle=0;
			default_camera=new byte[11];
		}
	}

	// Material texture map flags
	[Flags]
	public enum Lib3dsTextureMapFlags
	{
		LIB3DS_TEXTURE_DECALE=0x0001,
		LIB3DS_TEXTURE_MIRROR=0x0002,
		LIB3DS_TEXTURE_NEGATE=0x0008,
		LIB3DS_TEXTURE_NO_TILE=0x0010,
		LIB3DS_TEXTURE_SUMMED_AREA=0x0020,
		LIB3DS_TEXTURE_ALPHA_SOURCE=0x0040,
		LIB3DS_TEXTURE_TINT=0x0080,
		LIB3DS_TEXTURE_IGNORE_ALPHA=0x0100,
		LIB3DS_TEXTURE_RGB_TINT=0x0200
	}

	// Material texture map
	public class Lib3dsTextureMap
	{
		//public uint user_id;
		//public object user_ptr;
		public string name="";
		public Lib3dsTextureMapFlags flags;
		public float percent;
		public float blur;
		public float[] scale=new float[2];
		public float[] offset=new float[2];
		public float rotation;
		public float[] tint_1=new float[3];
		public float[] tint_2=new float[3];
		public float[] tint_r=new float[3];
		public float[] tint_g=new float[3];
		public float[] tint_b=new float[3];
	}

	// Auto reflection texture map flags
	[Flags]
	public enum Lib3dsAutoReflMapFlags
	{
		LIB3DS_AUTOREFL_USE=0x0001,
		LIB3DS_AUTOREFL_READ_FIRST_FRAME_ONLY=0x0004,
		LIB3DS_AUTOREFL_FLAT_MIRROR=0x0008
	}

	// Material shading type
	public enum Lib3dsShading
	{
		LIB3DS_SHADING_WIRE_FRAME=0,
		LIB3DS_SHADING_FLAT=1,
		LIB3DS_SHADING_GOURAUD=2,
		LIB3DS_SHADING_PHONG=3,
		LIB3DS_SHADING_METAL=4
	}

	// Material
	public class Lib3dsMaterial
	{
		//public uint user_id;
		//public object user_ptr;
		public string name="";					// Material name
		public float[] ambient=new float[3];	// Material ambient reflectivity
		public float[] diffuse=new float[3];	// Material diffuse reflectivity
		public float[] specular=new float[3];	// Material specular reflectivity
		public float shininess;					// Material specular exponent
		public float shin_strength;
		public bool use_blur;
		public float blur;
		public float transparency;
		public float falloff;
		public bool is_additive;
		public bool self_illum_flag;
		public float self_illum;
		public bool use_falloff;
		public int shading;
		public bool soften;
		public bool face_map;
		public bool two_sided;			// Material visible from back
		public bool map_decal;
		public bool use_wire;
		public bool use_wire_abs;
		public float wire_size;
		public Lib3dsTextureMap texture1_map=new Lib3dsTextureMap();
		public Lib3dsTextureMap texture1_mask=new Lib3dsTextureMap();
		public Lib3dsTextureMap texture2_map=new Lib3dsTextureMap();
		public Lib3dsTextureMap texture2_mask=new Lib3dsTextureMap();
		public Lib3dsTextureMap opacity_map=new Lib3dsTextureMap();
		public Lib3dsTextureMap opacity_mask=new Lib3dsTextureMap();
		public Lib3dsTextureMap bump_map=new Lib3dsTextureMap();
		public Lib3dsTextureMap bump_mask=new Lib3dsTextureMap();
		public Lib3dsTextureMap specular_map=new Lib3dsTextureMap();
		public Lib3dsTextureMap specular_mask=new Lib3dsTextureMap();
		public Lib3dsTextureMap shininess_map=new Lib3dsTextureMap();
		public Lib3dsTextureMap shininess_mask=new Lib3dsTextureMap();
		public Lib3dsTextureMap self_illum_map=new Lib3dsTextureMap();
		public Lib3dsTextureMap self_illum_mask=new Lib3dsTextureMap();
		public Lib3dsTextureMap reflection_map=new Lib3dsTextureMap();
		public Lib3dsTextureMap reflection_mask=new Lib3dsTextureMap();
		public uint autorefl_map_flags;
		public int autorefl_map_anti_alias; // 0=None, 1=Low, 2=Medium, 3=High
		public int autorefl_map_size;
		public int autorefl_map_frame_step;
	}

	// Object flags for cameras, lights and meshes
	[Flags]
	public enum Lib3dsObjectFlags
	{
		LIB3DS_OBJECT_HIDDEN=0x01,
		LIB3DS_OBJECT_VIS_LOFTER=0x02,
		LIB3DS_OBJECT_DOESNT_CAST=0x04,
		LIB3DS_OBJECT_MATTE=0x08,
		LIB3DS_OBJECT_DONT_RCVSHADOW=0x10,
		LIB3DS_OBJECT_FAST=0x20,
		LIB3DS_OBJECT_FROZEN=0x40
	}

	// Camera object
	public class Lib3dsCamera
	{
		//public uint user_id;
		//public object user_ptr;
		public string name="";
		public Lib3dsObjectFlags object_flags;	// see Lib3dsObjectFlags
		public float[] position=new float[3];
		public float[] target=new float[3];
		public float roll;
		public float fov;
		public bool see_cone;
		public float near_range;
		public float far_range;
	}

	// Light object
	public class Lib3dsLight
	{
		//public uint user_id;
		//public object user_ptr;
		public string name="";
		public Lib3dsObjectFlags object_flags;
		public bool spot_light;
		public bool see_cone;
		public float[] color=new float[3];
		public float[] position=new float[3];
		public float[] target=new float[3];
		public float roll;
		public bool off;
		public float outer_range;
		public float inner_range;
		public float multiplier;
		//public List<string> excludes=new List<string>();
		public float attenuation;
		public bool rectangular_spot;
		public bool shadowed;
		public float shadow_bias;
		public float shadow_filter;
		public int shadow_size;
		public float spot_aspect;
		public bool use_projector;
		public string projector;
		public bool spot_overshoot;
		public bool ray_shadows;
		public float ray_bias;
		public float hotspot;
		public float falloff;
	}

	// Texture map projection
	public enum Lib3dsMapType
	{
		LIB3DS_MAP_NONE=-1,
		LIB3DS_MAP_PLANAR=0,
		LIB3DS_MAP_CYLINDRICAL=1,
		LIB3DS_MAP_SPHERICAL=2
	}

	// Meaning of Lib3dsFace::flags. ABC are points of the current face
	// (A: is 1st vertex, B is 2nd vertex, C is 3rd vertex)
	[Flags]
	public enum Lib3dsFaceFlags
	{
		LIB3DS_FACE_VIS_AC=0x01,		// Bit 0: Edge visibility AC
		LIB3DS_FACE_VIS_BC=0x02,		// Bit 1: Edge visibility BC
		LIB3DS_FACE_VIS_AB=0x04,		// Bit 2: Edge visibility AB
		LIB3DS_FACE_WRAP_U=0x08,		// Bit 3: Face is at tex U wrap seam
		LIB3DS_FACE_WRAP_V=0x10,		// Bit 4: Face is at tex V wrap seam
		LIB3DS_FACE_SELECT_3=(1<<13),	// Bit 13: Selection of the face in selection 3
		LIB3DS_FACE_SELECT_2=(1<<14),	// Bit 14: Selection of the face in selection 2
		LIB3DS_FACE_SELECT_1=(1<<15),	// Bit 15: Selection of the face in selection 1
	}

	public class Lib3dsFace
	{
		public ushort[] index=new ushort[3];
		public ushort flags;
		public int material;
		public uint smoothing_group;
	}

	public class Lib3dsVertex
	{
		public float x, y, z;

		public Lib3dsVertex() { }

		public Lib3dsVertex(double x, double y, double z)
		{
			this.x=(float)x;
			this.y=(float)y;
			this.z=(float)z;
		}

		public Lib3dsVertex(Lib3dsVertex v)
		{
			x=v.x;
			y=v.y;
			z=v.z;
		}

		public float[] ToArray()
		{
			return new float[] { x, y, z };
		}
	}

	public class Lib3dsTexturecoordinate
	{
		public float s, t;

		public Lib3dsTexturecoordinate() { }

		public Lib3dsTexturecoordinate(double s, double t)
		{
			this.s=(float)s;
			this.t=(float)t;
		}
	}

	// Triangular mesh object
	public class Lib3dsMesh
	{
		//public uint user_id;
		//public object user_ptr;
		public string name="";					// Mesh name. Don't use more than 8 characters
		public Lib3dsObjectFlags object_flags;	// see Lib3dsObjectFlags
		public byte color;						// Index to editor palette [0..255]
		public float[,] matrix=new float[4, 4];	// Transformation matrix for mesh data
		public ushort nvertices;				// Number of vertices in vertex array (max. 65535)
		public List<Lib3dsVertex> vertices;
		public List<Lib3dsTexturecoordinate> texcos;
		public List<ushort> vflags;
		public ushort nfaces;					// Number of faces in face array (max. 65535)
		public List<Lib3dsFace> faces;
		public string box_front="";
		public string box_back="";
		public string box_left="";
		public string box_right="";
		public string box_top="";
		public string box_bottom="";
		public Lib3dsMapType map_type;
		public float[] map_pos=new float[3];
		public float[,] map_matrix=new float[4, 4];
		public float map_scale;
		public float[] map_tile=new float[2];
		public float[] map_planar_size=new float[2];
		public float map_cylinder_height;
	}

	public enum Lib3dsNodeType
	{
		LIB3DS_NODE_AMBIENT_COLOR=0,
		LIB3DS_NODE_MESH_INSTANCE=1,
		LIB3DS_NODE_CAMERA=2,
		LIB3DS_NODE_CAMERA_TARGET=3,
		LIB3DS_NODE_OMNILIGHT=4,
		LIB3DS_NODE_SPOTLIGHT=5,
		LIB3DS_NODE_SPOTLIGHT_TARGET=6
	}

	[Flags]
	public enum Lib3dsNodeFlags
	{
		LIB3DS_NODE_HIDDEN=0x000800,
		LIB3DS_NODE_SHOW_PATH=0x010000,
		LIB3DS_NODE_SMOOTHING=0x020000,
		LIB3DS_NODE_MOTION_BLUR=0x100000,
		LIB3DS_NODE_MORPH_MATERIALS=0x400000
	}

	public class Lib3dsNode
	{
		public ushort parent_id;
		public List<Lib3dsNode> childs=new List<Lib3dsNode>();
		public Lib3dsNode parent;
		public Lib3dsNodeType type;
		public ushort node_id;		// 0..65535
		public bool hasNodeID=false;
		public string name="";
		public uint flags;
		public float[,] matrixNode=new float[4, 4];
	}

	[Flags]
	public enum Lib3dsKeyFlags
	{
		LIB3DS_KEY_USE_TENS=0x01,
		LIB3DS_KEY_USE_CONT=0x02,
		LIB3DS_KEY_USE_BIAS=0x04,
		LIB3DS_KEY_USE_EASE_TO=0x08,
		LIB3DS_KEY_USE_EASE_FROM=0x10
	}

	public class Lib3dsKey
	{
		public int frame;
		public Lib3dsKeyFlags flags;
		public float tens;
		public float cont;
		public float bias;
		public float ease_to;
		public float ease_from;
		public float[] value=new float[4];
	}

	public enum Lib3dsTrackType
	{
		LIB3DS_TRACK_BOOL=0,
		LIB3DS_TRACK_FLOAT=1,
		LIB3DS_TRACK_VECTOR=3,
		LIB3DS_TRACK_QUAT=4
	}

	[Flags]
	public enum Lib3dsTrackFlags
	{
		LIB3DS_TRACK_REPEAT=0x0001,
		LIB3DS_TRACK_SMOOTH=0x0002,
		LIB3DS_TRACK_LOCK_X=0x0008,
		LIB3DS_TRACK_LOCK_Y=0x0010,
		LIB3DS_TRACK_LOCK_Z=0x0020,
		LIB3DS_TRACK_UNLINK_X=0x0100,
		LIB3DS_TRACK_UNLINK_Y=0x0200,
		LIB3DS_TRACK_UNLINK_Z=0x0400
	}

	public class Lib3dsTrack
	{
		public Lib3dsTrackFlags flags;
		public Lib3dsTrackType type;
		public List<Lib3dsKey> keys=new List<Lib3dsKey>();
	}

	public class Lib3dsAmbientColorNode : Lib3dsNode
	{
		public float[] color=new float[3];
		public Lib3dsTrack color_track=new Lib3dsTrack();
	}

	public class Lib3dsMeshInstanceNode : Lib3dsNode
	{
		public float[] pivot=new float[3];
		public string instance_name="";
		public float[] bbox_min=new float[3];
		public float[] bbox_max=new float[3];
		public bool hide=false;
		public float[] pos=new float[3];
		public float[] rot=new float[4];
		public float[] scl=new float[3];
		public float morph_smooth;
		public string morph="";
		public Lib3dsTrack pos_track=new Lib3dsTrack();
		public Lib3dsTrack rot_track=new Lib3dsTrack();
		public Lib3dsTrack scl_track=new Lib3dsTrack();
		public Lib3dsTrack hide_track=new Lib3dsTrack();
	}

	public class Lib3dsCameraNode : Lib3dsNode
	{
		public float[] pos=new float[3];
		public float fov;
		public float roll;
		public Lib3dsTrack pos_track=new Lib3dsTrack();
		public Lib3dsTrack fov_track=new Lib3dsTrack();
		public Lib3dsTrack roll_track=new Lib3dsTrack();
	}

	public class Lib3dsTargetNode : Lib3dsNode
	{
		public float[] pos=new float[3];
		public Lib3dsTrack pos_track=new Lib3dsTrack();
	}

	public class Lib3dsOmnilightNode : Lib3dsNode
	{
		public float[] pos=new float[3];
		public float[] color=new float[3];
		public Lib3dsTrack pos_track=new Lib3dsTrack();
		public Lib3dsTrack color_track=new Lib3dsTrack();
	}

	public class Lib3dsSpotlightNode : Lib3dsNode
	{
		public float[] pos=new float[3];
		public float[] color=new float[3];
		public float hotspot;
		public float falloff;
		public float roll;
		public Lib3dsTrack pos_track=new Lib3dsTrack();
		public Lib3dsTrack color_track=new Lib3dsTrack();
		public Lib3dsTrack hotspot_track=new Lib3dsTrack();
		public Lib3dsTrack falloff_track=new Lib3dsTrack();
		public Lib3dsTrack roll_track=new Lib3dsTrack();
	}

	public class Lib3dsFile
	{
		//public uint user_id;
		//public object user_ptr;
		public uint mesh_version;
		public uint keyf_revision;
		public string name="";
		public float master_scale;
		public float[] construction_plane=new float[3];
		public float[] ambient=new float[3];
		public Lib3dsShadow shadow=new Lib3dsShadow();
		public Lib3dsBackground background=new Lib3dsBackground();
		public Lib3dsAtmosphere atmosphere=new Lib3dsAtmosphere();
		public Lib3dsViewport viewport=new Lib3dsViewport();
		public Lib3dsViewport viewport_keyf=new Lib3dsViewport();
		public int frames;
		public int segment_from;
		public int segment_to;
		public int current_frame;
		public List<Lib3dsMaterial> materials=new List<Lib3dsMaterial>();
		public List<Lib3dsCamera> cameras=new List<Lib3dsCamera>();
		public List<Lib3dsLight> lights=new List<Lib3dsLight>();
		public List<Lib3dsMesh> meshes=new List<Lib3dsMesh>();
		public List<Lib3dsNode> nodes=new List<Lib3dsNode>();
	}
}
