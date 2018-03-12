using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

class Program
{
    WebRequestHandler handler;
    HttpClient client;
    Queue<Uri> q = new Queue<Uri>();
    HashSet<Uri> set = new HashSet<Uri>();

    async Task SearchURI(Uri uri_to_search) {
        Console.Out.WriteLine("Searching " + uri_to_search);

        HttpResponseMessage response = await client.GetAsync(uri_to_search);
        String body = await response.Content.ReadAsStringAsync();
        //Console.Out.WriteLine(body);

        int count = 0;
        for (int i = 0; (i = body.IndexOf("<a ", i)) != -1; i++)
        {
            int a_end = body.IndexOf(">", i);
            if (a_end == -1) continue;
            String a_tag = body.Substring(i, 1+ a_end - i);
            //Console.Out.WriteLine(a_tag);

            int url_begin = a_tag.IndexOf("href")+4;
            while (url_begin!=a_tag.Length && "= \'\"".IndexOf(a_tag[url_begin])!=-1)
                url_begin++;
            int url_end = url_begin;
            while (url_end != a_tag.Length && "\'\"".IndexOf(a_tag[url_end]) == -1)
                url_end++;
            if (url_end == a_tag.Length)
                continue;

            String parsed_uri = a_tag.Substring(url_begin, url_end - url_begin);
            Uri uri_found;
            try
            {
                uri_found = new Uri(uri_to_search, parsed_uri);
                if (!set.Contains(uri_found)) {
                    //Console.Out.WriteLine(uri_found);
                    count++;
                    set.Add(uri_found);
                    q.Enqueue(uri_found);
                }
                
            }
            catch (UriFormatException e) {}            
            
        }
        Console.Out.WriteLine(count + " new URLs extracted\n");
    }

    async Task BeginAsync()
    {
        while (set.Count() < 100 && q.Count != 0)
        {
            await SearchURI(q.Dequeue());
        }
        if (set.Count() >= 100)        
            System.Console.WriteLine("Stopping because crawler discovered 100 unique URLs");        
        else
            System.Console.WriteLine("Stopping because crawler exhausted all paths");

        int n = 0;
        foreach (Uri uri in set)
        {
            if (n++ == 100)
                break;
            System.Console.WriteLine(n + "\t" + uri);            
        }
    }

    String default_input = "http://www.bbc.co.uk/news";

    Program() {
        handler = new WebRequestHandler();
        //handler.ClientCertificates.Add(new X509Certificate());
        client = new HttpClient(handler);

        Console.Write("Please enter a Start URL (or leave empty for default):");
        String input = Console.ReadLine();
        if (input == "")
            input = default_input;

        Uri root;
        try
        {
            root = new Uri(input);
            q.Enqueue(root);
            set.Add(root);
        }
        catch (UriFormatException e) {
            Console.WriteLine(e);
            return;
        }        

        BeginAsync().GetAwaiter().GetResult();
    }

    static void Main(string[] args)
    {
        Program p = new Program();
    }
}
