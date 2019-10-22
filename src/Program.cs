using System.Collections.Generic;
using System.Threading;
using System;

namespace sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = 5000;

            var categories = new List<Category>();
            categories.Add(new Category(1, "Beverages"));
            categories.Add(new Category(2, "Condiments"));
            categories.Add(new Category(3, "Confections"));

            var router = new Router(categories);

            var server = new RDJTPServer(port, router.onMessage);
            server.Run();
        }
    }

}