using System;
using System.Globalization;
using MVCFramework;
using MVCFramework.Commons;
using MVCFramework.Serializer.Commons;
using JsonDataObjects;
using uServices;

namespace uBase.Controller
{
    [MVCPath("/")]
    public class BaseController : MVCController
    {
        protected JsonObject PageData;
        private TGenreService _genreService;
        private TMovieService _movieService;

        protected override void OnBeforeAction(TWebContext context, string actionName, ref bool handled)
        {
            base.OnBeforeAction(context, actionName, ref handled);
            PageData = new JsonObject();
            ViewData["page"] = PageData;
            PageData["copyright"] = string.Format("Copyright {0}", DateTime.Now.ToString("yyyy", CultureInfo.InvariantCulture));
            PageData["version"] = string.Format("Version {0}", GetFileVersion(ParamStr(0)));
        }

        protected override void OnAfterAction(TWebContext context, string actionName)
        {
            PageData.Dispose();
            base.OnAfterAction(context, actionName);
        }

        public override void Dispose()
        {
            if (_genreService != null)
            {
                _genreService.Dispose();
                _genreService = null;
            }
            if (_movieService != null)
            {
                _movieService.Dispose();
                _movieService = null;
            }
            base.Dispose();
        }

        protected TMovieService GetMovieService()
        {
            if (_movieService == null)
                _movieService = new TMovieService();
            return _movieService;
        }

        protected TGenreService GetGenreService()
        {
            if (_genreService == null)
                _genreService = new TGenreService();
            return _genreService;
        }

        [MVCPath("")]
        [MVCHTTPMethod(HttpMethod.GET)]
        [MVCProduces("text/html")]
        public string Index()
        {
            return RenderViews(new string[] { "header", "index", "footer" });
        }

        private int GetFileVersion(string fileName)
        {
            return 1;
        }

        private string ParamStr(int index)
        {
            return AppDomain.CurrentDomain.FriendlyName;
        }
    }
}
