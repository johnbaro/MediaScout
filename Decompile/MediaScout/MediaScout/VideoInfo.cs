using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace MediaScout
{
	public static class VideoInfo
	{
		[System.Runtime.InteropServices.Guid("0579154A-2B53-4994-B0D0-E773148EFF85"), System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), System.Security.SuppressUnmanagedCodeSecurity]
		[System.Runtime.InteropServices.ComImport]
		public interface ISampleGrabberCB
		{
			[System.Runtime.InteropServices.PreserveSig]
			int SampleCB(double SampleTime, VideoInfo.IMediaSample pSample);

			[System.Runtime.InteropServices.PreserveSig]
			int BufferCB(double SampleTime, System.IntPtr pBuffer, int BufferLen);
		}

		[System.Runtime.InteropServices.Guid("56a8689a-0ad4-11ce-b03a-0020af0ba770"), System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), System.Security.SuppressUnmanagedCodeSecurity]
		[System.Runtime.InteropServices.ComImport]
		public interface IMediaSample
		{
			[System.Runtime.InteropServices.PreserveSig]
			int GetPointer(out System.IntPtr ppBuffer);

			[System.Runtime.InteropServices.PreserveSig]
			int GetSize();

			[System.Runtime.InteropServices.PreserveSig]
			int GetTime(out long pTimeStart, out long pTimeEnd);

			[System.Runtime.InteropServices.PreserveSig]
			int SetTime([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] [System.Runtime.InteropServices.In] VideoInfo.DsLong pTimeStart, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] [System.Runtime.InteropServices.In] VideoInfo.DsLong pTimeEnd);

			[System.Runtime.InteropServices.PreserveSig]
			int IsSyncPoint();

			[System.Runtime.InteropServices.PreserveSig]
			int SetSyncPoint([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)] [System.Runtime.InteropServices.In] bool bIsSyncPoint);

			[System.Runtime.InteropServices.PreserveSig]
			int IsPreroll();

			[System.Runtime.InteropServices.PreserveSig]
			int SetPreroll([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)] [System.Runtime.InteropServices.In] bool bIsPreroll);

			[System.Runtime.InteropServices.PreserveSig]
			int GetActualDataLength();

			[System.Runtime.InteropServices.PreserveSig]
			int SetActualDataLength([System.Runtime.InteropServices.In] int len);

			[System.Runtime.InteropServices.PreserveSig]
			int GetMediaType([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] out VideoInfo.AMMediaType ppMediaType);

			[System.Runtime.InteropServices.PreserveSig]
			int SetMediaType([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] [System.Runtime.InteropServices.In] VideoInfo.AMMediaType pMediaType);

			[System.Runtime.InteropServices.PreserveSig]
			int IsDiscontinuity();

			[System.Runtime.InteropServices.PreserveSig]
			int SetDiscontinuity([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)] [System.Runtime.InteropServices.In] bool bDiscontinuity);

			[System.Runtime.InteropServices.PreserveSig]
			int GetMediaTime(out long pTimeStart, out long pTimeEnd);

			[System.Runtime.InteropServices.PreserveSig]
			int SetMediaTime([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] [System.Runtime.InteropServices.In] VideoInfo.DsLong pTimeStart, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] [System.Runtime.InteropServices.In] VideoInfo.DsLong pTimeEnd);
		}

		[System.Runtime.InteropServices.Guid("6B652FFF-11FE-4fce-92AD-0266B5D7C78F"), System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), System.Security.SuppressUnmanagedCodeSecurity]
		[System.Runtime.InteropServices.ComImport]
		public interface ISampleGrabber
		{
			[System.Runtime.InteropServices.PreserveSig]
			int SetOneShot([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)] [System.Runtime.InteropServices.In] bool OneShot);

			[System.Runtime.InteropServices.PreserveSig]
			int SetMediaType([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] [System.Runtime.InteropServices.In] VideoInfo.AMMediaType pmt);

			[System.Runtime.InteropServices.PreserveSig]
			int GetConnectedMediaType([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] [System.Runtime.InteropServices.Out] VideoInfo.AMMediaType pmt);

			[System.Runtime.InteropServices.PreserveSig]
			int SetBufferSamples([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)] [System.Runtime.InteropServices.In] bool BufferThem);

			[System.Runtime.InteropServices.PreserveSig]
			int GetCurrentBuffer(ref int pBufferSize, System.IntPtr pBuffer);

			[System.Runtime.InteropServices.PreserveSig]
			int GetCurrentSample(out VideoInfo.IMediaSample ppSample);

			[System.Runtime.InteropServices.PreserveSig]
			int SetCallback(VideoInfo.ISampleGrabberCB pCallback, int WhichMethodToCallback);
		}

		[System.Runtime.InteropServices.Guid("65BD0710-24D2-4ff7-9324-ED2E5D3ABAFA"), System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), System.Security.SuppressUnmanagedCodeSecurity]
		[System.Runtime.InteropServices.ComImport]
		public interface IMediaDet
		{
			[System.Runtime.InteropServices.PreserveSig]
			int get_Filter([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] out object pVal);

			[System.Runtime.InteropServices.PreserveSig]
			int put_Filter([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] object newVal);

			[System.Runtime.InteropServices.PreserveSig]
			int get_OutputStreams(out int pVal);

			[System.Runtime.InteropServices.PreserveSig]
			int get_CurrentStream(out int pVal);

			[System.Runtime.InteropServices.PreserveSig]
			int put_CurrentStream(int newVal);

			[System.Runtime.InteropServices.PreserveSig]
			int get_StreamType(out System.Guid pVal);

			[System.Runtime.InteropServices.PreserveSig]
			int get_StreamTypeB([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.BStr)] out string pVal);

			[System.Runtime.InteropServices.PreserveSig]
			int get_StreamLength(out double pVal);

			[System.Runtime.InteropServices.PreserveSig]
			int get_Filename([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.BStr)] out string pVal);

			[System.Runtime.InteropServices.PreserveSig]
			int put_Filename([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.BStr)] string newVal);

			[System.Runtime.InteropServices.PreserveSig]
			int GetBitmapBits(double StreamTime, out int pBufferSize, [System.Runtime.InteropServices.In] System.IntPtr pBuffer, int Width, int Height);

			[System.Runtime.InteropServices.PreserveSig]
			int WriteBitmapBits(double StreamTime, int Width, int Height, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.BStr)] [System.Runtime.InteropServices.In] string Filename);

			[System.Runtime.InteropServices.PreserveSig]
			int get_StreamMediaType([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] [System.Runtime.InteropServices.Out] VideoInfo.AMMediaType pVal);

			[System.Runtime.InteropServices.PreserveSig]
			int GetSampleGrabber(out VideoInfo.ISampleGrabber ppVal);

			[System.Runtime.InteropServices.PreserveSig]
			int get_FrameRate(out double pVal);

			[System.Runtime.InteropServices.PreserveSig]
			int EnterBitmapGrabMode(double SeekTime);
		}

		[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
		public class AMMediaType
		{
			public System.Guid majorType;

			public System.Guid subType;

			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
			public bool fixedSizeSamples;

			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
			public bool temporalCompression;

			public int sampleSize;

			public System.Guid formatType;

			public System.IntPtr unkPtr;

			public int formatSize;

			public System.IntPtr formatPtr;
		}

		[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
		public class DsRect
		{
			public int left;

			public int top;

			public int right;

			public int bottom;

			public DsRect()
			{
				this.left = 0;
				this.top = 0;
				this.right = 0;
				this.bottom = 0;
			}

			public DsRect(int left, int top, int right, int bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}

			public DsRect(Rectangle rectangle)
			{
				this.left = rectangle.Left;
				this.top = rectangle.Top;
				this.right = rectangle.Right;
				this.bottom = rectangle.Bottom;
			}

			public override string ToString()
			{
				return string.Format("[{0}, {1} - {2}, {3}]", new object[]
				{
					this.left,
					this.top,
					this.right,
					this.bottom
				});
			}

			public override int GetHashCode()
			{
				return this.left.GetHashCode() | this.top.GetHashCode() | this.right.GetHashCode() | this.bottom.GetHashCode();
			}

			public static implicit operator Rectangle(VideoInfo.DsRect r)
			{
				return r.ToRectangle();
			}

			public static implicit operator VideoInfo.DsRect(Rectangle r)
			{
				return new VideoInfo.DsRect(r);
			}

			public Rectangle ToRectangle()
			{
				return new Rectangle(this.left, this.top, this.right - this.left, this.bottom - this.top);
			}

			public static VideoInfo.DsRect FromRectangle(Rectangle r)
			{
				return new VideoInfo.DsRect(r);
			}
		}

		[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential, Pack = 2)]
		public class BitmapInfoHeader
		{
			public int Size;

			public int Width;

			public int Height;

			public short Planes;

			public short BitCount;

			public int Compression;

			public int ImageSize;

			public int XPelsPerMeter;

			public int YPelsPerMeter;

			public int ClrUsed;

			public int ClrImportant;
		}

		[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
		public class VideoInfoHeader
		{
			public VideoInfo.DsRect SrcRect;

			public VideoInfo.DsRect TargetRect;

			public int BitRate;

			public int BitErrorRate;

			public long AvgTimePerFrame;

			public VideoInfo.BitmapInfoHeader BmiHeader;
		}

		[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
		public class DsLong
		{
			private long Value;

			public DsLong(long Value)
			{
				this.Value = Value;
			}

			public override string ToString()
			{
				return this.Value.ToString();
			}

			public override int GetHashCode()
			{
				return this.Value.GetHashCode();
			}

			public static implicit operator long(VideoInfo.DsLong l)
			{
				return l.Value;
			}

			public static implicit operator VideoInfo.DsLong(long l)
			{
				return new VideoInfo.DsLong(l);
			}

			public long ToInt64()
			{
				return this.Value;
			}

			public static VideoInfo.DsLong FromInt64(long l)
			{
				return new VideoInfo.DsLong(l);
			}
		}

		[System.Runtime.InteropServices.Guid("65BD0711-24D2-4ff7-9324-ED2E5D3ABAFA")]
		[System.Runtime.InteropServices.ComImport]
		public class MediaDet
		{
			//[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
			//public extern MediaDet();
		}

		private static System.Guid videoType = new System.Guid("73646976-0000-0010-8000-00AA00389B71");

		public static bool SaveThumb(string videoFilename, string thumbFilename, double positionPercent)
		{
			bool result = false;
			VideoInfo.IMediaDet mediaDet = new VideoInfo.MediaDet() as VideoInfo.IMediaDet;
			mediaDet.put_Filename(videoFilename);
			int num;
			int arg_1D_0 = mediaDet.get_OutputStreams(out num);
			VideoInfo.AMMediaType aMMediaType = new VideoInfo.AMMediaType();
			for (int i = 0; i < num; i++)
			{
				int arg_33_0 = mediaDet.get_StreamMediaType(aMMediaType);
				VideoInfo.VideoInfoHeader videoInfoHeader = (VideoInfo.VideoInfoHeader)System.Runtime.InteropServices.Marshal.PtrToStructure(aMMediaType.formatPtr, typeof(VideoInfo.VideoInfoHeader));
				if (videoInfoHeader != null)
				{
					double num2;
					int arg_5F_0 = mediaDet.get_StreamLength(out num2);
					num2 = (double)((int)(num2 * positionPercent));
					int width = videoInfoHeader.BmiHeader.Width;
					int height = videoInfoHeader.BmiHeader.Height;
					if (height >= 10 && width >= 10)
					{
						string text = System.IO.Path.GetTempFileName() + ".bmp";
						mediaDet.WriteBitmapBits(num2, width, height, text);
						if (System.IO.File.Exists(text))
						{
							using (Bitmap bitmap = new Bitmap(text))
							{
								bitmap.Save(thumbFilename, ImageFormat.Jpeg);
							}
							System.IO.File.Delete(text);
							result = true;
							break;
						}
						break;
					}
				}
			}
			System.Runtime.InteropServices.Marshal.ReleaseComObject(mediaDet);
			return result;
		}

		public static double GetRuntime(string videoFilename)
		{
			double result = 0.0;
			VideoInfo.IMediaDet mediaDet = new VideoInfo.MediaDet() as VideoInfo.IMediaDet;
			mediaDet.put_Filename(videoFilename);
			int num;
			int arg_25_0 = mediaDet.get_OutputStreams(out num);
			VideoInfo.AMMediaType aMMediaType = new VideoInfo.AMMediaType();
			for (int i = 0; i < num; i++)
			{
				int arg_38_0 = mediaDet.get_StreamMediaType(aMMediaType);
				VideoInfo.VideoInfoHeader videoInfoHeader = (VideoInfo.VideoInfoHeader)System.Runtime.InteropServices.Marshal.PtrToStructure(aMMediaType.formatPtr, typeof(VideoInfo.VideoInfoHeader));
				if (videoInfoHeader != null)
				{
					int arg_61_0 = mediaDet.get_StreamLength(out result);
				}
			}
			return result;
		}
	}
}
