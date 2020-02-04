using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TODODATABASE.Data;
using TODODATABASE.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TODODATABASE.Controllers
{
    [ApiController]
    [Route("/todo")]
    public class TodoControllers : ControllerBase
    {
    
        private AppDbContext _appDbContext;

        public TodoControllers(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Get()
        {
            var text = System.IO.File.ReadAllLines("Token.txt").Last();
            var token = text;
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;
            var sub = securityToken.Claims.First(u => u.Type == "sub").Value;
            var x = from l in _appDbContext.Todos where l.UserId == Convert.ToInt32(sub) select l;

            return Ok(x);
        }

        [Authorize]
        [HttpPost("add")]
        public ActionResult<ToDo> Post([FromBody] ToDo todo)
        {
            var text = System.IO.File.ReadAllLines("Token.txt").Last();
            var token = text;
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;
            _appDbContext.Add(todo);
            todo.status="Do It Now";
            _appDbContext.SaveChanges();

            return Ok(_appDbContext.Todos.Include("User").ToList());
        }

        [Authorize]
        [HttpPatch("update/{id}")]
        public IActionResult Update(int id, [FromBody] ToDo todoRequest)
        {
            var text = System.IO.File.ReadAllLines("Token.txt").Last();
            var token = text;
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;
            var todo = _appDbContext.Todos.Find(id);
            todo.activity = todoRequest.activity;
            _appDbContext.SaveChanges();

            return Ok(_appDbContext.Todos.Include("User").ToList());
        }

        [Authorize]
        [HttpPatch]
        [Route("done/{id}")]
        public IActionResult Done(int id)
        {
            var text = System.IO.File.ReadAllLines("Token.txt").Last();
            var token = text;
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;
            var todo = _appDbContext.Todos.Find(id);
            todo.status = "Done" ;
            _appDbContext.SaveChanges();

            return Ok(_appDbContext.Todos.Include("User").ToList());
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public ActionResult<string> Delete(int id)
        {
            var text = System.IO.File.ReadAllLines("Token.txt").Last();
            var token = text;
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;
            var activity = _appDbContext.Todos.Find(id);
            _appDbContext.Attach(activity);
            _appDbContext.Remove(activity);
            _appDbContext.SaveChanges();
            return $"Menghapus data ID: {id}";
        }

        [Authorize]
        [HttpDelete("clear")]
        public ActionResult<string> clear(int id)
        {
            var text = System.IO.File.ReadAllLines("Token.txt").Last();
            var token = text;
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;
            var sub = securityToken.Claims.First(u => u.Type == "sub").Value;
            var x = from l in _appDbContext.Todos where l.UserId == Convert.ToInt32(sub) select l;
            _appDbContext.Todos.RemoveRange(x);
            _appDbContext.SaveChanges();
            return Ok("ToDo List Clearenced");
        }
    }
}
