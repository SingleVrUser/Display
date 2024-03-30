using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Windows.ApplicationModel;
using Display.Helper.FileProperties.Name;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace Display.Services;

internal class GetImageByOpenCv
{
    private static readonly string FaceProto = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/deploy.prototxt");
    private static readonly string FaceModel = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/res10_300x300_ssd_iter_140000_fp16.caffemodel");

    private static readonly string GenderProto = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/gender_deploy.prototxt");
    private static readonly string GenderModel = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/gender_net.caffemodel");

    private static readonly string AgeConfigFile = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/age_deploy.prototxt");
    private static readonly string AgeFaceModel = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/age_net.caffemodel");

    public static bool IsModelFilesExists =>
        File.Exists(FaceProto) && File.Exists(FaceModel)
                               && File.Exists(GenderProto) && File.Exists(GenderModel)
                               && File.Exists(AgeConfigFile) && File.Exists(AgeFaceModel);

    private readonly Net _faceNet = CvDnn.ReadNetFromCaffe(FaceProto, FaceModel);
    private readonly Net _genderNet = CvDnn.ReadNetFromCaffe(GenderProto, GenderModel);
    private readonly Net _ageNet = CvDnn.ReadNetFromCaffe(AgeConfigFile, AgeFaceModel);


    public GetImageByOpenCv(bool isUseGpu = false)
    {
        InitializeNet(isUseGpu);
    }

    private void InitializeNet(bool isUseGpu)
    {
        if (isUseGpu)
        {
            _faceNet.SetPreferableBackend(Backend.CUDA);
            _faceNet.SetPreferableTarget(Target.CUDA);

            _genderNet.SetPreferableBackend(Backend.CUDA);
            _genderNet.SetPreferableTarget(Target.CUDA);

            _ageNet.SetPreferableBackend(Backend.CUDA);
            _ageNet.SetPreferableTarget(Target.CUDA);
        }
        else
        {
            _faceNet.SetPreferableBackend(Backend.DEFAULT);
            _faceNet.SetPreferableTarget(Target.CPU);

            _genderNet.SetPreferableBackend(Backend.DEFAULT);
            _genderNet.SetPreferableTarget(Target.CPU);

            _ageNet.SetPreferableBackend(Backend.DEFAULT);
            _ageNet.SetPreferableTarget(Target.CPU);
        }
    }

    public double GetTotalFrameCount(string url)
    {
        var cap = new VideoCapture(url);
        return cap.Get(VideoCaptureProperties.FrameCount);
    }

