using System.Collections.Generic;
using MobileConference.Enums;
using MobileConference.Models;

namespace MobileConference.Interface
{
    public interface IImageRepository
    {
        PictureNameModel GetAvatar(int userId);
        PictureNameModel GetStandardAvatar(bool forUser = false);
        PictureNameModel GetIdeaAvatar(int ideaId);
        PictureNameModel GetPlatformAvatar(int? platformId);
        PictureNameModel GetMaterialAvatar(int materialId);
        PictureNameModel GetEventAvatar(int eventId);
        PictureNameModel GetCompanyAvatar(int companyId);
        PictureNameModel GetPictureById(int id);
        List<PictureNameModel> GetIdeaPictures(int ideaId);
        List<PictureNameModel> GetEventPictures(int eventId);
        List<PictureNameModel> GetImagesFor(int assignId, PictureType assignType, bool withDeleted = false);

        void SaveAvatar(PictureNameModel picture, int userId);
        void SaveIdeaPicture(PictureNameModel picture, int ideaId);
        void SaveEventPicture(PictureNameModel picture, int eventId);
        int SaveImagesFor(PictureNameModel picture, int assignId, PictureType assignType);
        int SavePicture(PictureNameModel pic, bool asTemporary = false);

        void SetIdeaAvatar(PictureNameModel picture, int ideaId);
        void SetPlatformAvatar(PictureNameModel picture, int platformId);
        void SetMaterialAvatar(PictureNameModel picture, int materialId);
        void SetEventAvatar(PictureNameModel picture, int eventId);
        void SetCompanyAvatar(PictureNameModel picture, int companyId);
        
        void AssignPicture(int id, PictureType pictureType, int assignId);

        void DeleteIdeaPicture(PictureNameModel picture, int ideaId);
        void DeleteEventPicture(PictureNameModel picture, int eventId);
        void DeleteIdeaPicture(int pictureId, int ideaId);
        void DeleteImageForComment(int commentId);
        void DeleteImageFor(PictureNameModel picture, int assignId, PictureType assignType);
        void DeleteImageFor(int pictureId, int assignId, PictureType assignType);

        int? GetIdForUrl(string url);
        bool IsCorrectImageForIdea(int ideaId, int imageId);

        IEnumerable<PictureStore> GetTemporaryImageForDeleting();
        bool DeleteImages(IEnumerable<PictureStore> list);
    }
}
