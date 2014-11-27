using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using MobileConference.GlobalData;
using MobileConference.Interface;
using ImageResizer;
using MobileConference.Models;

namespace MobileConference.Managers
{
    /// <summary>
    /// Work with image as phisical objects (not database entities, for this case it's should used Image Repository)
    /// </summary>
    public class ImageManager:IImageManager
    {
        public PictureNameModel Save(HttpPostedFileBase picture)
        {
            if (picture == null) return null;
            string fileName;
            string fileNameMini;
            DateTime currentDate;
            string guid = GetNewImageName(out fileName, out fileNameMini, out currentDate);

            var extension = Path.GetExtension(picture.FileName);
            if (extension == null || picture.ContentLength==0) return null;
            extension = extension.ToLower();
            if (extension != ".gif" && extension != ".jpg" && extension != ".png") return null;
           
            BuildImage(picture, fileName, GlobalValuesAndStrings.ImageWidth, GlobalValuesAndStrings.ImageHeight);
            BuildImage(picture, fileNameMini, GlobalValuesAndStrings.MiniImageWidth, GlobalValuesAndStrings.MiniImageHeight);
            return new PictureNameModel{CreationDate = currentDate, GuidString = guid};
        }


        public PictureNameModel Save(string fileName, ImageSizeParams cropParams)
        {
            var fullFileName = HttpContext.Current.Server.MapPath(fileName);
            if (!File.Exists(fullFileName)) return null;
            string newFileName;
            string fileNameMini;
            DateTime currentDate;
            string guid = GetNewImageName(out newFileName, out fileNameMini, out currentDate);
            TransformImage(fullFileName, newFileName, cropParams);
            TransformImage(newFileName+".jpg",fileNameMini,GlobalValuesAndStrings.MiniImageWidth, GlobalValuesAndStrings.MiniImageHeight);
            return new PictureNameModel { CreationDate = currentDate, GuidString = guid };            
        }


