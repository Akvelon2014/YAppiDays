using System.Collections.Generic;
using System.Web;
using MobileConference.Models;

namespace MobileConference.Interface
{
    public interface IImageManager
    {

        /// <param name="picture"></param>
        /// <returns>New name as model for IImageRepository</returns>
        PictureNameModel Save(HttpPostedFileBase picture);
        
        PictureNameModel Save(string fileName, ImageSizeParams cropParams);
        PictureNameModel LoadFromThirdSite(string pictureLink, ImageSizeParams cropSize);
        PictureNameModel Rotate(string fileName);
        /// <summary>
        /// Get full name of image
        /// </summary>
        /// <param name="picture">model which describe the picture (get from IImageRepository)</param>
        /// <returns>name of picture</returns>
        string GetImageName(PictureNameModel picture, bool forUser = false, bool isGetNullIfEmpty = false);
        string GetStandardImageName(bool forUser = false);

        /// <summary>
        /// Get full name of mini-image
        /// </summary>
        /// <param name="picture">model which describe the picture (get from IImageRepository)</param>
        /// <returns>name of mini picture</returns>
        string GetMiniImageName(PictureNameModel picture, bool forUser = false, bool isGetNullIfEmpty = false);
        string GetStandardMiniImageName(bool forUser = false);
        
        bool ClearTempororyImages(List<PictureNameModel> list);
        void RemoveImage(string imageName);
        void RemoveImage(PictureNameModel imageName);

        PictureNameModel PhotoLoad(HttpPostedFileBase picture);
    }
}
