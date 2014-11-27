using System.Web.Optimization;

namespace MobileConference
{
    /// <summary>
    /// Register Bundling when application was started
    /// </summary>
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-1.7.2.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js", "~/Scripts/scrollbar.js", "~/Scripts/jquery.imgareaselect.js",
                        "~/Scripts/jquery-maginfict.js", "~/Scripts/jquery.datetimepicker.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/MainScript").Include("~/Scripts/MainScript.js",
               "~/Scripts/CustomInputs.js", "~/Scripts/PictureScript.js", "~/Scripts/CustomTips.js"));
            bundles.Add(new ScriptBundle("~/bundles/PictureDialog").Include("~/Scripts/PictureDialog.js"));
            bundles.Add(new ScriptBundle("~/bundles/ModernIElikeOld").Include("~/Scripts/modernIE_likeOld.js"));
            bundles.Add(new ScriptBundle("~/bundles/AdminScript").Include("~/Scripts/AdminScript.js"));
            bundles.Add(new ScriptBundle("~/bundles/Chat").Include("~/Scripts/ChatUpdate.js"));
            bundles.Add(new ScriptBundle("~/bundles/CommentsShow").Include("~/Scripts/CommentsShow.js"));

            // Use of custom stylesheets
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/CustomInputs.css",
                "~/Content/960.css",         
                "~/Content/PagedList.css", 
                "~/Content/PictureDialog.css",
                "~/Content/Site.css",
                "~/Content/themes/base/imgareaselect.css",
                "~/Content/CustomTips.css",
                "~/Content/jquery.datetimepicker.css"
                ));

            bundles.Add(new StyleBundle("~/Content/Admin").Include("~/Content/Admin.css"));

            // jQuery styles
            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css",
                        "~/Content/themes/base/jquery.ui.maginfict.css",
                        "~/Content/themes/base/jquery.ui.scrollbar.css"));
        }
    }
}