        public PictureNameModel Rotate(string fileName)
        {
            var fullFileName = HttpContext.Current.Server.MapPath(fileName);
            if (!File.Exists(fullFileName)) return null;
            string newFileName;
            string fileNameMini;
            DateTime currentDate;
            string guid = GetNewImageName(out newFileName, out fileNameMini, out currentDate);
            newFileName += ".jpg";
            using (Image img = Image.FromFile(fullFileName))
            {
                //rotate the picture by 90 degrees and re-save the picture as a Jpeg
                img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                img.Save(newFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            TransformImage(newFileName, fileNameMini, GlobalValuesAndStrings.MiniImageWidth, GlobalValuesAndStrings.MiniImageHeight);
            RemoveImage(fileName.Replace("Images", "ImagesMini"));
            RemoveImage(fileName);
            return new PictureNameModel { CreationDate = currentDate, GuidString = guid };            
        }

        public bool ClearTempororyImages(List<PictureNameModel> list)
        {
            var standard = GetStandardImageName();
            var standardMini = GetStandardMiniImageName();
            foreach (var picture in list)
            {
                string imageName = GetImageName(picture);
                if (imageName != standard)
                {
                    RemoveImage(imageName);
                }
                imageName = GetMiniImageName(picture);
                if (imageName != standardMini)
                {
                    RemoveImage(imageName);
                }
            }
            return true;
        }


        public void RemoveImage(string imageName)
        {
            if (Exists(imageName))
            {
                File.Delete(HttpContext.Current.Server.MapPath(imageName));
            }
        }

        public void RemoveImage(PictureNameModel image)
        {
            string name = GetImageName(image, false, true);
            if (name != null)
            {
                RemoveImage(name);
            }
            name = GetMiniImageName(image, false, true);
            if (name != null)
            {
                RemoveImage(name);
            }
        }

        public string GetImageName(PictureNameModel picture, bool forUser = false, bool isGetNullIfEmpty = false)
        {
            if (picture == null)
            {
                if (isGetNullIfEmpty) return null;
                return (forUser) ? "/Images/std.jpg" : "/Images/StdProject.jpg";
            }
            string file = String.Format("/Images/{0}/{1}.jpg", GetFolderNameByDate(picture.CreationDate), picture.GuidString);
            return (Exists(file)) ? file : ((isGetNullIfEmpty)?null:GetStandardImageName(forUser));
        }


        public string GetMiniImageName(PictureNameModel picture, bool forUser = false, bool isGetNullIfEmpty = false)
        {
            if (picture == null)
            {
                if (isGetNullIfEmpty) return null;
                return (forUser) ? "/ImagesMini/std.jpg" : "/ImagesMini/StdProject.jpg";
            }
            string file = String.Format("/ImagesMini/{0}/{1}.jpg", GetFolderNameByDate(picture.CreationDate), picture.GuidString);
            return (Exists(file)) ? file : ((isGetNullIfEmpty) ? null : GetStandardMiniImageName(forUser));
        }

        public PictureNameModel LoadFromThirdSite(string pictureLink, ImageSizeParams cropSize)
        {
            string newFileName;
            string fileNameMini;
            DateTime currentDate;
            string guid = GetNewImageName(out newFileName, out fileNameMini, out currentDate);
            var webClient = new WebClient();
            webClient.DownloadFile(pictureLink, newFileName);
            TransformImage(newFileName, newFileName, cropSize);
            TransformImage(newFileName + ".jpg", fileNameMini, GlobalValuesAndStrings.MiniImageWidth, GlobalValuesAndStrings.MiniImageHeight);
            return new PictureNameModel { CreationDate = currentDate, GuidString = guid };   
        }

        private string GetNewImageName(out string fullFileName, out string fullFileNameMini, out DateTime currentDate)
        {
            string guid;
            currentDate = DateTime.Now;
            CreateFoldersIfItNeed(currentDate, "Images");
            CreateFoldersIfItNeed(currentDate, "ImagesMini");
            do
            {
                guid = Guid.NewGuid().ToString().Replace('{', ' ').Replace('}', ' ').Trim();
                fullFileName = HttpContext.Current.Server.MapPath(String.Format("/Images/{0}/{1}", GetFolderNameByDate(currentDate),
                    guid));
            } while (File.Exists(fullFileName));
            fullFileNameMini = HttpContext.Current.Server.MapPath(String.Format("/ImagesMini/{0}/{1}",
               GetFolderNameByDate(currentDate), guid));
            return guid;
        }

        private bool Exists(string pictureName)
        {
            return File.Exists(HttpContext.Current.Server.MapPath(pictureName));
        }


        private string GetFolderNameByDate(DateTime date)
        {
            return date.Month.ToString() + "-" + date.Year.ToString();
        }

        private void BuildImage(HttpPostedFileBase picture, string fileName, int width, int height)
        {
            var job = new ImageJob(picture, fileName, new Instructions(string.Format("mode=max;format=jpg;width={0};height={1}",
                width, height)))
            {
                AddFileExtension = true
            };
            job.Build();
        }


        private void TransformImage(string fileNameSource, string fileNameDest, ImageSizeParams cropParams)
        {
            var job = new ImageJob(fileNameSource, fileNameDest, new Instructions(string.Format("mode=max;format=jpg;crop=({0},{1},{2},{3}",
                cropParams.x1, cropParams.y1, cropParams.x2,cropParams.y2)))
            {
                AddFileExtension = true
            };
            job.Build();
        }

        
        private void TransformImage(string fileNameSource, string fileNameDest, int width, int height)
        {
            var job = new ImageJob(fileNameSource, fileNameDest, new Instructions(string.Format("mode=max;format=jpg;width={0};height={1}",
                width,height)))
            {
                AddFileExtension = true
            };
            job.Build();
        }


        private void CreateFoldersIfItNeed(DateTime date, string folder)
        {
            string folderName = HttpContext.Current.Server.MapPath(String.Format("/{0}/{1}",folder, GetFolderNameByDate(date)));
            if (!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);
        }


        public string GetStandardImageName(bool forUser = false)
        {
            return (forUser) ? "/Images/std.jpg" : "/Images/StdProject.jpg";
        }

        public string GetStandardMiniImageName(bool forUser = false)
        {
            return (forUser) ? "/ImagesMini/std.jpg" : "/ImagesMini/StdProject.jpg";
        }

        public PictureNameModel PhotoLoad(HttpPostedFileBase picture)
        {
            string newFileName;
            string fileNameMini;
            DateTime currentDate;
            string guid = GetNewImageName(out newFileName, out fileNameMini, out currentDate);

            var extension = Path.GetExtension(picture.FileName);
            if (extension == null || picture.ContentLength == 0) return null;
            extension = extension.ToLower();
            if (extension != ".gif" && extension != ".jpg" && extension != ".png") return null;

            BuildImage(picture, newFileName, GlobalValuesAndStrings.ImageWidth, GlobalValuesAndStrings.ImageHeight);
            var job = new ImageJob(picture, fileNameMini, new Instructions(string.Format("mode=crop;format=jpg;width={0};" +
                                "height={1};scale=both;alignment=middlecenter",GlobalValuesAndStrings.MiniImageWidth,
                                GlobalValuesAndStrings.MiniImageHeight)))
            {
                AddFileExtension = true
            };
            job.Build();
            return new PictureNameModel {CreationDate = currentDate, GuidString = guid};
        }
    }
}