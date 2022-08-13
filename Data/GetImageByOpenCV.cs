using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using Windows.ApplicationModel;

namespace Data
{
    public class GetImageByOpenCV
    {
        static string faceProto = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/deploy.prototxt");
        static string faceModel = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/res10_300x300_ssd_iter_140000_fp16.caffemodel");
        
        static string genderProto = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/gender_deploy.prototxt");
        static string genderModel = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/gender_net.caffemodel");
        
        static string AgeconfigFile = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/age_deploy.prototxt");
        static string AgefaceModel = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Models/caffe/age_net.caffemodel");

        Net faceNet = CvDnn.ReadNetFromCaffe(faceProto, faceModel);
        Net genderNet = CvDnn.ReadNetFromCaffe(genderProto, genderModel);
        Net ageNet = CvDnn.ReadNetFromCaffe(AgeconfigFile, AgefaceModel);


        void InitializeNet(bool isUseGPU)
        {
            if (isUseGPU)
            {
                faceNet.SetPreferableBackend(Backend.CUDA);
                faceNet.SetPreferableTarget(Target.CUDA);

                genderNet.SetPreferableBackend(Backend.CUDA);
                genderNet.SetPreferableTarget(Target.CUDA);

                ageNet.SetPreferableBackend(Backend.CUDA);
                ageNet.SetPreferableTarget(Target.CUDA);
            }
            else
            {
                faceNet.SetPreferableBackend(Backend.DEFAULT);
                faceNet.SetPreferableTarget(Target.CPU);

                genderNet.SetPreferableBackend(Backend.DEFAULT);
                genderNet.SetPreferableTarget(Target.CPU);

                ageNet.SetPreferableBackend(Backend.DEFAULT);
                ageNet.SetPreferableTarget(Target.CPU);
            }
        }

        public double getTotalFrameCount(string url)
        {
            var cap = new VideoCapture(url);
            return cap.Get(VideoCaptureProperties.FrameCount);
        }


