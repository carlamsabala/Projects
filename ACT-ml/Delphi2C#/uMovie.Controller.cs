using System;
using System.Collections.Generic;
using MVCFramework;
using MVCFramework.Commons;
using JsonDataObjects;
using System.Threading;

namespace MovieApp.Controllers
{
    [MVCPath("/movie")]
    public class MovieController : BaseController
    {
        private bool _insertMode;
        private bool _includeBlankRow;

        protected void RenderForm(string[] viewNames)
        {
            PageData["InsertMode"] = _insertMode;
            PageData["IncludeBlankRow"] = _includeBlankRow;
            ResponseStream.Append(RenderViews(viewNames));
            RenderResponseStream();
        }

        [MVCDoc("Trigger a client side error")]
        [MVCPath("/error/400")]
        [MVCHTTPMethod(HttpMethod.GET)]
        [MVCConsumes("application/json")]
        public void TriggerError()
        {
            throw new Exception("Exception for client side");
        }

        [MVCDoc("Ask server to swap something on the page")]
        [MVCPath("/swap")]
        [MVCHTTPMethod(HttpMethod.PATCH)]
        [MVCConsumes("application/json")]
        [MVCProduces("text/html")]
        public void TriggerSwap()
        {
            Thread.Sleep(500);
            Context.Response.HXSetReswap(SwapOption.InnerHTML, 1000);
            Render(string.Format("... Button was clicked at {0}", DateTime.Now.ToString("hh:mm:ss")));
        }

        [MVCDoc("Get the list of Movies")]
        [MVCPath("/page")]
        [MVCHTTPMethod(HttpMethod.GET)]
        [MVCProduces("text/html")]
        public void GetMoviesPage()
        {
            var movies = GetMovieService().ListAll();
            try
            {
                ViewData["Movies"] = movies;
                if (Context.Request.IsHTMX)
                {
                    PageData["Explanation"] = "Loaded via a seamless ajax call";
                    Context.Response.HXSetPushUrl("/movie/page");
                    RenderForm(new string[] { "Movie" });
                }
                else
                {
                    PageData["Explanation"] = "Loaded via a full page reload, watch for the flicker in the title bar and see the calls in the browser Dev Tools console (network tab)";
                    RenderForm(new string[] { "Header", "Movie", "Footer" });
                }
            }
            finally
            {
                movies.Dispose();
            }
        }

        [MVCDoc("Search for movies")]
        [MVCPath("/search")]
        [MVCHTTPMethod(HttpMethod.POST)]
        [MVCConsumes("application/json")]
        [MVCProduces("text/html")]
        public void SearchMovies()
        {
            var parameters = JsonObject.Parse(Context.Request.Body) as JsonObject;
            var movies = GetMovieService().ListBySearchTerm(parameters["search"]);
            try
            {
                ViewData["Movies"] = movies;
                RenderForm(new string[] { "MovieDataRow" });
            }
            finally
            {
                movies.Dispose();
                parameters.Dispose();
            }
        }

        [MVCDoc("Get the page to edit an individual Movie")]
        [MVCPath("/($MovieID)/edit")]
        [MVCHTTPMethod(HttpMethod.GET)]
        [MVCProduces("text/html")]
        public void GetEditPanel(int movieID)
        {
            var movie = GetMovieService().GetByID(movieID);
            var genres = GetGenreService().GetGenresAsList(movie.GenreID);
            try
            {
                ViewData["Movies"] = movie;
                ViewData["Genres"] = genres;
                Context.Response.HXTriggerClientEvent("setFocus", ".focus", ClientEventType.Swapped);
                RenderForm(new string[] { "MovieDataEdit" });
            }
            finally
            {
                movie.Dispose();
                genres.Dispose();
            }
        }

        [MVCDoc("Render a dialog for an insert")]
        [MVCPath("/insert")]
        [MVCHTTPMethod(HttpMethod.GET)]
        [MVCProduces("text/html")]
        public void GetInsertPanel()
        {
            var movie = TMovie.CreateNew(true);
            var genres = GetGenreService().GetGenresAsList(movie.GenreID);
            try
            {
                movie.MovieID = GetMovieService().GetNextID();
                _insertMode = true;
                ViewData["Genres"] = genres;
                ViewData["Movies"] = movie;
                Context.Response.HXTriggerClientEvent("setFocus", ".focus", ClientEventType.Swapped);
                RenderForm(new string[] { "MovieDataEdit" });
            }
            finally
            {
                genres.Dispose();
                movie.Dispose();
            }
        }

        [MVCDoc("Render a specified Movie to the grid")]
        [MVCPath("/($MovieID)")]
        [MVCHTTPMethod(HttpMethod.GET)]
        [MVCProduces("text/html")]
        public void GetMovie(int movieID)
        {
            var movie = GetMovieService().GetByID(movieID);
            try
            {
                if (movie != null)
                    ViewData["Movies"] = movie;
                else
                    _includeBlankRow = true;
                RenderForm(new string[] { "MovieDataRow" });
            }
            finally
            {
                if (movie != null)
                    movie.Dispose();
            }
        }

        [MVCDoc("Update a specified Movie")]
        [MVCPath("/($MovieID)")]
        [MVCHTTPMethod(HttpMethod.PUT)]
        [MVCProduces("text/html")]
        public void UpdateMovie(int movieID)
        {
            var movie = Context.Request.BodyAs<TMovie>();
            try
            {
                if (!GetMovieService().UpdateMovie(movie, false))
                    throw new Exception("Could not update Movie");
            }
            finally
            {
                movie.Dispose();
            }
            Context.Response.HXTriggerClientEvent("savedEvent", "Movie Saved OK");
            GetMovie(movieID);
        }

        [MVCDoc("Create a specified Movie")]
        [MVCPath("")]
        [MVCHTTPMethod(HttpMethod.POST)]
        [MVCProduces("text/html")]
        public void CreateMovie()
        {
            var movie = Context.Request.BodyAs<TMovie>();
            try
            {
                if (!GetMovieService().UpdateMovie(movie, true))
                    throw new Exception("Could not create Movie");
                int movieID = movie.MovieID;
                Context.Response.HXTriggerClientEvent("myEventObject", movie);
            }
            finally
            {
                movie.Dispose();
            }
            _includeBlankRow = true;
            GetMovie(movieID);
        }

        [MVCDoc("Delete a specified Movie")]
        [MVCPath("/($MovieID)")]
        [MVCHTTPMethod(HttpMethod.DELETE)]
        [MVCConsumes("application/json")]
        [MVCProduces("text/html")]
        public void DeleteMovie(int movieID)
        {
            if (!GetMovieService().DeleteMovie(movieID))
                throw new Exception("Could not delete Movie");
            Context.Response.HXTriggerClientEvent("savedEvent", "Movie Deleted OK");
            RenderForm(new string[] { });
        }

        private IMovieService GetMovieService() => ServiceLocator.GetMovieService();
        private IGenreService GetGenreService() => ServiceLocator.GetGenreService();
    }
}
