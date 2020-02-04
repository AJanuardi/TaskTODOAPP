using System;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace TODO
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var text = System.IO.File.ReadAllLines("Token.txt").Last();
            var token = text;
            Token tkn = new Token();
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            HttpClient client = new HttpClient(clientHandler);
            
            var root = new CommandLineApplication()
            {
                Name = "To Do List App",
                Description = "Create ToDo List with User Authentication",
                ShortVersionGetter = () => "1.0.0",
            };

            root.Command("list",app => 
            {
                app.Description = "Get List from Account";
                
                app.OnExecuteAsync(async cancellationToken => 
                {
            
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,"https://localhost:5001/todo");
                    if( token != "")
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    HttpResponseMessage response = await client.SendAsync(request);
                    var json = await response.Content.ReadAsStringAsync();
                    
                    var list = JsonSerializer.Deserialize<List<Todo>>(json);
                    Console.WriteLine("To Do List");
                    foreach(var x in list)
                    {
                        Console.WriteLine(x.id+"."+" | "+x.activity+" | "+ x.status);
                    }
                });
            });

            root.Command("add",app => 
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    
                app.Description = "Add ToDo List Activity";
                var text = app.Argument("Text","Masukkan Text");
                app.OnExecuteAsync(async cancellationToken => 
                {
                    var add = new Todo()
                    {
                        activity = text.Value,
                    };
                    var data = JsonSerializer.Serialize(add);
                    var hasil = new StringContent(data,Encoding.UTF8,"application/json");
                    var response = await client.PostAsync("https://localhost:5001/todo/add",hasil);
                });
            });

            root.Command("clear",app => 
            {
                app.Description = "Delete All ToDo List";
                
                
                app.OnExecuteAsync(async cancellationToken => 
                {
                    Prompt.GetYesNo("Are you sure delete all ToDo List?",false);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,"https://localhost:5001/todo/clear");
                    HttpResponseMessage response = await client.SendAsync(request);
                });
            });

            root.Command("update",app => 
            {
                app.Description = "Get update of Activity from ToDo List";

                var text = app.Argument("Text","Masukkan Text",true);
                app.OnExecuteAsync(async cancellationToken => 
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                    var add = "{" + "\"activity\":" + $"\"{text.Values[1]}\"" + "}";
                    var hasil = new StringContent(add,Encoding.UTF8,"application/json");
                    var responses = await client.PatchAsync($"https://localhost:5001/todo/update{text.Values[0]}",hasil);
                });
            });

            root.Command("delete",app => 
            {
                app.Description = "Delete an Item from ToDo List";
                var text = app.Argument("Text","Masukkan Text");
                app.OnExecuteAsync(async cancellationToken => 
                {   
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,$"https://localhost:5001/todo/delete/{text.Value}");
                    HttpResponseMessage response = await client.SendAsync(request);
                });
            });

            root.Command("done",app => 
            {
                app.Description = "Checklist an Item in ToDo List";

                var text = app.Argument("Text","Masukkan Text");
                app.OnExecuteAsync(async cancellationToken => 
                {   
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,$"https://localhost:5001/todo/done/{text.Value}");
                    HttpResponseMessage response = await client.SendAsync(request);
                });
            });

            root.Command("register",app => 
            {
                app.Description = "Register new account";

                var text = app.Argument("Text","Masukkan Text");
                var pass = app.Argument("Type Password", "Text");
                app.OnExecuteAsync(async cancellationToken => 
                {   
                    var add = new User()
                    {
                        Nama = text.Value,
                        Password = pass.Value
                    };
                    var data = JsonSerializer.Serialize(add);
                    var hasil = new StringContent(data,Encoding.UTF8,"application/json");
                    var response = await client.PostAsync("https://localhost:5001/user/register",hasil);
                });
            });

            root.Command("login",app => 
            {
                app.Description = "Logged In";

                var text = app.Argument("Text","Masukkan Text");
                var pass = app.Argument("Type Password", "Text");
                app.OnExecuteAsync(async cancellationToken => 
                {   
                    var add = new User()
                    {
                        Nama = text.Value,
                        Password = pass.Value
                    };
                    var data = JsonSerializer.Serialize(add);
                    var hasil = new StringContent(data,Encoding.UTF8,"application/json");
                    var response = await client.PostAsync("https://localhost:5001/user/login",hasil);
                    var json = await response.Content.ReadAsStringAsync();
                    var token = JsonSerializer.Deserialize<Token>(json);
                    Console.WriteLine(token.token);
                    token.SaveToken();
                });
            });

            return root.Execute(args);
        }

        public class Todo
        {
            public int id{get;set;}
            public string activity {get;set;}
            public string status {get;set;}
        }
        public class User
        {
            public int UserId {get; set;}
            public string Nama {get; set;}
            public string Password {get; set;}
            public ICollection<Todo> Todo { get; set; }
        }
    }
}