using OpenCvSharp;
using System.Threading;

public class KBase
{
	public static Mat ApplyMedianBlur(Mat src, int kernelSize = 5)
	{
		Mat blurred = new Mat();
		Cv2.MedianBlur(src, blurred, kernelSize);
		return blurred;
	}

	public static Mat ApplyGusianBlur(Mat src, int kernelSize = 5)
	{
		Mat blurred = new Mat();
		Cv2.GaussianBlur(src, blurred, new Size(kernelSize,kernelSize), 0);
		return blurred;
	}

	public static Mat DetectEdges(Mat src, double threshold1 = 5, double threshold2 = 100)
	{
		Mat edges = new Mat();
		Cv2.Canny(src, edges, threshold1, threshold2);
		return edges;
	}

	public static Mat DetectSobelEdges(Mat src)
	{
		Mat gradX = new Mat(), gradY = new Mat();
		Cv2.Sobel(src, gradX, MatType.CV_16S, 1, 0);
		Cv2.Sobel(src, gradY, MatType.CV_16S, 0, 1);
		Mat absGradX = new Mat(), absGradY = new Mat();
		Cv2.ConvertScaleAbs(gradX, absGradX);
		Cv2.ConvertScaleAbs(gradY, absGradY);
		Mat edges = new Mat();
		Cv2.AddWeighted(absGradX, 0.5, absGradY, 0.5, 0, edges);
		return edges;
	}

	public static Mat DetectLaplacianEdges(Mat src)
	{
		Mat edges = new Mat();
		Cv2.Laplacian(src, edges, MatType.CV_16S, ksize: 3);
		Cv2.ConvertScaleAbs(edges, edges);
		return edges;
	}


	public static Mat AdjustBrightnessContrast(Mat src, double alpha = 1.1, int beta = 10)
	{
		Mat adjusted = new Mat();
		src.ConvertTo(adjusted, MatType.CV_8UC3, alpha, beta);
		return adjusted;
	}

	public static List<(int y, int left, int right)> GetFabricEdges(Mat edges)
	{
		List<(int y, int left, int right)> fabricEdges = new List<(int y, int left, int right)>();

		for (int y = 0; y < edges.Rows; y++)
		{
			int left = -1, right = -1;
			for (int x = 0; x < edges.Cols; x++)
			{
				if (edges.At<byte>(y, x) > 0)
				{
					if (left == -1) left = x;
					right = x;
				}
			}
			if (left != -1 && right != -1)
			{
				fabricEdges.Add((y, left, right));
			}
		}

		return fabricEdges;
	}

	public static List<(int y, int widthChange)> CalculateWidthChanges(List<(int y, int left, int right)> fabricEdges)
	{
		List<(int y, int widthChange)> widthChanges = new List<(int y, int widthChange)>();

		for (int i = 1; i < fabricEdges.Count; i++)
		{
			int previousWidth = fabricEdges[i - 1].right - fabricEdges[i - 1].left;
			int currentWidth = fabricEdges[i].right - fabricEdges[i].left;
			int widthChange = currentWidth - previousWidth;
			widthChanges.Add((fabricEdges[i].y, widthChange));
		}

		return widthChanges;
	}

	public static Mat ChangeColor(Mat src, ColorConversionCodes code)
	{
		Mat dest = new Mat();
		Cv2.CvtColor(src,dest,code);
		return dest;
	}

	public static List<Point[]> FindContours(Mat src)
	{
		Cv2.FindContours(src, out Point[][] contours, out HierarchyIndex[] hirarachy, RetrievalModes.External, ContourApproximationModes.ApproxNone);
		return contours.ToList();
	}

	public static Mat FindAndDrawContoursGray(Mat graysrc, Mat edges, int thickness = 2)
	{
		return FindAndDrawContours(ChangeColor(graysrc, ColorConversionCodes.GRAY2BGR), edges, thickness);
	}

	public static Mat FindAndDrawContours(Mat src,  Mat edges, int thickness = 2)
	{
		Cv2.FindContours(edges, out Point[][] contours, out HierarchyIndex[] hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);
		Mat result = src.Clone();
		Cv2.DrawContours(result, contours, -1, Scalar.Red, thickness);

		return result;
	}

	public static Mat ApplyMorphologicalOperations(Mat edges, Size kernelsize)
	{
		Mat morphed = new Mat();
		Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, kernelsize);
		Cv2.MorphologyEx(edges, morphed, MorphTypes.Close, kernel);
		return morphed;
	}

	public static Mat ApplyErode(Mat src, Size kernelsize)
	{
		Mat morphed = new Mat();
		Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, kernelsize);
		Cv2.Erode(src, morphed, kernel);
		return morphed;
	}

	public static Mat ApplyDilate(Mat src, Size kernelsize)
	{
		Mat morphed = new Mat();
		Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, kernelsize);
		Cv2.Dilate(src, morphed, kernel);
		return morphed;
	}

	public static Mat Combine(Mat first, Mat second, CVCombineMode mode)
	{
		Mat result = new Mat();
		switch (mode)
		{
			case CVCombineMode.BitwiseOr:
				Cv2.BitwiseOr(first, second, result);
				break;
			case CVCombineMode.AddWeight:
				Cv2.AddWeighted(first, 0.5, second, 0.5, 0, result);
				break;
		}
		return result;
	}

	public enum CVCombineMode
	{
		BitwiseOr,
		AddWeight
	}

	public static List<OpenCvSharp.Point[]> FilterContours(Mat src, List<OpenCvSharp.Point[]> contours)
	{
		List<Point[]> filteredContours = new();
		foreach (var contour in contours)
		{
			filteredContours.Add(contour);
		}
		return filteredContours;
	}

	public static Mat DrawContours(Mat src, List<Point[]> contours)
	{
		Mat result = src.Clone();
		Cv2.DrawContours(result, contours, -1, Scalar.Red, 1);
		return result;
	}

	public static void LogContours(List<Point[]> contours)
	{
		Console.WriteLine("START ------------------------------------------ Contours;");
		foreach(var contour in contours)
		{
			Console.WriteLine("------ MINI -------");
			foreach (var f in contour)
			{
				Console.WriteLine($"{f.ToString()} ");
			}
			Console.WriteLine("------ MINI -------");
		}
		Console.WriteLine("STOP ------------------------------------------ Contours;");
	}

	public static Mat Threshold(Mat src, double thresh = 100, double max = 255, ThresholdTypes type = ThresholdTypes.Binary)
	{
		Mat result = new Mat();
		Cv2.Threshold(src, result, thresh, max, type);
		return result;
	}
}

