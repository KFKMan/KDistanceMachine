using System.Drawing;
using System.Runtime.Versioning;

public static class YoticExtensions
{
	[SupportedOSPlatform("windows")]
	public static Bitmap BitmapFromFile(this string filename)
	{
		Bitmap bitmap;
		using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
		{
			bitmap = new Bitmap(fs);
		}
		return bitmap;
	}
}

public class YoticFinder
{
	[SupportedOSPlatform ("windows")]
	public static void SelectPoint(Bitmap src, System.Drawing.Point pt, Color color, uint W = 1)
	{
		int i = 0;
		while(W > i)
		{
			src.SetPixel(pt.X + i, pt.Y, color);
			src.SetPixel(pt.X, pt.Y + i, color);
			src.SetPixel(pt.X + i, pt.Y + i, color);

			i++;
		}
	}

	[SupportedOSPlatform("windows")]
	public static void SelectLine(Bitmap src, int X, Color color, uint W = 1)
	{
		for(int y= 0; y < src.Height; y++)
		{
			int i = 0;
			while (W > i)
			{
				src.SetPixel(X + i, y, color);
				src.SetPixel(X - i, y, color);

				i++;
			}
		}
	}

	[SupportedOSPlatform("windows")]
	public static Bitmap ProcessFabricDetailed(Bitmap src, int RESCALE_COEFFICIENT = 3, int brightnessThreshold = 30)
	{
		Bitmap bitmap = src;
		var bmp = new Bitmap(bitmap.Width / RESCALE_COEFFICIENT, bitmap.Height / RESCALE_COEFFICIENT);
		var g = Graphics.FromImage(bmp);
		g.DrawImage(bitmap, 0, 0, bmp.Width, bmp.Height);

		var img = YoticBase.GetImage(bmp);
		var diffImg = YoticBase.GetDiffImage(img);
		var grayImg = YoticBase.ToGrayMap(diffImg);
		var bright = YoticBase.Brightness(grayImg);
		var result = YoticBase.GetBitmap(bright);

		var sums = new float[result.Width];
		for (var x = 0; x < result.Width; x++)
		{
			for (var y = 0; y < result.Height; y++)
			{
				var pixel = result.GetPixel(x, y);
				sums[x] += pixel.R + pixel.G + pixel.B;
			}

			sums[x] /= byte.MaxValue * 2;

			for (var y = 0; y < 5; y++)
			{
				var pElement = sums[x] / result.Height * 10;
				result.SetPixel(x, result.Height - y - 1, new YoticBase.Pixel(pElement, pElement, pElement).ToColor());
			}
		}

		Random rnd = Random.Shared;

		Console.WriteLine($"Target {bitmap.Width}-{bitmap.Height}");
		foreach(var AX in sums.Select((v, i) => (v: (int)v, i))) //.Where(z => z.v >= brightnessThreshold)
		{
			if (AX.v >= brightnessThreshold)
			{
				int fY = 0;
				var pt = new Point(AX.i * RESCALE_COEFFICIENT, fY);
				Console.WriteLine(pt.ToString());
				SelectPoint(bitmap, pt, Color.Red, 3);
				//SelectLine(bitmap, AX.i * RESCALE_COEFFICIENT, Color.FromArgb(rnd.Next(0,255), rnd.Next(0,255), rnd.Next(0,255)), 3);
			}
		}

		/*
		var leftBound = sums.Select((v, i) => (v: (int)v, i)).First(z => z.v >= brightnessThreshold).i * RESCALE_COEFFICIENT;
		var rightBound = sums.Select((v, i) => (v: (int)v, i)).Last(z => z.v >= brightnessThreshold).i * RESCALE_COEFFICIENT;

		foreach (var x in (int[])[leftBound, rightBound])
		{
			for (var y = 0; y < bitmap.Height; y++)
			{
				bitmap.SetPixel(x, y, Color.Red);
			}
		}
		*/

		return bitmap;
	}

