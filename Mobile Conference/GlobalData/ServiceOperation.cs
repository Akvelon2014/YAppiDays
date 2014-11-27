using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using MobileConference.Helper;
using MobileConference.Interface;
using MobileConference.Models;

namespace MobileConference.GlobalData
{
    public static class ServiceOperation
    {
        public static bool DoneAllOperations(string password)
        {
            var dataProtector = ContainerDI.Container.Resolve<IDataProtectorManager>();
            string legalPassword = ConfigurationManager.AppSettings.Get("ServicePassword");
            string encryptPassword = dataProtector.Decrypt(legalPassword);
            if (password != encryptPassword) return false;
            ClearAllTemporaryImages();
            return true;
        }

        private static bool ClearAllTemporaryImages()
        {
            var imageManager = ContainerDI.Container.Resolve<IImageManager>();
            var imageRepository = ContainerDI.Container.Resolve<IImageRepository>();
            IEnumerable<PictureStore> list = imageRepository.GetTemporaryImageForDeleting().ToList();
            bool result = imageManager.ClearTempororyImages(list: list.ToModel());
            if (!result) return false;
            return imageRepository.DeleteImages(list);
        }
    }
}