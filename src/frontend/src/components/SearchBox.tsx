import { useState, useEffect } from 'react';
import { Search, X } from 'lucide-react';
import { useAppSelector, useAppDispatch } from '../store/hooks';
import { setSearchQuery, setSearchResults, clearSearch } from '../store/fileSystemSlice';
import { mockFileSystem } from '../data/mockFileSystem';
import { type FileNode } from '../types/fileSystem';

export function SearchBox() {
  const dispatch = useAppDispatch();
  const searchQuery = useAppSelector((state) => state.fileSystem.searchQuery);
  const searchResults = useAppSelector((state) => state.fileSystem.searchResults);
  const [localQuery, setLocalQuery] = useState(searchQuery);

  // Search function
  const searchFiles = (query: string, node: FileNode, results: string[] = []): string[] => {
    if (node.name.toLowerCase().includes(query.toLowerCase())) {
      results.push(node.id);
    }
    
    if (node.children) {
      node.children.forEach(child => {
        searchFiles(query, child, results);
      });
    }
    
    return results;
  };

  // Update search results when query changes
  useEffect(() => {
    if (localQuery.trim()) {
      const results = searchFiles(localQuery.trim(), mockFileSystem);
      dispatch(setSearchResults(results));
      dispatch(setSearchQuery(localQuery));
    } else {
      dispatch(clearSearch());
    }
  }, [localQuery, dispatch]);

  const handleClear = () => {
    setLocalQuery('');
    dispatch(clearSearch());
  };

  const hasResults = searchResults.length > 0;
  const hasQuery = searchQuery.trim().length > 0;

  return (
    <div className="px-3 py-2 border-b border-gray-700">
      <div className="relative">
        <Search 
          size={16} 
          className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" 
        />
        <input
          type="text"
          placeholder="Search files..."
          value={localQuery}
          onChange={(e) => setLocalQuery(e.target.value)}
          className="w-full pl-10 pr-8 py-2 bg-gray-700 border border-gray-600 rounded-md text-sm text-gray-300 placeholder-gray-500 focus:outline-none focus:border-blue-500 focus:bg-gray-650"
        />
        {localQuery && (
          <button
            onClick={handleClear}
            className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-300"
          >
            <X size={16} />
          </button>
        )}
      </div>
      
      {/* Results counter */}
      {hasQuery && (
        <div className="mt-2 text-xs text-gray-400">
          {hasResults ? (
            `${searchResults.length} result${searchResults.length === 1 ? '' : 's'} found`
          ) : (
            'No results found'
          )}
        </div>
      )}
    </div>
  );
}