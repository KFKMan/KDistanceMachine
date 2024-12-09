using Iot.Device.Display;
using Iot.Device.Graphics;
using OpenCvSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

public class ImageHelper
{
	public static Mat ColorsToMat(Color[] colors, System.Drawing.Size captureSize)
	{
		int width = captureSize.Width;
		int height = captureSize.Height;

		// Create a byte array to hold BGR values
		byte[] bgrData = new byte[width * height * 3];

		for (int i = 0; i < colors.Length; i++)
		{
			int offset = i * 3;
			bgrData[offset] = colors[i].B;
			bgrData[offset + 1] = colors[i].G;
			bgrData[offset + 2] = colors[i].R;
		}

		// Create a Mat with the appropriate size and type
		Mat mat = new Mat(height, width, MatType.CV_8UC3);

		// Set the byte array data to the Mat
		//mat.SetArray(0, 0, bgrData);
		Marshal.Copy(bgrData, 0, mat.Data, bgrData.Length);

		return mat;
	}

	public static Mat Nv12ToMat(MemoryStream ms, OpenCvSharp.Size captureSize)
	{
		// Read NV12 byte array from memory stream
		byte[] nv12Bytes = ms.ToArray();

		// Create a Mat for the YUV data
		Mat yuv = new Mat(captureSize,MatType.CV_8UC1);

		// Convert YUV to BGR format
		Mat bgr = new Mat();
		Cv2.CvtColor(yuv, bgr, ColorConversionCodes.YUV2BGR_NV12);

		return bgr;
	}

	public static Mat ConvertBitmapImageToMat(BitmapImage bitmapImage)
	{
		int width = bitmapImage.Width;
		int height = bitmapImage.Height;
		Mat mat = new Mat(height, width, MatType.CV_8UC4);

		// Piksel verilerini BitmapImage'den Mat'e kopyala
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				var color = bitmapImage.GetPixel(x, y);
				mat.Set(y, x, new Vec4b(color.B, color.G, color.R, color.A));
			}
		}

		// ARGB -> BGR dönüşümü
		Mat matBgr = new Mat();
		Cv2.CvtColor(mat, matBgr, ColorConversionCodes.RGBA2BGR); // RGBA ve ARGB aynı veri düzenine sahiptir

		return matBgr;
	}
}