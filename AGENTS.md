\# Woolly Bunny Development Rules



<!-- codebase-memory-mcp:start -->

\# Codebase Knowledge Graph (codebase-memory-mcp)



This project uses codebase-memory-mcp to maintain a knowledge graph of the codebase.

ALWAYS prefer MCP graph tools over grep/glob/file-search for code discovery.



\## Priority Order

1\. `search\_graph` — find functions, classes, routes, variables by pattern

2\. `trace\_path` — trace who calls a function or what it calls

3\. `get\_code\_snippet` — read specific function/class source code

4\. `query\_graph` — run Cypher queries for complex patterns

5\. `get\_architecture` — high-level project summary



\## When to fall back to grep/glob

\- Searching for string literals, error messages, config values

\- Searching non-code files (Dockerfiles, shell scripts, configs)

\- When MCP tools return insufficient results

<!-- codebase-memory-mcp:end -->

