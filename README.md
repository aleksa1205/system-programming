# System Programming (Sistemsko programiranje)
**Grade: 97/100**

This repository contains three projects developed as part of a university course on **System Programming**. Each project involved implementing a web server with increasing complexity, focusing on concurrency, asynchronous programming, and reactive programming principles.
## First project
In this project, we developed a web server as a console application. The server was responsible for logging all incoming requests and capturing details about their processing, including errors and successful responses.

To enhance performance, we implemented in-memory caching, allowing the server to return precomputed responses for repeated requests. The server used the **New York Times Books API** as a data source, enabling users to search for books by author. Requests were handled exclusively via the `GET` method, and if no books were found for a given author, the server returned an error message.


## Second project
In the second project, the goal remained the same as in the first one, but instead of using threads, we were required to implement tasks and asynchronous operations where applicable.
## Third project
In this project, we developed a web server as a console application that logged all incoming requests and provided detailed execution information. The implementation required using **.NET Reactive Extensions (Rx)** to follow the reactive programming paradigm. To ensure concurrency, Rx had to be multithreaded, leveraging schedulers for task execution.

The server utilized the **News API** to retrieve and display the content of an article based on a given keyword. Additionally, sentiment analysis was performed on the collected data.
## API Reference
#### First and second project
Retrieve all books written by a given author.

```http
  GET http://address:port/name_surname
```

| Parameter      | Type     | Description                |
| :--------      | :------- | :------------------------- |
| `address`      | `string` | localhost by default.      |
| `port`         | `number` | 5050 by default.           |
| `name_surname` | `string` | **Required**. Name and surname of the author to be searched. |

#### Third project
Retrieve all articles where a specific keyword is present.
```http
  GET http://address:port/keyword
```

| Parameter      | Type     | Description                |
| :--------      | :------- | :------------------------- |
| `address`      | `string` | localhost by default.      |
| `port`         | `number` | 5050 by default.           |
| `keyword`      | `string` | **Required.** What to search for in articles.   |

## Tech Stack

⚙️ **Framework:** .NET 8 (C#)

📡 **APIs Used:** New York Times Books API and News API


## Installation

### Prerequisites:
- **.NET**: [Download .NET](https://dotnet.microsoft.com/en-us/download)
- **API key for New York Times Books API**: [Register](https://developer.nytimes.com/accounts/create)
- **API key for News API**: [Register](https://newsapi.org/register)

### Steps to run the projects:

1. Clone the repository: 
```bash
git clone https://github.com/aleksa1205/parfumique
cd system-programming
```
2. Navigate to the specific project directory:
`cd NYTimes_HTPPServer`

3. Update API key(if expired):
In the `ApiService.cs`, locate the constructor. Inside the constructor, change the value of `apiKey` with your new API key.

4. Run the application:
```bash
dotnet restore
dotnet run
```

5. Make API request using web browser or Postman.
## Authors
👤  [Aleksa Perić](https://github.com/aleksa1205)

👤  [Jovan Cvetković](https://github.com/CJovan02)