        private async Task MultThread(string url, int Task_Count,int startJumpFrame_num = 1000, int endJumpFrame_num = 0, bool isShowWindow = true)
        {
            //string url = @"D:\库\Downloads\115\普通\【楓ふうあ】 SSIS-428\SSIS-428 大嫌いなあなたに懇願ー 生涯最高のイラマチオをください。 楓ふうあ.mp4";


            ////文件名
            //FileName_TextBlock.Text = Path.GetFileName(url);

            ////进度条最大值
            //AllProgressBar.Maximum = Task_Count;

            //AllProgressBar.Value = 0;

            //int Task_Count = 20;

            var frames_num = getTotalFrameCount(url);
            if (frames_num == 0) return;

            ////跳过开头
            //int startJumpFrame_num = 1000;

            ////跳过结尾
            //int endJumpFrame_num = 0;

            var actualEndFrame = frames_num - endJumpFrame_num;

            //平均长度
            var averageLength = (int)(actualEndFrame - startJumpFrame_num) / Task_Count;

            //Progress_TextBlock.Text = $"{AllProgressBar.Value}/{Task_Count}";
            //AllProgressBar.Visibility = Visibility.Visible;

            var startTime = DateTimeOffset.Now;
            for (double start_frame = startJumpFrame_num; start_frame + averageLength <= actualEndFrame; start_frame += averageLength)
            {

                double taskStartFrame = start_frame;
                double length = actualEndFrame - startJumpFrame_num < 2 * averageLength ? actualEndFrame - startJumpFrame_num : averageLength;

                //Debug.WriteLine($"进程开始：{start_frame}，长度{length}");

                //进度
                var progress = new Progress<progressInfo>(info =>
                {
                    //tryStartShowFaceCanv();

                    //RichTextBlock richTextBlock = new RichTextBlock();
                    //Paragraph paragraph = new Paragraph();

                    //var faces = info.faceList;

                    //paragraph.Inlines.Add(new Run() { Text = $"捕获人脸({faces.Count})：" });
                    //paragraph.Inlines.Add(new LineBreak());
                    //for (int i = 0; i < faces.Count; i++)
                    //{
                    //    string index = string.Empty;
                    //    if (faces.Count > 1)
                    //    {
                    //        index = $"{i + 1}. ";
                    //    }

                    //    paragraph.Inlines.Add(new Run() { Text = $"{index}{faces[i].gender.result}（{faces[i].gender.confidence:0.00}）：{faces[i].age.result}岁（{faces[i].age.confidence:0.00}）" });
                    //    if (i < faces.Count - 1)
                    //    {
                    //        paragraph.Inlines.Add(new LineBreak());
                    //    }
                    //}

                    //showImage.Source = new BitmapImage(new Uri(info.imagePath));

                    //updateFaceCanv(info);

                    //if (imageIntroduce_RichTextBlock.Blocks.Count == 0)
                    //{
                    //    imageIntroduce_RichTextBlock.Blocks.Add(new Paragraph());
                    //}

                    //imageIntroduce_RichTextBlock.Blocks[0] = paragraph;
                });

                await Task.Run(() => Task_GenderByVideo(url, taskStartFrame, length, isShowWindow, progress, @"D:\库\Pictures\Screenshots\"));

                //AllProgressBar.Value++;

                //Progress_TextBlock.Text = $"{AllProgressBar.Value}/{Task_Count}";

                ////计算剩余时间
                //leftTime_TextBlock.Text = $"预计剩余：{ConvertInt32ToDateStr((Task_Count - AllProgressBar.Value) * ((DateTimeOffset.Now - startTime) / AllProgressBar.Value).TotalSeconds)}";

            }

            ////结束
            //leftTime_TextBlock.Text = $"总耗时：{FileMatch.ConvertInt32ToDateStr((DateTimeOffset.Now - startTime).TotalSeconds)}";
        }

        public void Task_GenderByVideo(string videoPath, double start_frame, double length, bool isShowWindow, IProgress<progressInfo> progress, string SavePath, string imageName="")
        {
            bool isOnlySaveFace = imageName == "face" ? true: false;

            List<string> genderList = new List<string> { "Male", "Female" };
            List<string> ageList = new List<string> { "(0-2)", "(4-6)", "(8-12)", "(15-20)", "(25-32)", "(38-43)", "(48-53)", "(60-100)" };

            var cap = new VideoCapture(videoPath);

            cap.Set(VideoCaptureProperties.PosFrames, start_frame);

            var pause = false;
            var detectFaces = true;
            int freq = 100;

            //失败尝试次数
            int tryAgainCount = 3;
            //int tryCount = 0;

            for (int idx = 0, tryCount = 0; idx < length && tryCount < tryAgainCount; idx++)
            {
                var starttime = DateTime.Now;

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
                if (!pause)
                {
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
                        var bboxes = getFaceBox(image);

                        int padding = 20;

                        //检测到人脸信息
                        if (bboxes.Count == 0)
                        {
                            //源
                            if (isShowWindow)
                            {
                                showOriginImage(image, starttime);
                            }
                            continue;
                        }
                        else
                        {
                            progressInfo progressInfo = new();
                            progressInfo.faceList = new();

                            if(imageName == string.Empty)
                            {
                                imageName = $"frame{start_frame + idx}";
                            }

                            string imagePath =  Path.Combine(SavePath, $"{imageName}.jpg");
                            progressInfo.imagePath = imagePath;
                            progressInfo.PixelWidth = image.Cols;
                            progressInfo.PixelHeight = image.Rows;

                            if(!isOnlySaveFace)
                            {
                                FileMatch.CreateDirectoryIfNotExists(SavePath);
                                image.SaveImage(imagePath);
                            }

                            bool isGetFemaleImage = false;

                            foreach (var bbox in bboxes)
                            {
                                var newFaceRecognition = new FaceRecognition();
                                newFaceRecognition.facebox = bbox;

                                int x1 = bbox[0];
                                int y1 = bbox[1];
                                int x2 = bbox[2];
                                int y2 = bbox[3];

                                if (x1 >= x2 || y1 >= y2) continue;

                                var height = image.Rows;
                                var width = image.Cols;

                                var Range1 = new OpenCvSharp.Range(Math.Max(0, y1 - padding), Math.Min(height, y2 + padding));
                                var Range2 = new OpenCvSharp.Range(Math.Max(0, x1 - padding), Math.Min(width, x2 + padding));

                                Mat face = image[Range1, Range2];

                                using var blob = CvDnn.BlobFromImage(face, 1.0, new OpenCvSharp.Size(227, 227), new Scalar(78.4263377603, 87.7689143744, 114.895847746), false);

                                genderNet.SetInput(blob);

                                using var genderPreds = genderNet.Forward();

                                double min_index_gender, max_index_gender;
                                OpenCvSharp.Point minLoc, maxLoc;
                                genderPreds.MinMaxLoc(out min_index_gender, out max_index_gender, out minLoc, out maxLoc);
                                var gender = genderList[maxLoc.X];
                                newFaceRecognition.gender = new() { result = gender, confidence = max_index_gender };

                                ageNet.SetInput(blob);
                                using var agePreds = ageNet.Forward();
                                agePreds.MinMaxLoc(out min_index_gender, out max_index_gender, out minLoc, out maxLoc);
                                var age = ageList[maxLoc.X];
                                newFaceRecognition.age = new() { result = age, confidence = max_index_gender };

                                string label = $"{gender},{age}";

                                //人脸
                                var faceimg = new Mat(image,
                                    new OpenCvSharp.Range(y1, y2),
                                    new OpenCvSharp.Range(x1, x2));

                                var newsize = new OpenCvSharp.Size(faceimg.Cols, faceimg.Rows);
                                using var frame = new Mat();
                                Cv2.Resize(faceimg, frame, newsize);

                                //Mat input_imag = new();
                                //Cv2.CopyTo(frame, input_imag);

                                //通过宽高比筛选
                                bool isWHRatioMatch = false;

                                float wh_ratio = (float)faceimg.Cols / faceimg.Rows;

                                if (0.7 < wh_ratio && wh_ratio < 0.9)
                                {
                                    isWHRatioMatch = true;
                                }

                                //hsv，主要筛选出过暗/过亮头像
                                Mat hsv = new();
                                Cv2.CvtColor(frame, hsv, ColorConversionCodes.BGR2HSV);

                                Scalar scalar = Cv2.Mean(hsv);

                                bool ishsv_qualified = false;

                                //HSV 中的色相表示颜色，HSV 中的饱和度表示灰色，HSV 中的值表示亮度。
                                //HSV 中的色相范围为[0，179]，HSV 中的饱和度范围为[0，255]，HSV 中的值范围为[0，255]
                                if (0 < scalar.Val0 && scalar.Val0 < 179 &&
                                    0 < scalar.Val1 && scalar.Val1 < 255 &&
                                    85 < scalar.Val2 && scalar.Val2 < 200)
                                {
                                    ishsv_qualified = true;
                                }

                                //只挑选符合条件的
                                if (gender == "Female" && newFaceRecognition.gender.confidence>0.9 && ishsv_qualified && isWHRatioMatch)
                                {
                                    if(isOnlySaveFace)
                                    {
                                        //检查是否有SaveImage无法识别的字符
                                        //若有，则先Base64编码
                                        bool needModifyDirctory = false;
                                        var DirectoryName = Path.GetFileName(SavePath);
                                        if (DirectoryName.Contains("・"))
                                        {
                                            DirectoryName = Convert.ToBase64String(Encoding.UTF8.GetBytes(DirectoryName));
                                            SavePath = Path.Combine(Path.GetDirectoryName(SavePath), DirectoryName);
                                            imagePath = Path.Combine(SavePath, $"{imageName}.jpg");
                                            needModifyDirctory = true;
                                        }

                                        FileMatch.CreateDirectoryIfNotExists(SavePath);

                                        faceimg.SaveImage(imagePath);

                                        //目录重命名
                                        if (needModifyDirctory)
                                        {
                                            //var srcimagePath = imagePath;
                                            DirectoryName = Encoding.UTF8.GetString(Convert.FromBase64String(DirectoryName));
                                            var dstimagePath = Path.Combine(Path.GetDirectoryName(SavePath), DirectoryName);

                                            Directory.Move(Path.GetDirectoryName(imagePath), dstimagePath);
                                        }
                                    }
                                    isGetFemaleImage = true;
                                }

                                Cv2.PutText(frame, label, new OpenCvSharp.Point(5, 25), HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 255, 255), 2, LineTypes.Link4);


                                progressInfo.faceList.Add(newFaceRecognition);

                                if (isShowWindow)
                                {
                                    Cv2.ImShow($"{Thread.CurrentThread.ManagedThreadId} Age Gender Demo", faceimg);

                                    //showOriginImage(image, starttime);
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
                        if (isShowWindow)
                        {
                            image.PutText("Playing", new OpenCvSharp.Point(10, 300), HersheyFonts.HersheyScriptComplex, 5, Scalar.Blue, 3);
                            showOriginImage(image, starttime);
                        }
                    }
                }
            }
            GC.Collect();

            if (isShowWindow)
            {
                Cv2.DestroyAllWindows();
                //Cv2.DestroyWindow($"{Thread.CurrentThread.ManagedThreadId} Origin Video");
                //Cv2.DestroyWindow($"{Thread.CurrentThread.ManagedThreadId} Age Gender Demo");
            }
            cap.Release();
        }

        private void SaveOriginOrFaceImage(Mat faceimg, string imagePath, string iamgeName)
        {
        }


        void showOriginImage(Mat image, DateTime starttime, List<int> bbox = null)
        {
            //源
            var diff = DateTime.Now - starttime;
            var fpsinfo = $"fps: nan";
            if (diff.Milliseconds > 0)
            {
                var fpsval = 1.0 / diff.Milliseconds * 1000;
                fpsinfo = $"fps: {fpsval:00}";
            }

            Cv2.PutText(image, fpsinfo, new OpenCvSharp.Point(10, 120), HersheyFonts.HersheySimplex, 5, Scalar.White, 2, LineTypes.Link4);
            //Cv2.PutText(image, fpsinfo, new OpenCvSharp.Point(10, 20), HersheyFonts.HersheyComplexSmall, 1, Scalar.White);

            if (bbox != null)
            {
                Cv2.Rectangle(image, new OpenCvSharp.Point(bbox[0], bbox[1]), new OpenCvSharp.Point(bbox[2], bbox[3]), Scalar.Green, image.Rows / 150);
            }

            var demosize = new OpenCvSharp.Size(image.Cols / 3f, image.Rows / 3f);
            using var demoframe = new Mat();
            Cv2.Resize(image, demoframe, demosize);
            Cv2.ImShow($"{Thread.CurrentThread.ManagedThreadId} Origin Video", demoframe);
        }

        //人脸框检测
        List<List<int>> getFaceBox(Mat frameImage)
        {
            List<List<int>> bboxes = new();


            using var blob = CvDnn.BlobFromImage(frameImage, 1.0, new OpenCvSharp.Size(300, 300),
                        new Scalar(104, 117, 123), false, false);
            faceNet.SetInput(blob, "data");

            using var detection = faceNet.Forward("detection_out");
            using var detectionmat = new Mat(detection.Size(2), detection.Size(3), MatType.CV_32F, detection.Ptr(0));

            var frameHeight = frameImage.Rows;
            var frameWidth = frameImage.Cols;

            for (int i = 0; i < detectionmat.Rows; i++)
            {
                float confidence = detectionmat.At<float>(i, 2);

                if (confidence > 0.7)
                {
                    int x1 = (int)(detectionmat.At<float>(i, 3) * frameWidth);
                    int y1 = (int)(detectionmat.At<float>(i, 4) * frameHeight);
                    int x2 = (int)(detectionmat.At<float>(i, 5) * frameWidth);
                    int y2 = (int)(detectionmat.At<float>(i, 6) * frameHeight);

                    bboxes.Add(new List<int> { x1, y1, x2, y2 });

                }
            }

            return bboxes;
        }

    }


    public class FaceRecognition
    {
        public predictLable gender;
        public predictLable age;

        public List<int> facebox;

        public class predictLable
        {
            public string result;
            public double confidence;
        }

    }

    public class progressInfo
    {
        public int PixelHeight;
        public int PixelWidth;
        public string imagePath;
        public List<FaceRecognition> faceList;
    }
}
