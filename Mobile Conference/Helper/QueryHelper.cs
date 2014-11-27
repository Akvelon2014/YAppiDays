using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Castle.Core.Internal;
using MobileConference.Models;

namespace MobileConference.Helper
{
    /// <summary>
    /// Extensions for some Entities and Queriable collection of Entities
    /// </summary>
    public static class QueryHelper
    {
        public static string FullName(this UserProfile user)
        {
            return String.Format("{0} {1} {2} ({3})", user.LastName, user.FirstName, user.SecondName ?? string.Empty, user.Login);
        }


        public static string FullName(this ProfileModel user)
        {
            if (user.LastName.IsNullOrEmpty() && user.FirstName.IsNullOrEmpty())
            {
                return user.Login;
            }
            return String.Format("{0} {1} {2}", user.LastName, user.FirstName, user.SecondName ?? string.Empty);
        }


        public static IQueryable<Idea> Filter(this IQueryable<Idea> ideas, string search)
        {
            string searchText = PrepareForFilter(search);
            return ideas.Where(idea => idea.Title.ToLower().Contains(searchText));
        }


        public static IQueryable<UserProfile> Filter(this IQueryable<UserProfile> users, string search)
        {
            string searchText = PrepareForFilter(search);
            return users.Where(user => user.Login.ToLower().Contains(searchText) ||
                                       user.LastName.ToLower().Contains(searchText) ||
                                       user.FirstName.ToLower().Contains(searchText) ||
                                       searchText.Contains(user.Login.ToLower()) ||
                                       searchText.Contains(user.LastName.ToLower()) ||
                                       searchText.Contains(user.FirstName.ToLower()));
        }


        public static IQueryable<Material> Filter(this IQueryable<Material> materials, string search)
        {
            string searchText = PrepareForFilter(search);
            return materials.Where(material => material.Title.ToLower().Contains(searchText));
        }


        public static IEnumerable<SelectListItem> ToListItem(this Dictionary<int, string> dictionary, int? selectedItem,
            List<int> exceptItem = null )
        {
            if (exceptItem != null)
            {
                dictionary = dictionary.Where(pair => !exceptItem.Contains(pair.Key)).ToDictionary(pair=>
                    pair.Key,pair=>pair.Value);
            }
            return dictionary.Select(pair => new SelectListItem
            {
                Selected = (selectedItem!=null && selectedItem == pair.Key),
                Text = pair.Value,
                Value = pair.Key.ToString()
            });
        }


        public static IEnumerable<SelectListItem> ToListItem(this Dictionary<int, string> dictionary, string selectedItem,
            List<string> exceptItem = null )
        {
            if (exceptItem != null)
            {
                dictionary = dictionary.Where(pair => !exceptItem.Contains(pair.Value)).ToDictionary(pair =>
                    pair.Key, pair => pair.Value);
            }
            return dictionary.Select(pair => new SelectListItem
            {
                Selected = (selectedItem != null && selectedItem == pair.Value),
                Text = pair.Value,
                Value = pair.Key.ToString()
            });
        }

        private static string PrepareForFilter(string text)
        {
            return text.ToLower().Trim().Replace("  ", " ");
        }
    }
}