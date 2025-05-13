using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Controllers.Base;       
using BusinessObjects;        
using Services;                

namespace ControllersArticles
{
    
    [ApiController]
    [Route("articles")]
    public class ArticlesController : BaseController
    {
        private readonly IArticlesService _articlesService;

        public ArticlesController(IArticlesService articlesService)
            : base()
        {
            _articlesService = articlesService;
        }

        
        [HttpGet]
        public IActionResult GetArticles()
        {
            var articles = _articlesService.GetAll();
            return Ok(articles);
        }

        
        [HttpGet("searches")]
        public IActionResult GetArticlesByDescription([FromQuery(Name = "q")] string search = "")
        {
            var articles = _articlesService.GetArticles(search);
            return Ok(articles);
        }

        
        [HttpGet("meta")]
        public IActionResult GetArticleMeta()
        {
            var meta = _articlesService.GetMeta();
            return Ok(meta);
        }

        
        [HttpGet("{id}")]
        public IActionResult GetArticleByID(int id)
        {
            var article = _articlesService.GetByID(id);
            return Ok(article);
        }

 
        [HttpDelete("{id}")]
        public IActionResult DeleteArticleByID(int id)
        {
            var article = _articlesService.GetByID(id);
            _articlesService.Delete(article);
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateArticleByID([FromBody] TArticle article, int id)
        {
            article.ID = id;
            _articlesService.Update(article);
            return Ok();
        }

        [HttpPost]
        public IActionResult CreateArticle([FromBody] TArticle article)
        {
            _articlesService.Add(article);
            return Created($"/articles/{article.ID}", "Article Created");
        }

        
        [HttpPost("bulk")]
        public IActionResult CreateArticles([FromBody] List<TArticle> articleList)
        {
            _articlesService.CreateArticles(articleList);
            return Created(string.Empty, "Articles created");
        }
    }
}
