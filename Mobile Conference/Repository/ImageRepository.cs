using System.Data.Objects;
using System.Runtime.Remoting.Messaging;
using MobileConference.Enums;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Managers;
using MobileConference.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;


namespace MobileConference.Repository
{
    /// <summary>
    /// Repository for manage Image in database
    /// </summary>
    public class ImageRepository:IImageRepository
    {
        private Entities context
        {
            get { return ContextEF.Context; }
        }

        #region User Avatars

        /// <summary>
        /// Save avatar for user
        /// </summary>
        public void SaveAvatar(PictureNameModel pic, int userId)
        {
            UserProfile userProfile = context.UserProfiles.FirstOrDefault(user => user.Id == userId);
            if (userProfile==null) return;
            int pictureId = SavePicture(pic);
            int oldId = -1;
            if (userProfile.Avatar != null)
            {
                oldId = (int)userProfile.Avatar;
            }
            AssignPicture(pictureId, PictureType.UserAvatar, userId);
            userProfile.Avatar = pictureId;
            context.SaveChanges();
            if (oldId >= 0)
            {
                DeleteImageFor(oldId, userId, PictureType.UserAvatar);
            }
            CacheManager.UpdateAvatar(userId,pic);
        }


        /// <summary>
        /// Get avatar for user
        /// </summary>
        public PictureNameModel GetAvatar(int userId)
        {
            PictureNameModel model;
            if (CacheManager.TryReadAvatar(userId, out model)) return model;
            UserProfile userProfile = context.UserProfiles.FirstOrDefault(user => user.Id == userId);
            if (userProfile == null || userProfile.Avatar==null) return null;
            model = new PictureNameModel
            {
                GuidString = userProfile.PictureStore.Picture,
                CreationDate = userProfile.PictureStore.Date
            };
            CacheManager.UpdateAvatar(userId,model);
            return model;
        }

        #endregion //User Avatars

        #region Idea pictures and avatars

        public void SetIdeaAvatar(PictureNameModel picture, int ideaId)
        {
            Idea idea = context.Ideas.FirstOrDefault(i => i.Id == ideaId);
            if (idea == null) return;
            int pictureId = SavePicture(picture);
            int oldId = -1;
            if (idea.Avatar != null)
            {
                oldId = (int)idea.Avatar;
            }
            AssignPicture(pictureId, PictureType.IdeaAvatar, ideaId);
            idea.Avatar = pictureId;
            context.SaveChanges();
            if (oldId >= 0)
            {
                DeleteImageFor(oldId,ideaId,PictureType.IdeaAvatar);
            }
            CacheManager.UpdateIdeaAvatar(ideaId, picture);
        }


        public PictureNameModel GetIdeaAvatar(int ideaId)
        {
            PictureNameModel model;
            if (CacheManager.TryReadIdeaAvatar(ideaId, out model)) return model;
            Idea idea = context.Ideas.FirstOrDefault(i => i.Id == ideaId);
            if (idea == null || idea.Avatar == null) return GetStandardAvatar();
            model = new PictureNameModel
            {
                GuidString = idea.PictureStore.Picture,
                CreationDate = idea.PictureStore.Date
            };
            CacheManager.UpdateIdeaAvatar(ideaId, model);
            return model;
        }


        public void SaveIdeaPicture(PictureNameModel picture, int ideaId)
        {
            Idea idea = context.Ideas.FirstOrDefault(i => i.Id == ideaId);
            if (idea == null) return;
            int pictureId = SavePicture(picture);
            AssignPicture(pictureId, PictureType.Idea, ideaId);
            CacheManager.ClearPicturesForIdea(ideaId);
            CacheManager.UpdateIdeaPictures(ideaId,GetIdeaPictures(ideaId));
        }

        public List<PictureNameModel> GetIdeaPictures(int ideaId)
        {
            List<PictureNameModel> pictures;
            if (CacheManager.TryReadIdeaPictures(ideaId, out pictures)) return pictures;
            pictures = context.PictureStores.Where(pic => !pic.IsDeleted && pic.PictureAssigns
                .FirstOrDefault(assign => assign.AssignId == ideaId &&
                assign.PictureAssignType.Id == (int)PictureType.Idea) != null).Select(pic => new PictureNameModel
                {
                    CreationDate = pic.Date,
                    GuidString = pic.Picture,
                    Id = pic.Id
                }).ToList();
            CacheManager.UpdateIdeaPictures(ideaId, pictures);
            return pictures;
        }


