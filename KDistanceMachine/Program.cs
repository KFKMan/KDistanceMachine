
using Iot.Device.Graphics.SkiaSharpAdapter;
using Iot.Device.Media;
using KDistanceMachine.Devices;
using KDistanceMachine.Handlers;
using OpenCvSharp;
using System.Diagnostics;
using System.Drawing;
using static KFinder;
using static System.Net.Mime.MediaTypeNames;

Config Config = new Config();
AppData Data = new();

Data.BaseData.HorizontalFOV = 54;
Data.BaseData.VerticalFOV = 41;
Data.BaseData.QualityWidth = 2592;
Data.BaseData.QualityHeight = 300;


SkiaSharpAdapter.Register();

void LogMatVertical(Mat mat, List<VerticalLine> imageLines, string path)
{
	void AddLine(OpenCvSharp.Point p,OpenCvSharp.Point p2)
	{
		Cv2.Line(mat, p, p2, Scalar.Red, 1);
		//Cv2.Circle(mat, new OpenCvSharp.Point(x, y), 3, Scalar.Red, -1);
	}

	foreach (var line in imageLines)
	{
		AddLine(new(line.X, line.StartY), new(line.X, line.EndY));
		//AddCircle(line.LeftX, line.Y);
		//AddCircle(line.RightX, line.Y);
	}

	Cv2.ImWrite(path, mat);
}

void LogMat(Mat mat, List<HorizontalLine> imageLines, string path)
{
	void AddCircle(int x, int y)
	{
		Cv2.Circle(mat, new OpenCvSharp.Point(x, y), 3, Scalar.Red, -1);
	}

	foreach (var line in imageLines)
	{

		AddCircle(line.LeftX, line.Y);
		AddCircle(line.RightX, line.Y);
	}

	Cv2.ImWrite(path, mat);
}

var dr = new DirectoryInfo(Directory.GetCurrentDirectory());

var files = dr.GetFiles("Z_*");

foreach (var file in files)
{
	Console.WriteLine($"Deleting {file.Name}");
	file.Delete();
}


while (true)
{
	Console.Write("Set Image Location: ");
	string imagept = Console.ReadLine();

	/*
	var res = YoticFinder.ProcessFabric(imagept.BitmapFromFile());
	var res2 = YoticFinder.ProcessFabricDetailed(imagept.BitmapFromFile());

	var gd = Guid.NewGuid().ToString();
	res.Save($"Z_{gd}.jpg");
	res2.Save($"Z_ADVANCED_{gd}.jpg"); */
	
	var image = Cv2.ImRead(imagept);

	KFinderBase.ProcessF(image);
	//KFinderBase.ProcessFabric(image);

	/* KFinderBase.ProcessFabric(image);
	*/

	//KFinder.FindLinesWithCountors(image);

	/*
	var imageLines = KFinder.FindVerticalLines(image);
	Console.WriteLine("Lines Finded");


	/*
	foreach (var line in imageLines)
	{
		var distance = KFinder.CalculateWidthAsCM(Data, line.LeftX, line.RightX);
		Console.Write($"{distance} ");
	}
	*/

	/*
	LogMatVertical(image, imageLines, Guid.NewGuid().ToString() + ".png"); */
	Console.WriteLine("Processed Capture");
	//Cv2.ImShow("l",mat);
	//Cv2.WaitKey(10);
}

var CancellationTokenSource = new CancellationTokenSource();

// See https://aka.ms/new-console-template for more information
void WriteLine(string str)
{
	Console.WriteLine(str);
}

WriteLine($"Started at {DateTime.Now.ToString()}");

var DistanceSensor = new HCS04(Config.HCSTrig, Config.HCSEcho);

var DistanceSensorHandler = new AppDataHandler<UnitsNet.Length>(DistanceSensor, Data, (p) => p.DistanceData);

var DistanceSensorHandlerToken = CancellationTokenSource.Token;

var DistanceSensorHandlerState = DistanceSensorHandler.Handle(null,DistanceSensorHandlerToken); //Arkada çalışacak

var VideoCaptureToken = CancellationTokenSource.Token;

