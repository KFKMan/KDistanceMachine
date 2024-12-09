using OpenCvSharp;
using System;
using System.Linq;

public struct HorizontalLine(int Left,int Right,int y)
{
	public int LeftX = Left; //Sol
	public int RightX = Right; //Sağ

	public int Y = y;
}

public class KFinder
{
	public static double CalculateWidthAsCM(AppData data,int LeftX, int RightX)
	{
		var DistanceCM = data.DistanceData.Data.Centimeters;

		//Console.WriteLine($"Used Distance Value {DistanceCM.ToString()}");

		var AnglePerPixel = data.BaseData.HorizontalFOV / data.BaseData.QualityWidth;

		var MiddleX = data.BaseData.QualityWidth / 2; //750

		var LeftPixel = Math.Abs(MiddleX - LeftX); //Sağ
		var LeftAngle = AnglePerPixel * LeftPixel;

		var LeftDistance = Math.Tan(LeftAngle) * DistanceCM;


		var RigthPixel = Math.Abs(MiddleX - RightX); //Sol
		var RigthAngle = AnglePerPixel * RigthPixel;

		var RigthDistance = Math.Tan(RigthAngle) * DistanceCM;

		return LeftDistance + RigthDistance;
		
	}

	/*
	public static List<HorizontalLine> FindLines(Mat original)
	{
		// Görüntüyü gri tonlamalı hale getir
		Mat img = new();
		Cv2.CvtColor(original, img, ColorConversionCodes.BGR2GRAY);
		if (img.Empty())
		{
			throw new ArgumentException($"Image is Empty");
		}

		// Gürültüyü azaltmak için Gauss bulanıklaştırma uygulama
		Mat blurred = new Mat();
		Cv2.GaussianBlur(img, blurred, new Size(5, 5), 0); //5, 5

		// Kenar algılama (Canny)
		Mat edges = new Mat();
		Cv2.Canny(blurred, edges, 5, 100); //Min, Max 50, 100

		int heights = edges.Rows;
		int widths = edges.Cols;

		var lines = new List<HorizontalLine>();

		for (int y = 0; y < heights; y++)
		{
			var row = edges.Row(y);
			byte[] rowData = new byte[widths];

			if (row.GetArray<byte>(out rowData))
			{
				int maxLength = 0;
				int currentLength = 0;
				int startX = -1;
				int maxStartX = -1;

				for (int x = 0; x < widths; x++)
				{
					if (rowData[x] == 255)
					{
						if (startX == -1)
						{
							startX = x;
						}
						currentLength++;
					}
					else
					{
						if (currentLength > maxLength)
						{
							maxLength = currentLength;
							maxStartX = startX;
						}
						currentLength = 0;
						startX = -1;
					}
				}

				if (currentLength > maxLength)
				{
					maxLength = currentLength;
					maxStartX = startX;
				}

				if (maxStartX != -1)
				{
					lines.Add(new HorizontalLine(maxStartX, maxStartX + maxLength - 1, y));
				}
			}
		}

		return lines;
	}
	*/

	/*
	public static List<VerticalLine> FindVerticalLines(Mat original)
	{
		// Görüntüyü gri tonlamalı hale getir
		Mat img = new();
		Cv2.CvtColor(original, img, ColorConversionCodes.BGR2GRAY);
		if (img.Empty())
		{
			throw new ArgumentException($"Image is Empty");
		}

		// Kontrast ve parlaklık ayarlaması
		Mat adjusted = new Mat();
		img.ConvertTo(adjusted, -1, 1.2, 50); // Kontrast (1.2) ve parlaklık (+50) ayarları

		// Histogram eşitleme
		Mat equalized = new Mat();
		Cv2.EqualizeHist(adjusted, equalized);

		// Gürültüyü azaltmak için Gauss bulanıklaştırma uygulama
		Mat blurred = new Mat();
		Cv2.GaussianBlur(equalized, blurred, new Size(5, 5), 0);

		// Kenar algılama (Canny)
		Mat edges = new Mat();
		Cv2.Canny(blurred, edges, 30, 90); // Eşik değerlerini ayarlayın (örneğin, 30, 90)

		// Morfolojik işlemler (örneğin, genişletme ve erozyon)
		Mat dilated = new Mat();
		Mat eroded = new Mat();
		Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
		Cv2.Dilate(edges, dilated, kernel);
		Cv2.Erode(dilated, eroded, kernel);

		// Hough Line Transform kullanarak çizgi tespiti
		LineSegmentPoint[] linesP = Cv2.HoughLinesP(eroded, 1, Cv2.PI / 180, 50, 50, 10); // Başlangıç değerleri

		var verticalLines = new List<VerticalLine>();

		foreach (var line in linesP)
		{
			if (Math.Abs(line.P1.X - line.P2.X) < 5) // Dikey çizgileri seçin, daha hassas
			{
				verticalLines.Add(new VerticalLine(line.P1.Y, line.P2.Y, line.P1.X));
			}
		}

		return verticalLines;
	} */