        public void DeleteIdeaPicture(PictureNameModel picture, int ideaId)
        {
            DeleteImageFor(picture, ideaId, PictureType.Idea);
            CacheManager.ClearPicturesForIdea(ideaId);
            CacheManager.UpdateIdeaPictures(ideaId, GetIdeaPictures(ideaId));
        }


        public void DeleteIdeaPicture(int pictureId, int ideaId)
        {
            DeleteImageFor(pictureId, ideaId, PictureType.Idea);
            CacheManager.ClearPicturesForIdea(ideaId);
            CacheManager.UpdateIdeaPictures(ideaId, GetIdeaPictures(ideaId));
        }

        public bool IsCorrectImageForIdea(int ideaId, int imageId)
        {
            var picture = context.PictureStores.FirstOrDefault(pic => pic.Id == imageId && !pic.IsDeleted &&
                pic.PictureAssigns.FirstOrDefault(im => im.AssignId == ideaId && (im.PictureAssignType.Id == (int)PictureType.Idea ||
                im.PictureAssignType.Id == (int)PictureType.IdeaAvatar)) != null);
            return picture != null;
        }

        #endregion // Idea pictures and avatars

        #region Event pictures and avatars

        public void SetEventAvatar(PictureNameModel picture, int eventId)
        {
            var eventProfile = context.EventProfiles.FirstOrDefault(e => e.Id == eventId);
            if (eventProfile == null) return;
            int pictureId = SavePicture(picture);
            int oldId = -1;
            if (eventProfile.Avatar != null)
            {
                oldId = (int)eventProfile.Avatar;
            }
            AssignPicture(pictureId, PictureType.EventAvatar, eventId);
            eventProfile.Avatar = pictureId;
            context.SaveChanges();
            if (oldId >= 0)
            {
                DeleteImageFor(oldId, eventId, PictureType.EventAvatar);
            }
        }


        public PictureNameModel GetEventAvatar(int eventId)
        {
            EventProfile eventProfile = context.EventProfiles.FirstOrDefault(e => e.Id == eventId);
            if (eventProfile == null || eventProfile.Avatar == null) return null;
            return GetPictureById((int)eventProfile.Avatar);
        }


        public void SaveEventPicture(PictureNameModel picture, int eventId)
        {
            EventProfile eventProfile = context.EventProfiles.FirstOrDefault(e => e.Id == eventId);
            if (eventProfile == null) return;
            int pictureId = SavePicture(picture);
            AssignPicture(pictureId, PictureType.EventPicture, eventId);
        }


        public List<PictureNameModel> GetEventPictures(int eventId)
        {
            List<PictureNameModel> pictures = context.PictureStores.Where(pic => !pic.IsDeleted && pic.PictureAssigns
                .FirstOrDefault(assign => assign.AssignId == eventId &&
                assign.PictureAssignType.Id == (int)PictureType.EventPicture) != null).Select(pic => new PictureNameModel
                {
                    CreationDate = pic.Date,
                    GuidString = pic.Picture,
                    Id = pic.Id
                }).ToList();
            return pictures;
        }


        public void DeleteEventPicture(PictureNameModel picture, int eventId)
        {
            DeleteImageFor(picture, eventId, PictureType.EventPicture);
        }

        public void DeleteImageForComment(int commentId)
        {
            var images = GetImagesFor(commentId, PictureType.Comment);
            if (images.Any())
            {
                foreach (var image in images)
                {
                    DeleteImageFor(image,commentId,PictureType.Comment);
                }
            }
        }

        #endregion //Event pictures and avatars

        #region Platforms, Materials and Companies avatars

        public void SetPlatformAvatar(PictureNameModel picture, int platformId)
        {
            var platform = context.Platforms.FirstOrDefault(pl => pl.Id == platformId);
            if (platform == null) return;
            int pictureId = SavePicture(picture);
            int oldId = -1;
            if (platform.Picture != null)
            {
                oldId = (int)platform.Picture;
            }
            AssignPicture(pictureId, PictureType.PlatformAvatar, platformId);
            platform.Picture = pictureId;
            context.SaveChanges();
            PlatformModel.SinchronizeWithCache(platformId);
            if (oldId >= 0)
            {
                DeleteImageFor(oldId, platformId, PictureType.PlatformAvatar);
            }
        }


