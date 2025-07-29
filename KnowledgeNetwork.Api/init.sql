-- Initialize database with AGE extension for graph capabilities
CREATE EXTENSION IF NOT EXISTS age;

-- Load AGE into the search path
LOAD 'age';
SET search_path = ag_catalog, "$user", public;

-- Create a graph for knowledge network
SELECT create_graph('knowledge_graph');