    public bool Task_GetThumbnailByVideoPath(string videoPath, double startFrame, bool isShowWindow, IProgress<ProgressInfo> progress, string savePath, string imageName = "", bool isNeedDetectFaces = true, double? length = null)
    {
        var isGetImage = false;

        List<string> genderList = ["Male", "Female"];
        //List<string> ageList = new List<string> { "(0-2)", "(4-6)", "(8-12)", "(15-20)", "(25-32)", "(38-43)", "(48-53)", "(60-100)" };

        var cap = new VideoCapture(videoPath);

        if (startFrame != 0)
        {
            cap.Set(VideoCaptureProperties.PosFrames, startFrame);
        }

        var pause = false;
        var detectFaces = true;
        var freq = 100;

        length ??= GetTotalFrameCount(videoPath) - startFrame;

        //失败尝试次数
        var tryAgainCount = 3;

        ////人脸信息尝试获取次数
        //int GetFaceCount = 30;

        for (int idx = 0, tryCount = 0; idx < length && tryCount < tryAgainCount; idx++)
        {
            var startTime = DateTime.Now;

            var key = Cv2.WaitKey(1);
            switch ((char)key)
            {
                //case (char)27: // esc
                //    break;
                case (char)32: // space
                    pause = !pause;
                    break;
                case 'f':
                    detectFaces = !detectFaces;
                    break;
            }

            //暂停
            if (pause) continue;

            using Mat image = new();
            var ret = cap.Grab();
            if (!ret)
            {
                break;
            }
            cap.Retrieve(image);

            if (image.Empty())
            {
                tryCount++;
                continue;
            }

            //是否检测
            if (detectFaces && idx % freq == 1)
            {
                var padding = 20;

                //需要检测人脸信息
                if (isNeedDetectFaces)
                {
                    var bBoxes = GetFaceBox(image);

                    //未检测到人脸信息
                    if (bBoxes.Count == 0)
                    {
                        //源
                        if (isShowWindow)
                        {
                            ShowOriginImage(image, startTime);
                        }
                    }
                    //检测到人脸信息
                    else
                    {
                        ProgressInfo progressInfo = new()
                        {
                            FaceList = []
                        };

                        if (string.IsNullOrEmpty(imageName))
                        {
                            imageName = $"frame{startFrame + idx}";
                        }

                        var imagePath = Path.Combine(savePath, $"{imageName}.jpg");
                        progressInfo.ImagePath = imagePath;
                        progressInfo.PixelWidth = image.Cols;
                        progressInfo.PixelHeight = image.Rows;

                        var isGetFemaleImage = false;

                        foreach (var bBox in bBoxes)
                        {
                            var newFaceRecognition = new FaceRecognition
                            {
                                Facebox = bBox
                            };

                            var x1 = bBox[0];
                            var y1 = bBox[1];
                            var x2 = bBox[2];
                            var y2 = bBox[3];

                            if (x1 >= x2 || y1 >= y2) continue;

                            var height = image.Rows;
                            var width = image.Cols;

                            var range1 = new OpenCvSharp.Range(Math.Max(0, y1 - padding), Math.Min(height, y2 + padding));
                            var range2 = new OpenCvSharp.Range(Math.Max(0, x1 - padding), Math.Min(width, x2 + padding));

                            var face = image[range1, range2];

                            using var blob = CvDnn.BlobFromImage(face, 1.0, new Size(227, 227), new Scalar(78.4263377603, 87.7689143744, 114.895847746), false);

                            _genderNet.SetInput(blob);

                            using var genderPres = _genderNet.Forward();

                            genderPres.MinMaxLoc(out _, out var maxIndexGender, out _, out var maxLoc);
                            var gender = genderList[maxLoc.X];
                            newFaceRecognition.Gender = new FaceRecognition.PredictLabel { Result = gender, Confidence = maxIndexGender };

                            var label = $"{gender}";

                            //人脸
                            var faceImg = new Mat(image,
                                new OpenCvSharp.Range(y1, y2),
                                new OpenCvSharp.Range(x1, x2));

                            var newSize = new Size(faceImg.Cols, faceImg.Rows);
                            using var frame = new Mat();
                            Cv2.Resize(faceImg, frame, newSize);

                            //hsv，主要筛选出过暗/过亮头像
                            var ishHsvQualified = false;

                            Mat hsv = new();
                            Cv2.CvtColor(frame, hsv, ColorConversionCodes.BGR2HSV);
                            var scalar = Cv2.Mean(hsv);

                            //HSV 中的色相表示颜色，HSV 中的饱和度表示灰色，HSV 中的值表示亮度。
                            //HSV 中的色相范围为[0，179]，HSV 中的饱和度范围为[0，255]，HSV 中的值范围为[0，255]
                            if (scalar.Val0 is > 0 and < 179 &&
                                0 < scalar.Val1 && scalar is { Val1: < 255, Val2: > 50 and < 200 })
                            {
                                ishHsvQualified = true;
                            }

                            //只挑选符合条件的
                            if (gender == "Female" && newFaceRecognition.Gender.Confidence > 0.9 && ishHsvQualified)
                            {
                                isGetFemaleImage = true;
                            }

                            Cv2.PutText(frame, label, new Point(5, 25), HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 255, 255), 2, LineTypes.Link4);

                            progressInfo.FaceList.Add(newFaceRecognition);

                            if (isShowWindow)
                            {
                                Cv2.ImShow($"{Thread.CurrentThread.ManagedThreadId} Age Gender Demo", faceImg);

                            }
                        }

                        progress.Report(progressInfo);

                        //获取到女生
                        if (!isGetFemaleImage) continue;

                        SaveOriginOrFaceImage(image, savePath, imageName);
                        //FileMatch.CreateDirectoryIfNotExists(SavePath);
                        //image.SaveImage(imagePath);
                        isGetImage = true;
                        image.Release();
                        break;
                    }
                }
                //不需要检测人脸信息
                else
                {
                    if (string.IsNullOrEmpty(imageName))
                    {
                        imageName = $"frame{startFrame + idx}";
                    }

                    //过暗的不要
                    Mat hsv = new();
                    Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
                    var scalar = Cv2.Mean(hsv);

                    //HSV 中的色相表示颜色，HSV 中的饱和度表示灰色，HSV 中的值表示亮度。
                    //HSV 中的色相范围为[0，179]，HSV 中的饱和度范围为[0，255]，HSV 中的值范围为[0，255]
                    if (!(0 < scalar.Val0) || !(scalar.Val0 < 179) ||
                        !(0 < scalar.Val1) || !(scalar.Val1 < 255) ||
                        !(50 < scalar.Val2) || !(scalar.Val2 < 200)) continue;
                    SaveOriginOrFaceImage(image, savePath, imageName);
                    isGetImage = true;
                    break;

                }

            }
            else
            {
                if (!isShowWindow) continue;

                image.PutText("Playing", new Point(10, 300), HersheyFonts.HersheyScriptComplex, 5, Scalar.Blue, 3);
                ShowOriginImage(image, startTime);
            }
        }
        GC.Collect();

