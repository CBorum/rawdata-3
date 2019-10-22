using System;
using System.Collections.Generic;
using System.Text.Json;

namespace sharp
{
    class Router
    {
        const string STATUS_OK = "1 Ok";
        const string STATUS_CREATED = "2 Created";
        const string STATUS_UPDATED = "3 Updated";
        const string STATUS_BAD_REQUEST = "4 Bad Request";
        const string STATUS_NOT_FOUND = "5 Not Found";
        const string STATUS_ERROR = "6 Error";
        string[] methods = new string[] { "create", "read", "update", "delete", "echo" };
        List<Category> categories;
        int index;
        // Dictionary<int, Category> categories;

        public Router(List<Category> categories)
        {
            this.categories = categories;
            this.index = categories.Count + 1;
        }

        public Response onMessage(string message)
        {
            Console.WriteLine("onMessage {0}", message);
            Request request;
            try
            {
                request = JsonSerializer.Deserialize<Request>(message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                return new Response(STATUS_ERROR);
            }

            var errors = CheckParameters(request);
            if (errors.Count > 0)
            {
                return new Response(STATUS_BAD_REQUEST + " missing resource " + String.Join(", ", errors.ToArray()));
            }

            if (request.method.Equals("echo"))
            {
                return new Response(STATUS_OK, request.body);
            }

            if (!request.path.StartsWith("/api/categories"))
            {
                return new Response(STATUS_BAD_REQUEST);
            }

            switch (request.method)
            {
                case "read":
                    return read(request);
                case "update":
                    return update(request);
                case "create":
                    return create(request);
                case "delete":
                    return delete(request);
                default:
                    return new Response(STATUS_BAD_REQUEST);
            }
        }

        Response read(Request request)
        {
            var pathsSplit = request.path.Split("/");
            if (pathsSplit.Length > 3)
            {
                try
                {
                    int cid = Int32.Parse(pathsSplit[3]);
                    foreach (var cat in categories)
                    {
                        if (cid == cat.cid)
                        {
                            return new Response(STATUS_OK, JsonSerializer.Serialize(cat));
                        }
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Format exception");
                    return new Response(STATUS_BAD_REQUEST);
                }
                return new Response(STATUS_NOT_FOUND);
            }
            else
            {
                return new Response(STATUS_OK, JsonSerializer.Serialize(categories));
            }
        }

        Response create(Request request)
        {
            var pathsSplit = request.path.Split("/");
            if (pathsSplit.Length > 3)
            {
                return new Response(STATUS_BAD_REQUEST);
            }
            try
            {
                var category = JsonSerializer.Deserialize<Category>(request.body);
                category.cid = index++;
                categories.Add(category);
                return new Response(STATUS_CREATED, category.AsJson());
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                return new Response(STATUS_ERROR);
            }
        }

        Response update(Request request)
        {
            var pathsSplit = request.path.Split("/");
            if (pathsSplit.Length > 3)
            {
                try
                {
                    int cid = Int32.Parse(pathsSplit[3]);
                    var body = JsonSerializer.Deserialize<Category>(request.body);
                    foreach (var cat in categories)
                    {
                        if (cid == cat.cid)
                        {

                            cat.name = body.name;
                            return new Response(STATUS_UPDATED);
                        }
                    }
                    return new Response(STATUS_NOT_FOUND);
                }
                catch (FormatException)
                {
                    Console.WriteLine("Format exception");
                    return new Response(STATUS_BAD_REQUEST);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e);
                    return new Response(STATUS_ERROR);
                }
            }
            return new Response(STATUS_BAD_REQUEST);
        }

        Response delete(Request request)
        {
            var pathsSplit = request.path.Split("/");
            if (pathsSplit.Length > 3)
            {
                try
                {
                    int cid = Int32.Parse(pathsSplit[3]);
                    Console.WriteLine(cid);
                    foreach (var cat in categories)
                    {
                        Console.WriteLine(cat.cid);
                        if (cid == cat.cid)
                        {
                            categories.Remove(cat);
                            return new Response(STATUS_OK);
                        }
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Format exception");
                    return new Response(STATUS_BAD_REQUEST);
                }
                return new Response(STATUS_NOT_FOUND);
            }
            return new Response(STATUS_BAD_REQUEST);
        }

        List<string> CheckParameters(Request request)
        {
            var errors = new List<string>();
            // Method
            if (request.method == null)
            {
                errors.Add("missing method");
            }
            else if (Array.IndexOf(methods, request.method) < 0)
            {
                errors.Add("illegal method");
            }
            // Path
            if (request.path == null && request.method == null)
            {
                errors.Add("missing path");
            }
            else if (request.path == null && request.method != null && !request.method.Equals("echo"))
            {
                errors.Add("missing path");
            }
            // Date
            if (request.date == null)
            { // regex \d+
                errors.Add("missing date");
            }
            else
            {
                try
                {
                    Int32.Parse(request.date);
                }
                catch (FormatException)
                {
                    errors.Add("illegal date");
                }
            }

            // Body
            if (request.method != null && request.body == null && (request.method.Equals("create") || request.method.Equals("update") || request.method.Equals("echo")))
            {
                errors.Add("missing body");
            }

            if (request.body != null && request.method != null && !request.method.Equals("echo"))
            {
                try
                {
                    JsonSerializer.Deserialize<Category>(request.body);
                }
                catch (Exception)
                {
                    errors.Add("illegal body");
                }
            }

            return errors;
        }
    }

    class Request
    {
        public Request() { }

        public Request(string method, string path, string date)
        {
            this.method = method;
            this.path = path;
            this.date = date;
        }

        public string method { get; set; }
        public string path { get; set; }
        public string date { get; set; } // Unix
        public string body { get; set; }
    }

    public class Response
    {
        public Response(string status)
        {
            this.status = status;
        }

        public Response(string status, string body)
        {
            this.status = status;
            this.body = body;
        }
        public string status { get; set; }
        public string body { get; set; }

        public string AsJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}