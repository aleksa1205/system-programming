# System programming (Sistemsko programiranje)

## Built for university course, graded 97 out of 100.

Course consisted of three projects.

### First project
In the first project, we had to implement a web server as console application. The server was responsible for logging all incoming requests and recording information about how they were processed.  

Additionally, we had to implement memory caching to optimize performance.
For this project, we were provided with NY Times API as a data source that should allow users to search for books. The search was filtered by author, as defined in the query and only the GET method was permitted. If no books were found for the given author, an error was returned.

### Second project
In the second project, the goal remained the same as in the first one, but the key difference was that tasks and asynchronous operations had to be used instead of threads for handling operations.

### Third project
In the third project, we were tasked with implementing web server as a console appliaction that logs all requests and provedes detailed information about their execution. The implementation required the use of .NET RX to follow the reactive programming paradigm. RX had to be multithreaded, utilizing schedulers for concurrency. We were provided with News API to display content of one article and for the data collection sentiment analysis was to be applied.
### API Reference

#### First and second project
Get all the books for the author with given name and surname.

```http
  GET http://address:port/name_surname
```

| Parameter      | Type     | Description                |
| :--------      | :------- | :------------------------- |
| `address`      | `string` | localhost by default.      |
| `port`         | `number` | 5050 by default.           |
| `name_surname` | `string` | **Required**. Name and surname of the author to be searched. |

#### Third project

```http
  GET http://address:port/keyword
```

| Parameter      | Type     | Description                |
| :--------      | :------- | :------------------------- |
| `address`      | `string` | localhost by default.      |
| `port`         | `number` | 5050 by default.           |
| `keyword`      | `string` | **Required.** What to search for in articles.   |


## Authors

- [Aleksa Perić](https://github.com/aleksa1205)
- [Jovan Cvetković](https://github.com/CJovan02)