	[SupportedOSPlatform("windows")]
	public static Bitmap ProcessFabric(Bitmap src, int RESCALE_COEFFICIENT = 3)
	{
		Bitmap bitmap = src;
		var bmp = new Bitmap(bitmap.Width / RESCALE_COEFFICIENT, bitmap.Height / RESCALE_COEFFICIENT);
		var g = Graphics.FromImage(bmp);
		g.DrawImage(bitmap, 0, 0, bmp.Width, bmp.Height);

		var img = YoticBase.GetImage(bmp);
		var diffImg = YoticBase.GetDiffImage(img);
		var grayImg = YoticBase.ToGrayMap(diffImg);
		var bright = YoticBase.Brightness(grayImg);
		var result = YoticBase.GetBitmap(bright);

		var sums = new float[result.Width];
		for (var x = 0; x < result.Width; x++)
		{
			for (var y = 0; y < result.Height; y++)
			{
				var pixel = result.GetPixel(x, y);
				sums[x] += pixel.R + pixel.G + pixel.B;
			}

			sums[x] /= byte.MaxValue * 2;

			for (var y = 0; y < 5; y++)
			{
				var pElement = sums[x] / result.Height * 10;
				result.SetPixel(x, result.Height - y - 1, new YoticBase.Pixel(pElement, pElement, pElement).ToColor());
			}
		}

		var leftBound = sums.Select((v, i) => (v: (int)v, i)).First(z => z.v >= 30).i * RESCALE_COEFFICIENT;
		var rightBound = sums.Select((v, i) => (v: (int)v, i)).Last(z => z.v >= 30).i * RESCALE_COEFFICIENT;

		foreach (var x in (int[])[leftBound, rightBound])
		{
			for (var y = 0; y < bitmap.Height; y++)
			{
				bitmap.SetPixel(x, y, Color.Red);
			}
		}

		return bitmap;
	}

	
}

public class YoticBase
{
	public static Pixel[,] GetImage(Bitmap bmp)
	{
		var image = new Pixel[bmp.Width, bmp.Height];
		for (var y = 0; y < bmp.Height; y++)
		{
			for (var x = 0; x < bmp.Width; x++)
			{
				image[x, y] = Pixel.FromColor(bmp.GetPixel(x, y));
			}
		}
		return image;
	}

	public static Pixel[,] GetDiffImage(Pixel[,] image)
	{
		var diffImage = new Pixel[image.GetLength(0) - 1, image.GetLength(1) - 1];
		for (var y = 0; y < diffImage.GetLength(1); y++)
		{
			for (var x = 0; x < diffImage.GetLength(0); x++)
			{
				var pixel = image[x, y];
				var diff = new Pixel();

				foreach (var (dx, dy) in ((int, int)[])[/*(-1, -1), (0, -1), (1, -1), */(-1, 0), (1, 0)/*, (-1, 1), (0, 1), (1, 1)*/])
				{
					diff += GetPixelDiff(x + dx, y + dy);
				}

				diffImage[x, y] = diff / 2;

				Pixel GetPixelDiff(int dx, int dy)
				{
					if (dx == -1 || dy == -1)
					{
						return new(0, 0, 0);
					}

					var diffPixel = image[dx, dy];
					return new(Math.Abs(diffPixel.R - pixel.R), Math.Abs(diffPixel.G - pixel.G), Math.Abs(diffPixel.B - pixel.B));
				}
			}
		}
		return diffImage;
	}

	public static Pixel[,] ToGrayMap(Pixel[,] image)
	{
		var result = new Pixel[image.GetLength(0), image.GetLength(1)];
		for (var y = 0; y < image.GetLength(1); y++)
		{
			for (var x = 0; x < image.GetLength(0); x++)
			{
				var pixel = image[x, y];
				var sum = (pixel.R + pixel.G + pixel.B) / 3;
				result[x, y] = new(sum, sum, sum);
			}
		}
		return result;
	}

	public static Pixel[,] Brightness(Pixel[,] image)
	{
		var result = new Pixel[image.GetLength(0), image.GetLength(1)];
		for (var y = 0; y < image.GetLength(1); y++)
		{
			for (var x = 0; x < image.GetLength(0); x++)
			{
				result[x, y] = image[x, y] * 3;
			}
		}
		return result;
	}

	public static Bitmap GetBitmap(Pixel[,] image)
	{
		var bitmap = new Bitmap(image.GetLength(0), image.GetLength(1));
		for (var y = 0; y < bitmap.Height; y++)
		{
			for (var x = 0; x < bitmap.Width; x++)
			{
				bitmap.SetPixel(x, y, image[x, y].ToColor());
			}
		}
			
		return bitmap;
	}

	public struct Pixel
	{
		public Pixel() { }

		public Pixel(float r, float g, float b)
		{
			R = r;
			G = g;
			B = b;
		}

		public float R, G, B;

		public Color ToColor() => Color.FromArgb((byte)(Math.Clamp(R, 0, 1) * byte.MaxValue), (byte)(Math.Clamp(G, 0, 1) * byte.MaxValue), (byte)(Math.Clamp(B, 0, 1) * byte.MaxValue));

		public static Pixel FromColor(Color color) => new((float)color.R / byte.MaxValue, (float)color.G / byte.MaxValue, (float)color.B / byte.MaxValue);

		public static Pixel operator +(Pixel left, Pixel right) => new(left.R + right.R, left.G + right.G, left.B + right.B);
		public static Pixel operator /(Pixel left, int value) => new(left.R / value, left.G / value, left.B / value);
		public static Pixel operator *(Pixel left, int value) => new(left.R * value, left.G * value, left.B * value);
	}
}

