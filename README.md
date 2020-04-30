# CosmosSQLANTLR

ANTLR project for Cosmos DB SQL query language: [https://docs.microsoft.com/en-us/azure/cosmos-db/sql-query-getting-started](https://docs.microsoft.com/en-us/azure/cosmos-db/sql-query-getting-started).

Included is a `sql.g4` grammar file that be used with ANTLR to generate a parser for the language of your choice.

This project has included a demo .net use case that leverages the parser and an AST to provide the following API:

```c#
string query = "SELECT * FROM c";
if (!SqlQuery.TryParse(query, out SqlQuery parsedQuery))
{
    throw new FormatException($"Failed to parse query: {query}");
}

DoSomethingWithParsedQuery(parsedQuery);
```

Something similar can be done with other languages that ANTLR supports.
