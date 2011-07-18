// lib3ds_viewport.cs - Viewport
//
// Based on lib3ds, Version 2.0 RC1 - 09-Sep-2008
// This code is released under the GNU Lesser General Public License.
// For conditions of distribution and use, see copyright notice in License.txt

namespace lib3ds.Net
{
	public static partial class LIB3DS
	{
		public static void lib3ds_viewport_read(Lib3dsViewport viewport, Lib3dsIo io)
		{
			Lib3dsChunk c=new Lib3dsChunk();
			Lib3dsChunks chunk;

			viewport.Clear();
			lib3ds_chunk_read_start(c, 0, io);
			switch(c.chunk)
			{
				case Lib3dsChunks.CHK_VIEWPORT_LAYOUT:
					viewport.layout_style=(Lib3dsLayoutStyle)lib3ds_io_read_word(io);
					viewport.layout_active=lib3ds_io_read_intw(io);
					lib3ds_io_read_intw(io);
					viewport.layout_swap=lib3ds_io_read_intw(io);
					lib3ds_io_read_intw(io);
					viewport.layout_swap_prior=lib3ds_io_read_intw(io);
					viewport.layout_swap_view=lib3ds_io_read_intw(io);
					lib3ds_chunk_read_tell(c, io);
					while((chunk=lib3ds_chunk_read_next(c, io))!=0)
					{
						switch(chunk)
						{
							case Lib3dsChunks.CHK_VIEWPORT_SIZE:
								viewport.layout_position[0]=lib3ds_io_read_word(io);
								viewport.layout_position[1]=lib3ds_io_read_word(io);
								viewport.layout_size[0]=lib3ds_io_read_word(io);
								viewport.layout_size[1]=lib3ds_io_read_word(io);
								break;
							case Lib3dsChunks.CHK_VIEWPORT_DATA_3:
								{
									lib3ds_io_read_intw(io);

									Lib3dsView layout_view=new Lib3dsView();
									layout_view.axis_lock=lib3ds_io_read_word(io);
									layout_view.position[0]=lib3ds_io_read_intw(io);
									layout_view.position[1]=lib3ds_io_read_intw(io);
									layout_view.size[0]=lib3ds_io_read_intw(io);
									layout_view.size[1]=lib3ds_io_read_intw(io);
									layout_view.type=(Lib3dsViewType)lib3ds_io_read_word(io);
									layout_view.zoom=lib3ds_io_read_float(io);
									lib3ds_io_read_vector(io, layout_view.center);
									layout_view.horiz_angle=lib3ds_io_read_float(io);
									layout_view.vert_angle=lib3ds_io_read_float(io);
									lib3ds_io_read(io, layout_view.camera, 11);

									viewport.layout_views.Add(layout_view);
								}
								break;
							case Lib3dsChunks.CHK_VIEWPORT_DATA: break; // 3DS R2 & R3 chunk unsupported
							default: lib3ds_chunk_unknown(chunk, io); break;
						}
					}
					break;
				case Lib3dsChunks.CHK_DEFAULT_VIEW:
					while((chunk=lib3ds_chunk_read_next(c, io))!=0)
					{
						switch(chunk)
						{
							case Lib3dsChunks.CHK_VIEW_TOP:
								viewport.default_type=Lib3dsViewType.LIB3DS_VIEW_TOP;
								lib3ds_io_read_vector(io, viewport.default_position);
								viewport.default_width=lib3ds_io_read_float(io);
								break;
							case Lib3dsChunks.CHK_VIEW_BOTTOM:
								viewport.default_type=Lib3dsViewType.LIB3DS_VIEW_BOTTOM;
								lib3ds_io_read_vector(io, viewport.default_position);
								viewport.default_width=lib3ds_io_read_float(io);
								break;
							case Lib3dsChunks.CHK_VIEW_LEFT:
								viewport.default_type=Lib3dsViewType.LIB3DS_VIEW_LEFT;
								lib3ds_io_read_vector(io, viewport.default_position);
								viewport.default_width=lib3ds_io_read_float(io);
								break;
							case Lib3dsChunks.CHK_VIEW_RIGHT:
								viewport.default_type=Lib3dsViewType.LIB3DS_VIEW_RIGHT;
								lib3ds_io_read_vector(io, viewport.default_position);
								viewport.default_width=lib3ds_io_read_float(io);
								break;
							case Lib3dsChunks.CHK_VIEW_FRONT:
								viewport.default_type=Lib3dsViewType.LIB3DS_VIEW_FRONT;
								lib3ds_io_read_vector(io, viewport.default_position);
								viewport.default_width=lib3ds_io_read_float(io);
								break;
							case Lib3dsChunks.CHK_VIEW_BACK:
								viewport.default_type=Lib3dsViewType.LIB3DS_VIEW_BACK;
								lib3ds_io_read_vector(io, viewport.default_position);
								viewport.default_width=lib3ds_io_read_float(io);
								break;
							case Lib3dsChunks.CHK_VIEW_USER:
								viewport.default_type=Lib3dsViewType.LIB3DS_VIEW_USER;
								lib3ds_io_read_vector(io, viewport.default_position);
								viewport.default_width=lib3ds_io_read_float(io);
								viewport.default_horiz_angle=lib3ds_io_read_float(io);
								viewport.default_vert_angle=lib3ds_io_read_float(io);
								viewport.default_roll_angle=lib3ds_io_read_float(io);
								break;
							case Lib3dsChunks.CHK_VIEW_CAMERA:
								viewport.default_type=Lib3dsViewType.LIB3DS_VIEW_CAMERA;
								lib3ds_io_read(io, viewport.default_camera, 11);
								break;
							default: lib3ds_chunk_unknown(chunk, io); break;
						}
					}
					break;
			}

			lib3ds_chunk_read_end(c, io);
		}

