using System.Web;
using System.Web.Optimization;

namespace Rebound
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/moment.min.js",
                      "~/Scripts/site.js",
                      "~/Content/chosen.jquery.js",
                      "~/vendor/datatables/bootstrap.bundle.js",
                      "~/vendor/datatables/jquery.dataTables.min.js",
                      "~/vendor/datatables/dataTables.bootstrap4.js",
                      "~/vendor/chart.js/Chart.min.js",
                      "~/Scripts/bootstrap-datepicker.min.js"
                      ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/style.css",
                      "~/Content/bootstrap.css",
                      "~/vendor/datatables/dataTables.bootstrap4.min.css",
                      "~/Content/chosen.css",
                      "~/Content/site.css"));
        }
    }
}
