using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NYTimes_HTTPServer;

public class Book
{
    public required string book_title { get; set; }
    public required string summary { get; set; }
    public List<string> isbn13 { get; set; }

    public Book() 
    {
        isbn13 = new List<string>();
    }
    public Book(string title, string sum)
    {
        book_title = title;
        summary = sum;
        isbn13 = new List<string>();
    }
}
