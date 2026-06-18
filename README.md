# csharp-crud-rest-api
Simple application to showcase a CRUD Rest API application in C# with ASP.NET Core

### Prerequisites
Make sure you have .NET 9 installed.
You can check which version you have installed using this command:
```bash script
dotnet --version
```

#### Docker
Make sure you have docker installed using this command:
```bash script
docker --version
```

#### Curl
Make sure you have curl installed using this command:
```bash script
curl --version
```

### Running the application locally

### Build code
Build the code without running it
```bash script
dotnet build
```

### Test code
Run all the tests
```bash script
dotnet test
```

#### Running the application locally

##### Create docker image of the C# app
Creating a docker image should be as simple as
```bash
sudo docker build -t csharpcrudrestapi .
```

##### 👟 Run all the needed services and the application
```bash script
docker compose up
```

##### 🧪 Test the applications endpoints

Request to get all the users:
```bash script
curl --location --request GET 'http://localhost:8080/users'
```
Example of a response:
`[
  {
    "id": 1,
    "name": "aaa",
    "email": "aaa@mail"
  },
  {
    "id": 2,
    "name": "bbb",
    "email": "bbb@mail"
  }
]`

Request to create a new user
```bash script
curl --location --request POST 'http://localhost:8080/user' \
--header 'Content-Type: application/json' \
--data-raw '{"name": "aaa","email": "aaa@mail"}'
```

Request to get one specific user:
```bash script
curl --location --request GET 'http://localhost:8080/user/2'
```
Example of a response:
`{
  "id": 2,
  "name": "aaa",
  "email": "aaa@mail"
}`

Request to update a user
```bash script
curl --location --request PUT 'http://localhost:8080/user/2' \
--header 'Content-Type: application/json' \
--data-raw '{"id":2,"name": "new","email": "new@mail"}'
```

Request to delete a user
```bash script
curl --location --request DELETE 'http://localhost:8080/user/3'
```

#### Api documentation
The api documentation is available here
> **_NOTE:_** The application has to be running

http://localhost:8080/swagger

### Contact

Questions? please create an
[issue](https://github.com/MikAoJk/csharp-crud-rest-api/issues)