		public static void lib3ds_viewport_write(Lib3dsViewport viewport, Lib3dsIo io)
		{
			if(viewport.layout_views.Count!=0)
			{
				Lib3dsChunk c_viewport_layout=new Lib3dsChunk();

				c_viewport_layout.chunk=Lib3dsChunks.CHK_VIEWPORT_LAYOUT;
				lib3ds_chunk_write_start(c_viewport_layout, io);

				lib3ds_io_write_word(io, (ushort)viewport.layout_style);
				lib3ds_io_write_intw(io, viewport.layout_active);
				lib3ds_io_write_intw(io, 0);
				lib3ds_io_write_intw(io, viewport.layout_swap);
				lib3ds_io_write_intw(io, 0);
				lib3ds_io_write_intw(io, viewport.layout_swap_prior);
				lib3ds_io_write_intw(io, viewport.layout_swap_view);

				{
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_VIEWPORT_SIZE;
					c.size=14;
					lib3ds_chunk_write(c, io);
					lib3ds_io_write_word(io, viewport.layout_position[0]);
					lib3ds_io_write_word(io, viewport.layout_position[1]);
					lib3ds_io_write_word(io, viewport.layout_size[0]);
					lib3ds_io_write_word(io, viewport.layout_size[1]);
				}

				foreach(Lib3dsView layout_view in viewport.layout_views)
				{
					Lib3dsChunk c=new Lib3dsChunk();
					c.chunk=Lib3dsChunks.CHK_VIEWPORT_DATA_3;
					c.size=55;
					lib3ds_chunk_write(c, io);

					lib3ds_io_write_intw(io, 0);
					lib3ds_io_write_word(io, layout_view.axis_lock);
					lib3ds_io_write_intw(io, layout_view.position[0]);
					lib3ds_io_write_intw(io, layout_view.position[1]);
					lib3ds_io_write_intw(io, layout_view.size[0]);
					lib3ds_io_write_intw(io, layout_view.size[1]);
					lib3ds_io_write_word(io, (ushort)layout_view.type);
					lib3ds_io_write_float(io, layout_view.zoom);
					lib3ds_io_write_vector(io, layout_view.center);
					lib3ds_io_write_float(io, layout_view.horiz_angle);
					lib3ds_io_write_float(io, layout_view.vert_angle);
					lib3ds_io_write(io, layout_view.camera, 11);
				}

				lib3ds_chunk_write_end(c_viewport_layout, io);
			}

			if(viewport.default_type!=0)
			{
				Lib3dsChunk c_default_view=new Lib3dsChunk();

				c_default_view.chunk=Lib3dsChunks.CHK_DEFAULT_VIEW;
				lib3ds_chunk_write_start(c_default_view, io);

				switch(viewport.default_type)
				{
					case Lib3dsViewType.LIB3DS_VIEW_TOP:
						{
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_VIEW_TOP;
							c.size=22;
							lib3ds_chunk_write(c, io);
							lib3ds_io_write_vector(io, viewport.default_position);
							lib3ds_io_write_float(io, viewport.default_width);
						}
						break;
					case Lib3dsViewType.LIB3DS_VIEW_BOTTOM:
						{
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_VIEW_BOTTOM;
							c.size=22;
							lib3ds_chunk_write(c, io);
							lib3ds_io_write_vector(io, viewport.default_position);
							lib3ds_io_write_float(io, viewport.default_width);
						}
						break;
					case Lib3dsViewType.LIB3DS_VIEW_LEFT:
						{
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_VIEW_LEFT;
							c.size=22;
							lib3ds_chunk_write(c, io);
							lib3ds_io_write_vector(io, viewport.default_position);
							lib3ds_io_write_float(io, viewport.default_width);
						}
						break;
					case Lib3dsViewType.LIB3DS_VIEW_RIGHT:
						{
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_VIEW_RIGHT;
							c.size=22;
							lib3ds_chunk_write(c, io);
							lib3ds_io_write_vector(io, viewport.default_position);
							lib3ds_io_write_float(io, viewport.default_width);
						}
						break;
					case Lib3dsViewType.LIB3DS_VIEW_FRONT:
						{
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_VIEW_FRONT;
							c.size=22;
							lib3ds_chunk_write(c, io);
							lib3ds_io_write_vector(io, viewport.default_position);
							lib3ds_io_write_float(io, viewport.default_width);
						}
						break;
					case Lib3dsViewType.LIB3DS_VIEW_BACK:
						{
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_VIEW_BACK;
							c.size=22;
							lib3ds_chunk_write(c, io);
							lib3ds_io_write_vector(io, viewport.default_position);
							lib3ds_io_write_float(io, viewport.default_width);
						}
						break;
					case Lib3dsViewType.LIB3DS_VIEW_USER:
						{
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_VIEW_USER;
							c.size=34;
							lib3ds_chunk_write(c, io);
							lib3ds_io_write_vector(io, viewport.default_position);
							lib3ds_io_write_float(io, viewport.default_width);
							lib3ds_io_write_float(io, viewport.default_horiz_angle);
							lib3ds_io_write_float(io, viewport.default_vert_angle);
							lib3ds_io_write_float(io, viewport.default_roll_angle);
						}
						break;
					case Lib3dsViewType.LIB3DS_VIEW_CAMERA:
						{
							Lib3dsChunk c=new Lib3dsChunk();
							c.chunk=Lib3dsChunks.CHK_VIEW_CAMERA;
							c.size=17;
							lib3ds_chunk_write(c, io);
							lib3ds_io_write(io, viewport.default_camera, 11);
						}
						break;
				}

				lib3ds_chunk_write_end(c_default_view, io);
			}
		}
	}
}