public class KFinderBase
{
	public static void FindAndSave(Mat _src, Mat color, string guid, string name)
	{
		Cv2.FindContours(color, out var contours, out var hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);

		Mat contoursimg = _src.Clone();
		Cv2.DrawContours(contoursimg, contours, -1, Scalar.Red, 2, lineType: LineTypes.AntiAlias);

		contoursimg.SaveImage($"Z_{name}{guid}.jpg");
	}

	public static void ProcessF(Mat _src)
	{
		Mat src = KBase.ChangeColor(_src, ColorConversionCodes.BGR2GRAY);

		Mat thresh = new();
		Cv2.Threshold(src, thresh, 220, 255, ThresholdTypes.Binary); //200, 255

		Cv2.FindContours(thresh, out var contours, out var hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);

		Mat contoursimg = _src.Clone();
		Cv2.DrawContours(contoursimg, contours, -1, Scalar.Red, 2, lineType: LineTypes.AntiAlias);

		var gd = Guid.NewGuid().ToString();
		thresh.SaveImage($"Z_Thresh{gd}.jpg");
		contoursimg.SaveImage($"Z_Contours{gd}.jpg");

		
		var colors = _src.Split();

		var blue = colors[0];
		var green = colors[1];
		var red = colors[2];

		FindAndSave(_src, blue, gd, "Blue");
		FindAndSave(_src, green, gd, "Green");
		FindAndSave(_src, red, gd, "Red");
	}

	public static List<HorizontalLine> ProcessFabric(Mat _src)
	{
		Mat src = KBase.ChangeColor(_src,ColorConversionCodes.BGR2GRAY);
		//Mat blurred = KBase.ApplyMedianBlur(src);
		Mat blurred = KBase.ApplyGusianBlur(src);


		Mat adjusted = KBase.AdjustBrightnessContrast(blurred);
		Mat cannyEdges = KBase.DetectEdges(adjusted,2);
		Mat sobelEdges = KBase.DetectSobelEdges(adjusted);
		Mat laplacianEdges = KBase.DetectLaplacianEdges(adjusted);

		/*
		Mat cannyOutput = new Mat(cannyEdges.Size(), MatType.CV_8UC1, Scalar.All(0));
		cannyEdges.CopyTo(cannyOutput);
		*/

		// Kenarları birleştir
		Mat combinedEdges = cannyEdges; //KBase.Combine(laplacianEdges, sobelEdges, KBase.CVCombineMode.AddWeight);
		//KBase.Combine(cannyEdges, sobelEdges, KBase.CVCombineMode.AddWeight);
		//combinedEdges = KBase.Combine(combinedEdges, laplacianEdges, KBase.CVCombineMode.AddWeight);

		/*
		Size kernelsize = new Size(5, 5);

		Mat morphedEdges = KBase.ApplyDilate(KBase.ApplyErode(KBase.ApplyMorphologicalOperations(combinedEdges,kernelsize),kernelsize),kernelsize);
		*/

		var morphedEdges = combinedEdges; //KBase.Threshold(combinedEdges);

		var Contours = KBase.FindContours(morphedEdges);

		KBase.LogContours(Contours);

		var filteredContours = KBase.FilterContours(src, Contours.ToList());

		
		Console.WriteLine($"Contours {Contours.Count} - Filterede Contours {filteredContours.Count}");

		var ContoursResult = KBase.DrawContours(_src, filteredContours);
		

		List<(int y, int left, int right)> fabricEdges = KBase.GetFabricEdges(combinedEdges);
		List<(int y, int widthChange)> widthChanges = KBase.CalculateWidthChanges(fabricEdges);

		foreach (var change in widthChanges)
		{
			Console.WriteLine($"Y: {change.y}, Width Change: {change.widthChange} pixels");
		}

		var gd = Guid.NewGuid().ToString();

		adjusted.SaveImage($"Z_Adjusted{gd}.jpg");
		cannyEdges.SaveImage($"Z_Canny{gd}.jpg");
		sobelEdges.SaveImage($"Z_Sobel{gd}.jpg");
		laplacianEdges.SaveImage($"Z_Laplacian{gd}.jpg");
		combinedEdges.SaveImage($"Z_Edges{gd}.jpg");

		ContoursResult.SaveImage($"Z_Contours{gd}.jpg");

		//result.SaveImage($"Z_Result{gd}.jpg");
		//morphedResult.SaveImage($"Z_MorphedResult{gd}.jpg");

		return fabricEdges.Select(x => new HorizontalLine(x.left,x.right,x.y)).ToList();
	}

}