        public PictureNameModel GetPlatformAvatar(int? platformId)
        {
            if (platformId == null) return GetStandardAvatar();
            PlatformModel platform = PlatformModel.ForPlatform((int)platformId);
            if (platform == null || platform.PictureId == null) return null;
            return GetPictureById((int)platform.PictureId);
        }


        public void SetMaterialAvatar(PictureNameModel picture, int materialId)
        {
            var material = context.Materials.FirstOrDefault(mat => mat.Id == materialId);
            if (material == null) return;
            int pictureId = SavePicture(picture);
            var imagesOld = GetMaterialAvatar(materialId);
            AssignPicture(pictureId, PictureType.MaterialAvatar, materialId);
            material.PictureId = pictureId;
            context.SaveChanges();
            DeleteImageFor(imagesOld, materialId, PictureType.MaterialAvatar);
        }


        public PictureNameModel GetMaterialAvatar(int materialId)
        {
            Material material = context.Materials.FirstOrDefault(mat => mat.Id == materialId);
            if (material == null) return null;
            return (material.PictureId != null)
                ? GetPictureById((int)material.PictureId)
                : GetPlatformAvatar(material.PlatformId);
        }

       
        public void SetCompanyAvatar(PictureNameModel picture, int companyId)
        {
            var company = context.Companies.FirstOrDefault(c => c.Id == companyId);
            if (company == null) return;
            int pictureId = SavePicture(picture);
            int oldId = -1;
            if (company.Avatar != null)
            {
                oldId = (int)company.Avatar;
            }
            AssignPicture(pictureId, PictureType.CompanyAvatar, companyId);
            company.Avatar = pictureId;
            context.SaveChanges();
            if (oldId >= 0)
            {
                DeleteImageFor(oldId, companyId, PictureType.CompanyAvatar);
            }
        }

       
        public PictureNameModel GetCompanyAvatar(int companyId)
        {
            Company company = context.Companies.FirstOrDefault(c => c.Id == companyId);
            if (company == null || company.Avatar == null) return null;
            return GetPictureById((int)company.Avatar);
        }

        #endregion //Platforms, Materials and Companies avatars

        // For other images use follow methods:

        #region Common utility for working with Images

        public List<PictureNameModel> GetImagesFor(int assignId, PictureType assignType, bool withDeleted = false)
        {
            List<PictureNameModel> pictures = context.PictureStores.Where(pic => !pic.IsDeleted && pic.PictureAssigns.
                FirstOrDefault(assign => assign.AssignId == assignId &&
                                         assign.AssignTypeId == (int)assignType) != null).Select(pic => new PictureNameModel
                                         {
                                             CreationDate = pic.Date,
                                             GuidString = pic.Picture,
                                             Id = pic.Id
                                         }).ToList();
            return pictures;
        }


        public PictureNameModel GetPictureById(int id)
        {
            var picture = context.PictureStores.FirstOrDefault(pic => pic.Id == id);
            if (picture == null) return null;
            return new PictureNameModel { CreationDate = picture.Date, GuidString = picture.Picture };
        }


        public int? GetIdForUrl(string url)
        {
            string[] array = url.Split('/');
            if (array.Count() < 3) return null;
            string folder = array[array.Count() - 2];
            string[] parseFolder = folder.Split('-');
            if (parseFolder.Count() != 2) return null;
            string fileName = array[array.Count() - 1];
            string[] parseFileName = fileName.Split('.');
            if (parseFileName.Count() != 2) return null;

            fileName = parseFileName[0];
            int month = int.Parse(parseFolder[0]);
            int year = int.Parse(parseFolder[1]);
            var picture = context.PictureStores.FirstOrDefault(pic => pic.Date.Year == year && pic.Date.Month == month &&
                                                        pic.Picture == fileName);
            return (picture == null) ? (int?)null : picture.Id;
        }


        public PictureNameModel GetStandardAvatar(bool forUser = false)
        {
            string name = (forUser) ? "Std" : "StdProject";
            return new PictureNameModel
            {
                GuidString = name,
                CreationDate = new DateTime(2012, 7, 1)//begin to work
            };
        }


        public int SaveImagesFor(PictureNameModel picture, int assignId, PictureType assignType)
        {
            int pictureId = SavePicture(picture);
            AssignPicture(pictureId, assignType, assignId);
            return pictureId;
        }


