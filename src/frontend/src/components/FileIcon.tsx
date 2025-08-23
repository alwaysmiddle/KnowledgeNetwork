import { 
  Folder, 
  File, 
  FileText, 
  Code2, 
  FileJson, 
  FileType 
} from 'lucide-react';

interface FileIconProps {
  type: 'file' | 'folder';
  extension?: string;
  isOpen?: boolean;
  size?: number;
}

export function FileIcon({ type, extension, size = 16 }: FileIconProps) {
  if (type === 'folder') {
    return <Folder size={size} className="text-blue-400" />;
  }

  // File icons based on extension
  switch (extension?.toLowerCase()) {
    case 'ts':
    case 'tsx':
      return <FileType size={size} className="text-blue-500" />;
    case 'js':
    case 'jsx':
      return <FileType size={size} className="text-yellow-500" />;
    case 'cs':
      return <Code2 size={size} className="text-purple-500" />;
    case 'json':
      return <FileJson size={size} className="text-yellow-600" />;
    case 'md':
      return <FileText size={size} className="text-gray-400" />;
    case 'txt':
      return <FileText size={size} className="text-gray-500" />;
    default:
      return <File size={size} className="text-gray-400" />;
  }
}