        if (isShowWindow)
        {
            Cv2.DestroyAllWindows();
        }
        cap.Release();

        return isGetImage;
    }

    public void Task_GenderByVideo(string videoPath, double startFrame, double length, bool isShowWindow, IProgress<ProgressInfo> progress, string savePath, string imageName = "")
    {
        var isOnlySaveFace = imageName == "face";

        List<string> genderList = ["Male", "Female"];
        List<string> ageList = ["(0-2)", "(4-6)", "(8-12)", "(15-20)", "(25-32)", "(38-43)", "(48-53)", "(60-100)"];

        var cap = new VideoCapture(videoPath);

        cap.Set(VideoCaptureProperties.PosFrames, startFrame);

        var pause = false;
        var detectFaces = true;
        var freq = 100;

        //失败尝试次数
        var tryAgainCount = 3;

        for (int idx = 0, tryCount = 0; idx < length && tryCount < tryAgainCount; idx++)
        {
            var startTime = DateTime.Now;

            var key = Cv2.WaitKey(1);
            switch ((char)key)
            {
                //case (char)27: // esc
                //    break;
                case (char)32: // space
                    pause = !pause;
                    break;
                case 'f':
                    detectFaces = !detectFaces;
                    break;
            }

            //暂停
            if (pause) continue;

            using Mat image = new();
            var ret = cap.Grab();
            if (!ret)
            {
                break;
            }
            cap.Retrieve(image);

            if (image.Empty())
            {
                tryCount++;
                continue;
            }

            //是否检测
            if (detectFaces && idx % freq == 1)
            {
                var bBoxes = GetFaceBox(image);

                var padding = 20;

                //检测到人脸信息
                if (bBoxes.Count == 0)
                {
                    //源
                    if (isShowWindow)
                    {
                        ShowOriginImage(image, startTime);
                    }
                }
                else
                {
                    ProgressInfo progressInfo = new()
                    {
                        FaceList = []
                    };

                    if (imageName == string.Empty)
                    {
                        imageName = $"frame{startFrame + idx}";
                    }

                    var imagePath = Path.Combine(savePath, $"{imageName}.jpg");
                    progressInfo.ImagePath = imagePath;
                    progressInfo.PixelWidth = image.Cols;
                    progressInfo.PixelHeight = image.Rows;

                    if (!isOnlySaveFace)
                    {
                        FileMatch.CreateDirectoryIfNotExists(savePath);
                        image.SaveImage(imagePath);
                    }

                    var isGetFemaleImage = false;

                    foreach (var bBox in bBoxes)
                    {
                        var newFaceRecognition = new FaceRecognition
                        {
                            Facebox = bBox
                        };

                        var x1 = bBox[0];
                        var y1 = bBox[1];
                        var x2 = bBox[2];
                        var y2 = bBox[3];

                        if (x1 >= x2 || y1 >= y2) continue;

                        var height = image.Rows;
                        var width = image.Cols;

                        var range1 = new OpenCvSharp.Range(Math.Max(0, y1 - padding), Math.Min(height, y2 + padding));
                        var range2 = new OpenCvSharp.Range(Math.Max(0, x1 - padding), Math.Min(width, x2 + padding));

                        var face = image[range1, range2];

                        using var blob = CvDnn.BlobFromImage(face, 1.0, new Size(227, 227), new Scalar(78.4263377603, 87.7689143744, 114.895847746), false);

                        _genderNet.SetInput(blob);

                        using var genderPres = _genderNet.Forward();

                        genderPres.MinMaxLoc(out _, out var maxIndexGender, out _, out var maxLoc);
                        var gender = genderList[maxLoc.X];
                        newFaceRecognition.Gender = new FaceRecognition.PredictLabel { Result = gender, Confidence = maxIndexGender };

                        _ageNet.SetInput(blob);
                        using var agePres = _ageNet.Forward();
                        agePres.MinMaxLoc(out _, out maxIndexGender, out _, out maxLoc);
                        var age = ageList[maxLoc.X];
                        newFaceRecognition.Age = new FaceRecognition.PredictLabel { Result = age, Confidence = maxIndexGender };

                        var label = $"{gender},{age}";

                        //人脸
                        var faceImg = new Mat(image,
                            new OpenCvSharp.Range(y1, y2),
                            new OpenCvSharp.Range(x1, x2));

                        var newSize = new Size(faceImg.Cols, faceImg.Rows);
                        using var frame = new Mat();
                        Cv2.Resize(faceImg, frame, newSize);

                        //通过宽高比筛选
                        var isWhRatioMatch = false;

                        var whRatio = (float)faceImg.Cols / faceImg.Rows;

                        if (0.7 < whRatio && whRatio < 0.9)
                        {
                            isWhRatioMatch = true;
                        }

                        //hsv，主要筛选出过暗/过亮头像
                        Mat hsv = new();
                        Cv2.CvtColor(frame, hsv, ColorConversionCodes.BGR2HSV);

                        var scalar = Cv2.Mean(hsv);

                        var isHsvQualified = 0 < scalar.Val0 && scalar is { Val0: < 179, Val1: > 0 and < 255, Val2: > 85 and < 200 };

                        //HSV 中的色相表示颜色，HSV 中的饱和度表示灰色，HSV 中的值表示亮度。
                        //HSV 中的色相范围为[0，179]，HSV 中的饱和度范围为[0，255]，HSV 中的值范围为[0，255]

                        //只挑选符合条件的
                        if (gender == "Female" && newFaceRecognition.Gender.Confidence > 0.9 && isHsvQualified && isWhRatioMatch)
                        {
                            if (isOnlySaveFace)
                            {
                                //检查是否有SaveImage无法识别的字符
                                //若有，则先Base64编码
                                var needModifyDir = false;
                                var directoryName = Path.GetFileName(savePath);
                                if (directoryName.Contains('・'))
                                {
                                    directoryName = Convert.ToBase64String(Encoding.UTF8.GetBytes(directoryName));
                                    savePath = Path.Combine(Path.GetDirectoryName(savePath), directoryName);
                                    imagePath = Path.Combine(savePath, $"{imageName}.jpg");
                                    needModifyDir = true;
                                }

                                FileMatch.CreateDirectoryIfNotExists(savePath);

                                faceImg.SaveImage(imagePath);

                                //目录重命名
                                if (needModifyDir)
                                {
                                    directoryName = Encoding.UTF8.GetString(Convert.FromBase64String(directoryName));
                                    var dstImagePath = Path.Combine(Path.GetDirectoryName(savePath), directoryName);

                                    Directory.Move(Path.GetDirectoryName(imagePath), dstImagePath);
                                }
                            }
                            isGetFemaleImage = true;
                        }

                        Cv2.PutText(frame, label, new Point(5, 25), HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 255, 255), 2, LineTypes.Link4);


                        progressInfo.FaceList.Add(newFaceRecognition);

                        if (isShowWindow)
                        {
                            Cv2.ImShow($"{Thread.CurrentThread.ManagedThreadId} Age Gender Demo", faceImg);

                        }
                    }

                    progress.Report(progressInfo);

                    //获取到女生
                    if (isGetFemaleImage)
                    {
                        image.Release();
                        break;
                    }
                }
            }
            else
            {
                if (!isShowWindow) continue;

                image.PutText("Playing", new Point(10, 300), HersheyFonts.HersheyScriptComplex, 5, Scalar.Blue, 3);
                ShowOriginImage(image, startTime);
            }
        }
        GC.Collect();

        if (isShowWindow)
        {
            Cv2.DestroyAllWindows();
        }
        cap.Release();
    }

    private void SaveOriginOrFaceImage(Mat image, string savePath, string imageName)
    {
        var imagePath = Path.Combine(savePath, $"{imageName}.jpg");
        FileMatch.CreateDirectoryIfNotExists(savePath);
        image.SaveImage(imagePath);
    }

    static void ShowOriginImage(Mat image, DateTime startTime, IReadOnlyList<int> bBox = null)
    {
        //源
        var diff = DateTime.Now - startTime;
        var fpsInfo = "fps: nan";
        if (diff.Milliseconds > 0)
        {
            var fpsVal = 1.0 / diff.Milliseconds * 1000;
            fpsInfo = $"fps: {fpsVal:00}";
        }

        Cv2.PutText(image, fpsInfo, new Point(10, 120), HersheyFonts.HersheySimplex, 5, Scalar.White, 2, LineTypes.Link4);

        if (bBox != null)
        {
            Cv2.Rectangle(image, new Point(bBox[0], bBox[1]), new Point(bBox[2], bBox[3]), Scalar.Green, image.Rows / 150);
        }

        var demoSize = new Size(image.Cols / 3f, image.Rows / 3f);
        using var demoFrame = new Mat();
        Cv2.Resize(image, demoFrame, demoSize);
        Cv2.ImShow($"{Thread.CurrentThread.ManagedThreadId} Origin Video", demoFrame);
    }

    //人脸框检测
    List<List<int>> GetFaceBox(Mat frameImage)
    {
        List<List<int>> bBoxes = [];

        using var blob = CvDnn.BlobFromImage(frameImage, 1.0, new Size(300, 300),
            new Scalar(104, 117, 123), false, false);
        _faceNet.SetInput(blob, "data");

        using var detection = _faceNet.Forward("detection_out");
        using var detectionMat = new Mat(detection.Size(2), detection.Size(3), MatType.CV_32F, detection.Ptr(0));

        var frameHeight = frameImage.Rows;
        var frameWidth = frameImage.Cols;

        for (var i = 0; i < detectionMat.Rows; i++)
        {
            var confidence = detectionMat.At<float>(i, 2);

            if (confidence <= 0.7) continue;

            var x1 = (int)(detectionMat.At<float>(i, 3) * frameWidth);
            var y1 = (int)(detectionMat.At<float>(i, 4) * frameHeight);
            var x2 = (int)(detectionMat.At<float>(i, 5) * frameWidth);
            var y2 = (int)(detectionMat.At<float>(i, 6) * frameHeight);

            bBoxes.Add([x1, y1, x2, y2]);
        }

        return bBoxes;
    }

    internal class FaceRecognition
    {
        public PredictLabel Gender { get; set; }
        public PredictLabel Age { get; set; }

        public List<int> Facebox { get; set; }

        public class PredictLabel
        {
            public string Result;
            public double Confidence;
        }

    }

    internal class ProgressInfo
    {
        public int PixelHeight;
        public int PixelWidth;
        public string ImagePath;
        public List<FaceRecognition> FaceList;
    }

}