        public int SavePicture(PictureNameModel pic, bool asTemporary = false)
        {
            var picture = new PictureStore { Picture = pic.GuidString, Date = pic.CreationDate };
            context.PictureStores.Add(picture);
            context.SaveChanges();
            if (asTemporary)
            {
                AssignPicture(picture.Id, PictureType.TemporaryPicture, ProfileModel.Current != null ? ProfileModel.Current.Id : -1);
            }
            return picture.Id;
        }


        public void AssignPicture(int id, PictureType pictureType, int assignId)
        {
            int assignType = (context.PictureAssignTypes.FirstOrDefault(type => type.Id == (int)pictureType) ??
                             context.PictureAssignTypes.First(type => type.Id == (int)PictureType.Unknown)).Id;
            int? user = null;
            var membershipUser = Membership.GetUser();
            if (membershipUser != null)
            {
                var providerUserKey = membershipUser.ProviderUserKey;
                if (providerUserKey != null) user = (int)providerUserKey;
            }

            context.PictureAssigns.Add(new PictureAssign
            {
                PictureId = id,
                AssignTypeId = assignType,
                AssignId = assignId,
                Creator = user,
                IsDeleted = false
            });
            context.SaveChanges();
        }


        public void DeleteImageFor(PictureNameModel picture, int assignId, PictureType assignType)
        {
            var pictureInDb = context.PictureStores.FirstOrDefault(pic => pic.Picture == picture.GuidString &&
                pic.Date.Year == picture.CreationDate.Year && pic.Date.Month == picture.CreationDate.Month 
                && pic.PictureAssigns.FirstOrDefault(assign => assign.AssignId == assignId &&
                assign.PictureAssignType.Id == (int)assignType) != null);
            if (pictureInDb == null) return;
            var currentAssign = pictureInDb.PictureAssigns.FirstOrDefault(assign => assign.AssignId == assignId &&
                assign.PictureAssignType.Id == (int)assignType);
            if (currentAssign == null) return;
            RemoveAssign(currentAssign,pictureInDb);
        }


        public void DeleteImageFor(int pictureId, int assignId, PictureType assignType)
        {
            PictureStore pict = context.PictureStores.FirstOrDefault(pic=>pic.Id == pictureId);
            if (pict == null) return;
            var currentAssign = pict.PictureAssigns.FirstOrDefault(assign => assign.AssignId == assignId &&
                assign.PictureAssignType.Id == (int)assignType);
            if (currentAssign == null) return;
            RemoveAssign(currentAssign, pict);
        }


        private void RemoveAssign(PictureAssign removedAssign, PictureStore store)
        {
            removedAssign.IsDeleted = true;
            context.SaveChanges();
            if (store.PictureAssigns.Count(assign => assign.IsDeleted != true) == 0)
            {
                store.IsDeleted = true;
                context.SaveChanges();
                if (OptionModel.Current.IsPhisicalDelete)
                {
                    var imageManager = ContainerDI.Container.Resolve<IImageManager>();
                    imageManager.RemoveImage(new PictureNameModel{CreationDate = store.Date, GuidString = store.Picture});
                }
            }
        }

        #endregion //Common utility for working with Images

        #region Temporary Images

        public IEnumerable<PictureStore> GetTemporaryImageForDeleting()
        {
            DateTime today = DateTime.Now;
            var temporaryImagesList =  context.PictureStores.Where(p => !p.IsDeleted && ((EntityFunctions.DiffDays(p.Date, today) 
                > GlobalValuesAndStrings.DaysForStoreTemporaryImagesBeforeRemoving && p.PictureAssigns.Where(a=>!a.IsDeleted)
                .All(i=>i.AssignTypeId == (int)PictureType.TemporaryPicture)) ||!p.PictureAssigns.Any()));
            return temporaryImagesList;
        }

        public bool DeleteImages(IEnumerable<PictureStore> list)
        {
            foreach (var image in list.ToList())
            {
                if (image.PictureAssigns != null && image.PictureAssigns.Any())
                {
                    foreach (var pictureAssign in image.PictureAssigns.ToList())
                    {
                        context.PictureAssigns.Remove(pictureAssign);
                    }
                }
                if(context.PictureStores.FirstOrDefault(i=>i.Id == image.Id)!=null) context.PictureStores.Remove(image);
            }
            context.SaveChanges();
            return true;
        }

        #endregion //Temporary Images
    }
}