	public static void FindLinesWithCountors(Mat src)
	{
		Mat gray = new Mat();
		Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

		// Gürültüyü azaltmak için bulanıklaştırma
		Mat blurred = new Mat();
		Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 0);

		// Kenarlık tespiti
		Mat edges = new Mat();
		Cv2.Canny(blurred, edges, 10, 50);

		// Konturları bul
		Point[][] contours;
		HierarchyIndex[] hierarchy;
		Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);

		// En büyük konturu seç
		Mat contourImage = src.Clone();
		for (int i = 0; i < contours.Length; i++)
		{
			Cv2.DrawContours(contourImage, contours, i, Scalar.Blue, 2);
		}

		// En büyük konturu seç ve farklı renkte çiz
		Point[] largestContour = null;
		double maxArea = 0;
		foreach (var contour in contours)
		{
			double area = Cv2.ContourArea(contour);
			if (area > maxArea)
			{
				maxArea = area;
				largestContour = contour;
			}
		}

		if (largestContour != null)
		{
			// En sol ve en sağ noktaları bul
			Point leftMost = largestContour[0];
			Point rightMost = largestContour[0];
			foreach (var point in largestContour)
			{
				if (point.X < leftMost.X) leftMost = point;
				if (point.X > rightMost.X) rightMost = point;
			}

			// En büyük konturu kırmızı renkte çiz
			Cv2.DrawContours(contourImage, new Point[][] { largestContour }, -1, Scalar.Red, 2);

			// Genişliği hesapla
			double width = rightMost.X - leftMost.X;
			Console.WriteLine($"Kumaş eni: {width} piksel");

			
		}
		else
		{
			Console.WriteLine("Kumaş bulunamadı.");
		}

		// Konturları gösteren görüntüyü kaydet
		Cv2.ImWrite($"WWW{Guid.NewGuid().ToString()}.jpg", contourImage);

		/*
		// Görüntüyü göster
		Cv2.ImShow("Konturlar", contourImage);
		Cv2.WaitKey(0); */
	}

	public static List<VerticalLine> FindVerticalLines(Mat original)
	{
		// Görüntüyü gri tonlamalı hale getir
		Mat img = new();
		Cv2.CvtColor(original, img, ColorConversionCodes.BGR2GRAY);
		if (img.Empty())
		{
			throw new ArgumentException($"Image is Empty");
		}

		// Kontrast ve parlaklık ayarlaması
		Mat adjusted = new Mat();
		img.ConvertTo(adjusted, -1, 1.1, 10); // Kontrast (1.5) ve parlaklık (+30) ayarları

		// Histogram eşitleme
		//Mat equalized = new Mat();
		//Cv2.EqualizeHist(adjusted, equalized);

		// Gürültüyü azaltmak için Gauss bulanıklaştırma uygulama
		Mat blurred = new Mat();
		Cv2.GaussianBlur(adjusted, blurred, new Size(5, 5), 0);

		// Kenar algılama (Canny)
		Mat edges = new Mat();
		Cv2.Canny(blurred, edges, 10, 50); // Eşik değerlerini ayarlayın (örneğin, 50, 150)

		// Morfolojik işlemler (örneğin, genişletme ve erozyon)
		Mat dilated = new Mat();
		Mat eroded = new Mat();
		Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
		Cv2.Dilate(edges, dilated, kernel);
		Cv2.Erode(dilated, eroded, kernel);

		eroded.SaveImage($"XXX{Guid.NewGuid().ToString()}.jpg");

		// Hough Line Transform kullanarak çizgi tespiti
		LineSegmentPoint[] linesP = Cv2.HoughLinesP(eroded, 1, Cv2.PI / 5, 3, 500, 60); // Eşik değerlerini ayarlayın (örneğin, threshold: 100, minLineLength: 500, maxLineGap: 10)
		//1, 5, 3, 500, 60


		var verticalLines = new List<VerticalLine>();

		foreach (var line in linesP)
		{
			if (Math.Abs(line.P1.X - line.P2.X) < 40) // Dikey çizgileri seçin ve 5-10 piksel kaymayı tolere edin
			{
				verticalLines.Add(new VerticalLine(line.P1.Y, line.P2.Y, (line.P1.X + line.P2.X) / 2));
			}
		}


		Mat mt = new();

		Cv2.Canny(eroded, mt, 20, 100);

		int heights = mt.Rows;
		int widths = mt.Cols;

		var lines = new List<HorizontalLine>();

		for (int y = 0; y < heights; y++)
		{
			var row = mt.Row(y);
			byte[] rowData = new byte[widths];
			//row.GetArray(0, 0, rowData);

			if (row.GetArray<byte>(out rowData))
			{
				int leftEdge = Array.IndexOf(rowData, (byte)255);
				int rightEdge = Array.LastIndexOf(rowData, (byte)255);

				if (leftEdge != -1 && rightEdge != -1 && rightEdge > leftEdge)
				{
					mt.Line(new(leftEdge, y), new(rightEdge, y), Scalar.Red, 1);
					//lines.Add(new(leftEdge, rightEdge, y));
				}
			}


		}

		mt.SaveImage($"YYY{Guid.NewGuid().ToString()}.jpg");
		

		return verticalLines;
	}

	// Dikey çizgiyi temsil eden sınıf
	public class VerticalLine
	{
		public int StartY { get; }
		public int EndY { get; }
		public int X { get; }

		public VerticalLine(int startY, int endY, int x)
		{
			StartY = startY;
			EndY = endY;
			X = x;
		}
	}

	public static List<HorizontalLine> FindLines(Mat original)
	{
		// Görüntüyü gri tonlamalı hale getir
		Mat img = new();
		Cv2.CvtColor(original, img, ColorConversionCodes.BGR2GRAY);
		if (img.Empty())
		{
			throw new ArgumentException($"Image is Empty");
		}

		// Kontrast ve parlaklık ayarlaması
		Mat adjusted = new Mat();
		img.ConvertTo(adjusted, -1, 1.2, 50); // Kontrast (1.2) ve parlaklık (+50) ayarları

		// Histogram eşitleme
		Mat equalized = new Mat();
		Cv2.EqualizeHist(adjusted, equalized);

		// Gürültüyü azaltmak için Gauss bulanıklaştırma uygulama
		Mat blurred = new Mat();
		Cv2.GaussianBlur(equalized, blurred, new Size(5, 5), 0);

		// Kenar algılama (Canny)
		Mat edges = new Mat();
		Cv2.Canny(blurred, edges, 30, 90); // Eşik değerlerini ayarlayın (örneğin, 30, 90)

		// Morfolojik işlemler (örneğin, genişletme ve erozyon)
		Mat dilated = new Mat();
		Mat eroded = new Mat();
		Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
		Cv2.Dilate(edges, dilated, kernel);
		Cv2.Erode(dilated, eroded, kernel);

		// Hough Line Transform kullanarak çizgi tespiti
		LineSegmentPoint[] linesP = Cv2.HoughLinesP(eroded, 1, Cv2.PI / 180, 50, 50, 10); // Başlangıç değerleri

		var horizontalLines = new List<HorizontalLine>();

		foreach (var line in linesP)
		{
			if (Math.Abs(line.P1.Y - line.P2.Y) < 5) // Yatay çizgileri seçin, daha hassas
			{
				horizontalLines.Add(new HorizontalLine(line.P1.X, line.P2.X, line.P1.Y));
			}
		}

		return horizontalLines;
	}


	/*
	public static List<HorizontalLine> FindLines(Mat original)
	{
		// Görüntüyü gri tonlamalı hale getir
		Mat img = new();
		Cv2.CvtColor(original, img, ColorConversionCodes.BGR2GRAY);
		if (img.Empty())
		{
			throw new ArgumentException($"Image is Empty");
		}

		// Gürültüyü azaltmak için Gauss bulanıklaştırma uygulama
		Mat blurred = new Mat();
		Cv2.GaussianBlur(img, blurred, new Size(5, 5), 0); //5, 5

		// Kenar algılama (Canny)
		Mat edges = new Mat();
		Cv2.Canny(blurred, edges, 30, 90); // Eşik değerlerini ayarlayın (örneğin, 50, 150)

		// Morfolojik işlemler (örneğin, genişletme ve erozyon)
		Mat dilated = new Mat();
		Mat eroded = new Mat();
		Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
		Cv2.Dilate(edges, dilated, kernel);
		Cv2.Erode(dilated, eroded, kernel);

		// Hough Line Transform kullanarak çizgi tespiti
		LineSegmentPoint[] linesP = Cv2.HoughLinesP(eroded, 1, Cv2.PI / 180, 30, 30, 10); // Parametreleri ayarlayın

		var horizontalLines = new List<HorizontalLine>();

		foreach (var line in linesP)
		{
			if (Math.Abs(line.P1.Y - line.P2.Y) < 10) // Yatay çizgileri seçin
			{
				horizontalLines.Add(new HorizontalLine(line.P1.X, line.P2.X, line.P1.Y));
			}
		}

		return horizontalLines;
	}

	/*
	public static List<HorizontalLine> FindLines(Mat original)
	{
		// Görüntüyü yükle ve gri tonlamalı hale getir
		//Mat img = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
		Mat img = new();
		Cv2.CvtColor(original, img, ColorConversionCodes.BGR2GRAY);
		if (img.Empty())
		{
			throw new ArgumentException($"Image is Empty");
		}

		// Gürültüyü azaltmak için Gauss bulanıklaştırma uygulama
		Mat blurred = new Mat();
		Cv2.GaussianBlur(img, blurred, new Size(5, 5), 0);

		// Kenar algılama (Canny)
		Mat edges = new Mat();
		Cv2.Canny(blurred, edges, 50, 100); //Min, Max 50, 150

		int heights = edges.Rows;
		int widths = edges.Cols;

		var lines = new List<HorizontalLine>();

		for (int y = 0; y < heights; y++)
		{
			var row = edges.Row(y);
			byte[] rowData = new byte[widths];
			//row.GetArray(0, 0, rowData);

			if(row.GetArray<byte>(out rowData))
			{
				int leftEdge = Array.IndexOf(rowData, (byte)255);
				int rightEdge = Array.LastIndexOf(rowData, (byte)255);

				if (leftEdge != -1 && rightEdge != -1 && rightEdge > leftEdge)
				{
					lines.Add(new(leftEdge, rightEdge,y));
				}
			}

			
		}

		return lines;
	} */

	public static int M2P(Mat frame, out Mat outputFrame)
	{
		outputFrame = frame.Clone();

		// Görüntüyü gri tonlamaya dönüştür
		Mat gray = new Mat();
		Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

		// Gürültüyü azaltmak için görüntüyü bulanıklaştır
		Mat blurred = new Mat();
		Cv2.GaussianBlur(gray, blurred, new OpenCvSharp.Size(5, 5), 0);

		// Kenarları tespit et
		Mat edges = new Mat();
		Cv2.Canny(blurred, edges, 50, 150);

		// Kenarları kullanarak konturları bul
		OpenCvSharp.Point[][] contours;
		HierarchyIndex[] hierarchy;
		Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

		// En büyük konturu bul
		if (contours.Length > 0)
		{
			OpenCvSharp.Point[] maxContour = contours[0];
			double maxArea = Cv2.ContourArea(maxContour);

			foreach (var contour in contours)
			{
				double area = Cv2.ContourArea(contour);
				if (area > maxArea)
				{
					maxContour = contour;
					maxArea = area;
				}
			}

			// Konturun en sol ve en sağ noktalarını bul
			OpenCvSharp.Point leftmost = maxContour[0];
			OpenCvSharp.Point rightmost = maxContour[0];

			foreach (var point in maxContour)
			{
				if (point.X < leftmost.X)
					leftmost = point;
				if (point.X > rightmost.X)
					rightmost = point;
			}

			// Kumaşın enini ölç (piksel cinsinden)
			int fabricWidth = rightmost.X - leftmost.X;

			// Sonucu görselleştir
			Cv2.DrawContours(outputFrame, new[] { maxContour }, -1, OpenCvSharp.Scalar.Green, 2);
			Cv2.Circle(outputFrame, leftmost, 8, OpenCvSharp.Scalar.Red, -1);
			Cv2.Circle(outputFrame, rightmost, 8, OpenCvSharp.Scalar.Red, -1);
			Cv2.PutText(outputFrame, $"Width: {fabricWidth}px", new OpenCvSharp.Point(leftmost.X, leftmost.Y - 10), HersheyFonts.HersheySimplex, 0.9, OpenCvSharp.Scalar.Green, 2);

			return fabricWidth;
		}

		return 0;
	}
}

