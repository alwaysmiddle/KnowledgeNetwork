import { useAppSelector, useAppDispatch } from '../store/hooks';
import { clearRecentActivity } from '../store/fileSystemSlice';
import { Trash2, Plus, Edit3, X, Clock } from 'lucide-react';

export function ActivityFeed() {
  const dispatch = useAppDispatch();
  const recentActivity = useAppSelector((state) => state.fileSystem.recentActivity);
  const lastActivityTime = useAppSelector((state) => state.fileSystem.lastActivityTime);

  const getActivityIcon = (type: 'added' | 'modified' | 'removed') => {
    switch (type) {
      case 'added':
        return <Plus className="w-4 h-4 text-green-400" />;
      case 'modified':
        return <Edit3 className="w-4 h-4 text-yellow-400" />;
      case 'removed':
        return <Trash2 className="w-4 h-4 text-red-400" />;
    }
  };

  const getActivityColor = (type: 'added' | 'modified' | 'removed') => {
    switch (type) {
      case 'added':
        return 'border-green-500 bg-green-900/20';
      case 'modified':
        return 'border-yellow-500 bg-yellow-900/20';
      case 'removed':
        return 'border-red-500 bg-red-900/20';
    }
  };

  const formatRelativeTime = (timestamp: string): string => {
    const now = new Date();
    const activityTime = new Date(timestamp);
    const diffMs = now.getTime() - activityTime.getTime();
    const diffSeconds = Math.floor(diffMs / 1000);
    const diffMinutes = Math.floor(diffSeconds / 60);

    if (diffSeconds < 10) return 'just now';
    if (diffSeconds < 60) return `${diffSeconds}s ago`;
    if (diffMinutes < 60) return `${diffMinutes}m ago`;
    
    return activityTime.toLocaleTimeString();
  };

  const handleClearActivity = () => {
    dispatch(clearRecentActivity());
  };

  if (recentActivity.length === 0) {
    return (
      <div className="bg-gray-800 rounded-lg border border-gray-700 p-4">
        <div className="flex items-center gap-2 mb-3">
          <Clock className="w-4 h-4 text-gray-400" />
          <h3 className="text-sm font-medium">Recent Activity</h3>
        </div>
        <div className="text-center py-6 text-gray-500">
          <Clock className="w-8 h-8 mx-auto mb-2 opacity-50" />
          <p className="text-sm">No recent file activity</p>
          <p className="text-xs mt-1">Start watching to see changes</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-gray-800 rounded-lg border border-gray-700 p-4">
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center gap-2">
          <Clock className="w-4 h-4 text-gray-400" />
          <h3 className="text-sm font-medium">Recent Activity</h3>
          <span className="px-2 py-0.5 bg-blue-600 text-xs rounded-full">
            {recentActivity.length}
          </span>
        </div>
        <button
          onClick={handleClearActivity}
          className="flex items-center gap-1 px-2 py-1 text-xs text-gray-400 hover:text-white transition-colors"
          title="Clear activity log"
        >
          <X className="w-3 h-3" />
        </button>
      </div>

      <div className="space-y-2 max-h-64 overflow-y-auto">
        {recentActivity.map((activity) => (
          <div
            key={activity.id}
            className={`flex items-start gap-3 p-3 rounded-lg border-l-2 transition-all duration-200 ${getActivityColor(activity.type)}`}
          >
            <div className="flex-shrink-0 mt-0.5">
              {getActivityIcon(activity.type)}
            </div>
            
            <div className="flex-1 min-w-0">
              <div className="flex items-start justify-between gap-2">
                <div className="min-w-0">
                  <p className="text-sm font-medium truncate">
                    {activity.fileName}
                  </p>
                  <p className="text-xs text-gray-400 mt-0.5 capitalize">
                    {activity.type}
                  </p>
                </div>
                <span className="text-xs text-gray-500 flex-shrink-0">
                  {formatRelativeTime(activity.timestamp)}
                </span>
              </div>
              
              {activity.filePath && (
                <p className="text-xs text-gray-500 mt-1 truncate font-mono">
                  {activity.filePath}
                </p>
              )}
            </div>
          </div>
        ))}
      </div>

      {lastActivityTime && (
        <div className="mt-3 pt-3 border-t border-gray-700">
          <p className="text-xs text-gray-500 text-center">
            Last activity: {formatRelativeTime(lastActivityTime)}
          </p>
        </div>
      )}
    </div>
  );
}