var VideoData = Directory.CreateDirectory("DataLog");

VideoConnectionSettings videoConnectionSettings = new VideoConnectionSettings(busId: 0, (2592, 300), pixelFormat: VideoPixelFormat.NV12);

Iot.Device.Media.WhiteBalanceEffect whiteBalanceEffect = WhiteBalanceEffect.Daylight;
videoConnectionSettings.WhiteBalanceEffect = whiteBalanceEffect;

using (VideoDevice device = VideoDevice.Create(videoConnectionSettings))
{
	foreach (VideoPixelFormat item in device.GetSupportedPixelFormats())
	{
		Console.Write($"{item} ");
	}
	Console.WriteLine();

	foreach (var resolution in device.GetPixelFormatResolutions(VideoPixelFormat.YUYV))
	{
		Console.Write($"[{resolution.MinWidth}x{resolution.MinHeight}]->[{resolution.MaxWidth}x{resolution.MaxHeight}], Step [{resolution.StepWidth},{resolution.StepHeight}] ");
	}

	Console.WriteLine();

	device.ImageBufferPoolingEnabled = true;

	/*
	device.StartCaptureContinuous();

	var th = new Thread(() =>
	{
		device.CaptureContinuous(VideoCaptureToken);
	});

	th.Start(); */

	int i = 0;

	while (true)
	{
		Console.WriteLine("Getting Capture");

		//string filename = Path.Combine(VideoData.FullName, DateTime.UtcNow.ToString("dd.MM.yyyy--HH:mm--ss--fff"));

		string original = i.ToString() + ".jpg";

		string at = @"""";

		Console.WriteLine(at);

		ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = $"-c {at}rpicam-jpeg --output {original}{at}", };
		startInfo.RedirectStandardOutput = true;
		startInfo.UseShellExecute = false;
		startInfo.CreateNoWindow = true;
		Process proc = new Process() { StartInfo = startInfo, };
		proc.Start();

		String result = proc.StandardOutput.ReadToEnd();

		Console.WriteLine(result);

		proc.WaitForExit();

		while (!File.Exists(original))
		{
			await Task.Delay(10);
		}

		await Task.Delay(3000);

		//device.Capture(filename + "f.jpg");

		//device.

		/*
		MemoryStream ms = new MemoryStream(device.Capture());
		Color[] colors = VideoDevice.Nv12ToRgb(ms, videoConnectionSettings.CaptureSize);
		var bitmap = VideoDevice.RgbToBitmap(videoConnectionSettings.CaptureSize, colors);

		var image = ImageHelper.ConvertBitmapImageToMat(bitmap);

		image.SaveImage(original); */

		/*
		MemoryStream ms = new MemoryStream(device.Capture());
		Console.WriteLine("Captured");
		Color[] colors = VideoDevice.Nv12ToRgb(ms, videoConnectionSettings.CaptureSize);
		Console.WriteLine("NV12 Handled");
		var rimg = VideoDevice.RgbToBitmap(videoConnectionSettings.CaptureSize, colors);
		Console.WriteLine("BitmapImage Getted");

		var image = ImageHelper.ConvertBitmapImageToMat(rimg);
		Console.WriteLine("Mat Getted From Imaged"); //*/
		//var image = ImageHelper.ColorsToMat(colors,new System.Drawing.Size(Data.BaseData.QualityWidth,Data.BaseData.QualityHeight)); //ImageHelper.Nv12ToMat(ms, new OpenCvSharp.Size(Data.BaseData.QualityWidth, Data.BaseData.QualityHeight));

		/* device.Capture(original);

		var image = Cv2.ImRead(original, ImreadModes.Grayscale); */

		var image = Cv2.ImRead(original);

		var imageLines = KFinder.FindLines(image);
		Console.WriteLine("Lines Finded");
		
		

		foreach(var line in imageLines)
		{
			var distance = KFinder.CalculateWidthAsCM(Data, line.LeftX, line.RightX);
			Console.Write($"{distance} ");
		}

		LogMat(image, imageLines,i.ToString() + "s" + ".png");
		Console.WriteLine("Processed Capture");

		i++;
	}
}
